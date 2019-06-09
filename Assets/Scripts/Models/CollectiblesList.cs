using System;
using System.Collections.Generic;
using UnityEngine;

namespace Models
{
    [Serializable]
    [CreateAssetMenu]
    public class CollectiblesList : ScriptableObject
    {
        [SerializeField] private List<Collectible> collectibles;
        
        public IEnumerable<Collectible> Collectibles => collectibles;
    }
}