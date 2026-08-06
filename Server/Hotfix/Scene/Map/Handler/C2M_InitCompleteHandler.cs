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

        // A rebuilt client starts its input sequence at tick 1. Clear movement state left by
        // the previous connection so those new inputs are not rejected as stale frames.
        player.MoveX = 0;
        player.MoveY = 0;
        player.LastReceivedInputTick = 0;
        player.LastProcessedInputTick = 0;
        player.LastBroadcastInputTick = 0;
        player.NextTickTimestamp = 0;
        player.PendingInputs.Clear();

        var scene = player.Scene;
        var playerId = player.Id;
        using var playerInfo = player.ToProtocol(false);
        var playerManageComponent = player.Scene.GetComponent<PlayerManageComponent>();
        
        // 1. 同步场景中其他单位给新玩家
        // Send the local player first so its entity starts loading before remote players.
        PlayerManageHelper.SendPlayerCreate(linkTerminus, player, true);
        // Sync the other players after the local player has been announced.
        PlayerManageHelper.SyncOtherPlayers(linkTerminus, player);
        // 3. 将新玩家广播给场景中的其他人
        PlayerManageHelper.BroadcastPlayerCreate(scene, playerInfo, playerId, playerManageComponent);
        
        await FTask.CompletedTask;
    }
}