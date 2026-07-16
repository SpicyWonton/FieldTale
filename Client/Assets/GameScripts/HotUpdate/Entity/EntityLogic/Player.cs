using System.Collections.Generic;
using Fantasy;
using UnityEngine;
using Log = UnityGameFramework.Runtime.Log;

namespace FieldTale.HotUpdate
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Player : Entity
    {
        private const float NetworkTickInterval = 0.05f;
        private const float RemoteInterpolationDelayTicks = 2f;

        private struct InputCommand
        {
            public InputCommand(uint clientTick, Vector2 input)
            {
                ClientTick = clientTick;
                Input = input;
            }

            public uint ClientTick;
            public Vector2 Input;
        }

        private struct NetworkSnapshot
        {
            public NetworkSnapshot(uint serverTick, Vector2 position)
            {
                ServerTick = serverTick;
                Position = position;
            }

            public uint ServerTick;
            public Vector2 Position;
        }

        [SerializeField]
        private PlayerData m_PlayerData = null;


        private readonly List<InputCommand> m_PendingInputs = new List<InputCommand>();
        private readonly List<NetworkSnapshot> m_RemoteSnapshots = new List<NetworkSnapshot>();

        private float m_NetworkTickTimer;
        private float m_RemoteRenderTick;
        private uint m_ClientTick;
        private uint m_LastServerTick;
        private bool m_HasRemoteRenderTick;

        public Rigidbody2D CachedRigidbody2D
        {
            get;
            private set;
        }

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            CachedRigidbody2D = GetComponent<Rigidbody2D>();
        }

        protected override void OnShow(object userData)
        {
            base.OnShow(userData);

            m_PlayerData = userData as PlayerData;
            if (m_PlayerData == null)
            {
                Log.Error("Player data is invalid.");
                return;
            }

            m_NetworkTickTimer = 0f;
            m_RemoteRenderTick = 0f;
            m_ClientTick = 0;
            m_LastServerTick = 0;
            m_HasRemoteRenderTick = false;
            m_PendingInputs.Clear();
            m_RemoteSnapshots.Clear();
        }

        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            if (m_PlayerData.IsSelf)
            {
                UpdateLocalPlayer(elapseSeconds);
            }
            else
            {
                UpdateRemotePlayer(elapseSeconds);
            }
        }

        public void ReceiveNetworkSnapshot(Vector3 position, uint serverTick, uint lastProcessedClientTick)
        {
            if (m_PlayerData == null)
            {
                return;
            }

            if (m_PlayerData.IsSelf)
            {
                ApplyAuthoritativeSnapshot((Vector2)position, serverTick, lastProcessedClientTick);
                return;
            }

            BufferRemoteSnapshot((Vector2)position, serverTick);
        }

        private void UpdateLocalPlayer(float elapseSeconds)
        {
            m_NetworkTickTimer += elapseSeconds;
            while (m_NetworkTickTimer >= NetworkTickInterval)
            {
                m_NetworkTickTimer -= NetworkTickInterval;

                Vector2 input = GetCurrentInput();
                uint clientTick = ++m_ClientTick;
                m_PendingInputs.Add(new InputCommand(clientTick, input));

                CachedRigidbody2D.position = SimulateMove(CachedRigidbody2D.position, input, NetworkTickInterval);
                Fantasy.Runtime.Session.C2M_PlayerMove(clientTick, Mathf.RoundToInt(input.x), Mathf.RoundToInt(input.y));
            }
        }

        private void UpdateRemotePlayer(float elapseSeconds)
        {
            if (m_RemoteSnapshots.Count == 0)
            {
                return;
            }

            if (m_RemoteSnapshots.Count == 1)
            {
                CachedRigidbody2D.position = m_RemoteSnapshots[0].Position;
                return;
            }

            float latestTick = m_RemoteSnapshots[m_RemoteSnapshots.Count - 1].ServerTick - RemoteInterpolationDelayTicks;
            m_RemoteRenderTick = Mathf.Min(m_RemoteRenderTick + elapseSeconds / NetworkTickInterval, latestTick);

            while (m_RemoteSnapshots.Count > 1 && m_RemoteSnapshots[1].ServerTick <= m_RemoteRenderTick)
            {
                m_RemoteSnapshots.RemoveAt(0);
            }

            if (m_RemoteSnapshots.Count == 1)
            {
                CachedRigidbody2D.position = m_RemoteSnapshots[0].Position;
                return;
            }

            NetworkSnapshot from = m_RemoteSnapshots[0];
            NetworkSnapshot to = m_RemoteSnapshots[1];
            float t = Mathf.InverseLerp(from.ServerTick, to.ServerTick, m_RemoteRenderTick);
            CachedRigidbody2D.position = Vector2.Lerp(from.Position, to.Position, t);
        }

        private void ApplyAuthoritativeSnapshot(Vector2 position, uint serverTick, uint lastProcessedClientTick)
        {
            if (serverTick < m_LastServerTick)
            {
                return;
            }

            m_LastServerTick = serverTick;
            m_PendingInputs.RemoveAll(command => command.ClientTick <= lastProcessedClientTick);

            Vector2 correctedPosition = position;
            for (int i = 0; i < m_PendingInputs.Count; i++)
            {
                correctedPosition = SimulateMove(correctedPosition, m_PendingInputs[i].Input, NetworkTickInterval);
            }

            CachedRigidbody2D.position = correctedPosition;
        }

        private void BufferRemoteSnapshot(Vector2 position, uint serverTick)
        {
            if (m_RemoteSnapshots.Count > 0 && serverTick <= m_RemoteSnapshots[m_RemoteSnapshots.Count - 1].ServerTick)
            {
                return;
            }

            m_RemoteSnapshots.Add(new NetworkSnapshot(serverTick, position));
            if (!m_HasRemoteRenderTick)
            {
                m_RemoteRenderTick = Mathf.Max(0f, serverTick - RemoteInterpolationDelayTicks);
                m_HasRemoteRenderTick = true;
            }
        }

        private static Vector2 GetCurrentInput()
        {
            return new Vector2(
                Mathf.RoundToInt(Input.GetAxisRaw("Horizontal")),
                Mathf.RoundToInt(Input.GetAxisRaw("Vertical")));
        }

        private Vector2 SimulateMove(Vector2 position, Vector2 input, float deltaTime)
        {
            if (input.sqrMagnitude > 1f)
            {
                input.Normalize();
            }

            return position + input * (m_PlayerData.Speed * deltaTime);
        }
    }
}