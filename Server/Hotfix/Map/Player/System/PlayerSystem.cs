using Fantasy.Entitas.Interface;

namespace Fantasy;

public sealed class PlayerDestroySystem : DestroySystem<Player>
{
    protected override void Destroy(Player self)
    {
        
    }
}

public static class PlayerSystem
{
    public static PlayerInfo ToProtocol(this Player self, bool autoReturn)
    {
        var playerInfo = PlayerInfo.Create(autoReturn);

        playerInfo.PlayerId = self.Id;
        playerInfo.Name = self.Name;
        playerInfo.Pos = self.Transform.ToProtocol();

        return playerInfo;
    }
}