using System;
using System.Collections;
using System.Collections.Generic;
using Server.Movement;
using Unity.Netcode;
using UnityEngine;

namespace Server.EnemySpawning
{
    public class ServerEnemySpawner : NetworkBehaviour
    {
        [SerializeField]
        private List<WaveData> waves;

        private Dictionary<GameObject, List<Vector3>> PathVectors;
        private int _nextWave;

        private void Awake()
        {
            PathVectors = new Dictionary<GameObject, List<Vector3>>();
        }

        public void SpawnNextWave()
        {
            var nextWave = waves[_nextWave];

            foreach (var batch in nextWave.Batches)
            {
                StartCoroutine(SpawnBatch(batch));
            }
        }

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

            var go = Instantiate(enemySpawnData.EnemyPrefab, transform);
            var pathFollower = go.GetComponent<ServerPathFollower>();

            if (pathFollower == null)
            {
                Debug.Log("Enemy spawn prefab missing component server path follower!");
                Destroy(go);
                return false;
            }

            var path = GetVectorPath(enemySpawnData.Path);
            pathFollower.SetNewPath(path);
            
            var networkObject = go.GetComponent<NetworkObject>();

            if (networkObject == null)
            {
                Debug.Log("Enemy spawn prefab missing component NetworkObject");
                Destroy(go);
                return false;
            }
            
            go.GetComponent<NetworkObject>().Spawn();

            return true;
        }

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
    }
}
