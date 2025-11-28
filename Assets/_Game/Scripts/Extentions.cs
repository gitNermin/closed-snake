using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public static class Extentions
    {
        public static void RemoveAllChildren(this Transform transform)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                GameObject.Destroy(transform.GetChild(i).gameObject);
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
        
        public static string ToClockFormat(this float totalSeconds)
        {
            totalSeconds = Mathf.Max(0f, totalSeconds);
            int seconds = (int)totalSeconds;

            int hours = seconds / 3600;
            int minutes = (seconds % 3600) / 60;
            int secs = seconds % 60;

            if (hours > 0)
                return $"{hours:D2}:{minutes:D2}:{secs:D2}";
            else
                return $"{minutes:D2}:{secs:D2}";
        }
    }
}