//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework;
using UnityEditor;
using UnityGameFramework.Runtime;

namespace UnityGameFramework.Editor
{
    [CustomEditor(typeof(EntityComponent))]
    internal sealed class EntityComponentInspector : GameFrameworkInspector
    {
        private SerializedProperty m_EnableShowEntityUpdateEvent = null;
        private SerializedProperty m_EnableShowEntityDependencyAssetEvent = null;
        private SerializedProperty m_InstanceRoot = null;
        private SerializedProperty m_EntityGroupConfigs = null;

        private HelperInfo<EntityHelperBase> m_EntityHelperInfo = new HelperInfo<EntityHelperBase>("Entity");

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            EntityComponent t = (EntityComponent)target;

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                EditorGUILayout.PropertyField(m_EnableShowEntityUpdateEvent);
                EditorGUILayout.PropertyField(m_EnableShowEntityDependencyAssetEvent);
                EditorGUILayout.PropertyField(m_InstanceRoot);
                m_EntityHelperInfo.Draw();
                EditorGUILayout.PropertyField(m_EntityGroupConfigs, true);
            }
            EditorGUI.EndDisabledGroup();

            if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
            {
                EditorGUILayout.LabelField("Entity Group Count", t.EntityGroupCount.ToString());
                EditorGUILayout.LabelField("Entity Count (Total)", t.EntityCount.ToString());
                EntityGroup[] entityGroups = t.GetAllEntityGroups();
                foreach (EntityGroup entityGroup in entityGroups)
                {
                    EditorGUILayout.LabelField(Utility.Text.Format("Entity Count ({0})", entityGroup.Name), entityGroup.EntityCount.ToString());
                }
            }

            serializedObject.ApplyModifiedProperties();

            Repaint();
        }

        protected override void OnCompileComplete()
        {
            base.OnCompileComplete();

            RefreshTypeNames();
        }

        private void OnEnable()
        {
            m_EnableShowEntityUpdateEvent = serializedObject.FindProperty("m_EnableShowEntityUpdateEvent");
            m_EnableShowEntityDependencyAssetEvent = serializedObject.FindProperty("m_EnableShowEntityDependencyAssetEvent");
            m_InstanceRoot = serializedObject.FindProperty("m_InstanceRoot");
            m_EntityGroupConfigs = serializedObject.FindProperty("m_EntityGroupConfigs");

            m_EntityHelperInfo.Init(serializedObject);

            RefreshTypeNames();
        }

        private void RefreshTypeNames()
        {
            m_EntityHelperInfo.Refresh();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
