using UnityEngine;

namespace TK.Core.Common
{
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