//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using UnityEngine;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 默认界面组辅助器。
    /// </summary>
    public class DefaultUIGroupHelper : UIGroupHelperBase
    {
        private Canvas m_Canvas;

        private void Awake()
        {
            m_Canvas = GetComponent<Canvas>();
            if (m_Canvas == null)
            {
                m_Canvas = gameObject.AddComponent<Canvas>();
            }

            m_Canvas.overrideSorting = true;
            m_Canvas.sortingLayerName = "UI";
        }

        /// <summary>
        /// 设置界面组深度。
        /// </summary>
        /// <param name="depth">界面组深度。</param>
        public override void SetDepth(int depth)
        {
            if (m_Canvas == null)
            {
                m_Canvas = GetComponent<Canvas>();
            }

            if (m_Canvas == null)
            {
                m_Canvas = gameObject.AddComponent<Canvas>();
            }

            m_Canvas.overrideSorting = true;
            m_Canvas.sortingLayerName = "UI";
            m_Canvas.sortingOrder = depth;
        }
    }
}
