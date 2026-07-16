using Fantasy.Entitas;

namespace Fantasy;

public static class AccountFactory
{
    public static Account Create(Scene scene, string name, string password)
    {
        var account = Entity.Create<Account>(scene);
        account.Name = name;
        account.Password = password;
        return account;
    }
}