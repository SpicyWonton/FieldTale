using System;
using UnityEngine;

namespace FieldTale.HotUpdate
{
    [Serializable]
    public class PlayerData : EntityData
    {
        [SerializeField]
        private readonly float m_Speed = 0f;

        public PlayerData(int entityId, int typeId, float speed) : base(entityId, typeId)
        {
            m_Speed = speed;
        }

        public float Speed
        {
            get
            {
                return m_Speed;
            }
        }
    }
}