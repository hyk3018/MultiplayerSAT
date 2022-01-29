using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace HelloWorld
{
    public class HelloWorldPlayer : NetworkBehaviour
    {
        public void Move()
        {
            print("Move!");
            // if (NetworkManager.Singleton.IsServer)
            // {
            //     var randomPosition = GetRandomPositionOnScreen();
            //     transform.position = randomPosition;
            //     Position.Value = randomPosition;
            // }
            // else
            // {
            //     SubmitPositionRequestServerRpc();
            // }
            SubmitPositionRequestServerRpc(GetRandomPositionOnScreen());
        }

        [ServerRpc]
        public void SubmitPositionRequestServerRpc(Vector3 newPosition)
        {
            //Position.Value = GetRandomPositionOnScreen();
            transform.position = newPosition;
        }

        static Vector3 GetRandomPositionOnScreen()
        {
            return new Vector3(Random.Range(-3f, 3f), Random.Range(-3f, 3f),1f);
        }

        void Update()
        {
            //transform.position = Position.Value;
        }
    }
}

