using System.Collections.Generic;
using Fantasy.Entitas;
using LightProto;
using MemoryPack;
using MongoDB.Bson.Serialization.Attributes;

namespace Fantasy;

// 一条客户端输入命令只允许服务端模拟一个 50 ms 逻辑帧。
public readonly struct InputCommand
{
    public InputCommand(uint tick, int moveX, int moveY)
    {
        Tick = tick;
        MoveX = moveX;
        MoveY = moveY;
    }

    public uint Tick { get; }
    public int MoveX { get; }
    public int MoveY { get; }
}

public sealed class Player : Entity
{
    public string Name = string.Empty;

    public int MoveX;
    public int MoveY;
    public uint LastReceivedInputTick;         // 已通过顺序校验并进入队列的最新逻辑帧。
    public uint LastProcessedInputTick;        // 已实际作用到权威位置的最新逻辑帧。
    public uint LastBroadcastInputTick;
    public uint ServerTick;
    public long NextTickTimestamp;

    [BsonIgnore]
    [MemoryPackIgnore]
    [ProtoIgnore]
    // 运行时输入队列不属于持久化状态，服务端逻辑帧按顺序逐条消费。
    public readonly Queue<InputCommand> PendingInputs = new();
    
    [BsonIgnore]
    [MemoryPackIgnore]
    [ProtoIgnore]
    public TransformComponent Transform;
}
