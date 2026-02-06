using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Character))]
public class CharacterPlayerController : MonoBehaviour
{
    [SerializeField] private InputActionReference analogMovementInput;

    private Character character;
    private Camera mainCamera;

    private void Awake()
    {
        character = GetComponent<Character>();
        mainCamera = Camera.main;

        analogMovementInput.asset.Enable();
    }

    private void Update()
    {
        if (!character.isAlive)
        {
            return;
        }

        bool isUsingMouse = false;
        if (Mouse.current != null)
        {
            var ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            var scenePoint = ray.origin + (ray.direction * 10);

            float angleRadians = Mathf.Atan2(scenePoint.y - transform.position.y,
                scenePoint.x - transform.position.x);

            float angleDegrees = angleRadians * Mathf.Rad2Deg;

            character.inputRotation = angleDegrees;

            if (Mouse.current?.leftButton.isPressed ?? false)
            {
                character.inputThrust = transform.right;
                isUsingMouse = true;
            }
        }

        if (!isUsingMouse)
        {
            var input = analogMovementInput.action.ReadValue<Vector2>();
            character.inputThrust = input;
        }
    }
}
