using Fantasy.Async;
using Fantasy.Network.Interface;
using Fantasy.Network.Roaming;

namespace Fantasy;

public sealed class G2M_OfflineRequestHandler : RoamingRPC<Player, G2M_OfflineRequest, M2G_OfflineResponse>
{
    protected override async FTask Run(Player player, G2M_OfflineRequest request, M2G_OfflineResponse response, Action reply)
    {
        // Remove the player from the scene immediately, but keep the linked entity alive.
        // The roaming lifecycle must stop forwarding before it disposes the Terminus and Player.
        PlayerManageHelper.RemovePlayer(player.Scene, player.Id, isDispose: false);
        await FTask.CompletedTask;
    }
}
