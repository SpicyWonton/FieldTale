using Fantasy.Entitas;
using LightProto;
using MemoryPack;
using MongoDB.Bson.Serialization.Attributes;

namespace Fantasy;

public sealed class Player : Entity
{
    public string Name = string.Empty;
    
    [BsonIgnore]
    [MemoryPackIgnore]
    [ProtoIgnore]
    public TransformComponent Transform;
}