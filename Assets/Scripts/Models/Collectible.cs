using System;
using UnityEngine;

namespace Models
{
    [Serializable]
    [CreateAssetMenu]
    public class Collectible : ScriptableObject
    {
        [SerializeField] private Sprite icon;

        [SerializeField] private string title;

        [SerializeField] private Tag[] tags;
        
        public Sprite Icon => icon;
        public string Title => title;
        public Tag[] Tags => tags;
    }
}