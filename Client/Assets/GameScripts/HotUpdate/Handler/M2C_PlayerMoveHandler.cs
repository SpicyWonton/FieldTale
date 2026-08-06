using Fantasy.Async;
using Fantasy.Network;
using Fantasy.Network.Interface;
using UnityEngine;

namespace FieldTale.HotUpdate
{
    public sealed class M2C_PlayerMoveHandler : Message<Fantasy.M2C_PlayerMove>
    {
        protected override async FTask Run(Session session, Fantasy.M2C_PlayerMove message)
        {
            if (message == null || message.Pos == null)
            {
                UnityGameFramework.Runtime.Log.Warning("[Network][M2C_PlayerMove] message or Pos is null.");
                await FTask.CompletedTask;
                return;
            }

            if (!PlayerFactory.TryGet(message.PlayerId, out Player player))
            {
                UnityGameFramework.Runtime.Log.Warning($"[Network][M2C_PlayerMove] PlayerId={message.PlayerId} is not in the client ECS world.");
                await FTask.CompletedTask;
                return;
            }

            PlayerSnapshotComponent snapshots = player.Snapshots;
            if (message.ServerTick > snapshots.LastQueuedServerTick)
            {
                snapshots.LastQueuedServerTick = message.ServerTick;
                snapshots.Incoming.Add(new PlayerNetworkSnapshot(
                    message.ServerTick,
                    message.LastProcessedClientTick,
                    new Vector2(message.Pos.X, message.Pos.Y)));
            }

            await FTask.CompletedTask;
        }
    }
}
