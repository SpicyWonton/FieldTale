//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework;
using System;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 实体。
    /// </summary>
    public sealed class Entity : MonoBehaviour
    {
        private int m_Id;
        private string m_EntityAssetName;
        private EntityGroup m_EntityGroup;
        private EntityLogic m_EntityLogic;

        /// <summary>
        /// 获取实体编号。
        /// </summary>
        public int Id
        {
            get
            {
                return m_Id;
            }
        }

        /// <summary>
        /// 获取实体资源名称。
        /// </summary>
        public string EntityAssetName
        {
            get
            {
                return m_EntityAssetName;
            }
        }

        /// <summary>
        /// 获取实体实例。
        /// </summary>
        internal object Handle
        {
            get
            {
                return gameObject;
            }
        }

        /// <summary>
        /// 获取实体所属的实体组。
        /// </summary>
        public EntityGroup EntityGroup
        {
            get
            {
                return m_EntityGroup;
            }
        }

        /// <summary>
        /// 获取实体逻辑。
        /// </summary>
        public EntityLogic Logic
        {
            get
            {
                return m_EntityLogic;
            }
        }

        /// <summary>
        /// 实体初始化。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        /// <param name="entityAssetName">实体资源名称。</param>
        /// <param name="entityGroup">实体所属的实体组。</param>
        /// <param name="isNewInstance">是否是新实例。</param>
        /// <param name="userData">用户自定义数据。</param>
        internal void OnInit(int entityId, string entityAssetName, EntityGroup entityGroup, bool isNewInstance, object userData)
        {
            m_Id = entityId;
            m_EntityAssetName = entityAssetName;
            if (isNewInstance)
            {
                m_EntityGroup = entityGroup;
            }
            else if (m_EntityGroup != entityGroup)
            {
                throw new GameFrameworkException("Entity group is inconsistent for non-new-instance entity.");
            }

            ShowEntityInfo showEntityInfo = userData as ShowEntityInfo;
            if (showEntityInfo == null)
            {
                throw new GameFrameworkException("Show entity info is invalid.");
            }

            Type entityLogicType = showEntityInfo.EntityLogicType;
            if (entityLogicType == null || !typeof(EntityLogic).IsAssignableFrom(entityLogicType) || entityLogicType.IsAbstract || entityLogicType.ContainsGenericParameters)
            {
                throw new GameFrameworkException("Entity logic type is invalid.");
            }

            if (m_EntityLogic != null)
            {
                if (m_EntityLogic.GetType() == entityLogicType)
                {
                    m_EntityLogic.enabled = true;
                    return;
                }

                Destroy(m_EntityLogic);
                m_EntityLogic = null;
            }

            m_EntityLogic = gameObject.AddComponent(entityLogicType) as EntityLogic;
            if (m_EntityLogic == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Entity '{0}' can not add entity logic.", entityAssetName));
            }

            m_EntityLogic.OnInit(showEntityInfo.UserData);
        }

        /// <summary>
        /// 实体回收。
        /// </summary>
        internal void OnRecycle()
        {
            try
            {
                m_EntityLogic?.OnRecycle();
            }
            catch (Exception exception)
            {
                Log.Error("Entity '[{0}]{1}' OnRecycle with exception '{2}'.", m_Id, m_EntityAssetName, exception);
            }
            finally
            {
                if (m_EntityLogic != null)
                {
                    m_EntityLogic.enabled = false;
                }

                m_Id = 0;
                m_EntityAssetName = null;
            }
        }

        /// <summary>
        /// 实体显示。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        internal void OnShow(object userData)
        {
            ShowEntityInfo showEntityInfo = userData as ShowEntityInfo;
            if (showEntityInfo == null)
            {
                throw new GameFrameworkException("Show entity info is invalid.");
            }

            if (m_EntityLogic == null)
            {
                throw new GameFrameworkException("Entity logic is invalid.");
            }

            m_EntityLogic.OnShow(showEntityInfo.UserData);
        }

        /// <summary>
        /// 实体隐藏。
        /// </summary>
        /// <param name="isShutdown">是否是关闭实体管理器时触发。</param>
        /// <param name="userData">用户自定义数据。</param>
        internal void OnHide(bool isShutdown, object userData)
        {
            try
            {
                m_EntityLogic.OnHide(isShutdown, userData);
            }
            catch (Exception exception)
            {
                Log.Error("Entity '[{0}]{1}' OnHide with exception '{2}'.", m_Id, m_EntityAssetName, exception);
            }
        }

        /// <summary>
        /// 实体附加子实体。
        /// </summary>
        /// <param name="childEntity">附加的子实体。</param>
        /// <param name="parentTransform">被附加父实体的位置。</param>
        /// <param name="userData">用户自定义数据。</param>
        internal void OnAttached(Entity childEntity, Transform parentTransform, object userData)
        {
            try
            {
                m_EntityLogic.OnAttached(childEntity.Logic, parentTransform, userData);
            }
            catch (Exception exception)
            {
                Log.Error("Entity '[{0}]{1}' OnAttached with exception '{2}'.", m_Id, m_EntityAssetName, exception);
            }
        }

        /// <summary>
        /// 实体解除子实体。
        /// </summary>
        /// <param name="childEntity">解除的子实体。</param>
        /// <param name="userData">用户自定义数据。</param>
        internal void OnDetached(Entity childEntity, object userData)
        {
            try
            {
                m_EntityLogic.OnDetached(childEntity.Logic, userData);
            }
            catch (Exception exception)
            {
                Log.Error("Entity '[{0}]{1}' OnDetached with exception '{2}'.", m_Id, m_EntityAssetName, exception);
            }
        }

        /// <summary>
        /// 实体附加子实体。
        /// </summary>
        /// <param name="parentEntity">被附加的父实体。</param>
        /// <param name="parentTransform">被附加父实体的位置。</param>
        /// <param name="userData">用户自定义数据。</param>
        internal void OnAttachTo(Entity parentEntity, Transform parentTransform, object userData)
        {
            try
            {
                m_EntityLogic.OnAttachTo(parentEntity.Logic, parentTransform, userData);
            }
            catch (Exception exception)
            {
                Log.Error("Entity '[{0}]{1}' OnAttachTo with exception '{2}'.", m_Id, m_EntityAssetName, exception);
            }
        }

        /// <summary>
        /// 实体解除子实体。
        /// </summary>
        /// <param name="parentEntity">被解除的父实体。</param>
        /// <param name="userData">用户自定义数据。</param>
        internal void OnDetachFrom(Entity parentEntity, object userData)
        {
            try
            {
                m_EntityLogic.OnDetachFrom(parentEntity.Logic, userData);
            }
            catch (Exception exception)
            {
                Log.Error("Entity '[{0}]{1}' OnDetachFrom with exception '{2}'.", m_Id, m_EntityAssetName, exception);
            }
        }

        /// <summary>
        /// 实体轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        internal void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            try
            {
                m_EntityLogic.OnUpdate(elapseSeconds, realElapseSeconds);
            }
            catch (Exception exception)
            {
                Log.Error("Entity '[{0}]{1}' OnUpdate with exception '{2}'.", m_Id, m_EntityAssetName, exception);
            }
        }
    }
}
