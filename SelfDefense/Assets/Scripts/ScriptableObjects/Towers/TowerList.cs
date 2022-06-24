using System.Collections.Generic;
using Shared.Entity.Towers;
using UnityEngine;

namespace ScriptableObjects.Towers
{
    [CreateAssetMenu(fileName = "TowerList", menuName = "MultiplayerSAT/TowerList", order = 0)]
    public class TowerList : ScriptableObject
    {
        [SerializeField]
        private List<GameObject> towerPrefabs;
        
        [SerializeField]
        private List<TowerType> towerTypes;

        /*
         * Look up tower prefab from tower type by matching up indexes
         * Dictionary is not used due to difficulty in exposing to Unity Editor
         */
        public GameObject GetTowerPrefabFromType(TowerType towerType)
        {
            var typeIndex = towerTypes.IndexOf(towerType);
            if (typeIndex == -1)
            {
                Debug.Log("Tried to access tower prefab type not registered.");
                return null;
            }

            if (typeIndex < towerPrefabs.Count)
            {
                return towerPrefabs[typeIndex];
            }

            Debug.Log("Tower prefab type index greater than prefab list length.");
            return null;
        }
    }
}