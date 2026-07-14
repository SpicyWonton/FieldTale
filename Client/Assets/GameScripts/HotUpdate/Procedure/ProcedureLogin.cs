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

            FrameworkRoot.UI.OpenUIForm(AssetUtility.GetUIAsset("Login", "LoginForm"), "Default");
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (!m_LoginForm.LoginSuccess)
            {
                return;
            }

            procedureOwner.SetData<VarInt32>("NextSceneId", 1002);
            ChangeState<ProcedureChangeScene>(procedureOwner);
        }
    }
}
