using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private Transform target;        // The player to follow
    [SerializeField] private Vector3 offset = new Vector3(0, 5, -5); // Reduced default distance
    [SerializeField] private float smoothSpeed = 5f; // How smoothly the camera follows

    [Header("Zoom Settings")]
    [SerializeField] private float minZoom = 3f;     // Minimum zoom distance
    [SerializeField] private float maxZoom = 10f;    // Maximum zoom distance
    [SerializeField] private float zoomSpeed = 2f;   // How fast the zoom changes
    [SerializeField] private float currentZoom = 5f; // Current zoom level

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (target != null)
        {
            // Set initial zoom
            currentZoom = 5f; // Set initial zoom level
            UpdateCameraPosition();
        }
        Debug.Log("CameraController started"); // Debug log
    }

    void LateUpdate() // LateUpdate is called after all Update functions have been called
    {
        if (target != null)
        {
            // Calculate desired position with zoom
            Vector3 desiredPosition = target.position + offset.normalized * currentZoom;
            
            // Smoothly move camera
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            transform.position = smoothedPosition;

            // Make camera look at the player
            transform.LookAt(target);
        }
        else
        {
            Debug.LogWarning("Camera has no target to follow!");
        }
    }

    // This method will be called by the Input System
    public void OnZoom(InputValue value)
    {
        Debug.Log("Zoom input received: " + value.Get<float>()); // Debug log
        float scrollInput = value.Get<float>();
        currentZoom = Mathf.Clamp(currentZoom - scrollInput * zoomSpeed, minZoom, maxZoom);
        Debug.Log("New zoom level: " + currentZoom);
        
        // Immediately update camera position when zooming
        UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        if (target != null)
        {
            // Calculate position with current zoom
            Vector3 newPosition = target.position + offset.normalized * currentZoom;
            transform.position = newPosition;
            transform.LookAt(target);
        }
    }
}
