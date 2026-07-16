using Fantasy.Async;
using Fantasy.Network.Interface;
using Fantasy.Network.Roaming;

namespace Fantasy;

public sealed class C2M_PlayerMoveHandler : Roaming<Player, C2M_PlayerMove>
{
    protected override async FTask Run(Player player, C2M_PlayerMove message)
    {
        if (message.Pos == null)
        {
            await FTask.CompletedTask;
            return;
        }

        message.Pos.Transform(ref player.Transform.Position);

        var moveMessage = M2C_PlayerMove.Create(false);
        moveMessage.PlayerId = player.Id;
        moveMessage.Pos = Position.Create();
        moveMessage.Pos.Transform(player.Transform.Position);

        try
        {
            var players = player.Scene.GetComponent<PlayerManageComponent>().Players;
            foreach (var (playerId, otherPlayer) in players)
            {
                if (playerId == player.Id || !otherPlayer.TryGetLinkTerminus(out var linkTerminus))
                {
                    continue;
                }

                linkTerminus.Send(moveMessage);
            }
        }
        finally
        {
            moveMessage.Return();
        }

        await FTask.CompletedTask;
    }
}