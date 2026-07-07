using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityGameFramework.Runtime;

namespace FieldTale.HotUpdate
{
    public class ProcedurePreload : ProcedureBase
    {
        private bool m_LoadDataTablesComplete = false;

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

            m_LoadDataTablesComplete = false;

            PreloadResources();
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (!m_LoadDataTablesComplete)
            {
                return;
            }

            ChangeState<ProcedureChangeScene>(procedureOwner);
        }

        private void PreloadResources()
        {
            DataTableManager.LoadTables(OnLoadDataTablesComplete);
        }

        private void OnLoadDataTablesComplete(bool success)
        {
            if (!success)
            {
                Log.Error("Load Luban data tables failed.");
                return;
            }

            Log.Info("Load Luban data tables OK.");
            m_LoadDataTablesComplete = true;
        }
    }
}
