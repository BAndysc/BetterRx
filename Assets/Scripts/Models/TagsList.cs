using System;
using UnityEngine;

namespace Models
{
    [Serializable]
    [CreateAssetMenu]
    public class TagsList : ScriptableObject
    {
        [SerializeField] private Tag[] tags;

        public Tag[] Tags => tags;
    }
}