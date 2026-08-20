using System;
using Fantasy;
using Fantasy.Async;
using Fantasy.Network;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityGameFramework.Runtime;

namespace FieldTale
{
    /// <summary>
    /// Integrates Fantasy's client runtime with Unity Game Framework.
    /// </summary>
    public sealed class FantasyComponent : GameFrameworkComponent
    {
        [Header("Network Settings")]
        [FormerlySerializedAs("RemoteIP")]
        public string remoteIP = "127.0.0.1";

        [FormerlySerializedAs("RemotePort")]
        public int remotePort = 20000;

        public FantasyRuntime.NetworkProtocolType protocol = FantasyRuntime.NetworkProtocolType.TCP;
        public bool enableHttps;
        public int connectTimeout = 5000;
        public bool enableReceiveMessageJsonLog;

        [Header("Heartbeat Settings")]
        public bool enableHeartbeat = true;
        public int heartbeatInterval = 2000;
        public int heartbeatTimeOut = 30000;
        public int heartbeatTimeOutInterval = 5000;
        public int maxPingSamples = 4;

        [Header("Startup Settings")]
        [Tooltip("Connect when the launcher scene starts. Disable this when a Procedure controls connection timing.")]
        public bool connectOnStart = true;

        [Header("Event Callbacks")]
        public UnityEvent onConnectComplete;
        public UnityEvent onConnectFail;
        public UnityEvent onConnectDisconnect;

        public Scene Scene { get; private set; }
        public Session Session { get; private set; }
        public bool IsConnecting { get; private set; }
        public bool IsConnected => Session is { IsDisposed: false };
        public float PingSeconds => Runtime.PingSeconds;
        public int PingMilliseconds => Runtime.PingMilliseconds;

        private bool _ownsRuntime;

        private void Start()
        {
            if (connectOnStart)
            {
                ConnectAsync().Coroutine();
            }
        }

        /// <summary>
        /// Creates the Fantasy scene and starts a client session using this component's configuration.
        /// </summary>
        public async FTask<Session> ConnectAsync()
        {
            if (IsConnecting)
            {
                throw new InvalidOperationException("A Fantasy connection attempt is already in progress.");
            }

            IsConnecting = true;
            try
            {
                Disconnect();
                Session = await Runtime.Connect(
                    remoteIP,
                    remotePort,
                    protocol,
                    enableHttps,
                    connectTimeout,
                    enableHeartbeat,
                    heartbeatInterval,
                    heartbeatTimeOut,
                    heartbeatTimeOutInterval,
                    maxPingSamples,
                    OnConnectComplete,
                    OnConnectFail,
                    OnConnectDisconnect,
                    enableReceiveMessageJsonLog);
                Scene = Runtime.Scene;
                _ownsRuntime = true;
                return Session;
            }
            catch
            {
                Scene = null;
                Session = null;
                throw;
            }
            finally
            {
                IsConnecting = false;
            }
        }

        /// <summary>
        /// Disposes the Fantasy scene and the session created by this component.
        /// </summary>
        public void Disconnect()
        {
            if (!_ownsRuntime)
            {
                return;
            }

            Runtime.OnDestroy();
            _ownsRuntime = false;
            Scene = null;
            Session = null;
        }

        protected override void OnDestroy()
        {
            Disconnect();
            base.OnDestroy();
        }

        private void OnConnectComplete()
        {
            onConnectComplete?.Invoke();
        }

        private void OnConnectFail()
        {
            onConnectFail?.Invoke();
        }

        private void OnConnectDisconnect()
        {
            onConnectDisconnect?.Invoke();
        }
    }
}
