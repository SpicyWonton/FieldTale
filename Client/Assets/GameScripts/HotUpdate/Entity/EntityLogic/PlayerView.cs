using UnityEngine;
using Log = UnityGameFramework.Runtime.Log;

namespace FieldTale.HotUpdate
{
    /// <summary>
    /// 玩家 Unity 表现层，仅接收 ECS 位姿并驱动 Rigidbody2D。
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerView : EntityView
    {
        private Player m_Player;
        private Vector2 m_TargetPosition;
        private Quaternion m_TargetRotation;
        private bool m_HasTargetPose;

        public Rigidbody2D CachedRigidbody2D { get; private set; }

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            CachedRigidbody2D = GetComponent<Rigidbody2D>();
            CachedRigidbody2D.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        protected override void OnShow(object userData)
        {
            // ShowEntity 是异步流程，玩家可能在资源加载完成前已经离开场景。
            Player player = userData as Player;
            if (player == null || player.IsDisposed)
            {
                Log.Error("Player is invalid.");
                FrameworkRoot.Entity.HideEntity(Id);
                return;
            }

            // 绑定后由 PlayerLateUpdateSystem 持续写入目标位姿。
            m_Player = player;
            m_Player.View = this;
            if (m_Player.IsSelf)
            {
                CameraFollowComponent.SetTarget(CachedTransform);
            }

            m_TargetPosition = m_Player.Transform.Position;
            m_TargetRotation = m_Player.Transform.Rotation;
            m_HasTargetPose = true;

            // 在 EntityLogic 激活 GameObject 前同步渲染与物理位姿，防止出生点闪现。
            CachedTransform.SetPositionAndRotation(m_TargetPosition, m_TargetRotation);
            CachedRigidbody2D.position = m_TargetPosition;
            CachedRigidbody2D.rotation = m_TargetRotation.eulerAngles.z;
            CachedRigidbody2D.velocity = Vector2.zero;
            CachedRigidbody2D.angularVelocity = 0f;

            base.OnShow(userData);
        }

        protected override void OnHide(bool isShutdown, object userData)
        {
            CameraFollowComponent.ClearTarget(CachedTransform);

            if (m_Player != null && ReferenceEquals(m_Player.View, this))
            {
                m_Player.View = null;
            }

            m_Player = null;
            m_HasTargetPose = false;
            base.OnHide(isShutdown, userData);
        }

        private void FixedUpdate()
        {
            if (!m_HasTargetPose)
            {
                return;
            }

            CachedRigidbody2D.MovePosition(m_TargetPosition);
            CachedRigidbody2D.MoveRotation(m_TargetRotation.eulerAngles.z);
        }

        public void SetTargetPose(Vector2 position, Quaternion rotation)
        {
            m_TargetPosition = position;
            m_TargetRotation = rotation;
            m_HasTargetPose = true;
        }
    }
}
