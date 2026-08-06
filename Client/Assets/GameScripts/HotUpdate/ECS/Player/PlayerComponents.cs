using System.Collections.Generic;
using UnityEngine;

namespace FieldTale.HotUpdate
{
    /// <summary>
    /// 已发送但尚未被服务端确认的本地输入。
    /// </summary>
    public readonly struct ClientInputCommand
    {
        public ClientInputCommand(uint tick, Vector2 input)
        {
            Tick = tick;
            Input = input;
        }

        public uint Tick { get; }
        public Vector2 Input { get; }
    }

    /// <summary>
    /// 服务端下发的玩家权威状态。
    /// </summary>
    public readonly struct PlayerNetworkSnapshot
    {
        public PlayerNetworkSnapshot(uint serverTick, uint processedClientTick, Vector2 position)
        {
            ServerTick = serverTick;
            ProcessedClientTick = processedClientTick;
            Position = position;
        }

        public uint ServerTick { get; }
        public uint ProcessedClientTick { get; }
        public Vector2 Position { get; }
    }

    /// <summary>
    /// 玩家逻辑位姿以及待平滑应用的位置修正量。
    /// </summary>
    public sealed class PlayerTransformComponent : Fantasy.Entitas.Entity
    {
        public Vector2 Position;
        public Quaternion Rotation = Quaternion.identity;

        /// <summary>
        /// 客户端预测位置与服务端权威位置之间尚未应用的偏差。
        /// </summary>
        public Vector2 Correction;
    }

    /// <summary>
    /// 本地移动模拟、输入 Tick 和服务端确认状态。
    /// </summary>
    public sealed class PlayerMovementComponent : Fantasy.Entitas.Entity
    {
        public float Speed;

        /// <summary>
        /// 当前逻辑 Tick 已累计的时间。
        /// </summary>
        public float TickTimer;
        public bool InputInitialized;

        /// <summary>
        /// 当前渲染帧采样到的输入。
        /// </summary>
        public Vector2 FrameInput;

        /// <summary>
        /// 当前逻辑 Tick 使用的固定输入。
        /// </summary>
        public Vector2 TickInput;
        public uint ClientTick;
        public uint LastServerTick;

        /// <summary>
        /// 已发送、等待服务端确认的输入，用于收到权威位置后重放。
        /// </summary>
        public readonly List<ClientInputCommand> PendingInputs = new List<ClientInputCommand>();
    }

    /// <summary>
    /// 网络线程投递的快照以及远端玩家的渲染缓冲区。
    /// </summary>
    public sealed class PlayerSnapshotComponent : Fantasy.Entitas.Entity
    {
        /// <summary>
        /// 远端玩家当前渲染到的服务端 Tick。
        /// </summary>
        public double RenderTick;
        public uint LastQueuedServerTick;

        /// <summary>
        /// Handler 写入、UpdateSystem 消费的快照队列。
        /// </summary>
        public readonly List<PlayerNetworkSnapshot> Incoming = new List<PlayerNetworkSnapshot>();

        /// <summary>
        /// 按服务端 Tick 排序的远端插值缓冲区。
        /// </summary>
        public readonly List<PlayerNetworkSnapshot> RenderBuffer = new List<PlayerNetworkSnapshot>();
    }

    /// <summary>
    /// 当前 Fantasy Scene 中的玩家索引以及 UGF 表现 ID 分配状态。
    /// </summary>
    public sealed class PlayerManageComponent : Fantasy.Entitas.Entity
    {
        public int NextClientEntityId = 1;
        public readonly Dictionary<long, Player> Players = new Dictionary<long, Player>();
    }
}
