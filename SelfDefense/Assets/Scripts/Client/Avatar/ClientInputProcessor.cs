using System;
using HelloWorld;
using Shared.Entity;
using UnityEngine;

namespace Client.Avatar
{
    public class ClientInputProcessor : MonoBehaviour
    {
        private bool m_moveRequested;
        private NetworkPlayerState _mNetworkPlayerState;
        private Camera m_MainCamera;
        private RaycastHit[] k_hit_cache = new RaycastHit[4];

        private void Awake()
        {
            _mNetworkPlayerState = GetComponent<NetworkPlayerState>();
            m_MainCamera = Camera.main;
        }

        private void FixedUpdate()
        {
            if (m_moveRequested)
            {
                m_moveRequested = false;
                var worldPos = m_MainCamera.ScreenToWorldPoint(Input.mousePosition);
                _mNetworkPlayerState.SubmitPositionRequestServerRpc(new Vector3(worldPos.x, worldPos.y, transform.position.z));
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                m_moveRequested = true;
            }
        }
    }
}
