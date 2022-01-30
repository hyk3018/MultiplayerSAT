using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Server.Movement
{
    public class ServerPathFollower : NetworkBehaviour
    {
        [SerializeField]
        private float moveSpeed;
        
        private List<Vector3> m_currentPath;
        private Vector3 m_nextMoveVector;
        private bool m_moving;

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
            m_currentPath = newPath;
            m_moving = true;
        }

        public void AddToCurrentPath(List<Vector3> pathToAdd)
        {
            m_currentPath.AddRange(pathToAdd);
            m_moving = true;
        }

        public bool HasPathTarget()
        {
            return m_currentPath.Count > 0;
        }
        
        public void CalculateNextMovementDirection()
        {
            if (m_currentPath == null || m_currentPath.Count < 1)
            {
                m_nextMoveVector = Vector3.zero;
                return;
            }

            var currentPosition = transform.position;
            var distanceToTarget = Vector3.Distance(currentPosition, m_currentPath[0]);
            if (distanceToTarget < moveSpeed)
            {
                transform.position = m_currentPath[0];
                m_currentPath.RemoveAt(0);
                distanceToTarget = moveSpeed - distanceToTarget;
            }
            
            // If we reached the end of the path midway through movement then stop
            if (m_currentPath.Count == 0)
            {
                m_nextMoveVector = Vector3.zero;
                m_moving = false;
                return;
            }
            
            m_nextMoveVector = Mathf.Min(distanceToTarget, moveSpeed) * (m_currentPath[0] - currentPosition).normalized;
        }

        private void FixedUpdate()
        {
            if (m_moving)
            {
                CalculateNextMovementDirection();
                transform.position += m_nextMoveVector;
            }
        }
    }
}
