using TMPro;
using Fantasy;
using Fantasy.Async;
using UnityEngine;
using UnityEngine.UI;
using Log = UnityGameFramework.Runtime.Log;
using FieldTale;
using GameFramework;
using UnityGameFramework.Runtime;

public class LoginForm : UIFormLogic
{
    [SerializeField]
    private TextMeshProUGUI m_TmpAccount;
    [SerializeField]
    private TextMeshProUGUI m_TmpPassword;
    [SerializeField]
    private Button m_BtnLogin;

    public bool LoginSuccess
    {
        get;
        private set;
    }

    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);

        LoginSuccess = false;
        m_BtnLogin.onClick.RemoveAllListeners();
        m_BtnLogin.onClick.AddListener(() =>
        {
            OnBtnLoginClicked().Coroutine();
        });
    }

    protected override void OnClose(bool isShutdown, object userData)
    {
        m_BtnLogin.onClick.RemoveAllListeners();

        base.OnClose(isShutdown, userData);
    }

    private async FTask OnBtnLoginClicked()
    {
        if (string.IsNullOrEmpty(m_TmpAccount.text))
        {
            LoginSuccess = false;
            Log.Error("Account is empty.");
            return;
        }

        if (string.IsNullOrEmpty(m_TmpPassword.text))
        {
            LoginSuccess = false;
            Log.Error("Password is empty.");
            return;
        }

        var response = await Fantasy.Runtime.Session.C2G_LoginGameRequest(m_TmpAccount.text, m_TmpPassword.text);
        if (response.ErrorCode != 0)
        {
            Log.Error(Utility.Text.Format("登录失败 {0}", response.ErrorCode));
            return;
        }

        LoginSuccess = true;
        FrameworkRoot.UI.CloseUIForm(UIForm);
    }
}
