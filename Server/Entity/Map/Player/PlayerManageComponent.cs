using Fantasy.Entitas;

namespace Fantasy;

public sealed class PlayerManageComponent : Entity
{
    public readonly Dictionary<long, Player> Players = new Dictionary<long, Player>();
}