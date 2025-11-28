using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
namespace Game
{
    public abstract class Collection<T> : ScriptableObject
    {
        [SerializeField] private List<T> Items;
        public T this[int index] => Items[index];
        public int Count => Items.Count;
        public T GetRandom() => Items[Random.Range(0, Items.Count)];
        public int IndexOf(T item) => Items.IndexOf(item);
        
        public List<T> GetRandomItems(int count)
        {
            var copy = new List<T>(Items);

            for (int i = copy.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (copy[i], copy[j]) = (copy[j], copy[i]);
            }

            return copy.GetRange(0, Mathf.Min(count, copy.Count));
        }
    }
}