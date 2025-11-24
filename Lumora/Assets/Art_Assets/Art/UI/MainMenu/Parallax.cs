using UnityEngine;
using UnityEngine.InputSystem;

public class Parallax : MonoBehaviour
{
    public float OffsetMultiplier = 1f;
    public float SmoothTime = .3f;

    private Vector3 StartPosition;
    private Vector3 Velocity;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 mouse = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;

        // get offset in viewport space (0..1)
        Vector3 offset = Camera.main.ScreenToViewportPoint(new Vector3(mouse.x, mouse.y, 0f));
        offset -= new Vector3(0.5f, 0.5f, 0f); // center around (0,0)

        // target keeps original z
        Vector3 target = new Vector3(
            StartPosition.x + offset.x * OffsetMultiplier,
            StartPosition.y + offset.y * OffsetMultiplier,
            StartPosition.z);
        transform.position = Vector3.SmoothDamp(transform.position, target, ref Velocity, SmoothTime);
    }
}
