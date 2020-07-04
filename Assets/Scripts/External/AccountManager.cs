using AccountModel;
using GameNet;
using GameNet.Account;
using GameNet.Operations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.AccountManagement
{
    public class AccountManager
    {
        public static AccountData LoggedAccount { get; set; }

        //public const string url_signup = "http://176.107.160.146/Account/Register?";
        //public const string uploadUrl = "http://176.107.160.146/Account/Update";
        //public const string url_getAvatar = "http://176.107.160.146/Account/GetAvatar?nick=";

        public async Task<OperationMessage> LogIn(string login, string password)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
                return new OperationMessage(OperationType.Fail, "No internet connection");

            OperationMessage msg = await NetCore.ServerActions.Account.LogIn(login, password);
            if (msg.Type != OperationType.Success) return msg;

            LoggedAccount = msg.Account;

            return msg;
        }
        //public async Task SignUp(string login, string password, Action<string> callback = null)
        //{
        //    await Task.Factory.StartNew(() =>
        //    {
        //        WebClient c = new WebClient();
        //        string result = c.DownloadString(url_signup + "nick=" + login + "&password=" + password);

        //        if (result.Contains("[ERR]"))
        //        {
        //            Debug.LogError("SignUp error: " + result);
        //            callback?.Invoke(result);

        //        }
        //        else if (result == "Registered")
        //        {
        //            callback?.Invoke("ok");
        //        }
        //    });
        //}

        public async void LoadAvatar(string nick, Image destImage)
        {
            byte[] bytes = await NetCore.ServerActions.Account.GetAvatar(nick);
            destImage.sprite = TheGreat.LoadSprite(bytes);
        }
    }
}
