using Unity.Netcode;
using UnityEngine;

namespace Client.UI
{
    public class ReadyListener : NetworkBehaviour
    {
        public NetworkVariable<int> readyCount;

        [ServerRpc(RequireOwnership = false)]
        public void SetReadyServerRpc(bool ready)
        {
            readyCount.Value = Mathf.Max(0, ready ? readyCount.Value + 1 : readyCount.Value - 1);
        }
    }
}