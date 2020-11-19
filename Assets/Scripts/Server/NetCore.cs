using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using GameNet.Operations;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;

[assembly: Preserve]
namespace GameNet
{
    /// <summary>
    /// This class is controlling SignalR connection and messaging between server and client
    /// </summary>
    [Preserve]
    public static class NetCore
    {
        static HubConnection conn;
        public static HubConnectionState State => conn == null ? HubConnectionState.Disconnected : conn.State;
        //public static ConnectionState State => conn.State;
        public static Subscriptions Subs { get; private set; }

        public static string Url_Server
        {
            get
            {
                return ConnType == ConnectionType.Local
                    ? "https://localhost:5011" : ConnType == ConnectionType.Development
                        ? "http://bsserver.tk:5020"
                            : "https://bsserver.tk";
            }
        }
        public static string Url_Hub
        {
            get { return Url_Server + "/EditorHub"; }
        }
        public enum ConnectionType { Production, Local, Development }

        public static ConnectionType ConnType = ConnectionType.Development;



        public static Action OnConnect, OnDisconnect, OnReconnect, OnFullReady;
        public static Action OnLogIn, OnLogOut;


        // There are delegates of NetCore config methods (Instead of Configure(Action config))
        // Subs here your config code. This is invoked when wrapper invoke NetCore.Configure
        public static Action Configurators;



        public static bool TryReconnect { get; set; }
        public static int ReconnectAttempt { get; private set; }







        static NetCore()
        {
            SceneManager.activeSceneChanged += (arg0, scene) =>
            {
                Debug.Log("OnSceneChanged");
                //NetCore.Configurators = null;
                //if(Time.realtimeSinceStartup > 5) OnSceneLoad(); 
            };
            Application.quitting += () =>
            {
                //conn?.Stop();
                conn?.StopAsync();
                TryReconnect = false;
            };

            TryReconnect = true;
        }


        // Use after completing Configurators subs
        // This method apply configuration for current scene
        // (External usage)
        public static void Configure()
        {
            Subs = new Subscriptions();
            OnFullReady = null;
            OnConnect = null;
            OnDisconnect = null;
            OnReconnect = null;
            //OnLogIn = null;

            OnSceneLoad();

            //Debug.Log(" > Configure()");
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                //Debug.Log(" > Configure() in unity thread");
                Configurators.Invoke();

                OnFullReady?.Invoke();
            });
        }
        public static void Configure(Action config)
        {
            Subs = new Subscriptions();
            OnFullReady = null;
            OnConnect = null;
            OnDisconnect = null;
            OnReconnect = null;

            OnSceneLoad();

            //Debug.Log(" > Configure()");
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                config();

                OnFullReady?.Invoke();
            });
        }



        #region Internal usage



        // This method is invoked when Scene changed
        // Connect if not already
        // (Internal usage)
        static void OnSceneLoad()
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (conn == null)
                {
                    CreateConnection();
                }
                else
                {
                    //Debug.Log("Dont create connection via Dispatcher (" + conn.State + ")");
                }
            });
        }

        // This method is invoked on first load
        // (Internal usage)
        static void CreateConnection()
        {
            /*ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.SecurityProtocol =
                    SecurityProtocolType.Tls11;*/

            BuildConnection();
            SubcribeOnServerCalls();
            Connect();
        }






        // This async method connects to server via existing HubConnection
        // (Internal usage)
        static async void Connect()
        {
            //Debug.Log("> Connect");
            try
            {
                await conn.StartAsync();
                if (conn.State == HubConnectionState.Connected)
                {
                    Log("[ Connected ]");
                    OnConnect?.Invoke();
                    ReconnectAttempt = 0;
                }
                else
                {
                    Log("[ Reconnecting ]");
                    if (Application.isPlaying)
                    {
                        OnReconnect?.Invoke();
                        Reconnect();
                    }
                }
            }
            catch (Exception err)
            {
                Debug.Log("Connection failed: " + err);
                Log("[ Reconnecting ]");
                if (Application.isPlaying)
                {
                    OnReconnect?.Invoke();
                    Reconnect();
                }
            }

        }

        /// <summary>
        /// Debug.Log if has internet access
        /// </summary>
        static void Log(string msg)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable) return;

            Debug.Log(msg);
        }

        static async void Reconnect(bool force = false)
        {
            if (!TryReconnect) return;

            if (!force) await Task.Delay(2000);

            ReconnectAttempt++;
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                Connect();
            });
        }

        // This method creates new connection
        // (Internal usage)
        static void BuildConnection()
        {
            Debug.Log("> Create HubConnection with " + Url_Hub);

            conn = new HubConnectionBuilder()
                 .WithUrl(Url_Hub, (o) =>
                 {
                     o.SkipNegotiation = false;
                     o.Transports = HttpTransportType.WebSockets;
                 })
                 .ConfigureLogging(logging =>
                 {
                     /*logging.ClearProviders();
                     logging.SetMinimumLevel(LogLevel.Information);
                     logging.AddProvider(new UnityLogger());*/
                 })
                 .Build();
            conn.ServerTimeout = TimeSpan.FromMinutes(10);

            conn.Closed += (err =>
            {
                Debug.Log("Conn closed due to " + err.Message);
                OnDisconnect?.Invoke();
                Reconnect(true);
                return null;
            });
        }

        // This method invoke conn.On<T1,T2,TN> for EACH FIELD in NetCore.Subs
        // This is internal method used only after building connection
        // (Internal usage)
        static void SubcribeOnServerCalls()
        {
            //Debug.Log("> Sub on server calls");
            Subs = new Subscriptions();


            var fields = typeof(Subscriptions).GetFields();
            foreach (var field in fields)
            {
                Type t = field.FieldType;
                //Debug.Log(" << " + field.Name);
                conn.On(field.Name, t.GenericTypeArguments, (objects =>
                {
                    //Debug.Log("[CONNECTION ON] << " + field.Name);
                    FieldInfo info = typeof(Subscriptions).GetField(field.Name, BindingFlags.Public | BindingFlags.Instance);

                    object yourfield = info.GetValue(Subs);

                    MethodInfo method = yourfield.GetType().GetMethod("Invoke");

                    //method.Invoke(yourfield, objects);
                    UnityMainThreadDispatcher.Instance().Enqueue(() => method.Invoke(yourfield, objects));

                    return Task.Delay(0);
                }));
            }
        }

        #endregion





        [Preserve]
        public static class ServerActions
        {

            public static class Account
            {
                public static Task<OperationMessage> LogIn(string nick, string password)
                {
                    return conn.InvokeAsync<OperationMessage>("LogIn", nick, password);
                }
                public static Task<OperationMessage> GetPublishedMaps(string nick, string password)
                {
                    return conn.InvokeAsync<OperationMessage>("GetPublishedMaps", nick, password);
                }
                public static Task<byte[]> GetAvatar(string nick) 
                { 
                    return conn.InvokeAsync<byte[]>("GetAvatar", nick);
                }

                public static void SignUp(string nick, string password, string country, string email) => NetCore.conn.InvokeAsync("Accounts_SignUp", nick, password, country, email);
            }
        }




        // This class contains all methods which can invoke server on client-side
        // How server determine what should invoke? -Field name :D
        // Server: Client.Caller.SendAsync("MethodName", args)
        // There must be Action with name same as MethodName, else server won't be able to find it.

        // Some rules to use SignalR (пиздец, а нормально можно было сделать?! Вот чтоб без ебли в жопу!)
        // 1) If you want to send class USE {GET;SET} !!!!
        // 2) Don't use ctors at all, data won't be sent
        [Preserve]
        public class Subscriptions// : ISubs
        {
            public Action OnTest;
            public Action<OperationMessage> Accounts_OnLogIn;
            public Action<OperationMessage> Accounts_OnSignUp;
        }
    }
}


public class UnityLogger : ILoggerProvider
{
    public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
    {
        return new UnityLog();
    }
    public class UnityLog : Microsoft.Extensions.Logging.ILogger
    {
        public IDisposable BeginScope<TState>(TState state)
        {
            var id = Guid.NewGuid();
            Debug.Log($"BeginScope ({id}): {state}");
            return new Scope<TState>(state, id);
        }
        struct Scope<TState> : IDisposable
        {
            public Scope(TState state, Guid id)
            {
                State = state;
                Id = id;
            }

            public TState State { get; }
            public Guid Id { get; }

            public void Dispose() => Debug.Log($"EndScope ({Id}): {State}");
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                case LogLevel.Information:
                    Debug.Log($"{logLevel}, {eventId}, {state}, {exception}");
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning($"{logLevel}, {eventId}, {state}, {exception}");
                    break;
                case LogLevel.Error:
                case LogLevel.Critical:
                    Debug.LogError($"{logLevel}, {eventId}, {state}, {exception}");
                    break;
                case LogLevel.None: break;
            }
        }
    }

    public void Dispose() { }
}