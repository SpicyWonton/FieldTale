using GameFramework;
using GameFramework.Fsm;
using GameFramework.Procedure;

namespace FieldTale.HotUpdate
{
    public static class HotUpdateEntry
    {
        public static void Start()
        {
#if UNITY_EDITOR
            LaunchHotUpdate();
#else
            LaunchHotUpdate();
#endif
        }

        private static void LaunchHotUpdate()
        {
            // 重置流程组件，初始化热更新流程。
            FrameworkRoot.Fsm.DestroyFsm<IProcedureManager>();
            var procedureManager = GameFrameworkEntry.GetModule<IProcedureManager>();
            ProcedureBase[] procedures =
            {
                new ProcedureChangeScene(),
                new ProcedureMain(),
                new ProcedurePreload(),
                new ProcedureLogin(),
            };
            procedureManager.Initialize(GameFrameworkEntry.GetModule<IFsmManager>(), procedures);
            procedureManager.StartProcedure<ProcedurePreload>();
        }
    }
}
