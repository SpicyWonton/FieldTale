using GameFramework;
using GameFramework.ObjectPool;
using GameFramework.Resource;
using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace UnityGameFramework.Runtime
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Entity")]
    public sealed partial class EntityComponent : GameFrameworkComponent
    {
        private const int DefaultPriority = 0;
        private EventComponent m_EventComponent;
        private bool m_IsReady;

        [SerializeField] private bool m_EnableShowEntityUpdateEvent = false;
        [SerializeField] private bool m_EnableShowEntityDependencyAssetEvent = false;
        [SerializeField] private Transform m_InstanceRoot = null;
        [SerializeField] private string m_EntityHelperTypeName = "UnityGameFramework.Runtime.DefaultEntityHelper";
        [SerializeField] private EntityHelperBase m_CustomEntityHelper = null;
        [SerializeField, FormerlySerializedAs("m_EntityGroups")]
        private EntityGroupConfig[] m_EntityGroupConfigs = Array.Empty<EntityGroupConfig>();

        protected override void Awake()
        {
            base.Awake();
            InitializeEntityCore();
            m_IsReady = false;
        }

        private void Start()
        {
            BaseComponent baseComponent = GameEntry.GetComponent<BaseComponent>();
            if (baseComponent == null)
            {
                Log.Fatal("Base component is invalid.");
                return;
            }

            m_EventComponent = GameEntry.GetComponent<EventComponent>();
            if (m_EventComponent == null)
            {
                Log.Fatal("Event component is invalid.");
                return;
            }

            SetResourceManager(baseComponent.EditorResourceMode ? baseComponent.EditorResourceHelper : GameFrameworkEntry.GetModule<IResourceManager>());
            SetObjectPoolManager(GameFrameworkEntry.GetModule<IObjectPoolManager>());

            EntityHelperBase entityHelper = Helper.CreateHelper(m_EntityHelperTypeName, m_CustomEntityHelper);
            if (entityHelper == null)
            {
                Log.Error("Can not create entity helper.");
                return;
            }

            entityHelper.name = "Entity Helper";
            entityHelper.transform.SetParent(transform, false);
            SetEntityHelper(entityHelper);

            if (m_InstanceRoot == null)
            {
                m_InstanceRoot = new GameObject("Entity Instances").transform;
                m_InstanceRoot.SetParent(transform, false);
            }

            EntityGroupConfig[] configs = m_EntityGroupConfigs ?? Array.Empty<EntityGroupConfig>();
            foreach (EntityGroupConfig config in configs)
            {
                if (!AddEntityGroupWithRoot(config.Name, config.InstanceAutoReleaseInterval, config.InstanceCapacity, config.InstanceExpireTime, config.InstancePriority))
                {
                    Log.Warning("Add entity group '{0}' failure.", config.Name);
                }
            }

            m_IsReady = true;
        }

        protected override void OnDestroy()
        {
            m_IsReady = false;
            ShutdownEntityCore();
            m_EventComponent = null;
            base.OnDestroy();
        }

        public bool AddEntityGroup(string name, float autoReleaseInterval, int capacity, float expireTime, int priority)
        {
            EnsureReady();
            return AddEntityGroupWithRoot(name, autoReleaseInterval, capacity, expireTime, priority);
        }

        public void ShowEntity<T>(int id, string assetName, string groupName, int priority = DefaultPriority, object userData = null) where T : EntityLogic
        {
            ShowEntity(id, typeof(T), assetName, groupName, priority, userData);
        }

        public void ShowEntity(int id, Type logicType, string assetName, string groupName, int priority = DefaultPriority, object userData = null)
        {
            EnsureReady();
            ValidateEntityLogicType(logicType);
            ShowEntityInternal(id, assetName, groupName, priority, ShowEntityInfo.Create(logicType, userData));
        }

        public void AttachEntity(int childId, int parentId, object userData = null) => AttachEntity(GetEntity(childId), GetEntity(parentId), string.Empty, userData);

        public void AttachEntity(Entity child, Entity parent, object userData = null) => AttachEntity(child, parent, string.Empty, userData);
        public void AttachEntity(int childId, int parentId, string path, object userData = null) => AttachEntity(GetEntity(childId), GetEntity(parentId), path, userData);

        public void AttachEntity(Entity child, Entity parent, string path, object userData = null)
        {
            EnsureReady();
            if (!ValidateAttachEntities(child, parent)) return;

            Transform parentTransform = string.IsNullOrEmpty(path) ? parent.Logic.CachedTransform : parent.Logic.CachedTransform.Find(path);
            if (parentTransform == null)
            {
                Log.Warning("Can not find transform path '{0}' from parent entity '{1}'.", path, parent.Logic.Name);
                parentTransform = parent.Logic.CachedTransform;
            }

            AttachEntityInternal(child, parent, parentTransform, userData);
        }

        public void AttachEntity(int childId, int parentId, Transform parentTransform, object userData = null) => AttachEntity(GetEntity(childId), GetEntity(parentId), parentTransform, userData);

        public void AttachEntity(Entity child, Entity parent, Transform parentTransform, object userData = null)
        {
            EnsureReady();
            if (!ValidateAttachEntities(child, parent)) return;
            AttachEntityInternal(child, parent, parentTransform ?? parent.Logic.CachedTransform, userData);
        }

        public void SetEntityInstanceLocked(Entity entity, bool locked)
        {
            EnsureReady();
            if (!TryGetEntityGroup(entity, out EntityGroup entityGroup)) return;
            entityGroup.SetEntityInstanceLocked(entity.gameObject, locked);
        }

        public void SetInstancePriority(Entity entity, int priority)
        {
            EnsureReady();
            if (!TryGetEntityGroup(entity, out EntityGroup entityGroup)) return;
            entityGroup.SetEntityInstancePriority(entity.gameObject, priority);
        }

        private bool AddEntityGroupWithRoot(string name, float autoReleaseInterval, int capacity, float expireTime, int priority)
        {
            if (HasEntityGroup(name)) return false;

            Transform groupRoot = new GameObject(Utility.Text.Format("Entity Group - {0}", name)).transform;
            groupRoot.SetParent(m_InstanceRoot, false);
            try
            {
                if (AddEntityGroupInternal(name, autoReleaseInterval, capacity, expireTime, priority, groupRoot))
                {
                    return true;
                }
            }
            catch
            {
                Destroy(groupRoot.gameObject);
                throw;
            }

            Destroy(groupRoot.gameObject);
            return false;
        }

        private void EnsureReady()
        {
            if (!m_IsReady || m_IsShutdown)
            {
                throw new GameFrameworkException("Entity component is not ready.");
            }
        }

        private static void ValidateEntityLogicType(Type logicType)
        {
            if (logicType == null || !typeof(EntityLogic).IsAssignableFrom(logicType) || logicType.IsAbstract || logicType.ContainsGenericParameters)
            {
                throw new GameFrameworkException("Entity logic type is invalid.");
            }
        }

        private bool ValidateAttachEntities(Entity child, Entity parent)
        {
            if (!IsValidEntity(child))
            {
                Log.Warning("Child entity is invalid or unmanaged.");
                return false;
            }

            if (!IsValidEntity(parent))
            {
                Log.Warning("Parent entity is invalid or unmanaged.");
                return false;
            }

            return true;
        }

        private bool TryGetEntityGroup(Entity entity, out EntityGroup entityGroup)
        {
            if (!IsValidEntity(entity))
            {
                Log.Warning("Entity is invalid or unmanaged.");
                entityGroup = null;
                return false;
            }

            entityGroup = entity.EntityGroup;
            if (entityGroup == null)
            {
                Log.Warning("Entity group is invalid.");
                return false;
            }

            return true;
        }

        private bool CanFireEvent()
        {
            return !m_IsShutdown && m_EventComponent != null;
        }

        private void OnShowEntitySuccess(object sender, ShowEntitySuccessEventArgs e)
        {
            if (CanFireEvent()) m_EventComponent.Fire(this, e);
            else ReferencePool.Release(e);
        }

        private void OnShowEntityFailure(object sender, ShowEntityFailureEventArgs e)
        {
            Log.Warning("Show entity failure, entity id '{0}', asset name '{1}', entity group name '{2}', error message '{3}'.", e.EntityId, e.EntityAssetName, e.EntityGroupName, e.ErrorMessage);
            if (CanFireEvent()) m_EventComponent.Fire(this, e);
            else ReferencePool.Release(e);
        }

        private void OnShowEntityUpdate(object sender, ShowEntityUpdateEventArgs e)
        {
            if (m_EnableShowEntityUpdateEvent && CanFireEvent()) m_EventComponent.Fire(this, e);
            else ReferencePool.Release(e);
        }

        private void OnShowEntityDependencyAsset(object sender, ShowEntityDependencyAssetEventArgs e)
        {
            if (m_EnableShowEntityDependencyAssetEvent && CanFireEvent()) m_EventComponent.Fire(this, e);
            else ReferencePool.Release(e);
        }

        private void OnHideEntityComplete(object sender, HideEntityCompleteEventArgs e)
        {
            if (CanFireEvent()) m_EventComponent.Fire(this, e);
            else ReferencePool.Release(e);
        }
    }
}
