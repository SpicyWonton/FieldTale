using GameFramework.Event;
using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityGameFramework.Runtime;

namespace FieldTale.HotUpdate
{
    public class ProcedureLogin : ProcedureBase
    {
        private LoginForm m_LoginForm;

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

            m_LoginForm = null;
            FrameworkRoot.Event.Subscribe(OpenUIFormSuccessEventArgs.EventId, OnOpenUIFormSuccess);
            FrameworkRoot.UI.OpenUIForm(AssetUtility.GetUIAsset("Login", "LoginForm"), "Default", this);
        }

        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            FrameworkRoot.Event.Unsubscribe(OpenUIFormSuccessEventArgs.EventId, OnOpenUIFormSuccess);
            m_LoginForm = null;

            base.OnLeave(procedureOwner, isShutdown);
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (m_LoginForm == null || !m_LoginForm.LoginSuccess)
            {
                return;
            }

            procedureOwner.SetData<VarInt32>("NextSceneId", 1002);
            ChangeState<ProcedureChangeScene>(procedureOwner);
        }

        private void OnOpenUIFormSuccess(object sender, GameEventArgs e)
        {
            OpenUIFormSuccessEventArgs ne = (OpenUIFormSuccessEventArgs)e;
            if (!ReferenceEquals(ne.UserData, this))
            {
                return;
            }

            m_LoginForm = ne.UIForm.Logic as LoginForm;
            if (m_LoginForm == null)
            {
                Log.Error("Login UI form logic is invalid.");
            }
        }
    }
}
