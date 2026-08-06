using Fantasy.Async;
using Fantasy.Network;
using Fantasy.Network.Interface;

namespace FieldTale.HotUpdate
{
    public sealed class M2C_PlayerCreateHandler : Message<Fantasy.M2C_PlayerCreate>
    {
        protected override async FTask Run(Session session, Fantasy.M2C_PlayerCreate message)
        {
            if (message == null || message.Player == null)
            {
                UnityGameFramework.Runtime.Log.Warning("[Network][M2C_PlayerCreate] message or Player is null.");
                await FTask.CompletedTask;
                return;
            }

            if (PlayerFactory.TryGet(message.Player.PlayerId, out Player existingPlayer))
            {
                UnityGameFramework.Runtime.Log.Warning($"[Network][M2C_PlayerCreate] Skip existing PlayerId={message.Player.PlayerId}, EntityId={existingPlayer.Id}");
                await FTask.CompletedTask;
                return;
            }

            Player player = PlayerFactory.Create(message.Player, message.IsSelf);
            UnityGameFramework.Runtime.Log.Info($"[Network][M2C_PlayerCreate] Created PlayerId={message.Player.PlayerId}, EntityId={player.Id}, ViewId={player.ClientEntityId}, IsSelf={message.IsSelf}");

            await FTask.CompletedTask;
        }
    }
}
