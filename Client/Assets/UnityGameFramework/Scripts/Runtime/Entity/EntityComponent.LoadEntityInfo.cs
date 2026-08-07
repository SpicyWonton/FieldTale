//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework;

namespace UnityGameFramework.Runtime
{
    public sealed partial class EntityComponent : GameFrameworkComponent
    {
        private sealed class LoadEntityInfo : IReference
        {
            private int m_EntityId;
            private EntityGroup m_EntityGroup;
            private ShowEntityInfo m_ShowEntityInfo;
            private bool m_IsCancelled;

            public LoadEntityInfo()
            {
                m_EntityId = 0;
                m_EntityGroup = null;
                m_ShowEntityInfo = null;
                m_IsCancelled = false;
            }

            public int EntityId
            {
                get
                {
                    return m_EntityId;
                }
            }

            public EntityGroup EntityGroup
            {
                get
                {
                    return m_EntityGroup;
                }
            }

            public ShowEntityInfo ShowEntityInfo
            {
                get
                {
                    return m_ShowEntityInfo;
                }
            }

            public bool IsCancelled
            {
                get
                {
                    return m_IsCancelled;
                }
            }

            public static LoadEntityInfo Create(int entityId, EntityGroup entityGroup, ShowEntityInfo showEntityInfo)
            {
                LoadEntityInfo loadEntityInfo = ReferencePool.Acquire<LoadEntityInfo>();
                loadEntityInfo.m_EntityId = entityId;
                loadEntityInfo.m_EntityGroup = entityGroup;
                loadEntityInfo.m_ShowEntityInfo = showEntityInfo;
                return loadEntityInfo;
            }

            public void Cancel()
            {
                m_IsCancelled = true;
            }

            public void Clear()
            {
                m_EntityId = 0;
                m_EntityGroup = null;
                m_ShowEntityInfo = null;
                m_IsCancelled = false;
            }
        }
    }
}