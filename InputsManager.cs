using UnityEngine;
using UnityEngine.InputSystem;

namespace ACC
{
    public class InputsManager : ACC_Base
    {
        [Header("Inputs")]
        public InputStructure Inputs;

        [Header("Cursor Settings")]
        public bool HideCursor;

        public override void InitializeModule()
        {
            if (HideCursor)
            {
                Cursor.visible = false;

                if(Application.platform != RuntimePlatform.Android || Application.platform != RuntimePlatform.IPhonePlayer)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }

            OnModuleInitiated?.Invoke();
        }

        public void Move(InputAction.CallbackContext ctx)
        {
            Inputs.Move = ctx.ReadValue<Vector2>();
        }

        public void Interact(InputAction.CallbackContext ctx)
        {
            Inputs.IsInteracting = ctx.ReadValueAsButton();
        }

        public void Sprint(InputAction.CallbackContext ctx)
        {
            Inputs.Sprinting = ctx.ReadValueAsButton();
        }

        public void Jump(InputAction.CallbackContext ctx)
        {
            Inputs.Jumping = ctx.ReadValueAsButton();
        }

        public void Crouch(InputAction.CallbackContext ctx)
        {
            Inputs.IsCrouching = ctx.ReadValueAsButton();
        }
    }
}