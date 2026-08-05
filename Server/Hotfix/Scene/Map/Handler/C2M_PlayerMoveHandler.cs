using Fantasy.Async;
using Fantasy.Network.Interface;
using Fantasy.Network.Roaming;

namespace Fantasy;

public sealed class C2M_PlayerMoveHandler : Roaming<Player, C2M_PlayerMove>
{
    // 防止异常客户端无限推高尚未消费的输入数量。
    private const int MaxPendingInputs = 64;

    protected override async FTask Run(Player player, C2M_PlayerMove message)
    {
        // 旧包、重复包以及超过积压上限的输入不能进入权威模拟队列。
        if (message.ClientTick <= player.LastReceivedInputTick ||
            player.PendingInputs.Count >= MaxPendingInputs)
        {
            await FTask.CompletedTask;
            return;
        }

        // 收包只代表 received；processed 必须等逻辑帧真正模拟后才能推进。
        player.LastReceivedInputTick = message.ClientTick;
        player.PendingInputs.Enqueue(new InputCommand(
            message.ClientTick,
            Math.Clamp(message.MoveX, -1, 1),
            Math.Clamp(message.MoveY, -1, 1)));

        await FTask.CompletedTask;
    }
}
