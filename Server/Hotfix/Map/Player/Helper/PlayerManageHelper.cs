using System.Runtime.CompilerServices;
using Fantasy.Network.Interface;
using Fantasy.Network.Roaming;
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

namespace Fantasy;

/// <summary>
/// 玩家单位管理辅助类，提供玩家单位的添加、同步和广播功能
/// </summary>
public static class PlayerManageHelper
{
    /// <summary>
    /// 添加玩家单位时的通知方式
    /// </summary>
    public enum EAddPlayerNotify
    {
        /// <summary>
        /// 不通知
        /// </summary>
        NoNotification,
        /// <summary>
        /// 通知自己
        /// </summary>
        SyncSelf,
        /// <summary>
        /// 通知所有人
        /// </summary>
        SyncEveryone
    }
    
    /// <summary>
    /// 移除玩家单位时的通知方式
    /// </summary>
    public enum ERemovePlayerNotify
    {
        /// <summary>
        /// 不通知
        /// </summary>
        NoNotification,
        /// <summary>
        /// 通知自己
        /// </summary>
        SyncSelf,
        /// <summary>
        /// 通知所有人
        /// </summary>
        SyncEveryone
    }

    /// <summary>
    /// 添加玩家单位到场景管理组件，并根据通知方式同步给客户端
    /// </summary>
    /// <param name="player">要添加的玩家单位</param>
    /// <param name="notify">通知方式：不通知、仅通知自己、通知所有人</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddPlayer(Player player, EAddPlayerNotify notify)
    {
        var playerManageComponent = player.Scene.GetComponent<PlayerManageComponent>();
        playerManageComponent.Add(player);

        switch (notify)
        {
            case EAddPlayerNotify.NoNotification:
            {
                return;
            }
            case EAddPlayerNotify.SyncSelf:
            {
                if (!player.TryGetLinkTerminus(out var linkTerminus))
                {
                    return;
                }

                var playerCreateMessage = M2C_PlayerCreate.Create();
                playerCreateMessage.Player = player.ToProtocol(true);
                playerCreateMessage.IsSelf = true;
                linkTerminus.Send(playerCreateMessage);
                return;
            }
            case EAddPlayerNotify.SyncEveryone:
            {
                BroadcastPlayerCreate(player, playerManageComponent);
                return;
            }
            default:
            {
                throw new ArgumentOutOfRangeException(nameof(notify), notify, null);
            }
        }
    }

    /// <summary>
    /// 在单位场景中移除一个PlayerUnit，并根据通知方式同步给客户端
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="playerId"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RemovePlayer(Scene scene, long playerId)
    {
        // var playerUnitManageComponent = scene.GetComponent<PlayerUnitManageComponent>();
        //
        // switch (notify)
        // {
        //     case RemovePlayerUnitNotify.NoNotification:
        //     {
        //         return;
        //     }
        //     case RemovePlayerUnitNotify.SyncSelf:
        //     {
        //         return;
        //     }
        //     case RemovePlayerUnitNotify.SyncEveryone:
        //     {
        //         return;
        //     }
        // }
    }

    /// <summary>
    /// 向场景内所有玩家广播单位创建消息（不包括单位自己）
    /// </summary>
    /// <param name="player">要广播的玩家单位</param>
    /// <param name="playerManageComponent">玩家单位管理组件（可选，为null时自动获取）</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void BroadcastPlayerCreate(Player player, PlayerManageComponent? playerManageComponent = null)
    {
        var playerCreateMessage = M2C_PlayerCreate.Create(false);

        try
        {
            playerCreateMessage.Player = player.ToProtocol(true);
            playerCreateMessage.IsSelf = false;  // 广播给其他人，标记为非自己
            BroadcastToAllPlayers(player.Scene, playerCreateMessage, playerManageComponent);
        }
        finally
        {
            playerCreateMessage.Return();
        }
    }

    /// <summary>
    /// 向场景内除指定玩家外的其他玩家广播单位创建消息
    /// </summary>
    /// <param name="scene">场景对象</param>
    /// <param name="playerInfo">要广播的单位信息</param>
    /// <param name="skipPlayerId">要跳过的玩家ID（通常是单位的所有者）</param>
    /// <param name="playerManageComponent">玩家单位管理组件（可选，为null时自动获取）</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void BroadcastPlayerCreate(Scene scene, PlayerInfo playerInfo, long skipPlayerId, PlayerManageComponent? playerManageComponent = null)
    {
        var playerCreateMessage = M2C_PlayerCreate.Create(false);
        playerManageComponent ??= scene.GetComponent<PlayerManageComponent>();

        try
        {
            playerCreateMessage.Player = playerInfo;
            playerCreateMessage.IsSelf = false;  // 广播给其他人，标记为非自己

            foreach (var (playerId, player) in playerManageComponent.Players)
            {
                // 跳过指定玩家
                if (playerId == skipPlayerId)
                {
                    continue;
                }

                if (!player.TryGetLinkTerminus(out var linkTerminus))
                {
                    continue;
                }

                linkTerminus.Send(playerCreateMessage);
            }
        }
        finally
        {
            playerCreateMessage.Return();
        }
    }

    /// <summary>
    /// 向客户端发送玩家创建消息
    /// </summary>
    /// <param name="linkTerminus">目标客户端连接</param>
    /// <param name="player">玩家信息</param>
    /// <param name="isSelf">是否是接收者自己的玩家</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SendPlayerCreate(Terminus linkTerminus, Player player, bool isSelf)
    {
        var playerCreateMessage = M2C_PlayerCreate.Create();
        playerCreateMessage.Player = player.ToProtocol(true);
        playerCreateMessage.IsSelf = isSelf;
        linkTerminus.Send(playerCreateMessage);
    }

    /// <summary>
    /// 将场景内所有玩家信息同步给指定玩家（常用于新玩家加入场景时）
    /// </summary>
    /// <param name="linkTerminus">目标客户端连接</param>
    /// <param name="player">目标玩家</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SyncAllPlayersToPlayer(Terminus linkTerminus, Player player)
    {
        var playerCreateMessage = M2C_PlayerCreate.Create(false);
        var playerManageComponent = player.Scene.GetComponent<PlayerManageComponent>();

        try
        {
            foreach (var (_, playerInfo) in playerManageComponent.Players)
            {
                playerCreateMessage.Player = playerInfo.ToProtocol(true);
                linkTerminus.Send(playerCreateMessage);
            }
        }
        finally
        {
            playerCreateMessage.Return();
        }
    }

    /// <summary>
    /// 将场景内除指定玩家外的其他玩家信息同步给目标客户端（常用于新玩家加入场景时，避免重复同步自己）
    /// </summary>
    /// <param name="linkTerminus">目标客户端连接</param>
    /// <param name="player">目标玩家</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SyncOtherPlayers(Terminus linkTerminus, Player player)
    {
        var scene = player.Scene;
        var skipPlayerId = player.Id;
        var playerCreateMessage = M2C_PlayerCreate.Create(false);
        var playerManageComponent = scene.GetComponent<PlayerManageComponent>();

        try
        {
            playerCreateMessage.IsSelf = false;  // 同步的是其他人的单位

            foreach (var (playerId, otherPlayer) in playerManageComponent.Players)
            {
                // 跳过指定单位
                if (playerId == skipPlayerId)
                {
                    continue;
                }

                playerCreateMessage.Player = otherPlayer.ToProtocol(true);
                linkTerminus.Send(playerCreateMessage);
            }
        }
        finally
        {
            playerCreateMessage.Return();
        }
    }

    /// <summary>
    /// 向场景内所有玩家广播指定消息（通用广播方法）
    /// </summary>
    /// <typeparam name="T">消息类型，必须实现IRoamingMessage接口</typeparam>
    /// <param name="scene">场景对象</param>
    /// <param name="message">要广播的消息</param>
    /// <param name="playerManageComponent">玩家单位管理组件（可选，为null时自动获取）</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void BroadcastToAllPlayers<T>(Scene scene, T message, PlayerManageComponent? playerManageComponent = null) where T : IRoamingMessage
    {
        playerManageComponent ??= scene.GetComponent<PlayerManageComponent>();

        foreach (var (_, player) in playerManageComponent.Players)
        {
            if (!player.TryGetLinkTerminus(out var linkTerminus))
            {
                continue;
            }

            linkTerminus.Send(message);
        }
    }
}