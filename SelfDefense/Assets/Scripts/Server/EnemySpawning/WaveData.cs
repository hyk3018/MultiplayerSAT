using System;
using System.Collections.Generic;
using UnityEngine;

namespace Server.EnemySpawning
{
    [Serializable]
    public struct WaveData
    {
        [SerializeField]
        public List<EnemySpawnData> Spawns;
        public float MinNextSpawnTime;
    }
}