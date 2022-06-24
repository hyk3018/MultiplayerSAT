using UnityEngine;

namespace TK.Core.Common
{
    /*
     * Collection of extension methods useful in Unity development
     */
    
    public static class ExtensionMethods
    {
        public static void RemoveAllChildGameObjects(this Transform parent)
        {
            foreach (Transform child in parent)
            {
                GameObject.Destroy(child.gameObject);
            }
        }
    }
}