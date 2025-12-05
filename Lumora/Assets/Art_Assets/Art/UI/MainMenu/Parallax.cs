using UnityEngine;
using UnityEngine.InputSystem;

public class Parallax : MonoBehaviour
{
    public float OffsetMultiplier = 1f;
    public float SmoothTime = .3f;

    private Vector3 StartPosition;
    private Vector3 Velocity;

    void Start()
    {
        StartPosition = transform.position;
    }

    void Update()
    {
        Vector2 input = Vector2.zero;

        // Mouse
        if (Mouse.current != null)
        {
            input = Mouse.current.position.ReadValue();
            Vector3 offset = Camera.main.ScreenToViewportPoint(new Vector3(input.x, input.y, 0f));
            offset -= new Vector3(0.5f, 0.5f, 0f);

            Move(offset);
            return;
        }

        // Gamepad
        if (Gamepad.current != null)
        {
           
            Vector2 stick = Gamepad.current.rightStick.ReadValue();
            Vector3 offset = new Vector3(stick.x, stick.y, 0f) * 0.5f;

            Move(offset);
        }
    }

    void Move(Vector3 offset)
    {
        Vector3 target = new Vector3(
            StartPosition.x + offset.x * OffsetMultiplier,
            StartPosition.y + offset.y * OffsetMultiplier,
            StartPosition.z
        );

        transform.position = Vector3.SmoothDamp(transform.position, target, ref Velocity, SmoothTime);
    }
}
