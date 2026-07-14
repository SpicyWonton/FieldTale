using Fantasy;
using Fantasy.Async;
using Fantasy.Entitas;
using Fantasy.Network;
using Fantasy.Network.Interface;
using Fantasy.Network.Roaming;

namespace Fantasy;

public sealed class C2G_LoginGameRequestHandler : MessageRPC<C2G_LoginGameRequest,G2C_LoginGameResponse>
{
    protected override async FTask Run(Session session, C2G_LoginGameRequest request, G2C_LoginGameResponse response, Action reply)
    {
        var account = request.Account;
        var password = request.Password;

        if (string.IsNullOrEmpty(account))
        {
            response.ErrorCode = 1;
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            response.ErrorCode = 1;
            return;
        }
        
        // if (!AccountManageHelper.Add(session.Scene, accountName, out var account))
        // {
        //     response.ErrorCode = 1;
        //     return;
        // }
        // // var account = Entity.Create<Account>(session.Scene);
        // account.Session = session;
        // // 挂载组件用来标记这个Session下的Account，后面下线流程也会用到
        // session.AddComponent<GateAccountFlagComponent>().Account = account;
        // // 执行上线流程
        // await AccountHelper.Online(session, account);
    }
}