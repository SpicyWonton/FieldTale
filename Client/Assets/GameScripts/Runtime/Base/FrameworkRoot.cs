using UnityEngine;

namespace FieldTale
{
    public partial class FrameworkRoot : MonoBehaviour
    {
        private void Start()
        {
            InitBuiltinComponents();
            InitCustomComponents();
        }
    }
}