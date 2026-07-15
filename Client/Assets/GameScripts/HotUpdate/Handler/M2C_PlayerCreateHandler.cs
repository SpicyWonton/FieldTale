using Fantasy.Async;
using Fantasy.Network;
using Fantasy.Network.Interface;

namespace FieldTale.HotUpdate
{
    public sealed class M2C_PlayerCreateHandler : Message<Fantasy.M2C_PlayerCreate>
    {
        protected override async FTask Run(Session session, Fantasy.M2C_PlayerCreate message)
        {
            FrameworkRoot.Entity.ShowEntity(10000, typeof(Player), "Assets/GameRes/Entities/Player.prefab", "Entity", 0, new PlayerData(10000, 1, 10f));

            await FTask.CompletedTask;
        }
    }
}