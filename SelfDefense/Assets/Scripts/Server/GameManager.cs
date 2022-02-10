using Unity.Netcode;
using UnityEngine;

namespace Server
{
    public class GameManager : NetworkBehaviour
    {
        [SerializeField]
        private GameObject playerSpawnPrefab;
        
        private int _readyCount;

        [ServerRpc(RequireOwnership = false)]
        public void SetReadyServerRpc(bool ready)
        {
            _readyCount = ready ? _readyCount + 1 : _readyCount - 1;
            _readyCount = Mathf.Max(0, _readyCount);
            if (_readyCount == 2)
            {
                Debug.Log("Game starts!");

                foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
                {
                    var go = Instantiate(playerSpawnPrefab);
                    go.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
                }
            }
        }
    }
}