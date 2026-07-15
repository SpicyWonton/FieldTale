using Fantasy;
using UnityEngine;
using Log = UnityGameFramework.Runtime.Log;

namespace FieldTale.HotUpdate
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Player : Entity
    {
        private const float NetworkSyncInterval = 0.05f;

        [SerializeField]
        private PlayerData m_PlayerData = null;

        private float m_NetworkSyncTimer;

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

            m_NetworkSyncTimer = 0f;
        }

        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            if (!m_PlayerData.IsSelf)
            {
                return;
            }

            float inputX = Input.GetAxis("Horizontal");
            float inputY = Input.GetAxis("Vertical");
            Vector3 movement = new Vector3(inputX, inputY, 0f) * (m_PlayerData.Speed * elapseSeconds);
            if (movement.sqrMagnitude <= 0f)
            {
                return;
            }

            CachedRigidbody2D.MovePosition(CachedTransform.position + movement);
            m_NetworkSyncTimer -= elapseSeconds;
            if (m_NetworkSyncTimer > 0f)
            {
                return;
            }

            m_NetworkSyncTimer = NetworkSyncInterval;
            Fantasy.Position position = Fantasy.Position.Create();
            position.X = CachedTransform.position.x + movement.x;
            position.Y = CachedTransform.position.y + movement.y;
            position.Z = CachedTransform.position.z + movement.z;
            Fantasy.Runtime.Session.C2M_PlayerMove(position);
        }

        public void SetNetworkPosition(Vector3 position)
        {
            CachedRigidbody2D.position = position;
        }
    }
}