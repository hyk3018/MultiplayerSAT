using System;
using Unity.Netcode;
using UnityEngine;

namespace Shared.Entity
{
    public class AffectionPoints : NetworkBehaviour
    {
        [SerializeField]
        private int affectionPointsStart;

        public NetworkVariable<int> Points;

        private void Start()
        {
            Points.Value = affectionPointsStart;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.U))
            {
                Points.Value = Points.Value + 1;
            }
            else if (Input.GetKeyDown(KeyCode.I))
            {
                Points.Value = Points.Value - 1;
            }
        }
    }
}