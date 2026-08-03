using GameFramework;
using GameFramework.ObjectPool;
using GameFramework.Resource;
using GameFramework.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public sealed partial class UIComponent : GameFrameworkComponent
    {
        private Dictionary<string, UIGroup> m_UIGroups;
        private Dictionary<int, string> m_UIFormsBeingLoaded;
        private HashSet<int> m_UIFormsToReleaseOnLoad;
        private Queue<IUIForm> m_RecycleQueue;
        private LoadAssetCallbacks m_LoadAssetCallbacks;
        private IObjectPoolManager m_ObjectPoolManager;
        private IResourceManager m_ResourceManager;
        private IObjectPool<UIFormInstanceObject> m_InstancePool;
        private IUIFormHelper m_UIFormHelper;
        private int m_Serial;
        private bool m_IsShutdown;

        /// <summary>
        /// 设置对象池管理器。
        /// </summary>
        /// <param name="objectPoolManager">对象池管理器。</param>
        public void SetObjectPoolManager(IObjectPoolManager objectPoolManager)
        {
            if (objectPoolManager == null)
            {
                throw new GameFrameworkException("Object pool manager is invalid.");
            }

            m_ObjectPoolManager = objectPoolManager;
            m_InstancePool = m_ObjectPoolManager.CreateSingleSpawnObjectPool<UIFormInstanceObject>("UI Instance Pool");
        }

        /// <summary>
        /// 设置资源管理器。
        /// </summary>
        /// <param name="resourceManager">资源管理器。</param>
        public void SetResourceManager(IResourceManager resourceManager)
        {
            if (resourceManager == null)
            {
                throw new GameFrameworkException("Resource manager is invalid.");
            }

            m_ResourceManager = resourceManager;
        }

        /// <summary>
        /// 设置界面辅助器。
        /// </summary>
        /// <param name="uiFormHelper">界面辅助器。</param>
        public void SetUIFormHelper(IUIFormHelper uiFormHelper)
        {
            if (uiFormHelper == null)
            {
                throw new GameFrameworkException("UI form helper is invalid.");
            }

            m_UIFormHelper = uiFormHelper;
        }

        /// <summary>
        /// 是否存在界面组。
        /// </summary>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <returns>是否存在界面组。</returns>
        public bool HasUIGroup(string uiGroupName)
        {
            if (string.IsNullOrEmpty(uiGroupName))
            {
                throw new GameFrameworkException("UI group name is invalid.");
            }

            return m_UIGroups.ContainsKey(uiGroupName);
        }

        /// <summary>
        /// 获取界面组。
        /// </summary>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <returns>要获取的界面组。</returns>
        public UIGroup GetUIGroup(string uiGroupName)
        {
            if (string.IsNullOrEmpty(uiGroupName))
            {
                throw new GameFrameworkException("UI group name is invalid.");
            }

            UIGroup uiGroup = null;
            if (m_UIGroups.TryGetValue(uiGroupName, out uiGroup))
            {
                return uiGroup;
            }

            return null;
        }

        /// <summary>
        /// 获取所有界面组。
        /// </summary>
        /// <returns>所有界面组。</returns>
        public IReadOnlyList<UIGroup> GetAllUIGroups()
        {
            List<UIGroup> results = new List<UIGroup>();
            foreach (KeyValuePair<string, UIGroup> uiGroup in m_UIGroups)
            {
                results.Add(uiGroup.Value);
            }

            return results;
        }

        /// <summary>
        /// 增加界面组。
        /// </summary>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="uiGroupDepth">界面组深度。</param>
        /// <returns>是否增加界面组成功。</returns>
        public bool AddUIGroup(string uiGroupName, int uiGroupDepth = 0)
        {
            if (string.IsNullOrEmpty(uiGroupName))
            {
                Log.Error("UI group name is invalid.");
                return false;
            }

            if (HasUIGroup(uiGroupName))
            {
                Log.Error($"UI group name exists.");
                return false;
            }

            UIGroupHelperBase uiGroupHelper = Helper.CreateHelper(m_UIGroupHelperTypeName, m_CustomUIGroupHelper, UIGroupCount);
            if (uiGroupHelper == null)
            {
                Log.Error("Can not create UI group helper.");
                return false;
            }

            uiGroupHelper.name = Utility.Text.Format("UI Group - {0}", uiGroupName);
            uiGroupHelper.gameObject.layer = LayerMask.NameToLayer("UI");
            Transform transform = uiGroupHelper.transform;
            transform.SetParent(m_InstanceRoot);
            transform.localScale = Vector3.one;

            m_UIGroups.Add(uiGroupName, new UIGroup(uiGroupName, uiGroupDepth, uiGroupHelper));

            return true;
        }

        /// <summary>
        /// 是否存在界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <returns>是否存在界面。</returns>
        public bool HasUIForm(int serialId)
        {
            foreach (KeyValuePair<string, UIGroup> uiGroup in m_UIGroups)
            {
                if (uiGroup.Value.HasUIForm(serialId))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 是否存在界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>是否存在界面。</returns>
        public bool HasUIForm(string uiFormAssetName)
        {
            if (string.IsNullOrEmpty(uiFormAssetName))
            {
                throw new GameFrameworkException("UI form asset name is invalid.");
            }

            foreach (KeyValuePair<string, UIGroup> uiGroup in m_UIGroups)
            {
                if (uiGroup.Value.HasUIForm(uiFormAssetName))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 获取界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <returns>要获取的界面。</returns>
        public IUIForm GetUIForm(int serialId)
        {
            foreach (KeyValuePair<string, UIGroup> uiGroup in m_UIGroups)
            {
                IUIForm uiForm = uiGroup.Value.GetUIForm(serialId);
                if (uiForm != null)
                {
                    return uiForm;
                }
            }

            return null;
        }

        /// <summary>
        /// 获取界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>要获取的界面。</returns>
        public IUIForm GetUIForm(string uiFormAssetName)
        {
            if (string.IsNullOrEmpty(uiFormAssetName))
            {
                throw new GameFrameworkException("UI form asset name is invalid.");
            }

            foreach (KeyValuePair<string, UIGroup> uiGroup in m_UIGroups)
            {
                IUIForm uiForm = uiGroup.Value.GetUIForm(uiFormAssetName);
                if (uiForm != null)
                {
                    return uiForm;
                }
            }

            return null;
        }

        /// <summary>
        /// 获取界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>要获取的界面。</returns>
        public IReadOnlyList<IUIForm> GetUIForms(string uiFormAssetName)
        {
            if (string.IsNullOrEmpty(uiFormAssetName))
            {
                throw new GameFrameworkException("UI form asset name is invalid.");
            }

            List<IUIForm> results = new List<IUIForm>();
            foreach (KeyValuePair<string, UIGroup> uiGroup in m_UIGroups)
            {
                uiGroup.Value.GetUIForms(uiFormAssetName, results, false);
            }

            return results;
        }

        /// <summary>
        /// 获取所有已加载的界面。
        /// </summary>
        /// <returns>所有已加载的界面。</returns>
        public IReadOnlyList<IUIForm> GetAllLoadedUIForms()
        {
            List<IUIForm> results = new List<IUIForm>();
            foreach (KeyValuePair<string, UIGroup> uiGroup in m_UIGroups)
            {
                uiGroup.Value.GetAllUIForms(results, false);
            }

            return results;
        }

        /// <summary>
        /// 获取所有正在加载界面的序列编号。
        /// </summary>
        /// <returns>所有正在加载界面的序列编号。</returns>
        public IReadOnlyList<int> GetAllLoadingUIFormSerialIds()
        {
            List<int> results = new List<int>();
            foreach (KeyValuePair<int, string> uiFormBeingLoaded in m_UIFormsBeingLoaded)
            {
                results.Add(uiFormBeingLoaded.Key);
            }

            return results;
        }

        /// <summary>
        /// 是否正在加载界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <returns>是否正在加载界面。</returns>
        public bool IsLoadingUIForm(int serialId)
        {
            return m_UIFormsBeingLoaded.ContainsKey(serialId);
        }

        /// <summary>
        /// 是否正在加载界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>是否正在加载界面。</returns>
        public bool IsLoadingUIForm(string uiFormAssetName)
        {
            if (string.IsNullOrEmpty(uiFormAssetName))
            {
                throw new GameFrameworkException("UI form asset name is invalid.");
            }

            return m_UIFormsBeingLoaded.ContainsValue(uiFormAssetName);
        }

        /// <summary>
        /// 是否是合法的界面。
        /// </summary>
        /// <param name="uiForm">界面。</param>
        /// <returns>界面是否合法。</returns>
        public bool IsValidUIForm(IUIForm uiForm)
        {
            if (uiForm == null)
            {
                return false;
            }

            return HasUIForm(uiForm.SerialId);
        }

        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="priority">加载界面资源的优先级。</param>
        /// <param name="pauseCoveredUIForm">是否暂停被覆盖的界面。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>界面的序列编号。</returns>
        public int OpenUIForm(string uiFormAssetName, string uiGroupName, int priority = 0, bool pauseCoveredUIForm = false, object userData = null)
        {
            if (m_ResourceManager == null)
            {
                throw new GameFrameworkException("You must set resource manager first.");
            }

            if (m_UIFormHelper == null)
            {
                throw new GameFrameworkException("You must set UI form helper first.");
            }

            if (string.IsNullOrEmpty(uiFormAssetName))
            {
                throw new GameFrameworkException("UI form asset name is invalid.");
            }

            if (string.IsNullOrEmpty(uiGroupName))
            {
                throw new GameFrameworkException("UI group name is invalid.");
            }

            UIGroup uiGroup = (UIGroup)GetUIGroup(uiGroupName);
            if (uiGroup == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("UI group '{0}' is not exist.", uiGroupName));
            }

            int serialId = ++m_Serial;
            UIFormInstanceObject uiFormInstanceObject = m_InstancePool.Spawn(uiFormAssetName);
            if (uiFormInstanceObject == null)
            {
                m_UIFormsBeingLoaded.Add(serialId, uiFormAssetName);
                m_ResourceManager.LoadAsset(uiFormAssetName, priority, m_LoadAssetCallbacks, OpenUIFormInfo.Create(serialId, uiGroup, pauseCoveredUIForm, userData));
            }
            else
            {
                InternalOpenUIForm(serialId, uiFormAssetName, uiGroup, uiFormInstanceObject.Target, pauseCoveredUIForm, false, 0f, userData);
            }

            return serialId;
        }

        /// <summary>
        /// 关闭界面。
        /// </summary>
        /// <param name="serialId">要关闭界面的序列编号。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void CloseUIForm(int serialId, object userData = null)
        {
            if (IsLoadingUIForm(serialId))
            {
                m_UIFormsToReleaseOnLoad.Add(serialId);
                m_UIFormsBeingLoaded.Remove(serialId);
                return;
            }

            IUIForm uiForm = GetUIForm(serialId);
            if (uiForm == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find UI form '{0}'.", serialId));
            }

            CloseUIForm(uiForm, userData);
        }

        /// <summary>
        /// 关闭界面。
        /// </summary>
        /// <param name="uiForm">要关闭的界面。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void CloseUIForm(IUIForm uiForm, object userData = null)
        {
            if (uiForm == null)
            {
                throw new GameFrameworkException("UI form is invalid.");
            }

            UIGroup uiGroup = (UIGroup)uiForm.UIGroup;
            if (uiGroup == null)
            {
                throw new GameFrameworkException("UI group is invalid.");
            }

            uiGroup.RemoveUIForm(uiForm);
            uiForm.OnClose(m_IsShutdown, userData);
            uiGroup.Refresh();

            CloseUIFormCompleteEventArgs closeUIFormCompleteEventArgs = CloseUIFormCompleteEventArgs.Create(uiForm.SerialId, uiForm.UIFormAssetName, uiGroup, userData);
            OnCloseUIFormComplete(this, closeUIFormCompleteEventArgs);

            m_RecycleQueue.Enqueue(uiForm);
        }

        /// <summary>
        /// 关闭所有已加载的界面。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        public void CloseAllLoadedUIForms(object userData = null)
        {
            IReadOnlyList<IUIForm> uiForms = GetAllLoadedUIForms();
            foreach (IUIForm uiForm in uiForms)
            {
                if (!HasUIForm(uiForm.SerialId))
                {
                    continue;
                }

                CloseUIForm(uiForm, userData);
            }
        }

        /// <summary>
        /// 关闭所有正在加载的界面。
        /// </summary>
        public void CloseAllLoadingUIForms()
        {
            foreach (KeyValuePair<int, string> uiFormBeingLoaded in m_UIFormsBeingLoaded)
            {
                m_UIFormsToReleaseOnLoad.Add(uiFormBeingLoaded.Key);
            }

            m_UIFormsBeingLoaded.Clear();
        }

        /// <summary>
        /// 激活界面。
        /// </summary>
        /// <param name="uiForm">要激活的界面。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void RefocusUIForm(IUIForm uiForm, object userData = null)
        {
            if (uiForm == null)
            {
                throw new GameFrameworkException("UI form is invalid.");
            }

            UIGroup uiGroup = (UIGroup)uiForm.UIGroup;
            if (uiGroup == null)
            {
                throw new GameFrameworkException("UI group is invalid.");
            }

            uiGroup.RefocusUIForm(uiForm);
            uiGroup.Refresh();
            uiForm.OnRefocus(userData);
        }

        /// <summary>
        /// 设置界面实例是否被加锁。
        /// </summary>
        /// <param name="uiFormInstance">要设置是否被加锁的界面实例。</param>
        /// <param name="locked">界面实例是否被加锁。</param>
        public void SetUIFormInstanceLocked(object uiFormInstance, bool locked)
        {
            if (uiFormInstance == null)
            {
                throw new GameFrameworkException("UI form instance is invalid.");
            }

            m_InstancePool.SetLocked(uiFormInstance, locked);
        }

        /// <summary>
        /// 设置界面实例的优先级。
        /// </summary>
        /// <param name="uiFormInstance">要设置优先级的界面实例。</param>
        /// <param name="priority">界面实例优先级。</param>
        public void SetUIFormInstancePriority(object uiFormInstance, int priority)
        {
            if (uiFormInstance == null)
            {
                throw new GameFrameworkException("UI form instance is invalid.");
            }

            m_InstancePool.SetPriority(uiFormInstance, priority);
        }

        private void InternalOpenUIForm(int serialId, string uiFormAssetName, UIGroup uiGroup, object uiFormInstance, bool pauseCoveredUIForm, bool isNewInstance, float duration, object userData)
        {
            try
            {
                IUIForm uiForm = m_UIFormHelper.CreateUIForm(uiFormInstance, uiGroup, userData);
                if (uiForm == null)
                {
                    throw new GameFrameworkException("Can not create UI form in UI form helper.");
                }

                uiForm.OnInit(serialId, uiFormAssetName, uiGroup, pauseCoveredUIForm, isNewInstance, userData);
                uiGroup.AddUIForm(uiForm);
                uiForm.OnOpen(userData);
                uiGroup.Refresh();

                OpenUIFormSuccessEventArgs openUIFormSuccessEventArgs = OpenUIFormSuccessEventArgs.Create(uiForm, duration, userData);
                OnOpenUIFormSuccess(this, openUIFormSuccessEventArgs);
            }
            catch (Exception exception)
            {
                OpenUIFormFailureEventArgs openUIFormFailureEventArgs = OpenUIFormFailureEventArgs.Create(serialId, uiFormAssetName, uiGroup.Name, pauseCoveredUIForm, exception.ToString(), userData);
                OnOpenUIFormFailure(this, openUIFormFailureEventArgs);
                return;
            }
        }

        private void LoadAssetSuccessCallback(string uiFormAssetName, object uiFormAsset, float duration, object userData)
        {
            OpenUIFormInfo openUIFormInfo = (OpenUIFormInfo)userData;
            if (openUIFormInfo == null)
            {
                throw new GameFrameworkException("Open UI form info is invalid.");
            }

            if (m_UIFormsToReleaseOnLoad.Contains(openUIFormInfo.SerialId))
            {
                m_UIFormsToReleaseOnLoad.Remove(openUIFormInfo.SerialId);
                ReferencePool.Release(openUIFormInfo);
                m_UIFormHelper.ReleaseUIForm(uiFormAsset, null);
                return;
            }

            m_UIFormsBeingLoaded.Remove(openUIFormInfo.SerialId);
            UIFormInstanceObject uiFormInstanceObject = UIFormInstanceObject.Create(uiFormAssetName, uiFormAsset, m_UIFormHelper.InstantiateUIForm(uiFormAsset), m_UIFormHelper);
            m_InstancePool.Register(uiFormInstanceObject, true);

            InternalOpenUIForm(openUIFormInfo.SerialId, uiFormAssetName, openUIFormInfo.UIGroup, uiFormInstanceObject.Target, openUIFormInfo.PauseCoveredUIForm, true, duration, openUIFormInfo.UserData);
            ReferencePool.Release(openUIFormInfo);
        }

        private void LoadAssetFailureCallback(string uiFormAssetName, LoadResourceStatus status, string errorMessage, object userData)
        {
            OpenUIFormInfo openUIFormInfo = (OpenUIFormInfo)userData;
            if (openUIFormInfo == null)
            {
                throw new GameFrameworkException("Open UI form info is invalid.");
            }

            if (m_UIFormsToReleaseOnLoad.Contains(openUIFormInfo.SerialId))
            {
                m_UIFormsToReleaseOnLoad.Remove(openUIFormInfo.SerialId);
                return;
            }

            m_UIFormsBeingLoaded.Remove(openUIFormInfo.SerialId);

            string appendErrorMessage = Utility.Text.Format("Load UI form failure, asset name '{0}', status '{1}', error message '{2}'.", uiFormAssetName, status, errorMessage);
            OpenUIFormFailureEventArgs openUIFormFailureEventArgs = OpenUIFormFailureEventArgs.Create(openUIFormInfo.SerialId, uiFormAssetName, openUIFormInfo.UIGroup.Name, openUIFormInfo.PauseCoveredUIForm, appendErrorMessage, openUIFormInfo.UserData);
            OnOpenUIFormFailure(this, openUIFormFailureEventArgs);

            return;
        }

        private void LoadAssetUpdateCallback(string uiFormAssetName, float progress, object userData)
        {
            OpenUIFormInfo openUIFormInfo = (OpenUIFormInfo)userData;
            if (openUIFormInfo == null)
            {
                throw new GameFrameworkException("Open UI form info is invalid.");
            }

            OpenUIFormUpdateEventArgs openUIFormUpdateEventArgs = OpenUIFormUpdateEventArgs.Create(openUIFormInfo.SerialId, uiFormAssetName, openUIFormInfo.UIGroup.Name, openUIFormInfo.PauseCoveredUIForm, progress, openUIFormInfo.UserData);
            OnOpenUIFormUpdate(this, openUIFormUpdateEventArgs);
        }

        private void LoadAssetDependencyAssetCallback(string uiFormAssetName, string dependencyAssetName, int loadedCount, int totalCount, object userData)
        {
            OpenUIFormInfo openUIFormInfo = (OpenUIFormInfo)userData;
            if (openUIFormInfo == null)
            {
                throw new GameFrameworkException("Open UI form info is invalid.");
            }

            OpenUIFormDependencyAssetEventArgs openUIFormDependencyAssetEventArgs = OpenUIFormDependencyAssetEventArgs.Create(openUIFormInfo.SerialId, uiFormAssetName, openUIFormInfo.UIGroup.Name, openUIFormInfo.PauseCoveredUIForm, dependencyAssetName, loadedCount, totalCount, openUIFormInfo.UserData);
            OnOpenUIFormDependencyAsset(this, openUIFormDependencyAssetEventArgs);
        }

        private void OnOpenUIFormSuccess(object sender, OpenUIFormSuccessEventArgs e)
        {
            if (m_EnableOpenUIFormSuccessEvent)
            {
                m_EventComponent.Fire(this, e);
            }
            else
            {
                ReferencePool.Release(e);
            }
        }

        private void OnOpenUIFormFailure(object sender, OpenUIFormFailureEventArgs e)
        {
            Log.Warning("Open UI form failure, asset name '{0}', UI group name '{1}', pause covered UI form '{2}', error message '{3}'.", e.UIFormAssetName, e.UIGroupName, e.PauseCoveredUIForm, e.ErrorMessage);
            if (m_EnableOpenUIFormFailureEvent)
            {
                m_EventComponent.Fire(this, e);
            }
            else
            {
                ReferencePool.Release(e);
            }
        }

        private void OnOpenUIFormUpdate(object sender, OpenUIFormUpdateEventArgs e)
        {
            if (m_EnableOpenUIFormUpdateEvent)
            {
                m_EventComponent.Fire(this, e);
            }
            else
            {
                ReferencePool.Release(e);
            }
        }

        private void OnOpenUIFormDependencyAsset(object sender, OpenUIFormDependencyAssetEventArgs e)
        {
            if (m_EnableOpenUIFormDependencyAssetEvent)
            {
                m_EventComponent.Fire(this, e);
            }
            else
            {
                ReferencePool.Release(e);
            }
        }

        private void OnCloseUIFormComplete(object sender, CloseUIFormCompleteEventArgs e)
        {
            if (m_EnableCloseUIFormCompleteEvent)
            {
                m_EventComponent.Fire(this, e);
            }
            else
            {
                ReferencePool.Release(e);
            }
        }
    }
}
