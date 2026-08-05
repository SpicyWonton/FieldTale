using System.Collections.Generic;
using Fantasy;
using UnityEngine;
using Log = UnityGameFramework.Runtime.Log;

namespace FieldTale.HotUpdate
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Player : Entity
    {
        // 客户端输入命令和服务端权威模拟统一为 20 Hz。
        private const float TickInterval = 0.05f;
        // 远端玩家故意落后两个逻辑帧渲染，为快照抖动预留插值空间。
        private const float RenderDelayTicks = 2f;
        // 小误差渐进修正；误差过大时直接采用权威位置。
        private const float CorrectionSharpness = 12f;
        private const float TeleportDistance = 2f;

        // 已完成、已发送但可能尚未被服务端确认的本地输入帧。
        private struct InputCommand
        {
            public InputCommand(uint tick, Vector2 input)
            {
                Tick = tick;
                Input = input;
            }

            public uint Tick;
            public Vector2 Input;
        }

        // 远端玩家的一帧服务端权威状态。
        private struct Snapshot
        {
            public Snapshot(uint tick, Vector2 position)
            {
                Tick = tick;
                Position = position;
            }

            public uint Tick;
            public Vector2 Position;
        }

        [SerializeField]
        private PlayerData m_PlayerData = null;

        // 收到 ACK 后会移除已处理输入，再从权威位置回放剩余输入。
        private readonly List<InputCommand> m_PendingInputs = new List<InputCommand>();
        private readonly List<Snapshot> m_Snapshots = new List<Snapshot>();

        private float m_TickTimer;
        private float m_RenderTick;
        private Vector2 m_FrameInput;         // 当前渲染帧采样到的输入。
        private Vector2 m_TickInput;          // 当前 50 ms 逻辑帧锁存的输入。
        private Vector2 m_Correction;
        private uint m_ClientTick;
        private uint m_LastServerTick;
        private bool m_HasRenderTick;

        public Rigidbody2D CachedRigidbody2D
        {
            get;
            private set;
        }

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            CachedRigidbody2D = GetComponent<Rigidbody2D>();
        }

        protected override void OnShow(object userData)
        {
            base.OnShow(userData);

            m_PlayerData = userData as PlayerData;
            if (m_PlayerData == null)
            {
                Log.Error("Player data is invalid.");
                return;
            }

            m_TickTimer = 0f;
            m_RenderTick = 0f;
            m_FrameInput = SampleInput();
            m_TickInput = m_FrameInput;
            m_Correction = Vector2.zero;
            m_ClientTick = 0;
            m_LastServerTick = 0;
            m_HasRenderTick = false;
            m_PendingInputs.Clear();
            m_Snapshots.Clear();
            // 本地玩家由物理帧移动，需要刚体插值平滑到渲染帧；远端已经自行做快照插值。
            CachedRigidbody2D.interpolation = m_PlayerData.IsSelf
                ? RigidbodyInterpolation2D.Interpolate
                : RigidbodyInterpolation2D.None;
        }

        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            if (m_PlayerData.IsSelf)
            {
                // 输入按渲染帧采集，但只能在逻辑帧边界切换模拟输入。
                m_FrameInput = SampleInput();
            }
            else
            {
                RenderRemote(elapseSeconds);
            }
        }

        private void FixedUpdate()
        {
            if (m_PlayerData != null && m_PlayerData.IsSelf)
            {
                SimulateLocal(Time.fixedDeltaTime);
            }
        }

        public void ReceiveSnapshot(Vector3 position, uint serverTick, uint processedInputTick)
        {
            if (m_PlayerData == null)
            {
                return;
            }

            if (m_PlayerData.IsSelf)
            {
                Reconcile((Vector2)position, serverTick, processedInputTick);
                return;
            }

            BufferSnapshot((Vector2)position, serverTick);
        }

        private void SimulateLocal(float fixedDeltaSeconds)
        {
            Vector2 targetPosition = CachedRigidbody2D.position;

            float remainingSeconds = fixedDeltaSeconds;
            while (remainingSeconds > 0f)
            {
                // 一个FixedUpdate可能跨越20Hz逻辑帧边界，因此按边界拆分模拟时间。
                float stepSeconds = Mathf.Min(remainingSeconds, TickInterval - m_TickTimer);
                targetPosition = SimulateMove(targetPosition, m_TickInput, stepSeconds);
                m_TickTimer += stepSeconds;
                remainingSeconds -= stepSeconds;

                if (m_TickTimer < TickInterval)
                {
                    continue;
                }
                m_TickTimer = 0f;

                // 只有完整模拟满50ms的输入才能作为一条可回放命令发送。
                uint clientTick = ++m_ClientTick;
                m_PendingInputs.Add(new InputCommand(clientTick, m_TickInput));
                Fantasy.Runtime.Session.C2M_PlayerMove(
                    clientTick,
                    Mathf.RoundToInt(m_TickInput.x),
                    Mathf.RoundToInt(m_TickInput.y));

                // 下一逻辑帧使用最近一次Update采集到的输入。
                m_TickInput = m_FrameInput;
            }

            if (m_Correction.sqrMagnitude > Mathf.Epsilon)
            {
                // 逐物理帧消化小误差，避免每次收到权威快照都产生可见瞬移。
                float correctionFactor = 1f - Mathf.Exp(-CorrectionSharpness * fixedDeltaSeconds);
                Vector2 correctionStep = m_Correction * correctionFactor;
                targetPosition += correctionStep;
                m_Correction -= correctionStep;

                if (m_Correction.sqrMagnitude < 0.000001f)
                {
                    m_Correction = Vector2.zero;
                }
            }

            CachedRigidbody2D.MovePosition(targetPosition);
        }

        private void RenderRemote(float frameDeltaSeconds)
        {
            if (m_Snapshots.Count == 0)
            {
                return;
            }

            if (m_Snapshots.Count == 1)
            {
                CachedRigidbody2D.position = m_Snapshots[0].Position;
                return;
            }

            // 渲染时间线追赶到“最新服务端帧 - 插值延迟”，但不越过它。
            float targetRenderTick = m_Snapshots[m_Snapshots.Count - 1].Tick - RenderDelayTicks;
            m_RenderTick = Mathf.Min(
                m_RenderTick + frameDeltaSeconds / TickInterval,
                targetRenderTick);

            // 丢弃渲染时间线已经越过的旧快照，并保留前后两帧用于插值。
            while (m_Snapshots.Count > 1 && m_Snapshots[1].Tick <= m_RenderTick)
            {
                m_Snapshots.RemoveAt(0);
            }

            if (m_Snapshots.Count == 1)
            {
                CachedRigidbody2D.position = m_Snapshots[0].Position;
                return;
            }

            Snapshot fromSnapshot = m_Snapshots[0];
            Snapshot toSnapshot = m_Snapshots[1];
            float interpolationFactor = Mathf.InverseLerp(
                fromSnapshot.Tick,
                toSnapshot.Tick,
                m_RenderTick);
            CachedRigidbody2D.position = Vector2.Lerp(
                fromSnapshot.Position,
                toSnapshot.Position,
                interpolationFactor);
        }

        private void Reconcile(Vector2 authoritativePosition, uint serverTick, uint processedInputTick)
        {
            if (serverTick < m_LastServerTick)
            {
                return;
            }

            m_LastServerTick = serverTick;
            // 权威位置已经包含 processedInputTick 及之前的输入，不能重复回放。
            m_PendingInputs.RemoveAll(command => command.Tick <= processedInputTick);

            // 从服务端权威位置重新执行所有已发送但尚未确认的完整输入帧。
            Vector2 correctedPosition = authoritativePosition;
            for (int i = 0; i < m_PendingInputs.Count; i++)
            {
                correctedPosition = SimulateMove(correctedPosition, m_PendingInputs[i].Input, TickInterval);
            }

            // 当前逻辑帧尚未发送，也要按已经实际模拟的时长补回。
            correctedPosition = SimulateMove(correctedPosition, m_TickInput, m_TickTimer);
            Vector2 correction = correctedPosition - CachedRigidbody2D.position;
            if (correction.sqrMagnitude >= TeleportDistance * TeleportDistance)
            {
                // 大误差通常意味着严重丢帧或状态失配，继续平滑会长期不同步。
                CachedRigidbody2D.position = correctedPosition;
                m_Correction = Vector2.zero;
                return;
            }

            m_Correction = correction;
        }

        private void BufferSnapshot(Vector2 position, uint serverTick)
        {
            if (m_Snapshots.Count > 0 && serverTick <= m_Snapshots[m_Snapshots.Count - 1].Tick)
            {
                return;
            }

            m_Snapshots.Add(new Snapshot(serverTick, position));
            if (!m_HasRenderTick)
            {
                // 首帧快照直接建立延迟后的远端渲染时间线。
                m_RenderTick = Mathf.Max(0f, serverTick - RenderDelayTicks);
                m_HasRenderTick = true;
            }
        }

        private static Vector2 SampleInput()
        {
            return new Vector2(
                Mathf.RoundToInt(Input.GetAxisRaw("Horizontal")),
                Mathf.RoundToInt(Input.GetAxisRaw("Vertical")));
        }

        private Vector2 SimulateMove(Vector2 position, Vector2 input, float deltaTime)
        {
            if (input.sqrMagnitude > 1f)
            {
                input.Normalize();
            }

            return position + input * (m_PlayerData.Speed * deltaTime);
        }
    }
}
