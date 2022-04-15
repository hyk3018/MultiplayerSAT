﻿using System;
using Unity.Netcode;
using UnityEngine;

namespace Shared.Entity
{
    public class PlayerGoal : NetworkBehaviour
    {
        [SerializeField]
        private SpriteRenderer goalSpriteRenderer;

        [SerializeField]
        private float goalIncrementStep;
        
        [SerializeField]
        private Transform revealMaskTransform, revealStart, revealEnd;

        public float buildTime;
        
        private float _goalProgress;

        public void Initialise(Sprite goalSprite)
        {
            _goalProgress = 0f;
            goalSpriteRenderer.sprite = goalSprite;
        }

        public void IncrementGoal()
        {
            _goalProgress = Math.Min(_goalProgress + goalIncrementStep, 1f);
            revealMaskTransform.position = Vector3.Lerp(revealStart.position, 
                revealEnd.position, _goalProgress);
        }
    }
}