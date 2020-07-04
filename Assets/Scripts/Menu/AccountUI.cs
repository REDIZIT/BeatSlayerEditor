using AccountModel;
using Assets.AccountManagement;
using GameNet;
using GameNet.Account;
using GameNet.Operations;
using QuantumTek.EncryptedSave;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class AccountUI : MonoBehaviour
{
    public MenuScript ms;
    public AccountManager manager;

    public Sprite defaultAvatar;
    public Color32 errorColor, okColor;

    [Header("Window")]
    public GameObject authPanel;
    public InputField logIn_nick, logIn_pass, reg_nick, reg_pass, reg_passAgain;


    [Header("Sidebar")]
    public Image coverImage;
    public Text nickText;
    public Button logInBtn, /*signUpBtn,*/ logOutBtn;
    public GameObject moderationBtn;

    public void Init()
    {
        manager = new AccountManager();

        if (AccountManager.LoggedAccount == null) return;


        logInBtn.gameObject.SetActive(false);
        logOutBtn.gameObject.SetActive(false);

        nickText.text = AccountManager.LoggedAccount.Nick;
        manager.LoadAvatar(AccountManager.LoggedAccount.Nick, coverImage);

        moderationBtn.SetActive(AccountManager.LoggedAccount.Role == AccountRole.Moderator || AccountManager.LoggedAccount.Role == AccountRole.Developer);
    }



    public async void LogIn()
    {
        if (AccountManager.LoggedAccount != null) return;

        logInBtn.gameObject.SetActive(false);
        //signUpBtn.gameObject.SetActive(false);
        logOutBtn.gameObject.SetActive(false);

        bool isConnected = false;
        while (!isConnected)
        {
            isConnected = NetCore.State == Microsoft.AspNetCore.SignalR.Client.HubConnectionState.Connected;
            await Task.Delay(100);
        }


        if (ES_Save.Exists("/data/account/session"))
        {
            Session session = ES_Save.Load<Session>("/data/account/session");
            OperationMessage result = await LogIn(session.nick, session.password);

            OnLoggedIn(result);

            logOutBtn.gameObject.SetActive(true);
        }
        else
        {
            logInBtn.gameObject.SetActive(true);
            //signUpBtn.gameObject.SetActive(true);
        }
    }
    public async Task<OperationMessage> LogIn(string nick, string password)
    {
        return await manager.LogIn(nick, password);
    }
    public async void OnLogInBtnClicked()
    {
        string nick = logIn_nick.text;
        string password = logIn_pass.text;

        logIn_nick.GetComponent<Image>().color = okColor;
        logIn_pass.GetComponent<Image>().color = okColor;

        bool allIsOk = true;

        if (nick.Trim() == "") { logIn_nick.GetComponent<Image>().color = errorColor; allIsOk = false; }
        if (password.Trim() == "") { logIn_pass.GetComponent<Image>().color = errorColor; allIsOk = false; }



        if (!allIsOk) return;

        OperationMessage result = await LogIn(nick, password);

        if (result.Type == OperationType.Success)
        {
            authPanel.SetActive(false);
            OnLoggedIn(result);

            Session session = new Session() { nick = nick, password = password };
            ES_Save.Save(session, "/data/account/session");

            logInBtn.gameObject.SetActive(false);
            //signUpBtn.gameObject.SetActive(false);
            logOutBtn.gameObject.SetActive(true);
        }
        else
        {
            if (result.Message.ToLower().Contains("password"))
            {
                logIn_pass.GetComponent<Image>().color = errorColor;
            }
            else
            {
                logIn_nick.GetComponent<Image>().color = errorColor;
            }
        }
    }
    public void OnLoggedIn(OperationMessage result)
    {
        if (result.Type != OperationType.Success) { Debug.LogError("LogIn err: " + result); return; }

        nickText.text = AccountManager.LoggedAccount.Nick;
        manager.LoadAvatar(AccountManager.LoggedAccount.Nick, coverImage);

        moderationBtn.SetActive(AccountManager.LoggedAccount.Role == AccountRole.Moderator || AccountManager.LoggedAccount.Role == AccountRole.Developer);
    }


    public async void OnSignUpBtnClicked()
    {
        //string nick = reg_nick.text;
        //string password = reg_pass.text;
        //string passwordAgain = reg_passAgain.text;

        //reg_nick.GetComponent<Image>().color = okColor;
        //reg_pass.GetComponent<Image>().color = okColor;
        //reg_passAgain.GetComponent<Image>().color = okColor;

        //bool allIsOk = true;

        //if (nick.Trim() == "") { reg_nick.GetComponent<Image>().color = errorColor; allIsOk = false; }
        //if (password.Trim() == "") { reg_pass.GetComponent<Image>().color = errorColor; allIsOk = false; }
        //if (password != passwordAgain) { reg_passAgain.GetComponent<Image>().color = errorColor; allIsOk = false; }


        //if (!allIsOk) return;

        //string result = "";
        //await manager.SignUp(nick, password, (string r) => { result = r; });

        //if (result != "ok") { reg_nick.GetComponent<Image>().color = errorColor; return; }

        //await LogIn(nick, password);
        ////await manager.LogIn(nick, password, (string r) => { result = r; });

        ////if (result == "ok")
        ////{
        ////    authPanel.SetActive(false);
        ////    OnLoggedIn(result);

        ////    Session session = new Session() { nick = nick, password = password };
        ////    ES_Save.Save(session, "/data/account/session");

        ////    logInBtn.gameObject.SetActive(false);
        ////    signUpBtn.gameObject.SetActive(false);
        ////    logOutBtn.gameObject.SetActive(true);
        ////}
        ////else
        ////{
        ////    reg_nick.GetComponent<Image>().color = errorColor;
        ////}

       
    }



    public void LogOut()
    {
        ES_Save.DeleteData("/data/account/session");
        File.Delete(Application.persistentDataPath + "/data/account/avatar.pic");
        AccountManager.LoggedAccount = null;
        coverImage.sprite = defaultAvatar;
        nickText.text = "Guest";
        logInBtn.gameObject.SetActive(true);
        //signUpBtn.gameObject.SetActive(true);
        logOutBtn.gameObject.SetActive(false);
        moderationBtn.SetActive(false);
    }


    public void OnEyeClicked(InputField passwordField)
    {
        if (passwordField.contentType == InputField.ContentType.Password) passwordField.contentType = InputField.ContentType.Standard;
        else passwordField.contentType = InputField.ContentType.Password;

        passwordField.ForceLabelUpdate();
    }



    public class Session
    {
        public string nick, password;
    }
}
