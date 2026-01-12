using UnityEngine;

namespace ACC.ThirdPerson
{
    /// <summary>
    /// Used by the inputs manager to properly keep track of inputs per player
    /// </summary>
    [System.Serializable]
    public struct InputStructure
    {
        public Vector2 Move;
        public bool Jumping;
        public bool Sprinting;
        public bool IsInteracting;
        public bool IsCrouching;
    }
}