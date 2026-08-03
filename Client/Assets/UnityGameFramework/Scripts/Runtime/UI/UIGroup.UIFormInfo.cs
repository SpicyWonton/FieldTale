//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework;

namespace UnityGameFramework.Runtime
{
    public sealed partial class UIGroup
    {
        /// <summary>
        /// 界面组界面信息。
        /// </summary>
        private sealed class UIFormInfo : IReference
        {
            private UIForm m_UIForm;
            private bool m_Paused;
            private bool m_Covered;

            public UIFormInfo()
            {
                m_UIForm = null;
                m_Paused = false;
                m_Covered = false;
            }

            public UIForm UIForm
            {
                get
                {
                    return m_UIForm;
                }
            }

            public bool Paused
            {
                get
                {
                    return m_Paused;
                }
                set
                {
                    m_Paused = value;
                }
            }

            public bool Covered
            {
                get
                {
                    return m_Covered;
                }
                set
                {
                    m_Covered = value;
                }
            }

            public static UIFormInfo Create(UIForm uiForm)
            {
                if (uiForm == null)
                {
                    throw new GameFrameworkException("UI form is invalid.");
                }

                UIFormInfo uiFormInfo = ReferencePool.Acquire<UIFormInfo>();
                uiFormInfo.m_UIForm = uiForm;
                uiFormInfo.m_Paused = true;
                uiFormInfo.m_Covered = true;
                return uiFormInfo;
            }

            public void Clear()
            {
                m_UIForm = null;
                m_Paused = false;
                m_Covered = false;
            }
        }
    }
}
