using Unity.Netcode;
using UnityEngine;

namespace TK.Core.Common
{
    public class Singleton<T> : NetworkBehaviour where T : Component
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    var objs = FindObjectsOfType(typeof(T)) as T[];
                    if (objs.Length > 0)
                        _instance = objs[0];
                    if (objs.Length > 1)
                    {
                        Debug.Log("There exists more than one type of " + typeof(T).Name + "in the scene");
                    }

                    // if (_instance == null)
                    // {
                    //     GameObject obj = new GameObject();
                    //     obj.name = string.Format("_{0}", typeof(T).Name);
                    //     _instance = obj.AddComponent<T>();
                    // }
                }

                return _instance;
            }
        }

    }
}