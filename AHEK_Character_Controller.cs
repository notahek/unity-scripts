using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;

namespace ACC.ThirdPerson
{
    public class AHEK_Character_Controller : ACC_Base
    {
        //private vars
        [SerializeField] CharacterController controller;
        [SerializeField] CinemachineCamera _camera;
        [SerializeField] InputsManager inputManager;
        float player_movement_multiplier = 1, Collider_Initial_Height, Collider_Initial_centerY;
        int runIndex = -1, moveIndex = -1, crouchIndex = -1, idleIndex = -1;
        int activeStateIndex = 0;
        bool _IsClimbing;
        bool sprintState = false;
        float y_theta = 0;

        [Header("Settings")]
        public CameraStateEnum cameraStateEnum = CameraStateEnum.CameraRelativeMovement;
        public PlayerMotionStateEnum ActiveState = PlayerMotionStateEnum.Idle;
        public List<PlayerMotionSets> motion_states_array = new List<PlayerMotionSets>();
        public float CrouchHeightReduction_ratio = 0.75f;
        public float HeightLerpSmoothness = 0.8f;
        public float PlayerJumpHieght = 5f;
        public float GravityScale = 1;
        public float Rigidbodies_Pushforce = 2f;

        [Header("MotionsList")]
        public bool AllowCrouching = true;
        public bool AllowJumping = true;
        public bool AllowClimbing = true;
        public bool AllowSprinting = true;

        [Header("Detection Mechanics Settings")]
        [SerializeField] float GroundDetectionRadius = 1f;
        [SerializeField] float GroundDetectionSphereOffset = 1f;
        [SerializeField] LayerMask WhatIsGround;
        [SerializeField] float ClimbReachDistance = 2f;
        [SerializeField] float ClimbHeight = 1.3f;
        [SerializeField] float ClimbHeightRangeOffset = 0.2f;
        [SerializeField] LayerMask WhatIsClimbable;

        [Header("Outputs")]
        [HideInInspector] public Vector3 verticalVelocity;
        bool Grounded = false;
        bool JumpState = false;

        [Header("Events")]
        public UnityEvent OnCrouchStartedEvent;
        public UnityEvent OnCrouchStoppedEvent;
        public UnityEvent OnClimbEvent;
        public UnityEvent OnClimbAvailable;
        public UnityEvent<bool> OnJumpEvent;
        public UnityEvent<bool> OnSprintEvent;
        public UnityEvent<Vector2> OnPlayerMoveEvent;
        public UnityEvent<Vector2> OnCrouchMoveEvent;

        public override void InitializeModule()
        {
            //Its our character boy
            controller = GetComponent<CharacterController>();
            inputManager = GetComponent<InputsManager>();
            Collider_Initial_Height = controller.height;
            Collider_Initial_centerY = controller.center.y;

            if (_camera == null) _camera = transform.GetComponentInChildren<CinemachineCamera>();
            if (_camera != null)
            {
                _camera.Follow = transform;
            }
            else
            {
                Debug.LogWarning("  THE CAMERA REFERENCE IS NOT SET  ");
            }

            //All the sets for the motion
            PlayerMotionStateEnum moveEnum = PlayerMotionStateEnum.Walk;
            PlayerMotionStateEnum runEnum = PlayerMotionStateEnum.Run;
            PlayerMotionStateEnum idleEnum = PlayerMotionStateEnum.Idle;
            PlayerMotionStateEnum crouchEnum = PlayerMotionStateEnum.CrouchedMovement;
            for (int i = 0; i < motion_states_array.Count; i++)
            {
                var states = motion_states_array[i];
                if (states.MotionState == moveEnum)
                {
                    moveIndex = i;
                }
                else if (states.MotionState == runEnum)
                {
                    runIndex = i;
                }
                else if (states.MotionState == idleEnum)
                {
                    idleIndex = i;
                }
                else if (states.MotionState == crouchEnum)
                {
                    crouchIndex = i;
                }
            }

            if (crouchIndex == -1) Debug.LogError("Crouch state not defined in the motion sets");
            if (runIndex == -1) Debug.LogError("Run state not defined in the motion sets");
            if (idleIndex == -1) Debug.LogError("Idle state not defined in the motion sets");
            if (moveIndex == -1) Debug.LogError("Move state not defined in the motion sets");

            OnModuleInitiated?.Invoke();
        }

        private void Update()
        {
            if (InitializationState)
            {
                Gravity(Time.deltaTime);
                if (inputManager.InitializationState)
                {
                    var _inputs = inputManager.Inputs;
                    MoveCharacter(_inputs, Time.deltaTime);
                    Crouch(_inputs);
                    JumpOrClimb(_inputs);
                }
            }
        }
        private void MoveCharacter(InputStructure Inputs, float DeltaTime)
        {
            if (_IsClimbing) return;        //Disable all the movements when the player is climbing

            OnPlayerMoveEvent?.Invoke(Inputs.Move);
            if (Inputs.Move.magnitude >= 0.1f)
            {
                activeStateIndex = moveIndex;

                if (Inputs.Move.y > 0 && Inputs.Sprinting && AllowSprinting)
                {
                    if (!sprintState)
                    {
                        sprintState = true;
                        OnSprintEvent?.Invoke(sprintState);
                    }

                    activeStateIndex = runIndex;
                }
                else if (!Inputs.Sprinting || Inputs.Move.y <= 0)
                {
                    if (sprintState)
                    {
                        sprintState = false;
                        OnSprintEvent?.Invoke(sprintState);
                    }
                }
            }
            else {
                activeStateIndex = idleIndex;
                if (sprintState)
                {
                    sprintState = false;
                    OnSprintEvent?.Invoke(sprintState);
                }
            }
            if (Inputs.IsCrouching && AllowCrouching) activeStateIndex = crouchIndex;

            ActiveState = motion_states_array[activeStateIndex].MotionState;
            player_movement_multiplier = motion_states_array[activeStateIndex].MotionMultiplier;

            //The move values will control the basic direction for inputs
            Vector3 InputsDirection = Vector3.forward * Inputs.Move.y + Vector3.right * Inputs.Move.x;

            //This angle will refine the horizontal movement considering the camera direction
            float MoveAngle = Mathf.Atan2(InputsDirection.x, InputsDirection.z) * Mathf.Rad2Deg;
            float CameraAngle = 0;
            switch (cameraStateEnum)
            {
                case CameraStateEnum.CameraRelative:
                    CameraAngle = _camera.transform.eulerAngles.y;
                    break;

                case CameraStateEnum.CameraRelativeMovement:
                    if (Inputs.Move.magnitude >= 0.1f)
                    {
                        y_theta = _camera.transform.eulerAngles.y;
                    }

                    CameraAngle = y_theta;
                    break;

                case CameraStateEnum.Absolute:
                    CameraAngle = 0;
                    break;
            }

            //Calculating the Final Move Direction
            Vector3 move = Quaternion.Euler(0, MoveAngle + CameraAngle, 0) * Vector3.forward * Inputs.Move.magnitude;

            //Setting the characters rotation
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, CameraAngle, transform.eulerAngles.z);

            controller.Move(move * player_movement_multiplier * DeltaTime);
        }

        bool crouching = false;
        private void Crouch(InputStructure inputs)
        {
            if (!AllowCrouching) return;
            if (inputs.IsCrouching)
            {
                OnCrouchMoveEvent?.Invoke(inputs.Move);
                if (!crouching)
                {
                    StartCoroutine(AHEKUtility.LerpFloatOverTime(Collider_Initial_Height, CrouchHeightReduction_ratio * Collider_Initial_Height, HeightLerpSmoothness,
                        (height) =>
                        {
                            controller.height = height;
                        }));

                    StartCoroutine(AHEKUtility.LerpFloatOverTime(Collider_Initial_centerY, CrouchHeightReduction_ratio * Collider_Initial_centerY, HeightLerpSmoothness,
                        (y) =>
                        {
                            controller.center = new Vector3(controller.center.x, y, controller.center.z);
                        }));

                    OnCrouchStartedEvent?.Invoke();
                    crouching = true;
                }

            }
            else
            {
                if (crouching)
                {
                    StartCoroutine(AHEKUtility.LerpFloatOverTime(CrouchHeightReduction_ratio * Collider_Initial_Height, Collider_Initial_Height, HeightLerpSmoothness,
                        (height) =>
                        {
                            controller.height = height;
                        }));

                    StartCoroutine(AHEKUtility.LerpFloatOverTime(CrouchHeightReduction_ratio * Collider_Initial_centerY, Collider_Initial_centerY, HeightLerpSmoothness,
                        (y) =>
                        {
                            controller.center = new Vector3(controller.center.x, y, controller.center.z);
                        }));

                    OnCrouchStoppedEvent?.Invoke();
                    crouching = false;
                }
            }
        }

        private void Gravity(float DeltaTime)
        {
            DetectGround();

            if (!Grounded && !_IsClimbing)
            {
                verticalVelocity += 2 * Physics.gravity * GravityScale * DeltaTime;
            }
            else if (!JumpState)
            {
                verticalVelocity = new Vector3(0, -1, 0);
            }

            controller.Move(verticalVelocity * DeltaTime);
        }

        private void JumpOrClimb(InputStructure inputs)
        {
            //Both the actions are binded to the same Jumping key
            //Its required to be on ground to trigger both actions
            if (Grounded && inputs.Jumping && AllowClimbing && ClimbableObjectNearby())
            {
                //There is a climable object in the vicinity and we are allowed to climb on it 

            }
            else if (Grounded && inputs.Jumping && AllowJumping && !JumpState && !crouching)
            {
                float Velocity = 2 * 9.81f * PlayerJumpHieght;      //newtons 3rd equation [v^2 = u^2 + 2as] 'a' being the acceleration and 's' being the desired hieght

                verticalVelocity = new Vector3(0, Mathf.Sqrt(Velocity), 0);
                JumpState = true;
                OnJumpEvent?.Invoke(JumpState);
            }
        }

        private void DetectGround()
        {
            var colliders = Physics.OverlapSphere(transform.position + (Vector3.down * GroundDetectionSphereOffset), GroundDetectionRadius, WhatIsGround);
            bool hasPlayer = false;
            foreach (var item in colliders) if (item.transform == transform) hasPlayer = true;

            if (hasPlayer)
            {
                //we are also colliding with the player
                if (colliders.Length <= 1)
                {
                    //There is no ground beneath the player
                    Grounded = false;

                    //To disable jumping
                    StartCoroutine(ResetJumpState());
                }
                else
                {
                    //We are touching the ground and not ourselves
                    Grounded = true;
                }
            }
            else
            {
                if (colliders.Length == 0)
                {
                    //There is no ground beneath the player
                    Grounded = false;

                    //To disable jumping
                    StartCoroutine(ResetJumpState());
                }
                else
                {
                    //We are touching the ground and not ourselves
                    Grounded = true;
                }
            }
        }

        private bool ClimbableObjectNearby()
        {
            bool objectfound = Physics.Raycast(transform.position + (Vector3.up * ClimbHeightRangeOffset),
                transform.forward, out RaycastHit hit, ClimbReachDistance, WhatIsClimbable);

            return objectfound;
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody rb = hit.rigidbody;

            if (rb != null && !rb.isKinematic)
            {
                Vector3 force_direction = hit.moveDirection;
                rb.AddForce(force_direction * Rigidbodies_Pushforce, ForceMode.Impulse);
            }
        }

        IEnumerator ResetJumpState()
        {
            yield return new WaitForSeconds(0.2f);
            JumpState = false;
        }

        private void OnDrawGizmosSelected()
        {
            //Gravity gizmos
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position + (Vector3.down * GroundDetectionSphereOffset), GroundDetectionRadius);

            //Climb gizmos
            Gizmos.color = Color.white;
            Gizmos.DrawRay(transform.position + (Vector3.up * ClimbHeightRangeOffset), transform.forward * ClimbReachDistance);
        }
    }
}