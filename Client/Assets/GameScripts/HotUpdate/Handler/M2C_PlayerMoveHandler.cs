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
            if (message?.Pos == null)
            {
                UnityGameFramework.Runtime.Log.Info("[Network][M2C_PlayerMove] message or Pos is null.");
            }
            else
            {
                UnityGameFramework.Runtime.Log.Info($"[Network][M2C_PlayerMove] PlayerId={message.PlayerId}, Pos=({message.Pos.X}, {message.Pos.Y}, {message.Pos.Z})");
            }
            if (message == null || message.Pos == null)
            {
                await FTask.CompletedTask;
                return;
            }

            int playerId = PlayerIdMapper.GetOrCreate(message.PlayerId);
            UnityGameFramework.Runtime.Entity entity = FrameworkRoot.Entity.GetEntity(playerId);
            Player player = entity == null ? null : entity.Logic as Player;
            if (player != null)
            {
                player.SetNetworkPosition(new Vector3(message.Pos.X, message.Pos.Y, message.Pos.Z));
            }

            await FTask.CompletedTask;
        }
    }
}