using System.Numerics;
using System.Runtime.CompilerServices;
using Fantasy.Entitas;
using Fantasy.Network.Roaming;
// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract

namespace Fantasy;

public static class PlayerFactory
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Player CreatePlayer(Scene scene, string accountName)
    {
        var player = Entity.Create<Player>(scene);
        // 基础信息
        player.Name = accountName;
        // 挂载组件
        var transformComponent = player.AddComponent<TransformComponent>();
        transformComponent.Position = new Vector3(0, 0, 0);
        player.Transform = transformComponent;
        return player;
    }
}