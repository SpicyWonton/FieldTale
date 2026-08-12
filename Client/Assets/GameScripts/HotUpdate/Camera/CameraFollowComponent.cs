using UnityEngine;

namespace FieldTale.HotUpdate
{
    /// <summary>
    /// Follows the current local player view in the 2D game camera.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class CameraFollowComponent : MonoBehaviour
    {
        private static Transform s_Target;

        [SerializeField]
        private Vector3 m_Offset = new Vector3(0f, 0f, -10f);

        private bool m_HasPosition;

        public static void SetTarget(Transform target)
        {
            s_Target = target;
        }

        public static void ClearTarget(Transform target)
        {
            if (ReferenceEquals(s_Target, target))
            {
                s_Target = null;
            }
        }

        private void OnEnable()
        {
            m_HasPosition = false;
        }

        private void OnDisable()
        {
            m_HasPosition = false;
        }

        private void LateUpdate()
        {
            if (s_Target == null)
            {
                m_HasPosition = false;
                return;
            }

            Vector3 targetPosition = s_Target.position + m_Offset;
            if (!m_HasPosition)
            {
                transform.position = targetPosition;
                m_HasPosition = true;
                return;
            }

            transform.position = targetPosition;
        }
    }
}
