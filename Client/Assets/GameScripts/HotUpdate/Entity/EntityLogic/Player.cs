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
        // 小误差渐进修正；误差过大时直接采用权威位置。
        private const float CorrectionSharpness = 12f;
        private const float TeleportDistance = 2f;

        private struct ClientInput
        {
            public ClientInput(uint tick, Vector2 input)
            {
                Tick = tick;
                Input = input;
            }

            public uint Tick;
            public Vector2 Input;
        }

        private struct ServerSnapshot
        {
            public ServerSnapshot(uint tick, Vector2 position)
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
        private readonly List<ClientInput> m_PendingInputs = new List<ClientInput>();
        private readonly List<ServerSnapshot> m_Snapshots = new List<ServerSnapshot>();

        private float m_TickTimer;
        private double m_RenderTick;
        private Vector2 m_FrameInput;         // 当前渲染帧采样到的输入。
        private Vector2 m_TickInput;          // 当前 50 ms 逻辑帧锁存的输入。
        private Vector2 m_Correction;
        private uint m_ClientTick;
        private uint m_LastServerTick;

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
            m_RenderTick = 0d;
            m_FrameInput = SampleInput();
            m_TickInput = m_FrameInput;
            m_Correction = Vector2.zero;
            m_ClientTick = 0;
            m_LastServerTick = 0;
            m_PendingInputs.Clear();
            m_Snapshots.Clear();
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

        /// <summary>
        /// 收到后端更新的逻辑帧
        /// </summary>
        public void ReceiveLogicTick(Vector3 position, uint serverTick, uint clientTick)
        {
            if (m_PlayerData.IsSelf)
            {
                RenderLocal((Vector2)position, serverTick, clientTick);
            }
            else
            {
                BufferSnapshot((Vector2)position, serverTick);
            }
        }

        /// <summary>
        /// 本地玩家模拟移动
        /// </summary>
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
                m_PendingInputs.Add(new ClientInput(clientTick, m_TickInput));
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

        /// <summary>
        /// 本地玩家渲染移动
        /// </summary>
        private void RenderLocal(Vector2 authoritativePosition, uint serverTick, uint clientTick)
        {
            if (serverTick < m_LastServerTick)
            {
                return;
            }

            m_LastServerTick = serverTick;
            // 权威位置已经包含 processedInputTick 及之前的输入，不能重复回放。
            m_PendingInputs.RemoveAll(command => command.Tick <= clientTick);

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

        /// <summary>
        /// 远端玩家缓存快照
        /// </summary>
        private void BufferSnapshot(Vector2 position, uint serverTick)
        {
            if (m_Snapshots.Count > 0 && serverTick <= m_Snapshots[m_Snapshots.Count - 1].Tick)
            {
                return;
            }

            if (m_Snapshots.Count == 0)
            {
                m_RenderTick = serverTick;
            }

            m_Snapshots.Add(new ServerSnapshot(serverTick, position));
        }

        /// <summary>
        /// 远端玩家渲染移动
        /// </summary>
        private void RenderRemote(float frameDeltaSeconds)
        {
            if (m_Snapshots.Count == 0)
            {
                return;
            }

            // 更新渲染帧
            uint lastLogicTick = m_Snapshots[m_Snapshots.Count - 1].Tick;
            m_RenderTick = System.Math.Min(m_RenderTick + frameDeltaSeconds / TickInterval, lastLogicTick);

            if (m_Snapshots.Count == 1)
            {
                CachedRigidbody2D.position = m_Snapshots[0].Position;
                return;
            }

            // 丢弃渲染帧已经越过的逻辑帧，并保留前后两帧用于插值。
            while (m_Snapshots.Count >= 2 && m_Snapshots[1].Tick <= m_RenderTick)
            {
                m_Snapshots.RemoveAt(0);
            }

            if (m_Snapshots.Count == 1)
            {
                CachedRigidbody2D.position = m_Snapshots[0].Position;
                return;
            }

            ServerSnapshot from = m_Snapshots[0];
            ServerSnapshot to = m_Snapshots[1];
            float lerpFactor = (float)((m_RenderTick - from.Tick) / (to.Tick - from.Tick));
            CachedRigidbody2D.position = Vector2.Lerp(from.Position, to.Position, lerpFactor);
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
