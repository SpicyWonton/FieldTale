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

            if (!PlayerIdMapper.TryGet(message.PlayerId, out int entityId))
            {
                UnityGameFramework.Runtime.Log.Warning($"[Network][M2C_PlayerLeave] PlayerId={message.PlayerId} is not mapped.");
                await FTask.CompletedTask;
                return;
            }

            bool hasEntity = FrameworkRoot.Entity.HasEntity(entityId);
            bool isLoading = FrameworkRoot.Entity.IsLoadingEntity(entityId);
            if (hasEntity || isLoading)
            {
                FrameworkRoot.Entity.HideEntity(entityId);
            }
            else
            {
                UnityGameFramework.Runtime.Log.Warning($"[Network][M2C_PlayerLeave] PlayerId={message.PlayerId}, ClientId={entityId} has no entity to hide.");
            }

            PlayerIdMapper.Remove(message.PlayerId);
            UnityGameFramework.Runtime.Log.Info($"[Network][M2C_PlayerLeave] PlayerId={message.PlayerId}, ClientId={entityId}, HasEntity={hasEntity}, IsLoading={isLoading}");

            await FTask.CompletedTask;
        }
    }
}
