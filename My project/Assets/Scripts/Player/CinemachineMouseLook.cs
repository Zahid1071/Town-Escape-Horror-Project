using UnityEngine;
using UnityEngine.InputSystem;

public class CinemachineMouseLook : MonoBehaviour
{
    [SerializeField] private Transform playerBody; // Assign PlayerCapsule
    [SerializeField] private float mouseSensitivity = 1.5f;

    private float pitch = 0f;

    void Update()
    {
        if (!Application.isFocused) return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue() * mouseSensitivity;

        // Horizontal look — rotate the player (yaw)
        playerBody.Rotate(Vector3.up * mouseDelta.x);

        // Vertical look — rotate this camera root (pitch)
        pitch -= mouseDelta.y;
        pitch = Mathf.Clamp(pitch, -80f, 80f);
        transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}
