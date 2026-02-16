using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;

[RequireComponent(typeof(Character))]
public class CharacterPlayerController : MonoBehaviour
{
    public enum InputMode
    {
        None,
        Gamepad,
        Mouse,
        Touch,
        Pen
    }

    [SerializeField] private InputActionReference analogMovementInput;
    [SerializeField] private InputActionReference analogLookInput;

    public InputMode CurrentInputMode { get; private set; } = InputMode.None;

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

        var thrust = analogMovementInput.action.ReadValue<Vector2>();
        var lookInput = analogLookInput.action.ReadValue<Vector2>();

        InputChangeAwayFromInactiveDevices();
        InputChangeToHotDevices(thrust, lookInput);

        if (CurrentInputMode == InputMode.None)
        {
            CurrentInputMode = GetDefaultDevice();
        }

        bool isThrusting = false;
        bool isRotating = false;
        float rotationTarget = character.InputRotationTarget;

        switch (CurrentInputMode)
        {
            case InputMode.Gamepad:
                // Handle thrusting
                if (thrust.sqrMagnitude > 0.01f)
                {
                    isThrusting = true;
                    thrust = thrust.normalized;
                }

                // Handle rotation
                if (lookInput.sqrMagnitude > 0.01f)
                {
                    isRotating = true;
                    rotationTarget = Mathf.Atan2(lookInput.y, lookInput.x) * Mathf.Rad2Deg;
                }
                else if (isThrusting)
                {
                    // If not using look input but still thrusting, rotate in the direction of movement
                    isRotating = true;
                    rotationTarget = Mathf.Atan2(thrust.y, thrust.x) * Mathf.Rad2Deg;
                }
                break;
            case InputMode.Pen:
            case InputMode.Mouse:
            case InputMode.Touch:
                var pointerPosition = Vector2.zero;
                bool pointerFound = false;

                switch (CurrentInputMode)
                {
                    case InputMode.Mouse:
                        pointerPosition = Mouse.current.position.ReadValue();
                        // Mouse position check to ensure we have a valid pointer
                        if (pointerPosition.sqrMagnitude > 0.001f)
                        {
                            isThrusting = Mouse.current.leftButton.isPressed;
                            pointerFound = true;
                        }
                        break;
                    case InputMode.Touch:
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
                        break;
                    case InputMode.Pen:
                        if (Pen.current.tip.isPressed)
                        {
                            pointerPosition = Pen.current.position.ReadValue();
                            isThrusting = true;
                            pointerFound = true;
                        }
                        break;
                }

                if (pointerFound)
                {
                    var ray = mainCamera.ScreenPointToRay(pointerPosition);
                    var scenePoint = ray.origin + (ray.direction * 10);
                    var direction = (Vector2)scenePoint - (Vector2)transform.position;

                    isRotating = true;
                    rotationTarget = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                }

                if (isThrusting)
                {
                    thrust = (Vector2)transform.right;
                }
                break;
        }

        character.InputIsThrusting = isThrusting;
        character.InputThrust = thrust;

        character.InputIsRotating = isRotating;
        character.InputRotationTarget = rotationTarget;
    }

    private void InputChangeAwayFromInactiveDevices()
    {
        switch (CurrentInputMode)
        {
            case InputMode.Mouse:
                if (Mouse.current == null)
                {
                    CurrentInputMode = InputMode.None;
                }
                break;
            case InputMode.Touch:
                if (Touchscreen.current == null)
                {
                    CurrentInputMode = InputMode.None;
                }
                break;
            case InputMode.Pen:
                if (Pen.current == null)
                {
                    CurrentInputMode = InputMode.None;
                }
                break;
        }
    }

    private void InputChangeToHotDevices(Vector2 move, Vector2 look)
    {
        // Check Gamepad
        if (move.sqrMagnitude > 0.01f || look.sqrMagnitude > 0.01f)
        {
            CurrentInputMode = InputMode.Gamepad;
            return;
        }

        // Check Touch
        if (Touchscreen.current != null)
        {
            foreach (var touch in Touchscreen.current.touches)
            {
                if (touch.press.isPressed)
                {
                    CurrentInputMode = InputMode.Touch;
                    return;
                }
            }
        }

        // Check Pen
        if (Pen.current != null)
        {
            if (Pen.current.tip.isPressed)
            {
                CurrentInputMode = InputMode.Pen;
                return;
            }
        }

        // Check Mouse
        if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.isPressed)
            {
                CurrentInputMode = InputMode.Mouse;
            }
        }
    }

    private InputMode GetDefaultDevice()
    {
        // Check Touch
        if (Touchscreen.current != null)
        {
            return InputMode.Touch;
        }

        // Check Pen
        if (Pen.current != null)
        {
            return InputMode.Pen;
        }

        // Check Mouse
        if (Mouse.current != null)
        {
            return InputMode.Mouse;
        }

        // Default to Gamepad if no other devices are available
        return InputMode.Gamepad;
    }
}
