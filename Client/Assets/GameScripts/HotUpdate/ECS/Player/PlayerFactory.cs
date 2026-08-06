using System.Collections.Generic;
using UnityEngine;

namespace FieldTale.HotUpdate
{
    /// <summary>
    /// 负责创建玩家 ECS 实体、维护玩家索引并请求加载对应表现实体。
    /// </summary>
    public static class PlayerFactory
    {
        private const float PlayerSpeed = 10f;
        private const string PlayerAssetName = "Assets/GameRes/Entities/Player.prefab";
        private const string PlayerGroupName = "Entity";

        /// <summary>
        /// 根据服务端玩家信息创建完整的客户端玩家实体。
        /// </summary>
        public static Player Create(Fantasy.PlayerInfo playerInfo, bool isSelf)
        {
            PlayerManageComponent manager = GetOrCreateManager();
            if (manager.Players.TryGetValue(playerInfo.PlayerId, out Player existingPlayer))
            {
                return existingPlayer;
            }

            Player player = Fantasy.Entitas.Entity.Create<Player>(Fantasy.Runtime.Scene, false);
            player.ServerEntityId = playerInfo.PlayerId;
            player.ClientEntityId = AllocateClientEntityId(manager);
            player.IsSelf = isSelf;

            player.Transform = player.AddComponent<PlayerTransformComponent>(false);
            if (playerInfo.Pos != null)
            {
                player.Transform.Position = new Vector2(playerInfo.Pos.X, playerInfo.Pos.Y);
            }

            player.Movement = player.AddComponent<PlayerMovementComponent>(false);
            player.Movement.Speed = PlayerSpeed;
            player.Movement.FrameInput = Vector2.zero;
            player.Movement.TickInput = Vector2.zero;

            player.Snapshots = player.AddComponent<PlayerSnapshotComponent>(false);

            manager.Players.Add(player.ServerEntityId, player);

            // PlayerView 可能异步加载，直接传递 Player 以便 OnShow 时完成双向绑定。
            FrameworkRoot.Entity.ShowEntity(
                player.ClientEntityId,
                typeof(PlayerView),
                PlayerAssetName,
                PlayerGroupName,
                0,
                player);

            return player;
        }

        /// <summary>
        /// 通过服务端实体 ID 查找仍然有效的客户端玩家实体。
        /// </summary>
        public static bool TryGet(long serverPlayerId, out Player player)
        {
            PlayerManageComponent manager = Fantasy.Runtime.Scene.GetComponent<PlayerManageComponent>();
            if (manager == null || manager.IsDisposed)
            {
                player = null;
                return false;
            }

            if (!manager.Players.TryGetValue(serverPlayerId, out player))
            {
                return false;
            }

            if (player != null && !player.IsDisposed)
            {
                return true;
            }

            manager.Players.Remove(serverPlayerId);
            player = null;
            return false;
        }

        /// <summary>
        /// 销毁当前场景中的所有玩家，场景切换前调用。
        /// </summary>
        public static void DisposeAll()
        {
            PlayerManageComponent manager;
            try
            {
                manager = Fantasy.Runtime.Scene.GetComponent<PlayerManageComponent>();
            }
            catch (System.InvalidOperationException)
            {
                return;
            }

            if (manager == null || manager.IsDisposed || manager.Players.Count == 0)
            {
                return;
            }

            List<Player> players = new List<Player>(manager.Players.Values);
            for (int i = 0; i < players.Count; i++)
            {
                Player player = players[i];
                if (player != null && !player.IsDisposed)
                {
                    player.Dispose();
                }
            }

            manager.Players.Clear();
        }

        /// <summary>
        /// 从玩家索引中移除实体，由销毁系统调用。
        /// </summary>
        public static void RemoveFromManager(Player player)
        {
            PlayerManageComponent manager = Fantasy.Runtime.Scene.GetComponent<PlayerManageComponent>();
            if (manager != null && !manager.IsDisposed)
            {
                manager.Players.Remove(player.ServerEntityId);
            }
        }

        private static PlayerManageComponent GetOrCreateManager()
        {
            PlayerManageComponent manager = Fantasy.Runtime.Scene.GetComponent<PlayerManageComponent>();
            return manager ?? Fantasy.Runtime.Scene.AddComponent<PlayerManageComponent>(false);
        }

        /// <summary>
        /// 分配未被 UGF 已加载或加载中实体占用的表现 ID。
        /// </summary>
        private static int AllocateClientEntityId(PlayerManageComponent manager)
        {
            int entityId;
            do
            {
                entityId = manager.NextClientEntityId++;
                if (manager.NextClientEntityId <= 0)
                {
                    manager.NextClientEntityId = 1;
                }
            }
            while (FrameworkRoot.Entity.HasEntity(entityId) || FrameworkRoot.Entity.IsLoadingEntity(entityId));

            return entityId;
        }
    }
}
