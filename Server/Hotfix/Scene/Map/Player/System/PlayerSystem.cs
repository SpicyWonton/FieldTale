using Fantasy.Entitas.Interface;

namespace Fantasy;

public sealed class PlayerDestroySystem : DestroySystem<Player>
{
    protected override void Destroy(Player self)
    {
        var playerManageComponent = self.Scene.GetComponent<PlayerManageComponent>();
        if (playerManageComponent == null || playerManageComponent.IsDisposed)
        {
            return;
        }

        // Player可能由漫游超时直接销毁，此时也必须从场景管理器移除并通知其他玩家。
        PlayerManageHelper.RemovePlayer(self.Scene, self.Id, isDispose: false);
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
