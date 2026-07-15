using Fantasy.Entitas;
using MemoryPack;

namespace Fantasy;

[MemoryPackable]
public sealed partial class AccountLinkArgs : Entity
{
    public string AccountName = string.Empty;
}
