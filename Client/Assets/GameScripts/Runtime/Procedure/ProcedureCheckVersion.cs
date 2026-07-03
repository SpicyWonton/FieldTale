using GameFramework;
using GameFramework.Event;
using GameFramework.Fsm;
using GameFramework.Procedure;
using GameFramework.Resource;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace FieldTale
{
    public class ProcedureCheckVersion : ProcedureBase
    {
        private bool m_CheckVersionComplete = false;
        private bool m_NeedUpdateVersion = false;
        private VersionInfo m_VersionInfo = null;

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

            m_CheckVersionComplete = false;
            m_NeedUpdateVersion = false;
            m_VersionInfo = null;

            FrameworkRoot.Event.Subscribe(WebRequestSuccessEventArgs.EventId, OnWebRequestSuccess);
            FrameworkRoot.Event.Subscribe(WebRequestFailureEventArgs.EventId, OnWebRequestFailure);

            // 向服务器请求版本信息
            FrameworkRoot.WebRequest.AddWebRequest(Utility.Text.Format(FrameworkRoot.BuiltinData.BuildInfo.CheckVersionUrl, GetPlatformPath()), this);
        }

        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            FrameworkRoot.Event.Unsubscribe(WebRequestSuccessEventArgs.EventId, OnWebRequestSuccess);
            FrameworkRoot.Event.Unsubscribe(WebRequestFailureEventArgs.EventId, OnWebRequestFailure);

            base.OnLeave(procedureOwner, isShutdown);
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (!m_CheckVersionComplete)
            {
                return;
            }

            if (m_NeedUpdateVersion)
            {
                procedureOwner.SetData<VarInt32>("VersionListLength", m_VersionInfo.VersionListLength);
                procedureOwner.SetData<VarInt32>("VersionListHashCode", m_VersionInfo.VersionListHashCode);
                procedureOwner.SetData<VarInt32>("VersionListCompressedLength", m_VersionInfo.VersionListCompressedLength);
                procedureOwner.SetData<VarInt32>("VersionListCompressedHashCode", m_VersionInfo.VersionListCompressedHashCode);
                ChangeState<ProcedureUpdateVersion>(procedureOwner);
            }
            else
            {
                ChangeState<ProcedureVerifyResources>(procedureOwner);
            }
        }

        private void GotoUpdateApp(object userData)
        {
            string url = null;
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            url = FrameworkRoot.BuiltinData.BuildInfo.WindowsAppUrl;
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            url = FrameworkRoot.BuiltinData.BuildInfo.MacOSAppUrl;
#elif UNITY_IOS
            url = FrameworkRoot.BuiltinData.BuildInfo.IOSAppUrl;
#elif UNITY_ANDROID
            url = FrameworkRoot.BuiltinData.BuildInfo.AndroidAppUrl;
#endif
            if (!string.IsNullOrEmpty(url))
            {
                Application.OpenURL(url);
            }
        }

        private void OnWebRequestSuccess(object sender, GameEventArgs e)
        {
            WebRequestSuccessEventArgs ne = (WebRequestSuccessEventArgs) e;
            if (ne.UserData != this)
            {
                return;
            }

            // 解析版本信息
            byte[] versionInfoBytes = ne.GetWebResponseBytes();
            string versionInfoString = Utility.Converter.GetString(versionInfoBytes);
            m_VersionInfo = Utility.Json.ToObject<VersionInfo>(versionInfoString);
            if (m_VersionInfo == null)
            {
                Log.Error("Parse VersionInfo failure.");
                return;
            }

            Log.Info("Latest game version is '{0} ({1})', local game version is '{2} ({3})'.", m_VersionInfo.LatestGameVersion, m_VersionInfo.InternalGameVersion.ToString(), Version.GameVersion, Version.InternalGameVersion.ToString());

            if (m_VersionInfo.ForceUpdateGame)
            {
                // 需要强制更新游戏应用
                FrameworkRoot.BuiltinData.OpenDialog(new DialogParams
                {
                    Mode = 2,
                    Title = FrameworkRoot.Localization.GetString("ForceUpdate.Title"),
                    Message = FrameworkRoot.Localization.GetString("ForceUpdate.Message"),
                    ConfirmText = FrameworkRoot.Localization.GetString("ForceUpdate.UpdateButton"),
                    OnClickConfirm = GotoUpdateApp,
                    CancelText = FrameworkRoot.Localization.GetString("ForceUpdate.QuitButton"),
                    OnClickCancel = delegate(object userData) { UnityGameFramework.Runtime.GameEntry.Shutdown(ShutdownType.Quit); },
                });

                return;
            }

            // 设置资源更新下载地址
            FrameworkRoot.Resource.UpdatePrefixUri = Utility.Path.GetRegularPath(m_VersionInfo.UpdatePrefixUri);

            m_CheckVersionComplete = true;
            m_NeedUpdateVersion = FrameworkRoot.Resource.CheckVersionList(m_VersionInfo.InternalResourceVersion) == CheckVersionListResult.NeedUpdate;
        }

        private void OnWebRequestFailure(object sender, GameEventArgs e)
        {
            WebRequestFailureEventArgs ne = (WebRequestFailureEventArgs) e;
            if (ne.UserData != this)
            {
                return;
            }

            Log.Warning("Check version failure, error message is '{0}'.", ne.ErrorMessage);
        }

        /// <summary>
        /// 由 UnityEngine.RuntimePlatform 得到 平台标识符。
        /// </summary>
        /// <returns>平台标识符。</returns>
        private string GetPlatformPath()
        {
            // 这里和 GameBuildEventHandler.GetPlatformPath() 对应。
            // 使用 平台标识符 关联 UnityEngine.RuntimePlatform 和 UnityGameFramework.Editor.ResourceTools.Platform
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    return "Windows";

                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    return "MacOS";

                case RuntimePlatform.IPhonePlayer:
                    return "IOS";

                case RuntimePlatform.Android:
                    return "Android";

                case RuntimePlatform.WSAPlayerX64:
                case RuntimePlatform.WSAPlayerX86:
                case RuntimePlatform.WSAPlayerARM:
                    return "WSA";

                case RuntimePlatform.WebGLPlayer:
                    return "WebGL";

                case RuntimePlatform.LinuxEditor:
                case RuntimePlatform.LinuxPlayer:
                    return "Linux";

                default:
                    throw new System.NotSupportedException(Utility.Text.Format("Platform '{0}' is not supported.", Application.platform));
            }
        }
    }
}
