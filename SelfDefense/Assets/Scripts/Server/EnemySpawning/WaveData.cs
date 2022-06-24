using System;
using System.Collections.Generic;
using UnityEngine;

namespace Server.EnemySpawning
{
    [Serializable]
    public struct WaveBatchData
    {
        public EnemySpawnData EnemySpawned;
        public int Amount;
        public float SpawnStartTime;
        public float InterSpawnDelay;
    }
    
    [Serializable]
    public struct WaveData
    {
        [SerializeField]
        public List<WaveBatchData> Batches;

        // Currently unused, potential use for future start next wave early function
        public float MinNextSpawnTime;

        [TextArea]
        public string Prompt;
    }
}