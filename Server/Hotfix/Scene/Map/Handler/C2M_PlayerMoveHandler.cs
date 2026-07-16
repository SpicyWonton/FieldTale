using Fantasy.Async;
using Fantasy.Network.Interface;
using Fantasy.Network.Roaming;

namespace Fantasy;

public sealed class C2M_PlayerMoveHandler : Roaming<Player, C2M_PlayerMove>
{
    protected override async FTask Run(Player player, C2M_PlayerMove message)
    {
        if (message.ClientTick <= player.LastProcessedClientTick)
        {
            await FTask.CompletedTask;
            return;
        }

        player.LastProcessedClientTick = message.ClientTick;
        player.MoveX = Math.Clamp(message.MoveX, -1, 1);
        player.MoveY = Math.Clamp(message.MoveY, -1, 1);

        await FTask.CompletedTask;
    }
}