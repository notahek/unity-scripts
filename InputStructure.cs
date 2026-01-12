using UnityEngine;

namespace ACC.ThirdPerson
{
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