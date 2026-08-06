using Fantasy;
using Fantasy.Entitas.Interface;
using UnityEngine;

namespace FieldTale.HotUpdate
{
    /// <summary>
    /// 处理本地输入预测、服务端校正以及远端玩家快照插值。
    /// </summary>
    public sealed class PlayerUpdateSystem : UpdateSystem<Player>
    {
        private const float TickInterval = 0.05f;
        private const float CorrectionSharpness = 12f;
        private const float TeleportDistance = 2f;

        protected override void Update(Player self)
        {
            ConsumeSnapshots(self);

            if (self.IsSelf)
            {
                self.Movement.FrameInput = SampleInput();
                if (!self.Movement.InputInitialized)
                {
                    self.Movement.TickInput = self.Movement.FrameInput;
                    self.Movement.InputInitialized = true;
                }
                SimulateLocal(self, Time.deltaTime);
                return;
            }

            RenderRemote(self, Time.deltaTime);
        }

        /// <summary>
        /// 将 Handler 收到的快照转交给本地校正或远端插值流程。
        /// </summary>
        private static void ConsumeSnapshots(Player self)
        {
            PlayerSnapshotComponent snapshots = self.Snapshots;
            if (snapshots.Incoming.Count == 0)
            {
                return;
            }

            for (int i = 0; i < snapshots.Incoming.Count; i++)
            {
                PlayerNetworkSnapshot snapshot = snapshots.Incoming[i];
                if (self.IsSelf)
                {
                    ReconcileLocal(self, snapshot);
                }
                else
                {
                    BufferRemoteSnapshot(self, snapshot);
                }
            }

            snapshots.Incoming.Clear();
        }

        /// <summary>
        /// 按固定 Tick 推进本地预测，并将每个完整 Tick 的输入发送给服务端。
        /// </summary>
        private static void SimulateLocal(Player self, float deltaTime)
        {
            PlayerMovementComponent movement = self.Movement;
            PlayerTransformComponent transform = self.Transform;
            Vector2 targetPosition = transform.Position;
            float remainingSeconds = Mathf.Max(0f, deltaTime);

            while (remainingSeconds > 0f)
            {
                float stepSeconds = Mathf.Min(remainingSeconds, TickInterval - movement.TickTimer);
                targetPosition = SimulateMove(targetPosition, movement.TickInput, movement.Speed, stepSeconds);
                movement.TickTimer += stepSeconds;
                remainingSeconds -= stepSeconds;

                if (movement.TickTimer + Mathf.Epsilon < TickInterval)
                {
                    continue;
                }

                movement.TickTimer = 0f;
                uint clientTick = ++movement.ClientTick;
                movement.PendingInputs.Add(new ClientInputCommand(clientTick, movement.TickInput));
                Fantasy.Runtime.Session.C2M_PlayerMove(
                    clientTick,
                    Mathf.RoundToInt(movement.TickInput.x),
                    Mathf.RoundToInt(movement.TickInput.y));
                movement.TickInput = movement.FrameInput;
            }

            // 小偏差分帧收敛，避免权威位置校正造成明显抖动。
            if (transform.Correction.sqrMagnitude > Mathf.Epsilon)
            {
                float correctionFactor = 1f - Mathf.Exp(-CorrectionSharpness * deltaTime);
                Vector2 correctionStep = transform.Correction * correctionFactor;
                targetPosition += correctionStep;
                transform.Correction -= correctionStep;

                if (transform.Correction.sqrMagnitude < 0.000001f)
                {
                    transform.Correction = Vector2.zero;
                }
            }

            transform.Position = targetPosition;
        }

        /// <summary>
        /// 从服务端权威位置重放未确认输入，计算新的本地预测位置。
        /// </summary>
        private static void ReconcileLocal(Player self, PlayerNetworkSnapshot snapshot)
        {
            PlayerMovementComponent movement = self.Movement;
            if (snapshot.ServerTick < movement.LastServerTick)
            {
                return;
            }

            movement.LastServerTick = snapshot.ServerTick;
            movement.PendingInputs.RemoveAll(command => command.Tick <= snapshot.ProcessedClientTick);

            Vector2 correctedPosition = snapshot.Position;
            for (int i = 0; i < movement.PendingInputs.Count; i++)
            {
                correctedPosition = SimulateMove(
                    correctedPosition,
                    movement.PendingInputs[i].Input,
                    movement.Speed,
                    TickInterval);
            }

            correctedPosition = SimulateMove(
                correctedPosition,
                movement.TickInput,
                movement.Speed,
                movement.TickTimer);

            Vector2 correction = correctedPosition - self.Transform.Position;
            // 偏差过大时直接同步，较小偏差交给后续帧平滑吸收。
            if (correction.sqrMagnitude >= TeleportDistance * TeleportDistance)
            {
                self.Transform.Position = correctedPosition;
                self.Transform.Correction = Vector2.zero;
                return;
            }

            self.Transform.Correction = correction;
        }

        /// <summary>
        /// 将递增的远端快照写入渲染缓冲区，丢弃重复或乱序快照。
        /// </summary>
        private static void BufferRemoteSnapshot(Player self, PlayerNetworkSnapshot snapshot)
        {
            PlayerSnapshotComponent snapshots = self.Snapshots;
            if (snapshots.RenderBuffer.Count > 0 &&
                snapshot.ServerTick <= snapshots.RenderBuffer[snapshots.RenderBuffer.Count - 1].ServerTick)
            {
                return;
            }

            if (snapshots.RenderBuffer.Count == 0)
            {
                snapshots.RenderTick = snapshot.ServerTick;
            }

            snapshots.RenderBuffer.Add(snapshot);
        }

        /// <summary>
        /// 根据渲染 Tick 在相邻服务端快照之间插值远端玩家位置。
        /// </summary>
        private static void RenderRemote(Player self, float deltaTime)
        {
            PlayerSnapshotComponent snapshots = self.Snapshots;
            if (snapshots.RenderBuffer.Count == 0)
            {
                return;
            }

            uint lastLogicTick = snapshots.RenderBuffer[snapshots.RenderBuffer.Count - 1].ServerTick;
            snapshots.RenderTick = System.Math.Min(
                snapshots.RenderTick + deltaTime / TickInterval,
                lastLogicTick);

            while (snapshots.RenderBuffer.Count >= 2 &&
                   snapshots.RenderBuffer[1].ServerTick <= snapshots.RenderTick)
            {
                snapshots.RenderBuffer.RemoveAt(0);
            }

            if (snapshots.RenderBuffer.Count == 1)
            {
                self.Transform.Position = snapshots.RenderBuffer[0].Position;
                return;
            }

            PlayerNetworkSnapshot from = snapshots.RenderBuffer[0];
            PlayerNetworkSnapshot to = snapshots.RenderBuffer[1];
            double tickRange = to.ServerTick - from.ServerTick;
            float lerpFactor = tickRange <= 0d
                ? 1f
                : (float)((snapshots.RenderTick - from.ServerTick) / tickRange);
            self.Transform.Position = Vector2.Lerp(from.Position, to.Position, lerpFactor);
        }

        private static Vector2 SampleInput()
        {
            return new Vector2(
                Mathf.RoundToInt(Input.GetAxisRaw("Horizontal")),
                Mathf.RoundToInt(Input.GetAxisRaw("Vertical")));
        }

        private static Vector2 SimulateMove(Vector2 position, Vector2 input, float speed, float deltaTime)
        {
            if (input.sqrMagnitude > 1f)
            {
                input.Normalize();
            }

            return position + input * (speed * deltaTime);
        }
    }

    /// <summary>
    /// 在逻辑更新完成后将 ECS 位姿推送到 Unity 表现层。
    /// </summary>
    public sealed class PlayerLateUpdateSystem : LateUpdateSystem<Player>
    {
        protected override void LateUpdate(Player self)
        {
            if (self.Transform == null || self.View == null)
            {
                return;
            }

            self.View.SetTargetPose(self.Transform.Position, self.Transform.Rotation);
        }
    }

    /// <summary>
    /// 清理玩家索引并隐藏或取消加载对应的 UGF 表现实体。
    /// </summary>
    public sealed class PlayerDestroySystem : DestroySystem<Player>
    {
        protected override void Destroy(Player self)
        {
            try
            {
                PlayerFactory.RemoveFromManager(self);
            }
            catch (System.InvalidOperationException)
            {
                // The Fantasy scene may already be shutting down.
            }

            int viewEntityId = self.ClientEntityId;
            if (FrameworkRoot.Entity != null &&
                viewEntityId > 0 &&
                (FrameworkRoot.Entity.HasEntity(viewEntityId) ||
                 FrameworkRoot.Entity.IsLoadingEntity(viewEntityId)))
            {
                FrameworkRoot.Entity.HideEntity(viewEntityId);
            }

            self.ServerEntityId = 0;
            self.ClientEntityId = 0;
            self.IsSelf = false;
            self.View = null;
            self.Transform = null;
            self.Movement = null;
            self.Snapshots = null;
        }
    }
}
