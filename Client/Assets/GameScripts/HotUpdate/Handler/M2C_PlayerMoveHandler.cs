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

            int playerId = PlayerIdMapper.GetOrCreate(message.PlayerId);
            UnityGameFramework.Runtime.Entity entity = FrameworkRoot.Entity.GetEntity(playerId);
            Player player = entity == null ? null : entity.Logic as Player;
            if (player != null)
            {
                player.ReceiveNetworkSnapshot(
                    new Vector3(message.Pos.X, message.Pos.Y, message.Pos.Z),
                    message.ServerTick,
                    message.LastProcessedClientTick);
            }

            await FTask.CompletedTask;
        }
    }
}