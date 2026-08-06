using GameFramework;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace FieldTale.HotUpdate
{
    /// <summary>
    /// UGF 实体表现基类，缓存通用 Unity 组件并初始化基础显示状态。
    /// </summary>
    [RequireComponent(typeof(Animation))]
    public abstract class EntityView : EntityLogic
    {
        public int Id => Entity.Id;

        public Animation CachedAnimation { get; private set; }

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            CachedAnimation = GetComponent<Animation>();
        }

        protected override void OnShow(object userData)
        {
            Name = Utility.Text.Format("[Entity {0}]", Id);
            CachedTransform.localScale = Vector3.one;

            // 所有显示状态准备完成后再激活对象，避免对象池中的旧状态短暂可见。
            base.OnShow(userData);
        }
    }
}
