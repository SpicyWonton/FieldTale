using Fantasy.Async;
using Fantasy.Network.Interface;
using Fantasy.Network.Roaming;

namespace Fantasy;

public sealed class G2M_OfflineRequestHandler : RoamingRPC<Player, G2M_OfflineRequest, M2G_OfflineResponse>
{
    protected override async FTask Run(Player player, G2M_OfflineRequest request, M2G_OfflineResponse response, Action reply)
    {
        // OfflineTime is reserved for delayed logout. The current Gate flow sends 0,
        // so remove the player immediately and let the RPC base send the response.
        PlayerManageHelper.RemovePlayer(player.Scene, player.Id);
        await FTask.CompletedTask;
    }
}
