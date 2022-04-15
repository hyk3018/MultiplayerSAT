using System;
using Shared.Entity.Towers;
using Unity.Netcode;
using UnityEngine;

namespace Shared.Entity
{
    public class ChildhoodSelf : NetworkBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<Enemy>())
            {
                Debug.Log("Hit enemy");
            }
        }
    }
}