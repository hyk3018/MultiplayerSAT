﻿using System;
using Server;
using Unity.Netcode;
using UnityEngine;

namespace Shared.Entity
{
    public class PlayerOwnership : MonoBehaviour
    {
        public int OwnedPlayerIndex;
        public bool OwnedByPlayer;
        
        private ulong _player1Id;
        private ulong _player2Id;
        
        private void Awake()
        {
            GameManager.Instance.GameStarted += (player1Id, player2Id) =>
            {
                var ownerIndex = NetworkManager.Singleton.LocalClientId == player1Id ? 0 : 1;
                OwnedByPlayer = ownerIndex == OwnedPlayerIndex;
            };
        }
    }
}