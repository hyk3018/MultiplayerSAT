using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Server.Movement
{
    public class ServerPathFollower : NetworkBehaviour
    {
        [FormerlySerializedAs("moveSpeed")]
        public float MoveSpeed;

        public event Action ReachedPathEnd;
        
        private List<Vector3> _currentPath;
        private Vector3 _nextMoveVector;
        private bool _moving;

        private void Awake()
        {
            GameManager.Instance.Tick += HandleTick;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.Tick -= HandleTick;
            }
        }

        public override void OnNetworkSpawn()
        {
            if (!IsServer)
            {
                enabled = false;
                return;
            }
        }

        public void SetNewPath(List<Vector3> newPath)
        {
            _currentPath = newPath;
            _moving = true;
        }

        public void AddToCurrentPath(List<Vector3> pathToAdd)
        {
            _currentPath.AddRange(pathToAdd);
            _moving = true;
        }

        public bool HasPathTarget()
        {
            return _currentPath.Count > 0;
        }
        
        private void CalculateNextMovementDirection()
        {
            if (_currentPath == null || _currentPath.Count < 1)
            {
                _nextMoveVector = Vector3.zero;
                return;
            }

            var currentPosition = transform.position;
            var distanceToTarget = Vector3.Distance(currentPosition, _currentPath[0]);
            if (distanceToTarget < MoveSpeed)
            {
                transform.position = _currentPath[0];
                _currentPath.RemoveAt(0);
                distanceToTarget = MoveSpeed - distanceToTarget;
            }
            
            // If we reached the end of the path midway through movement then stop
            if (_currentPath.Count == 0)
            {
                _nextMoveVector = Vector3.zero;
                _moving = false;
                ReachedPathEnd?.Invoke();
                return;
            }
            
            _nextMoveVector = Mathf.Min(distanceToTarget, MoveSpeed) * (_currentPath[0] - currentPosition).normalized;
        }

        private void HandleTick(int tick)
        {
            if (_moving)
            {
                CalculateNextMovementDirection();
                transform.position += _nextMoveVector;
            }
        }
    }
}
