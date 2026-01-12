
namespace ACC.ThirdPerson{
    /// <summary>
    /// Contains all the motion states the player can cycle through
    /// </summary>
    [System.Serializable]
    public enum PlayerMotionStateEnum
    {
        Walk,
        Run,
        CrouchedMovement,
        Idle
    }
    
    /// <summary>
    /// Contains all the camera states the player can cycle through
    /// </summary>
    [System.Serializable]
    public enum CameraStateEnum
    {
        CameraRelative,
        CameraRelativeMovement,
        Absolute
    }
    
    /// <summary>
    /// Contains the definition and vars for each motion state
    /// </summary>
    [System.Serializable]
    public class PlayerMotionSets
    {
        public PlayerMotionStateEnum MotionState;       //The motion state
        public float MotionMultiplier = 1.0f;           //Defined motion var
    }
}
