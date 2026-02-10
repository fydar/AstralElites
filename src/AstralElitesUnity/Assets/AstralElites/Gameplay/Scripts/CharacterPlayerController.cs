using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

[RequireComponent(typeof(Character))]
public class CharacterPlayerController : MonoBehaviour
{
    [SerializeField] private InputActionReference analogMovementInput;
    [SerializeField] private InputActionReference analogLookInput;

    private Character character;
    private Camera mainCamera;

    private void Awake()
    {
        character = GetComponent<Character>();
        mainCamera = Camera.main;

        analogMovementInput.asset.Enable();
        analogLookInput.asset.Enable();
    }

    private void Update()
    {
        if (!character.isAlive)
        {
            return;
        }

        Vector2 thrust = analogMovementInput.action.ReadValue<Vector2>();
        Vector2 lookInput = analogLookInput.action.ReadValue<Vector2>();

        bool isRotating = false;
        float rotationTarget = character.inputRotation;
        bool isThrusting = false;

        if (lookInput.sqrMagnitude > 0.01f)
        {
            isRotating = true;
            rotationTarget = Mathf.Atan2(lookInput.y, lookInput.x) * Mathf.Rad2Deg;
        }
        else
        {
            Vector2 pointerPosition = Vector2.zero;
            bool pointerFound = false;

            if (Touchscreen.current != null)
            {
                var touchSum = Vector2.zero;
                int activeCount = 0;

                foreach (var touch in Touchscreen.current.touches)
                {
                    if (touch.press.isPressed)
                    {
                        touchSum += touch.position.ReadValue();
                        activeCount++;
                    }
                }

                if (activeCount > 0)
                {
                    pointerPosition = touchSum / activeCount;
                    isThrusting = true;
                    pointerFound = true;
                }
            }
            
            if (!pointerFound && Pen.current != null)
            {
                if (Pen.current.tip.isPressed)
                {
                    pointerPosition = Pen.current.position.ReadValue();
                    isThrusting = true;
                    pointerFound = true;
                }
            }
            
            if (!pointerFound && Mouse.current != null)
            {
                pointerPosition = Mouse.current.position.ReadValue();
                isThrusting = Mouse.current.leftButton.isPressed;
                pointerFound = true;
            }

            if (pointerFound)
            {
                var ray = mainCamera.ScreenPointToRay(pointerPosition);
                var scenePoint = ray.origin + (ray.direction * 10);
                Vector2 direction = (Vector2)scenePoint - (Vector2)transform.position;

                isRotating = true;
                rotationTarget = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            }

            if (isThrusting)
            {
                thrust = (Vector2)transform.right;
            }
        }

        if (isRotating)
        {
            character.inputRotation = rotationTarget;
        }

        character.inputThrust = thrust;
    }
}
