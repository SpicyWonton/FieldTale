using TMPro;
using Fantasy;
using Fantasy.Async;
using UnityEngine;
using UnityEngine.UI;
using Log = UnityGameFramework.Runtime.Log;
using GameFramework;

public class LoginForm : MonoBehaviour
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

    // Start is called before the first frame update
    private void Start()
    {
        m_BtnLogin.onClick.RemoveAllListeners();
        m_BtnLogin.onClick.AddListener(() =>
        {
            OnBtnLoginClicked().Coroutine();
        });
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
    }
}
