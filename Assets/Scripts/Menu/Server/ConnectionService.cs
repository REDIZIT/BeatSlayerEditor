using Assets.AccountManagement;
using GameNet;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameNet.NetCore;

public class ConnectionService : MonoBehaviour
{
    public ConnectionType connectionType;
    public AccountManager accountManager;
    public bool overrideConnType;


    private void Awake()
    {
        if (overrideConnType)
        {
            NetCore.ConnType = connectionType;
        }
    }
    private void Start()
    {
        NetCore.Configure(() =>
        {
            
        });
    }
}
