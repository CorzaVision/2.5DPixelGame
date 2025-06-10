using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class CameraController : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private Transform target;        // The player to follow
    [SerializeField] private Vector3 offset = new Vector3(0, 2, -2); // Much closer default distance
    [SerializeField] private float smoothSpeed = 5f; // How smoothly the camera follows
    [SerializeField] private float lookAheadFactor = 2f;
    [SerializeField] private float lookAheadSmoothTime = 0.5f;

    [Header("Zoom Settings")]
    [SerializeField] private float minZoom = 1.5f;    // Much closer minimum zoom
    [SerializeField] private float maxZoom = 4f;      // Much closer maximum zoom
    [SerializeField] private float zoomSpeed = 2f;    // Keep the same zoom speed
    [SerializeField] private float zoomSmoothTime = 0.3f;
    [SerializeField] private float currentZoom = 2f;  // Start much closer to the player

    [Header("Camera Settings")]
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float minVerticalAngle = -30f;
    [SerializeField] private float maxVerticalAngle = 60f;
    private float currentRotationX = 0f;
    private float currentRotationY = 0f;
    private bool isRotating = false;

    [Header("Debug")]
    [SerializeField] private bool showDebug = true;
    [SerializeField] private Color debugColor = Color.yellow;

    // Components
    private InputAction zoomAction;
    private Vector3 currentVelocity;
    private Vector3 lookAheadVelocity;
    private float zoomVelocity;

    // Events
    public event Action<float> OnZoomChanged;

    // Add reference to player's input
    [SerializeField] private PlayerInput playerInput;

    // Add this variable with your other zoom settings
    private float targetZoom;  // Tracks the desired zoom level

    void Start()
    {
        if (target != null)
        {
            // Don't override the currentZoom value
            UpdateCameraPosition();
        }
        Debug.Log("CameraController started");
    }

    private void Awake()
    {
        if (playerInput != null)
        {
            // Get actions from the Camera map
            var cameraMap = playerInput.actions.FindActionMap("Camera");
            if (cameraMap != null)
            {
                cameraMap.Enable();  // Enable the Camera map
                zoomAction = cameraMap.FindAction("Zoom");
                rotateAction = cameraMap.FindAction("CameraRotate");
                lookAction = cameraMap.FindAction("Look");
            }
        }
    }

    private void OnEnable()
    {
        if (zoomAction != null)
        {
            zoomAction.performed += OnZoomPerformed;
            zoomAction.Enable();
        }
        if (rotateAction != null)
        {
            rotateAction.performed += OnRotatePerformed;
            rotateAction.Enable();
        }
        if (lookAction != null)
        {
            lookAction.performed += OnLookPerformed;
            lookAction.Enable();
        }
    }

    private void OnDisable()
    {
        if (zoomAction != null)
        {
            zoomAction.performed -= OnZoomPerformed;
            zoomAction.Disable();
        }
        if (rotateAction != null)
        {
            rotateAction.performed -= OnRotatePerformed;
            rotateAction.Disable();
        }
        if (lookAction != null)
        {
            lookAction.performed -= OnLookPerformed;
            lookAction.Disable();
        }
    }

    private void OnZoomPerformed(InputAction.CallbackContext context)
    {
        float scrollInput = context.ReadValue<float>();
        targetZoom = Mathf.Clamp(currentZoom - scrollInput * zoomSpeed, minZoom, maxZoom);
        
        if (showDebug)
        {
            Debug.Log($"Zoom input: {scrollInput}, Target zoom: {targetZoom}");
        }
    }

    private void OnRotatePerformed(InputAction.CallbackContext context)
    {
        // Only process rotation when the right mouse button is held
        if (context.control.name == "rightButton")
        {
            isRotating = context.ReadValue<float>() > 0;
            
            if (isRotating)
            {
                // Get mouse delta movement
                Vector2 mouseDelta = Mouse.current.delta.ReadValue();
                
                // Apply sensitivity and update rotation
                currentRotationX += mouseDelta.x * rotationSpeed * Time.deltaTime;
                currentRotationY = Mathf.Clamp(
                    currentRotationY - mouseDelta.y * rotationSpeed * Time.deltaTime, 
                    minVerticalAngle, 
                    maxVerticalAngle
                );
                
                if (showDebug) Debug.Log($"Rotation input: {mouseDelta}, Angles: X={currentRotationX}, Y={currentRotationY}");
            }
        }
    }

    private void OnLookPerformed(InputAction.CallbackContext context)
    {
        Vector2 lookInput = context.ReadValue<Vector2>();
        // Implement look logic here
        if (showDebug) Debug.Log($"Look input: {lookInput}");
    }

    void LateUpdate()
    {
        if (target == null)
        {
            if (showDebug) Debug.LogWarning("Camera has no target to follow!");
            return;
        }

        // Smoothly interpolate current zoom to target zoom
        currentZoom = Mathf.SmoothDamp(currentZoom, targetZoom, ref zoomVelocity, zoomSmoothTime);
        OnZoomChanged?.Invoke(currentZoom);

        // Calculate look ahead position based on target's movement
        Vector3 targetVelocity = target.GetComponent<Rigidbody>()?.linearVelocity ?? Vector3.zero;
        Vector3 lookAheadPos = Vector3.SmoothDamp(
            Vector3.zero,
            targetVelocity * lookAheadFactor,
            ref lookAheadVelocity,
            lookAheadSmoothTime
        );

        // Calculate rotation
        Quaternion rotation = Quaternion.Euler(currentRotationY, currentRotationX, 0);

        // Calculate desired position with zoom, rotation, and look ahead
        Vector3 desiredPosition = target.position + rotation * (offset.normalized * currentZoom) + lookAheadPos;

        // Smoothly move camera
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref currentVelocity,
            smoothSpeed * Time.deltaTime
        );

        // Set camera rotation
        transform.rotation = rotation;

        // Draw debug visualization
        if (showDebug)
        {
            Debug.DrawLine(transform.position, target.position, debugColor);
            Debug.DrawRay(target.position, lookAheadPos, Color.blue);
            
            // Add rotation debug visualization
            if (isRotating)
            {
                Debug.DrawRay(transform.position, transform.forward * 2f, Color.red);
                Debug.DrawRay(transform.position, transform.right * 2f, Color.green);
                Debug.DrawRay(transform.position, transform.up * 2f, Color.blue);
            }
        }
    }

    private void UpdateCameraPosition()
    {
        if (target != null)
        {
            // Calculate rotation
            Quaternion rotation = Quaternion.Euler(currentRotationY, currentRotationX, 0);
            
            // Calculate position with current zoom and rotation
            Vector3 newPosition = target.position + rotation * (offset.normalized * currentZoom);
            transform.position = newPosition;
            transform.rotation = rotation;
        }
    }

    private void OnDrawGizmos()
    {
        if (!showDebug || target == null) return;

        // Draw camera bounds
        Gizmos.color = debugColor;
        Gizmos.DrawWireSphere(target.position, minZoom);
        Gizmos.DrawWireSphere(target.position, maxZoom);
        
        // Draw current zoom
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(target.position, currentZoom);
    }

    private InputAction rotateAction;
    private InputAction lookAction;
}