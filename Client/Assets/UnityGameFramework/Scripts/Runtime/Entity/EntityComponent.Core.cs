//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework;
using GameFramework.ObjectPool;
using GameFramework.Resource;
using System;
using System.Collections.Generic;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 实体组件核心。
    /// </summary>
    public sealed partial class EntityComponent : GameFrameworkComponent
    {
        private Dictionary<int, EntityInfo> m_EntityInfos;
        private Dictionary<string, EntityGroup> m_EntityGroups;
        private Dictionary<int, LoadEntityInfo> m_EntitiesBeingLoaded;
        private Queue<EntityInfo> m_RecycleQueue;
        private LoadAssetCallbacks m_LoadAssetCallbacks;
        private IObjectPoolManager m_ObjectPoolManager;
        private IResourceManager m_ResourceManager;
        private EntityHelperBase m_EntityHelper;
        private bool m_IsShutdown;

        /// <summary>
        /// 初始化实体组件核心。
        /// </summary>
        private void InitializeEntityCore()
        {
            m_EntityInfos = new Dictionary<int, EntityInfo>();
            m_EntityGroups = new Dictionary<string, EntityGroup>(StringComparer.Ordinal);
            m_EntitiesBeingLoaded = new Dictionary<int, LoadEntityInfo>();
            m_RecycleQueue = new Queue<EntityInfo>();
            m_LoadAssetCallbacks = new LoadAssetCallbacks(LoadAssetSuccessCallback, LoadAssetFailureCallback, LoadAssetUpdateCallback, LoadAssetDependencyAssetCallback);
            m_ObjectPoolManager = null;
            m_ResourceManager = null;
            m_EntityHelper = null;
            m_IsShutdown = false;
        }

        /// <summary>
        /// 获取实体数量。
        /// </summary>
        public int EntityCount
        {
            get
            {
                return m_EntityInfos.Count;
            }
        }

        /// <summary>
        /// 获取实体组数量。
        /// </summary>
        public int EntityGroupCount
        {
            get
            {
                return m_EntityGroups.Count;
            }
        }

        /// <summary>
        /// 实体组件核心轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        internal override void Tick(float elapseSeconds, float realElapseSeconds)
        {
            if (m_IsShutdown)
            {
                return;
            }

            while (m_RecycleQueue.Count > 0)
            {
                EntityInfo entityInfo = m_RecycleQueue.Dequeue();
                Entity entity = entityInfo.Entity;
                EntityGroup entityGroup = (EntityGroup)entity.EntityGroup;
                if (entityGroup == null)
                {
                    throw new GameFrameworkException("Entity group is invalid.");
                }

                entityInfo.Status = EntityStatus.WillRecycle;
                entity.OnRecycle();
                entityInfo.Status = EntityStatus.Recycled;
                entityGroup.UnspawnEntity(entity);
                ReferencePool.Release(entityInfo);
            }

            foreach (KeyValuePair<string, EntityGroup> entityGroup in m_EntityGroups)
            {
                entityGroup.Value.Update(elapseSeconds, realElapseSeconds);
            }
        }

        /// <summary>
        /// 关闭并清理实体组件核心。
        /// </summary>
        private void ShutdownEntityCore()
        {
            if (m_IsShutdown)
            {
                return;
            }

            m_IsShutdown = true;
            HideAllLoadingEntities();
            HideAllLoadedEntities();
            m_EntityGroups.Clear();
            m_RecycleQueue.Clear();
        }

        /// <summary>
        /// 设置对象池管理器。
        /// </summary>
        /// <param name="objectPoolManager">对象池管理器。</param>
        private void SetObjectPoolManager(IObjectPoolManager objectPoolManager)
        {
            if (objectPoolManager == null)
            {
                throw new GameFrameworkException("Object pool manager is invalid.");
            }

            m_ObjectPoolManager = objectPoolManager;
        }

        /// <summary>
        /// 设置资源管理器。
        /// </summary>
        /// <param name="resourceManager">资源管理器。</param>
        private void SetResourceManager(IResourceManager resourceManager)
        {
            if (resourceManager == null)
            {
                throw new GameFrameworkException("Resource manager is invalid.");
            }

            m_ResourceManager = resourceManager;
        }

        /// <summary>
        /// 设置实体辅助器。
        /// </summary>
        /// <param name="entityHelper">实体辅助器。</param>
        private void SetEntityHelper(EntityHelperBase entityHelper)
        {
            if (entityHelper == null)
            {
                throw new GameFrameworkException("Entity helper is invalid.");
            }

            m_EntityHelper = entityHelper;
        }

        /// <summary>
        /// 是否存在实体组。
        /// </summary>
        /// <param name="entityGroupName">实体组名称。</param>
        /// <returns>是否存在实体组。</returns>
        public bool HasEntityGroup(string entityGroupName)
        {
            if (string.IsNullOrEmpty(entityGroupName))
            {
                throw new GameFrameworkException("Entity group name is invalid.");
            }

            return m_EntityGroups.ContainsKey(entityGroupName);
        }

        /// <summary>
        /// 获取实体组。
        /// </summary>
        /// <param name="entityGroupName">实体组名称。</param>
        /// <returns>要获取的实体组。</returns>
        public EntityGroup GetEntityGroup(string entityGroupName)
        {
            if (string.IsNullOrEmpty(entityGroupName))
            {
                throw new GameFrameworkException("Entity group name is invalid.");
            }

            EntityGroup entityGroup = null;
            if (m_EntityGroups.TryGetValue(entityGroupName, out entityGroup))
            {
                return entityGroup;
            }

            return null;
        }

        /// <summary>
        /// 获取所有实体组。
        /// </summary>
        /// <returns>所有实体组。</returns>
        public EntityGroup[] GetAllEntityGroups()
        {
            int index = 0;
            EntityGroup[] results = new EntityGroup[m_EntityGroups.Count];
            foreach (KeyValuePair<string, EntityGroup> entityGroup in m_EntityGroups)
            {
                results[index++] = entityGroup.Value;
            }

            return results;
        }

        /// <summary>
        /// 获取所有实体组。
        /// </summary>
        /// <param name="results">所有实体组。</param>
        public void GetAllEntityGroups(List<EntityGroup> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (KeyValuePair<string, EntityGroup> entityGroup in m_EntityGroups)
            {
                results.Add(entityGroup.Value);
            }
        }

        /// <summary>
        /// 增加实体组。
        /// </summary>
        /// <param name="entityGroupName">实体组名称。</param>
        /// <param name="instanceAutoReleaseInterval">实体实例对象池自动释放可释放对象的间隔秒数。</param>
        /// <param name="instanceCapacity">实体实例对象池容量。</param>
        /// <param name="instanceExpireTime">实体实例对象池对象过期秒数。</param>
        /// <param name="instancePriority">实体实例对象池的优先级。</param>
        /// <param name="groupRoot">实体组根节点。</param>
        /// <returns>是否增加实体组成功。</returns>
        private bool AddEntityGroupInternal(string entityGroupName, float instanceAutoReleaseInterval, int instanceCapacity, float instanceExpireTime, int instancePriority, UnityEngine.Transform groupRoot)
        {
            if (string.IsNullOrEmpty(entityGroupName))
            {
                throw new GameFrameworkException("Entity group name is invalid.");
            }

            if (groupRoot == null)
            {
                throw new GameFrameworkException("Entity group root is invalid.");
            }

            if (m_ObjectPoolManager == null)
            {
                throw new GameFrameworkException("You must set object pool manager first.");
            }

            if (HasEntityGroup(entityGroupName))
            {
                return false;
            }

            m_EntityGroups.Add(entityGroupName, new EntityGroup(entityGroupName, instanceAutoReleaseInterval, instanceCapacity, instanceExpireTime, instancePriority, groupRoot, m_ObjectPoolManager));

            return true;
        }

        /// <summary>
        /// 是否存在实体。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        /// <returns>是否存在实体。</returns>
        public bool HasEntity(int entityId)
        {
            return m_EntityInfos.ContainsKey(entityId);
        }

        /// <summary>
        /// 是否存在实体。
        /// </summary>
        /// <param name="entityAssetName">实体资源名称。</param>
        /// <returns>是否存在实体。</returns>
        public bool HasEntity(string entityAssetName)
        {
            if (string.IsNullOrEmpty(entityAssetName))
            {
                throw new GameFrameworkException("Entity asset name is invalid.");
            }

            foreach (KeyValuePair<int, EntityInfo> entityInfo in m_EntityInfos)
            {
                if (entityInfo.Value.Entity.EntityAssetName == entityAssetName)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 获取实体。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        /// <returns>要获取的实体。</returns>
        public Entity GetEntity(int entityId)
        {
            EntityInfo entityInfo = GetEntityInfo(entityId);
            if (entityInfo == null)
            {
                return null;
            }

            return entityInfo.Entity;
        }

        /// <summary>
        /// 获取实体。
        /// </summary>
        /// <param name="entityAssetName">实体资源名称。</param>
        /// <returns>要获取的实体。</returns>
        public Entity GetEntity(string entityAssetName)
        {
            if (string.IsNullOrEmpty(entityAssetName))
            {
                throw new GameFrameworkException("Entity asset name is invalid.");
            }

            foreach (KeyValuePair<int, EntityInfo> entityInfo in m_EntityInfos)
            {
                if (entityInfo.Value.Entity.EntityAssetName == entityAssetName)
                {
                    return entityInfo.Value.Entity;
                }
            }

            return null;
        }

        /// <summary>
        /// 获取实体。
        /// </summary>
        /// <param name="entityAssetName">实体资源名称。</param>
        /// <returns>要获取的实体。</returns>
        public Entity[] GetEntities(string entityAssetName)
        {
            if (string.IsNullOrEmpty(entityAssetName))
            {
                throw new GameFrameworkException("Entity asset name is invalid.");
            }

            List<Entity> results = new List<Entity>();
            foreach (KeyValuePair<int, EntityInfo> entityInfo in m_EntityInfos)
            {
                if (entityInfo.Value.Entity.EntityAssetName == entityAssetName)
                {
                    results.Add(entityInfo.Value.Entity);
                }
            }

            return results.ToArray();
        }

        /// <summary>
        /// 获取实体。
        /// </summary>
        /// <param name="entityAssetName">实体资源名称。</param>
        /// <param name="results">要获取的实体。</param>
        public void GetEntities(string entityAssetName, List<Entity> results)
        {
            if (string.IsNullOrEmpty(entityAssetName))
            {
                throw new GameFrameworkException("Entity asset name is invalid.");
            }

            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (KeyValuePair<int, EntityInfo> entityInfo in m_EntityInfos)
            {
                if (entityInfo.Value.Entity.EntityAssetName == entityAssetName)
                {
                    results.Add(entityInfo.Value.Entity);
                }
            }
        }

        /// <summary>
        /// 获取所有已加载的实体。
        /// </summary>
        /// <returns>所有已加载的实体。</returns>
        public Entity[] GetAllLoadedEntities()
        {
            int index = 0;
            Entity[] results = new Entity[m_EntityInfos.Count];
            foreach (KeyValuePair<int, EntityInfo> entityInfo in m_EntityInfos)
            {
                results[index++] = entityInfo.Value.Entity;
            }

            return results;
        }

        /// <summary>
        /// 获取所有已加载的实体。
        /// </summary>
        /// <param name="results">所有已加载的实体。</param>
        public void GetAllLoadedEntities(List<Entity> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (KeyValuePair<int, EntityInfo> entityInfo in m_EntityInfos)
            {
                results.Add(entityInfo.Value.Entity);
            }
        }

        /// <summary>
        /// 获取所有正在加载实体的编号。
        /// </summary>
        /// <returns>所有正在加载实体的编号。</returns>
        public int[] GetAllLoadingEntityIds()
        {
            int index = 0;
            int[] results = new int[m_EntitiesBeingLoaded.Count];
            foreach (KeyValuePair<int, LoadEntityInfo> entityBeingLoaded in m_EntitiesBeingLoaded)
            {
                results[index++] = entityBeingLoaded.Key;
            }

            return results;
        }

        /// <summary>
        /// 获取所有正在加载实体的编号。
        /// </summary>
        /// <param name="results">所有正在加载实体的编号。</param>
        public void GetAllLoadingEntityIds(List<int> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (KeyValuePair<int, LoadEntityInfo> entityBeingLoaded in m_EntitiesBeingLoaded)
            {
                results.Add(entityBeingLoaded.Key);
            }
        }

        /// <summary>
        /// 是否正在加载实体。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        /// <returns>是否正在加载实体。</returns>
        public bool IsLoadingEntity(int entityId)
        {
            return m_EntitiesBeingLoaded.ContainsKey(entityId);
        }

        /// <summary>
        /// 是否是合法的实体。
        /// </summary>
        /// <param name="entity">实体。</param>
        /// <returns>实体是否合法。</returns>
        public bool IsValidEntity(Entity entity)
        {
            if (entity == null)
            {
                return false;
            }

            return GetEntity(entity.Id) == entity;
        }

        private void ShowEntityInternal(int entityId, string entityAssetName, string entityGroupName, int priority, ShowEntityInfo showEntityInfo)
        {
            if (showEntityInfo == null)
            {
                throw new GameFrameworkException("Show entity info is invalid.");
            }

            bool releaseShowEntityInfo = true;
            try
            {
                if (m_IsShutdown)
                {
                    throw new GameFrameworkException("Entity component is shutting down.");
                }

                if (m_ResourceManager == null)
                {
                    throw new GameFrameworkException("You must set resource manager first.");
                }

                if (m_EntityHelper == null)
                {
                    throw new GameFrameworkException("You must set entity helper first.");
                }

                if (string.IsNullOrEmpty(entityAssetName))
                {
                    throw new GameFrameworkException("Entity asset name is invalid.");
                }

                if (string.IsNullOrEmpty(entityGroupName))
                {
                    throw new GameFrameworkException("Entity group name is invalid.");
                }

                if (HasEntity(entityId))
                {
                    throw new GameFrameworkException(Utility.Text.Format("Entity id '{0}' is already exist.", entityId));
                }

                if (IsLoadingEntity(entityId))
                {
                    throw new GameFrameworkException(Utility.Text.Format("Entity '{0}' is already being loaded.", entityId));
                }

                EntityGroup entityGroup = GetEntityGroup(entityGroupName);
                if (entityGroup == null)
                {
                    throw new GameFrameworkException(Utility.Text.Format("Entity group '{0}' is not exist.", entityGroupName));
                }

                EntityInstanceObject entityInstanceObject = entityGroup.SpawnEntityInstanceObject(entityAssetName);
                if (entityInstanceObject != null)
                {
                    releaseShowEntityInfo = false;
                    InternalShowEntity(entityId, entityAssetName, entityGroup, entityInstanceObject.Target, false, 0f, showEntityInfo);
                    return;
                }

                LoadEntityInfo loadEntityInfo = LoadEntityInfo.Create(entityId, entityGroup, showEntityInfo);
                m_EntitiesBeingLoaded.Add(entityId, loadEntityInfo);
                releaseShowEntityInfo = false;
                try
                {
                    m_ResourceManager.LoadAsset(entityAssetName, priority, m_LoadAssetCallbacks, loadEntityInfo);
                }
                catch
                {
                    m_EntitiesBeingLoaded.Remove(entityId);
                    ReferencePool.Release(loadEntityInfo);
                    releaseShowEntityInfo = true;
                    throw;
                }
            }
            finally
            {
                if (releaseShowEntityInfo)
                {
                    ReferencePool.Release(showEntityInfo);
                }
            }
        }
        /// <summary>
        /// 隐藏实体。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        
        /// <summary>
        /// 隐藏实体。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void HideEntity(int entityId, object userData = null)
        {
            if (m_EntitiesBeingLoaded.TryGetValue(entityId, out LoadEntityInfo loadEntityInfo))
            {
                loadEntityInfo.Cancel();
                m_EntitiesBeingLoaded.Remove(entityId);
                return;
            }

            EntityInfo entityInfo = GetEntityInfo(entityId);
            if (entityInfo == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find entity '{0}'.", entityId));
            }

            if (entityInfo.Status < EntityStatus.WillShow)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not hide entity '{0}' while its status is '{1}'.", entityId, entityInfo.Status));
            }

            InternalHideEntity(entityInfo, userData);
        }

        /// <summary>
        /// 隐藏实体。
        /// </summary>
        /// <param name="entity">实体。</param>
        
        /// <summary>
        /// 隐藏实体。
        /// </summary>
        /// <param name="entity">实体。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void HideEntity(Entity entity, object userData = null)
        {
            if (!IsValidEntity(entity))
            {
                throw new GameFrameworkException("Entity is invalid or unmanaged.");
            }

            HideEntity(entity.Id, userData);
        }

        /// <summary>
        /// 隐藏所有已加载的实体。
        /// </summary>
        
        /// <summary>
        /// 隐藏所有已加载的实体。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        public void HideAllLoadedEntities(object userData = null)
        {
            while (m_EntityInfos.Count > 0)
            {
                foreach (KeyValuePair<int, EntityInfo> entityInfo in m_EntityInfos)
                {
                    InternalHideEntity(entityInfo.Value, userData);
                    break;
                }
            }
        }

        /// <summary>
        /// 隐藏所有正在加载的实体。
        /// </summary>
        public void HideAllLoadingEntities()
        {
            foreach (KeyValuePair<int, LoadEntityInfo> entityBeingLoaded in m_EntitiesBeingLoaded)
            {
                entityBeingLoaded.Value.Cancel();
            }

            m_EntitiesBeingLoaded.Clear();
        }

        /// <summary>
        /// 获取父实体。
        /// </summary>
        /// <param name="childEntityId">要获取父实体的子实体的实体编号。</param>
        /// <returns>子实体的父实体。</returns>
        public Entity GetParentEntity(int childEntityId)
        {
            EntityInfo childEntityInfo = GetEntityInfo(childEntityId);
            if (childEntityInfo == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find child entity '{0}'.", childEntityId));
            }

            return childEntityInfo.ParentEntity;
        }

        /// <summary>
        /// 获取父实体。
        /// </summary>
        /// <param name="childEntity">要获取父实体的子实体。</param>
        /// <returns>子实体的父实体。</returns>
        public Entity GetParentEntity(Entity childEntity)
        {
            if (!IsValidEntity(childEntity))
            {
                throw new GameFrameworkException("Child entity is invalid or unmanaged.");
            }

            return GetParentEntity(childEntity.Id);
        }

        /// <summary>
        /// 获取子实体数量。
        /// </summary>
        /// <param name="parentEntityId">要获取子实体数量的父实体的实体编号。</param>
        /// <returns>子实体数量。</returns>
        public int GetChildEntityCount(int parentEntityId)
        {
            EntityInfo parentEntityInfo = GetEntityInfo(parentEntityId);
            if (parentEntityInfo == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find parent entity '{0}'.", parentEntityId));
            }

            return parentEntityInfo.ChildEntityCount;
        }

        /// <summary>
        /// 获取子实体。
        /// </summary>
        /// <param name="parentEntityId">要获取子实体的父实体的实体编号。</param>
        /// <returns>子实体。</returns>
        public Entity GetChildEntity(int parentEntityId)
        {
            EntityInfo parentEntityInfo = GetEntityInfo(parentEntityId);
            if (parentEntityInfo == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find parent entity '{0}'.", parentEntityId));
            }

            return parentEntityInfo.GetChildEntity();
        }

        /// <summary>
        /// 获取子实体。
        /// </summary>
        /// <param name="parentEntity">要获取子实体的父实体。</param>
        /// <returns>子实体。</returns>
        public Entity GetChildEntity(Entity parentEntity)
        {
            if (!IsValidEntity(parentEntity))
            {
                throw new GameFrameworkException("Parent entity is invalid or unmanaged.");
            }

            return GetChildEntity(parentEntity.Id);
        }

        /// <summary>
        /// 获取所有子实体。
        /// </summary>
        /// <param name="parentEntityId">要获取所有子实体的父实体的实体编号。</param>
        /// <returns>所有子实体。</returns>
        public Entity[] GetChildEntities(int parentEntityId)
        {
            EntityInfo parentEntityInfo = GetEntityInfo(parentEntityId);
            if (parentEntityInfo == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find parent entity '{0}'.", parentEntityId));
            }

            return parentEntityInfo.GetChildEntities();
        }

        /// <summary>
        /// 获取所有子实体。
        /// </summary>
        /// <param name="parentEntityId">要获取所有子实体的父实体的实体编号。</param>
        /// <param name="results">所有子实体。</param>
        public void GetChildEntities(int parentEntityId, List<Entity> results)
        {
            EntityInfo parentEntityInfo = GetEntityInfo(parentEntityId);
            if (parentEntityInfo == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find parent entity '{0}'.", parentEntityId));
            }

            parentEntityInfo.GetChildEntities(results);
        }

        /// <summary>
        /// 获取所有子实体。
        /// </summary>
        /// <param name="parentEntity">要获取所有子实体的父实体。</param>
        /// <returns>所有子实体。</returns>
        public Entity[] GetChildEntities(Entity parentEntity)
        {
            if (!IsValidEntity(parentEntity))
            {
                throw new GameFrameworkException("Parent entity is invalid or unmanaged.");
            }

            return GetChildEntities(parentEntity.Id);
        }

        /// <summary>
        /// 获取所有子实体。
        /// </summary>
        /// <param name="parentEntity">要获取所有子实体的父实体。</param>
        /// <param name="results">所有子实体。</param>
        public void GetChildEntities(Entity parentEntity, List<Entity> results)
        {
            if (!IsValidEntity(parentEntity))
            {
                throw new GameFrameworkException("Parent entity is invalid or unmanaged.");
            }

            GetChildEntities(parentEntity.Id, results);
        }

        private void AttachEntityInternal(Entity childEntity, Entity parentEntity, UnityEngine.Transform parentTransform, object userData)
        {
            if (!IsValidEntity(childEntity))
            {
                throw new GameFrameworkException("Child entity is invalid or unmanaged.");
            }

            if (!IsValidEntity(parentEntity))
            {
                throw new GameFrameworkException("Parent entity is invalid or unmanaged.");
            }

            if (childEntity == parentEntity)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not attach entity when child entity id equals to parent entity id '{0}'.", parentEntity.Id));
            }

            EntityInfo childEntityInfo = GetEntityInfo(childEntity.Id);
            EntityInfo parentEntityInfo = GetEntityInfo(parentEntity.Id);
            if (childEntityInfo.Status >= EntityStatus.WillHide)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not attach entity when child entity status is '{0}'.", childEntityInfo.Status));
            }

            if (parentEntityInfo.Status >= EntityStatus.WillHide)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not attach entity when parent entity status is '{0}'.", parentEntityInfo.Status));
            }

            Entity ancestor = parentEntity;
            int remainingDepth = m_EntityInfos.Count;
            while (ancestor != null)
            {
                if (ancestor == childEntity)
                {
                    throw new GameFrameworkException(Utility.Text.Format("Can not attach entity '{0}' because it would create a hierarchy cycle.", childEntity.Id));
                }

                if (remainingDepth-- <= 0)
                {
                    throw new GameFrameworkException("Entity hierarchy already contains a cycle.");
                }

                EntityInfo ancestorInfo = GetEntityInfo(ancestor.Id);
                if (ancestorInfo == null || ancestorInfo.Entity != ancestor)
                {
                    throw new GameFrameworkException(Utility.Text.Format("Entity hierarchy contains unmanaged entity '{0}'.", ancestor.Id));
                }

                ancestor = ancestorInfo.ParentEntity;
            }

            DetachEntity(childEntity, userData);
            childEntityInfo.ParentEntity = parentEntity;
            parentEntityInfo.AddChildEntity(childEntity);
            parentEntity.OnAttached(childEntity, parentTransform, userData);
            childEntity.OnAttachTo(parentEntity, parentTransform, userData);
        }
        /// <summary>
        /// 解除子实体。
        /// </summary>
        /// <param name="childEntityId">要解除的子实体的实体编号。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void DetachEntity(int childEntityId, object userData = null)
        {
            EntityInfo childEntityInfo = GetEntityInfo(childEntityId);
            if (childEntityInfo == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find child entity '{0}'.", childEntityId));
            }

            Entity parentEntity = childEntityInfo.ParentEntity;
            if (parentEntity == null)
            {
                return;
            }

            EntityInfo parentEntityInfo = GetEntityInfo(parentEntity.Id);
            if (parentEntityInfo == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find parent entity '{0}'.", parentEntity.Id));
            }

            Entity childEntity = childEntityInfo.Entity;
            childEntityInfo.ParentEntity = null;
            parentEntityInfo.RemoveChildEntity(childEntity);
            parentEntity.OnDetached(childEntity, userData);
            childEntity.OnDetachFrom(parentEntity, userData);
        }

        /// <summary>
        /// 解除子实体。
        /// </summary>
        /// <param name="childEntity">要解除的子实体。</param>
        
        /// <summary>
        /// 解除子实体。
        /// </summary>
        /// <param name="childEntity">要解除的子实体。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void DetachEntity(Entity childEntity, object userData = null)
        {
            if (!IsValidEntity(childEntity))
            {
                throw new GameFrameworkException("Child entity is invalid or unmanaged.");
            }

            DetachEntity(childEntity.Id, userData);
        }

        /// <summary>
        /// 解除所有子实体。
        /// </summary>
        /// <param name="parentEntityId">被解除的父实体的实体编号。</param>
        
        /// <summary>
        /// 解除所有子实体。
        /// </summary>
        /// <param name="parentEntityId">被解除的父实体的实体编号。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void DetachChildEntities(int parentEntityId, object userData = null)
        {
            EntityInfo parentEntityInfo = GetEntityInfo(parentEntityId);
            if (parentEntityInfo == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find parent entity '{0}'.", parentEntityId));
            }

            while (parentEntityInfo.ChildEntityCount > 0)
            {
                Entity childEntity = parentEntityInfo.GetChildEntity();
                DetachEntity(childEntity.Id, userData);
            }
        }

        /// <summary>
        /// 解除所有子实体。
        /// </summary>
        /// <param name="parentEntity">被解除的父实体。</param>
        
        /// <summary>
        /// 解除所有子实体。
        /// </summary>
        /// <param name="parentEntity">被解除的父实体。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void DetachChildEntities(Entity parentEntity, object userData = null)
        {
            if (!IsValidEntity(parentEntity))
            {
                throw new GameFrameworkException("Parent entity is invalid or unmanaged.");
            }

            DetachChildEntities(parentEntity.Id, userData);
        }

        /// <summary>
        /// 获取实体信息。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        /// <returns>实体信息。</returns>
        private EntityInfo GetEntityInfo(int entityId)
        {
            if (m_EntityInfos.TryGetValue(entityId, out EntityInfo entityInfo))
            {
                return entityInfo;
            }

            return null;
        }

        private void InternalShowEntity(int entityId, string entityAssetName, EntityGroup entityGroup, object entityInstance, bool isNewInstance, float duration, ShowEntityInfo showEntityInfo)
        {
            Entity entity = null;
            EntityInfo entityInfo = null;
            bool entityInfoAdded = false;
            bool entityGroupAdded = false;

            try
            {
                try
                {
                    entity = m_EntityHelper.CreateEntity(entityInstance, entityGroup, showEntityInfo);
                    if (entity == null)
                    {
                        throw new GameFrameworkException("Can not create entity in entity helper.");
                    }

                    entityInfo = EntityInfo.Create(entity);
                    m_EntityInfos.Add(entityId, entityInfo);
                    entityInfoAdded = true;

                    entityInfo.Status = EntityStatus.WillInit;
                    entity.OnInit(entityId, entityAssetName, entityGroup, isNewInstance, showEntityInfo);
                    entityInfo.Status = EntityStatus.Inited;

                    entityGroup.AddEntity(entity);
                    entityGroupAdded = true;

                    entityInfo.Status = EntityStatus.WillShow;
                    entity.OnShow(showEntityInfo);
                    if (!IsCurrentEntityInfo(entityId, entityInfo) || entityInfo.Status != EntityStatus.WillShow)
                    {
                        return;
                    }

                    entityInfo.Status = EntityStatus.Showed;
                }
                catch (Exception exception)
                {
                    if (IsCurrentEntityInfo(entityId, entityInfo))
                    {
                        RollbackFailedEntity(entityId, entityInstance, entityGroup, entity, entityInfo, entityInfoAdded, entityGroupAdded, showEntityInfo);
                    }

                    NotifyShowEntityFailure(entityId, entityAssetName, entityGroup.Name, exception.ToString(), showEntityInfo);
                    return;
                }

                ShowEntitySuccessEventArgs eventArgs = ShowEntitySuccessEventArgs.Create(showEntityInfo.EntityLogicType, entity, duration, showEntityInfo.UserData);
                OnShowEntitySuccess(this, eventArgs);
            }
            finally
            {
                ReferencePool.Release(showEntityInfo);
            }
        }

        private void RollbackFailedEntity(int entityId, object entityInstance, EntityGroup entityGroup, Entity entity, EntityInfo entityInfo, bool entityInfoAdded, bool entityGroupAdded, ShowEntityInfo showEntityInfo)
        {
            if (entity != null && entityInfo != null && entityInfo.Status >= EntityStatus.WillShow)
            {
                entity.OnHide(false, showEntityInfo.UserData);
            }

            if (entityGroupAdded)
            {
                try
                {
                    entityGroup.RemoveEntity(entity);
                }
                catch (Exception exception)
                {
                    Log.Error("Rollback entity group failure with exception '{0}'.", exception);
                }
            }

            if (entityInfoAdded)
            {
                m_EntityInfos.Remove(entityId);
            }

            if (entity != null)
            {
                entity.OnRecycle();
            }

            try
            {
                entityGroup.UnspawnEntityInstance(entityInstance);
            }
            catch (Exception exception)
            {
                Log.Error("Rollback entity instance failure with exception '{0}'.", exception);
            }

            if (entityInfo != null)
            {
                ReferencePool.Release(entityInfo);
            }
        }

        private void NotifyShowEntityFailure(int entityId, string entityAssetName, string entityGroupName, string errorMessage, ShowEntityInfo showEntityInfo)
        {
            ShowEntityFailureEventArgs eventArgs = ShowEntityFailureEventArgs.Create(entityId, showEntityInfo.EntityLogicType, entityAssetName, entityGroupName, errorMessage, showEntityInfo.UserData);
            OnShowEntityFailure(this, eventArgs);
        }

        private void InternalHideEntity(EntityInfo entityInfo, object userData)
        {
            if (entityInfo.Status >= EntityStatus.WillHide)
            {
                return;
            }

            entityInfo.Status = EntityStatus.WillHide;
            while (entityInfo.ChildEntityCount > 0)
            {
                Entity childEntity = entityInfo.GetChildEntity();
                HideEntity(childEntity.Id, userData);
            }

            Entity entity = entityInfo.Entity;
            DetachEntity(entity.Id, userData);
            entity.OnHide(m_IsShutdown, userData);
            entityInfo.Status = EntityStatus.Hidden;

            EntityGroup entityGroup = entity.EntityGroup;
            if (entityGroup == null)
            {
                throw new GameFrameworkException("Entity group is invalid.");
            }

            entityGroup.RemoveEntity(entity);
            if (!m_EntityInfos.Remove(entity.Id))
            {
                throw new GameFrameworkException("Entity info is unmanaged.");
            }

            HideEntityCompleteEventArgs eventArgs = HideEntityCompleteEventArgs.Create(entity.Id, entity.EntityAssetName, entityGroup, userData);
            OnHideEntityComplete(this, eventArgs);
            m_RecycleQueue.Enqueue(entityInfo);
        }

        private void LoadAssetSuccessCallback(string entityAssetName, object entityAsset, float duration, object userData)
        {
            LoadEntityInfo loadEntityInfo = userData as LoadEntityInfo;
            if (loadEntityInfo == null)
            {
                throw new GameFrameworkException("Load entity info is invalid.");
            }

            ShowEntityInfo showEntityInfo = loadEntityInfo.ShowEntityInfo;
            RemoveLoadingEntity(loadEntityInfo);

            if (loadEntityInfo.IsCancelled || m_IsShutdown)
            {
                try
                {
                    m_EntityHelper?.ReleaseEntity(entityAsset, null);
                }
                finally
                {
                    ReferencePool.Release(showEntityInfo);
                    ReferencePool.Release(loadEntityInfo);
                }

                return;
            }

            object entityInstance = null;
            EntityInstanceObject entityInstanceObject = null;
            try
            {
                entityInstance = m_EntityHelper.InstantiateEntity(entityAsset);
                entityInstanceObject = EntityInstanceObject.Create(entityAssetName, entityAsset, entityInstance, m_EntityHelper);
                loadEntityInfo.EntityGroup.RegisterEntityInstanceObject(entityInstanceObject, true);
            }
            catch (Exception exception)
            {
                try
                {
                    m_EntityHelper.ReleaseEntity(entityAsset, entityInstance);
                    if (entityInstanceObject != null)
                    {
                        ReferencePool.Release(entityInstanceObject);
                    }

                    NotifyShowEntityFailure(loadEntityInfo.EntityId, entityAssetName, loadEntityInfo.EntityGroup.Name, exception.ToString(), showEntityInfo);
                }
                finally
                {
                    ReferencePool.Release(showEntityInfo);
                    ReferencePool.Release(loadEntityInfo);
                }

                return;
            }

            int entityId = loadEntityInfo.EntityId;
            EntityGroup entityGroup = loadEntityInfo.EntityGroup;
            ReferencePool.Release(loadEntityInfo);
            InternalShowEntity(entityId, entityAssetName, entityGroup, entityInstanceObject.Target, true, duration, showEntityInfo);
        }

        private void LoadAssetFailureCallback(string entityAssetName, LoadResourceStatus status, string errorMessage, object userData)
        {
            LoadEntityInfo loadEntityInfo = userData as LoadEntityInfo;
            if (loadEntityInfo == null)
            {
                throw new GameFrameworkException("Load entity info is invalid.");
            }

            ShowEntityInfo showEntityInfo = loadEntityInfo.ShowEntityInfo;
            RemoveLoadingEntity(loadEntityInfo);
            try
            {
                if (!loadEntityInfo.IsCancelled && !m_IsShutdown)
                {
                    string message = Utility.Text.Format("Load entity failure, asset name '{0}', status '{1}', error message '{2}'.", entityAssetName, status, errorMessage);
                    NotifyShowEntityFailure(loadEntityInfo.EntityId, entityAssetName, loadEntityInfo.EntityGroup.Name, message, showEntityInfo);
                }
            }
            finally
            {
                ReferencePool.Release(showEntityInfo);
                ReferencePool.Release(loadEntityInfo);
            }
        }

        private void LoadAssetUpdateCallback(string entityAssetName, float progress, object userData)
        {
            LoadEntityInfo loadEntityInfo = userData as LoadEntityInfo;
            if (loadEntityInfo == null || loadEntityInfo.IsCancelled || m_IsShutdown)
            {
                return;
            }

            ShowEntityInfo showEntityInfo = loadEntityInfo.ShowEntityInfo;
            ShowEntityUpdateEventArgs eventArgs = ShowEntityUpdateEventArgs.Create(loadEntityInfo.EntityId, showEntityInfo.EntityLogicType, entityAssetName, loadEntityInfo.EntityGroup.Name, progress, showEntityInfo.UserData);
            OnShowEntityUpdate(this, eventArgs);
        }

        private void LoadAssetDependencyAssetCallback(string entityAssetName, string dependencyAssetName, int loadedCount, int totalCount, object userData)
        {
            LoadEntityInfo loadEntityInfo = userData as LoadEntityInfo;
            if (loadEntityInfo == null || loadEntityInfo.IsCancelled || m_IsShutdown)
            {
                return;
            }

            ShowEntityInfo showEntityInfo = loadEntityInfo.ShowEntityInfo;
            ShowEntityDependencyAssetEventArgs eventArgs = ShowEntityDependencyAssetEventArgs.Create(loadEntityInfo.EntityId, showEntityInfo.EntityLogicType, entityAssetName, loadEntityInfo.EntityGroup.Name, dependencyAssetName, loadedCount, totalCount, showEntityInfo.UserData);
            OnShowEntityDependencyAsset(this, eventArgs);
        }

        private bool IsCurrentEntityInfo(int entityId, EntityInfo entityInfo)
        {
            return entityInfo != null && m_EntityInfos.TryGetValue(entityId, out EntityInfo current) && ReferenceEquals(current, entityInfo);
        }
        private void RemoveLoadingEntity(LoadEntityInfo loadEntityInfo)
        {
            if (m_EntitiesBeingLoaded.TryGetValue(loadEntityInfo.EntityId, out LoadEntityInfo current) && ReferenceEquals(current, loadEntityInfo))
            {
                m_EntitiesBeingLoaded.Remove(loadEntityInfo.EntityId);
            }
        }
    }
}
