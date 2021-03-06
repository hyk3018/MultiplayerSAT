using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Server.Movement;
using Shared.Entity.Towers;
using Unity.Netcode;
using UnityEngine;

namespace Server.EnemySpawning
{
    public class ServerEnemySpawner : NetworkBehaviour
    {
        [SerializeField]
        private List<WaveData> waves;

        [SerializeField]
        private GameObject redPath;

        public event Action<bool> NoEnemiesRemaining;

        private Dictionary<GameObject, List<Vector3>> PathVectors;
        private NetworkVariable<int> _nextWave;
        private int _enemiesInCurrentWave;

        private void Awake()
        {
            PathVectors = new Dictionary<GameObject, List<Vector3>>();
        }

        public void SpawnNextWave()
        {
            var nextWave = waves[_nextWave.Value];
            _enemiesInCurrentWave = CalculateEnemyCount(nextWave);
            foreach (var batch in nextWave.Batches)
            {
                StartCoroutine(SpawnBatch(batch));
            }

            _nextWave.Value = Math.Min(waves.Count - 1, _nextWave.Value + 1);
        }

        private int CalculateEnemyCount(WaveData nextWave)
        {
            return nextWave.Batches.Select(batch => batch.Amount).Sum();
        }

        /*
         * Spawn batch enemy in regular intervals after start time
         */
        private IEnumerator SpawnBatch(WaveBatchData batch)
        {
            yield return new WaitForSeconds(batch.SpawnStartTime);

            for (int i = 0; i < batch.Amount; i++)
            {
                var spawnSuccess = SpawnEnemy(batch.EnemySpawned);
                if (!spawnSuccess) break;
                
                yield return new WaitForSeconds(batch.InterSpawnDelay);
            }
        }

        private bool SpawnEnemy(EnemySpawnData enemySpawnData)
        {
            if (enemySpawnData.EnemyPrefab == null)
            {
                Debug.Log("Enemy spawn data missing enemy prefab!");
                return false;
            }
            
            if (enemySpawnData.Path == null)
            {
                Debug.Log("Enemy spawn data missing path object!");
                return false;
            }

            var go = Instantiate(enemySpawnData.EnemyPrefab);
            var pathFollower = go.GetComponent<ServerPathFollower>();

            if (pathFollower == null)
            {
                Debug.Log("Enemy spawn prefab missing component server path follower!");
                Destroy(go);
                return false;
            }

            var path = GetVectorPath(enemySpawnData.Path);
            go.transform.position = path[0];
            pathFollower.SetNewPath(path);
            
            var networkObject = go.GetComponent<NetworkObject>();
            networkObject.TrySetParent(transform);

            if (networkObject == null)
            {
                Debug.Log("Enemy spawn prefab missing component NetworkObject");
                Destroy(go);
                return false;
            }
            
            // Track enemy death to know when round ends
            go.GetComponent<NetworkObject>().Spawn();
            var enemy = go.GetComponent<Enemy>();
            enemy.OnDeath += () =>
            {
                _enemiesInCurrentWave--;
                if (_enemiesInCurrentWave == 0)
                {
                    NoEnemiesRemaining?.Invoke(_nextWave.Value == waves.Count);
                }
            };

            enemy.SetSideClientRpc(enemySpawnData.Path == redPath ? 0 : 1);

            return true;
        }

        /*
         * Used cached path if possible to avoid recalculation
         */
        private List<Vector3> GetVectorPath(GameObject pathObject)
        {
            if (PathVectors.TryGetValue(pathObject, out List<Vector3> path))
            {
                return path;
            }

            path = new List<Vector3>();
            foreach (Transform child in pathObject.transform)
            {
                path.Add(child.position);
            }

            return path;
        }

        public string GetNextSpawnPrompt()
        {
            return waves[_nextWave.Value].Prompt;
        }
    }

}
