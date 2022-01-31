using System;
using Client.Commands;
using HelloWorld;
using Shared.Entity;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Client.Avatar
{
    public class ClientInputProcessor : MonoBehaviour
    {
        private bool m_moveRequested;
        private bool m_commandRequested;
        private CommandData m_requestedCommand;
        private NetworkPlayerState _mNetworkPlayerState;
        private Camera m_MainCamera;
        private RaycastHit[] k_hit_cache = new RaycastHit[4];

        private void Awake()
        {
            _mNetworkPlayerState = GetComponent<NetworkPlayerState>();
            m_MainCamera = Camera.main;
        }

        public void RequestCommand(CommandData requestedCommand)
        {
            if (m_commandRequested) return;

            m_commandRequested = true;
            m_requestedCommand = requestedCommand;
        }

        private void FixedUpdate()
        {
            if (m_moveRequested)
            {
                m_moveRequested = false;
                var worldPos = m_MainCamera.ScreenToWorldPoint(Input.mousePosition);
                _mNetworkPlayerState.SubmitPositionRequestServerRpc(new Vector3(worldPos.x, worldPos.y, transform.position.z));
            }

            if (m_commandRequested)
            {
                m_commandRequested = false;
                _mNetworkPlayerState.SubmitCommandRequestServerRpc(m_requestedCommand);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                if (Input.GetMouseButtonDown(0))
                {
                    m_moveRequested = true;
                }
            }
        }
    }
}
