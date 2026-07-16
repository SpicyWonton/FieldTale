using Fantasy.Entitas;
using LightProto;
using MemoryPack;
using MongoDB.Bson.Serialization.Attributes;

namespace Fantasy;

public sealed class Player : Entity
{
    public string Name = string.Empty;

    public int MoveX;
    public int MoveY;
    public uint LastProcessedClientTick;
    public uint LastBroadcastClientTick;
    public uint ServerTick;
    public long NextMoveTickTimestamp;
    
    [BsonIgnore]
    [MemoryPackIgnore]
    [ProtoIgnore]
    public TransformComponent Transform;
}