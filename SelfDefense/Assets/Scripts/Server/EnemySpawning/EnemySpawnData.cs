using System;
using UnityEngine;

namespace Server.EnemySpawning
{
    [Serializable]
    public struct EnemySpawnData
    {
        public GameObject EnemyPrefab;
        public GameObject Path;
    }
}