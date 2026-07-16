using System.Diagnostics;
using System.Numerics;
using Fantasy.Entitas.Interface;
using Fantasy.Network.Roaming;

namespace Fantasy;

public sealed class PlayerUpdateSystem : UpdateSystem<Player>
{
    private const float MoveSpeed = 10f;
    private const float TickDeltaTime = 0.05f;
    private const int MaxCatchUpTicks = 5;
    private static readonly long TickDuration = Stopwatch.Frequency / 20;

    protected override void Update(Player self)
    {
        long now = Stopwatch.GetTimestamp();
        if (self.NextMoveTickTimestamp == 0)
        {
            self.NextMoveTickTimestamp = now + TickDuration;
            return;
        }

        int tickCount = 0;
        while (now >= self.NextMoveTickTimestamp && tickCount < MaxCatchUpTicks)
        {
            self.NextMoveTickTimestamp += TickDuration;
            tickCount++;
        }

        if (tickCount == 0)
        {
            return;
        }

        if (tickCount == MaxCatchUpTicks && now >= self.NextMoveTickTimestamp)
        {
            self.NextMoveTickTimestamp = now + TickDuration;
        }

        self.ServerTick += (uint)tickCount;

        Vector2 input = new Vector2(self.MoveX, self.MoveY);
        bool isMoving = input.LengthSquared() > 0f;
        if (isMoving)
        {
            input = Vector2.Normalize(input);
            self.Transform.Position += new Vector3(input.X, input.Y, 0f) * (MoveSpeed * TickDeltaTime * tickCount);
        }

        if (!isMoving && self.LastBroadcastClientTick == self.LastProcessedClientTick)
        {
            return;
        }

        BroadcastSnapshot(self);
        self.LastBroadcastClientTick = self.LastProcessedClientTick;
    }

    private static void BroadcastSnapshot(Player player)
    {
        var message = M2C_PlayerMove.Create(false);
        message.PlayerId = player.Id;
        message.Pos = Position.Create();
        message.Pos.Transform(player.Transform.Position);
        message.ServerTick = player.ServerTick;
        message.LastProcessedClientTick = player.LastProcessedClientTick;

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