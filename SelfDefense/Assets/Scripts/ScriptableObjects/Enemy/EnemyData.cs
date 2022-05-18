using System.Collections.Generic;
using Shared.Entity.Towers;
using UnityEngine;

namespace ScriptableObjects.Enemy
{
    [CreateAssetMenu(fileName = "EnemyData", menuName = "MultiplayerSAT/EnemyData", order = 0)]
    public class EnemyData : ScriptableObject
    {
        public int MaxHealth;
        public List<TowerType> AffectedBy;
        public List<float> EffectiveMultiplier;
        public List<int> StatusThreshold;
        public List<int> StatusDamage;
        public List<Sprite> StatusSprites;
        public int AffectionReward;

        public bool IsTargetedBy(TowerType towerType)
        {
            return AffectedBy.Contains(towerType);
        }

        public float GetEffectiveness(TowerType towerType)
        {
            if (!AffectedBy.Contains(towerType))
            {
                return 0;
            }

            var typeIndex = AffectedBy.IndexOf(towerType);
            return EffectiveMultiplier[typeIndex];
        }
    }
}