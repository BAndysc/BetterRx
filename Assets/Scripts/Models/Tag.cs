using System;
using UnityEngine;

namespace Models
{
    [Serializable]
    [CreateAssetMenu]
    public class Tag : ScriptableObject
    {
        [SerializeField] private string name;
        [SerializeField] private Color color;

        public string Name => name;
        public Color Color => color;
    }
}