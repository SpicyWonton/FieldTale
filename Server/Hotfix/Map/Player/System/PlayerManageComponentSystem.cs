using Fantasy.Entitas.Interface;

namespace Fantasy;

public sealed class PlayerManageComponentDestroySystem : DestroySystem<PlayerManageComponent>
{
    protected override void Destroy(PlayerManageComponent self)
    {
        foreach (var (_, player) in self.Players)
        {
            player.Dispose();
        }

        self.Players.Clear();
    }
}

public static class PlayerManageComponentSystem
{
    public static bool Add(this PlayerManageComponent self, Player player)
    {
        if (!self.Players.TryAdd(player.Id, player))
        {
            return false;
        }

        return true;
    }

    public static bool Remove(this PlayerManageComponent self, long playerId, bool isDispose = true)
    {
        if (!self.Players.Remove(playerId, out var player))
        {
            return false;
        }

        if (isDispose)
        {
            player.Dispose();
        }
        
        return true;
    }

    public static bool TryGetPlayer(this PlayerManageComponent self, long playerId, out Player player)
    {
        return self.Players.TryGetValue(playerId, out player!);
    }
}