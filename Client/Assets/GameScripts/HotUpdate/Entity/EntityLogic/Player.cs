using UnityEngine;
using UnityGameFramework.Runtime;

namespace FieldTale.HotUpdate
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Player : Entity
    {
        [SerializeField]
        private PlayerData m_PlayerData = null;

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
        }                                   

        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)                      
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            float inputX = Input.GetAxis("Horizontal");
            float inputY = Input.GetAxis("Vertical");
            Vector3 movement = new Vector3(inputX, inputY, 0) * (m_PlayerData.Speed * elapseSeconds);
            CachedRigidbody2D.MovePosition(CachedTransform.position + movement);                                                                   
        }
    }
}