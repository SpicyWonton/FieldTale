using System.Diagnostics;
using System.Numerics;
using Fantasy.Entitas.Interface;
using Fantasy.Network.Roaming;

namespace Fantasy;

public sealed class PlayerUpdateSystem : UpdateSystem<Player>
{
    // 与客户端 TickInterval 保持一致，服务端权威模拟频率为 20 Hz。
    private const float MoveSpeed = 10f;
    private const float TickInterval = 0.05f;
    private const int MaxCatchUpTicks = 5;
    private static readonly long TickDuration = Stopwatch.Frequency / 20;

    protected override void Update(Player self)
    {
        long now = Stopwatch.GetTimestamp();
        if (self.NextTickTimestamp == 0)
        {
            self.NextTickTimestamp = now + TickDuration;
            return;
        }

        // 使用单调时钟累计逻辑帧，并限制单次追帧数量，避免长时间阻塞后产生尖峰。
        int tickCount = 0;
        while (now >= self.NextTickTimestamp && tickCount < MaxCatchUpTicks)
        {
            self.NextTickTimestamp += TickDuration;
            tickCount++;
        }

        if (tickCount == 0)
        {
            return;
        }

        if (tickCount == MaxCatchUpTicks && now >= self.NextTickTimestamp)
        {
            self.NextTickTimestamp = now + TickDuration;
        }

        bool processedInput = false;
        for (int i = 0; i < tickCount; i++)
        {
            self.ServerTick++;
            // 一条客户端命令对应一个服务端逻辑帧；没有命令时不沿用旧输入。
            if (!self.PendingInputs.TryDequeue(out InputCommand command))
            {
                continue;
            }

            self.MoveX = command.MoveX;
            self.MoveY = command.MoveY;
            // 位置应用该输入后才允许 ACK，客户端才能安全删除并回放后续输入。
            self.LastProcessedInputTick = command.Tick;
            processedInput = true;

            Vector2 input = new Vector2(command.MoveX, command.MoveY);
            if (input.LengthSquared() > 0f)
            {
                input = Vector2.Normalize(input);
                self.Transform.Position +=
                    new Vector3(input.X, input.Y, 0f) * (MoveSpeed * TickInterval);
            }
        }

        if (!processedInput)
        {
            return;
        }

        // 快照中的位置与 LastProcessedInputTick 必须来自同一次模拟结果。
        BroadcastSnapshot(self);
        self.LastBroadcastInputTick = self.LastProcessedInputTick;
    }

    private static void BroadcastSnapshot(Player player)
    {
        var message = M2C_PlayerMove.Create(false);
        message.PlayerId = player.Id;
        message.Pos = Position.Create();
        message.Pos.Transform(player.Transform.Position);
        // 协议字段沿用 ServerTick/ClientTick 命名，业务层语义均为逻辑帧编号。
        message.ServerTick = player.ServerTick;
        message.LastProcessedClientTick = player.LastProcessedInputTick;

        try
        {
            var players = player.Scene.GetComponent<PlayerManageComponent>().Players;
            foreach (var (_, targetPlayer) in players)
            {
                if (targetPlayer.TryGetLinkTerminus(out var linkTerminus))
                {
                    linkTerminus.Send(message);
                }
            }
        }
        finally
        {
            message.Return();
        }
    }
}
