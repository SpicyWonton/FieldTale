//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework;
using System.Collections.Generic;

namespace UnityGameFramework.Runtime
{
    public sealed partial class EntityComponent : GameFrameworkComponent
    {
        /// <summary>
        /// 实体信息。
        /// </summary>
        private sealed class EntityInfo : IReference
        {
            private Entity m_Entity;
            private EntityStatus m_Status;
            private Entity m_ParentEntity;
            private List<Entity> m_ChildEntities;

            public EntityInfo()
            {
                m_Entity = null;
                m_Status = EntityStatus.Unknown;
                m_ParentEntity = null;
                m_ChildEntities = new List<Entity>();
            }

            public Entity Entity
            {
                get
                {
                    return m_Entity;
                }
            }

            public EntityStatus Status
            {
                get
                {
                    return m_Status;
                }
                set
                {
                    m_Status = value;
                }
            }

            public Entity ParentEntity
            {
                get
                {
                    return m_ParentEntity;
                }
                set
                {
                    m_ParentEntity = value;
                }
            }

            public int ChildEntityCount
            {
                get
                {
                    return m_ChildEntities.Count;
                }
            }

            public static EntityInfo Create(Entity entity)
            {
                if (entity == null)
                {
                    throw new GameFrameworkException("Entity is invalid.");
                }

                EntityInfo entityInfo = ReferencePool.Acquire<EntityInfo>();
                entityInfo.m_Entity = entity;
                entityInfo.m_Status = EntityStatus.WillInit;
                return entityInfo;
            }

            public void Clear()
            {
                m_Entity = null;
                m_Status = EntityStatus.Unknown;
                m_ParentEntity = null;
                m_ChildEntities.Clear();
            }

            public Entity GetChildEntity()
            {
                return m_ChildEntities.Count > 0 ? m_ChildEntities[0] : null;
            }

            public Entity[] GetChildEntities()
            {
                return m_ChildEntities.ToArray();
            }

            public void GetChildEntities(List<Entity> results)
            {
                if (results == null)
                {
                    throw new GameFrameworkException("Results is invalid.");
                }

                results.Clear();
                foreach (Entity childEntity in m_ChildEntities)
                {
                    results.Add(childEntity);
                }
            }

            public void AddChildEntity(Entity childEntity)
            {
                if (m_ChildEntities.Contains(childEntity))
                {
                    throw new GameFrameworkException("Can not add child entity which is already exist.");
                }

                m_ChildEntities.Add(childEntity);
            }

            public void RemoveChildEntity(Entity childEntity)
            {
                if (!m_ChildEntities.Remove(childEntity))
                {
                    throw new GameFrameworkException("Can not remove child entity which is not exist.");
                }
            }
        }
    }
}
