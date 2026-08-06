using Fantasy.Async;
using Fantasy.Network;
using Fantasy.Network.Interface;

namespace FieldTale.HotUpdate
{
    public sealed class M2C_PlayerLeaveHandler : Message<Fantasy.M2C_PlayerLeave>
    {
        protected override async FTask Run(Session session, Fantasy.M2C_PlayerLeave message)
        {
            if (message == null)
            {
                UnityGameFramework.Runtime.Log.Warning("[Network][M2C_PlayerLeave] message is null.");
                await FTask.CompletedTask;
                return;
            }

            if (!PlayerFactory.TryGet(message.PlayerId, out Player player))
            {
                UnityGameFramework.Runtime.Log.Warning($"[Network][M2C_PlayerLeave] PlayerId={message.PlayerId} is not in the client ECS world.");
                await FTask.CompletedTask;
                return;
            }

            int viewEntityId = player.ClientEntityId;
            player.Dispose();
            UnityGameFramework.Runtime.Log.Info($"[Network][M2C_PlayerLeave] Disposed PlayerId={message.PlayerId}, ViewId={viewEntityId}");

            await FTask.CompletedTask;
        }
    }
}
