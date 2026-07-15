using Fantasy.Async;
using Fantasy.Network;
using Fantasy.Network.Interface;
using UnityEngine;

namespace FieldTale.HotUpdate
{
    public sealed class M2C_PlayerCreateHandler : Message<Fantasy.M2C_PlayerCreate>
    {
        protected override async FTask Run(Session session, Fantasy.M2C_PlayerCreate message)
        {
            if (message?.Player == null)
            {
                UnityGameFramework.Runtime.Log.Info("[Network][M2C_PlayerCreate] message or Player is null.");
            }
            else
            {
                UnityGameFramework.Runtime.Log.Info($"[Network][M2C_PlayerCreate] PlayerId={message.Player.PlayerId}, IsSelf={message.IsSelf}, Pos=({message.Player.Pos?.X ?? 0f}, {message.Player.Pos?.Y ?? 0f}, {message.Player.Pos?.Z ?? 0f})");
            }
            if (message == null || message.Player == null)
            {
                await FTask.CompletedTask;
                return;
            }

            int playerId = PlayerIdMapper.GetOrCreate(message.Player.PlayerId);
            bool hasEntity = FrameworkRoot.Entity.HasEntity(playerId);
            bool isLoading = FrameworkRoot.Entity.IsLoadingEntity(playerId);
            if (hasEntity || isLoading)
            {
                UnityGameFramework.Runtime.Log.Info($"[Network][M2C_PlayerCreate] Skip PlayerId={message.Player.PlayerId}, ClientId={playerId}, IsSelf={message.IsSelf}, HasEntity={hasEntity}, IsLoading={isLoading}");
                await FTask.CompletedTask;
                return;
            }

            Fantasy.PlayerInfo playerInfo = message.Player;
            PlayerData playerData = new PlayerData(playerId, 1, 10f, message.IsSelf);

            if (playerInfo.Pos != null)
            {
                playerData.Position = new Vector3(playerInfo.Pos.X, playerInfo.Pos.Y, playerInfo.Pos.Z);
            }

            UnityGameFramework.Runtime.Log.Info($"[Network][M2C_PlayerCreate] ShowEntity PlayerId={message.Player.PlayerId}, ClientId={playerId}, IsSelf={message.IsSelf}");
            FrameworkRoot.Entity.ShowEntity(playerId, typeof(Player), "Assets/GameRes/Entities/Player.prefab", "Entity", 0, playerData);

            await FTask.CompletedTask;
        }
    }
}