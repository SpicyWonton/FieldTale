//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework;
using GameFramework.Event;
using System;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 显示实体成功事件。
    /// </summary>
    public sealed class ShowEntitySuccessEventArgs : GameEventArgs
    {
        /// <summary>
        /// 显示实体成功事件编号。
        /// </summary>
        public static readonly int EventId = typeof(ShowEntitySuccessEventArgs).GetHashCode();

        /// <summary>
        /// 初始化显示实体成功事件的新实例。
        /// </summary>
        public ShowEntitySuccessEventArgs()
        {
            EntityLogicType = null;
            Entity = null;
            Duration = 0f;
            UserData = null;
        }

        /// <summary>
        /// 获取显示实体成功事件编号。
        /// </summary>
        public override int Id
        {
            get
            {
                return EventId;
            }
        }

        /// <summary>
        /// 获取实体逻辑类型。
        /// </summary>
        public Type EntityLogicType
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取显示成功的实体。
        /// </summary>
        public Entity Entity
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取加载持续时间。
        /// </summary>
        public float Duration
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取用户自定义数据。
        /// </summary>
        public object UserData
        {
            get;
            private set;
        }

        /// <summary>
        /// 创建显示实体成功事件。
        /// </summary>
        /// <returns>创建的显示实体成功事件。</returns>
        internal static ShowEntitySuccessEventArgs Create(Type entityLogicType, Entity entity, float duration, object userData)
        {
            ShowEntitySuccessEventArgs e = ReferencePool.Acquire<ShowEntitySuccessEventArgs>();
            e.EntityLogicType = entityLogicType;
            e.Entity = entity;
            e.Duration = duration;
            e.UserData = userData;
            return e;
        }

        /// <summary>
        /// 清理显示实体成功事件。
        /// </summary>
        public override void Clear()
        {
            EntityLogicType = null;
            Entity = null;
            Duration = 0f;
            UserData = null;
        }
    }
}
