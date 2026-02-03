using System;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public class Spring
{
    public float Power;
    public float Damper;

    public float Target;

    public float Value;
    public float Velocity;

    public void Update(float deltaTime)
    {
        float direction = Target - Value;

        Velocity += direction * Power * deltaTime;

        Velocity -= Velocity * Damper * deltaTime;

        Value += Velocity * deltaTime;
    }
}

public class SpringBetween : MonoBehaviour
{
    public Spring spring;
    public float bounceForce = 10;

    public Transform A;
    public Transform B;

    private void Update()
    {
        if (Keyboard.current.xKey.wasPressedThisFrame)
        {
            spring.Velocity += bounceForce;
        }

        spring.Update(Time.deltaTime);

        transform.position = Vector3.LerpUnclamped(A.position, B.position, spring.Value);
    }
}
