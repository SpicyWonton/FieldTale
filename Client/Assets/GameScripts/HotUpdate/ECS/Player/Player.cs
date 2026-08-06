namespace FieldTale.HotUpdate
{
    /// <summary>
    /// 客户端玩家实体，保存稳定标识并组合玩家相关组件。
    /// </summary>
    public sealed class Player : Fantasy.Entitas.Entity
    {
        /// <summary>
        /// 服务端玩家实体 ID，用于匹配网络消息。
        /// </summary>
        public long ServerEntityId;

        /// <summary>
        /// UGF 表现实体 ID，与 Fantasy ECS 的 Entity.Id 相互独立。
        /// </summary>
        public int ClientEntityId;

        /// <summary>
        /// 是否为当前客户端控制的玩家。
        /// </summary>
        public bool IsSelf;

        /// <summary>
        /// 异步加载完成后绑定的 Unity 表现对象。
        /// </summary>
        public PlayerView View;

        /// <summary>
        /// 玩家位姿数据。
        /// </summary>
        public PlayerTransformComponent Transform;

        /// <summary>
        /// 本地移动预测与输入确认数据。
        /// </summary>
        public PlayerMovementComponent Movement;

        /// <summary>
        /// 服务端快照接收与远端插值数据。
        /// </summary>
        public PlayerSnapshotComponent Snapshots;
    }
}
