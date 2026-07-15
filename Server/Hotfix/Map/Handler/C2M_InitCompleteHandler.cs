using Fantasy.Async;
using Fantasy.Network.Interface;
using Fantasy.Network.Roaming;

namespace Fantasy;

public sealed class C2M_InitCompleteHandler : Roaming<Player, C2M_InitComplete>
{
    protected override async FTask Run(Player player, C2M_InitComplete message)
    {
        if (!player.TryGetLinkTerminus(out var linkTerminus))
        {
            Log.Error($"PlayerUnit:{player.Id} not link terminus");
            return;
        }

        var scene = player.Scene;
        var playerId = player.Id;
        using var playerInfo = player.ToProtocol(false);
        var playerManageComponent = player.Scene.GetComponent<PlayerManageComponent>();
        
        // 1. 同步场景中其他单位给新玩家
        PlayerManageHelper.SyncOtherPlayers(linkTerminus, player);
        // 2. 发送自己的单位给客户端
        PlayerManageHelper.SendPlayerCreate(linkTerminus, player, true);
        // 3. 将新玩家广播给场景中的其他人
        PlayerManageHelper.BroadcastPlayerCreate(scene, playerInfo, playerId, playerManageComponent);
        
        await FTask.CompletedTask;
    }
}