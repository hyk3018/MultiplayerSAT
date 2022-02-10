using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Server.EnemySpawning
{
    public class ServerEnemySpawner : NetworkBehaviour
    {
        [SerializeField]
        private List<WaveData> waves;
    }
}
