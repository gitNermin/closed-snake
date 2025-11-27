using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public static class Extentions
    {
        public static void RemoveAllChildren(this Transform transform)
        {
            int childCount = transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Object.Destroy(transform.GetChild(0).gameObject);
            }
        }
        
        
        public static void ShuffleChildren(this Transform transform)
        {
            int childCount = transform.childCount;
            for (int i = childCount - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);

                transform.GetChild(i).SetSiblingIndex(j);
            }
        }
    }
}