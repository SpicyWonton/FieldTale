using Fantasy.Async;
using Fantasy.Event;
using Fantasy.Network.Roaming;

namespace Fantasy;

public sealed class OnCreateTerminus_LinkPlayer : AsyncEventSystem<OnCreateTerminus>
{
    protected override async FTask Handler(OnCreateTerminus self)
    {
        var scene = self.Scene;
        var terminus = self.Terminus;
        
        switch (scene.SceneType)
        {
            case SceneType.Map:
            {
                switch (self.Type)
                {
                    case CreateTerminusType.Link:
                    {
                        // 创建一个新的Player
                        var accountLinkArgs = self.Args as AccountLinkArgs;
                        if (accountLinkArgs == null || string.IsNullOrEmpty(accountLinkArgs.AccountName))
                        {
                            Log.Error("创建Player失败，未获取到账号名");
                            break;
                        }
                        
                        var player = PlayerFactory.CreatePlayer(scene, accountLinkArgs.AccountName);
                        // 关联到Terminus后，下次接收消息会发送到Player上
                        await terminus.LinkTerminusEntity(player, true);
                        // 添加到管理器中
                        PlayerManageHelper.AddPlayer(player, PlayerManageHelper.EAddPlayerNotify.NoNotification);
                        break;
                    }
                    case CreateTerminusType.ReLink:
                    {
                        Log.Debug("ReLink");
                        break;
                    }
                }
                
                break;
            }
        }
    }
}