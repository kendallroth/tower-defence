using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraManager : MonoBehaviour
{
    #region Attributes
    [Header("Positioning")]
    [InfoBox("Positioning values are applied when game starts and during camera resets.")]
    [DisableInPlayMode]
    [SerializeField]
    private Vector3 startingOffset;
    [DisableInPlayMode]
    [SerializeField]
    private Vector3 startingRotation;

    [Header("Scroll")]
    [SerializeField]
    [Range(0f, 50f)]
    private float scrollSpeed = 10f;
    [SerializeField]
    private Vector2 scrollRange;

    [Header("Movement")]
    /// <summary>
    /// Maximum camera movement/offset limits
    /// </summary>
    [SerializeField]
    private Vector2 offsetLimit;
    [Range(5f, 25f)]
    [SerializeField]
    private float dragSpeed = 10f;
    [Range(5f, 25f)]
    [SerializeField]
    private float panSpeed = 10f;
    [SerializeField]
    private bool borderPan = true;
    [Range(0, 20)]
    [DisableIf("@!borderPan")]
    [SerializeField]
    private int borderPanWidth = 10;
    #endregion


    #region Properties
    #endregion


    private Camera mainCamera;
    private PlayerInput playerInput;


    #region Unity Methods
    void Awake()
    {
        mainCamera = Camera.main;
        playerInput = new PlayerInput();

        ResetPosition();
    }

    void Update()
    {

    }

    private void OnEnable()
    {
        playerInput.Camera.Enable();
        playerInput.Camera.Reset.performed += HandleReset;
    }

    private void OnDisable()
    {
        playerInput.Camera.Disable();
        playerInput.Camera.Reset.performed -= HandleReset;
    }

    private void LateUpdate()
    {
        HandleMovement();
        HandleScroll();
    }
    #endregion


    #region Custom Methods
    /// <summary>
    /// Camera movement supports several methods of movement:
    ///   - WASD / Arrow keys
    ///   - Mouse dragging (middle button)
    ///   - Screen-edge panning
    /// </summary>
    private void HandleMovement()
    {
        bool dragging = playerInput.Camera.Drag.ReadValue<float>() > 0;
        Vector2 dragInput = dragging ? -playerInput.Camera.MouseDelta.ReadValue<Vector2>() : Vector2.zero;

        Vector3 mouseInput = new Vector3(0, 0, 0);
        if (borderPan && !dragging)
        {
            Vector2 mousePosition = playerInput.Camera.Mouse.ReadValue<Vector2>();
            if (mousePosition.x <= borderPanWidth)
                mouseInput.x = -1;
            if (mousePosition.x >= Screen.width - borderPanWidth)
                mouseInput.x = 1;
            if (mousePosition.y >= Screen.height - borderPanWidth)
                mouseInput.z = 1;
            if (mousePosition.y <= borderPanWidth)
                mouseInput.z = -1;
        }

        Vector2 keyboardInput = !dragging ? playerInput.Camera.Movement.ReadValue<Vector2>() : Vector2.zero;

        Vector3 panMovement = new Vector3(keyboardInput.x, 0, keyboardInput.y) + mouseInput;
        panMovement = panMovement.normalized * panSpeed * Time.deltaTime;
        Vector3 dragMovement = new Vector3(dragInput.x, 0, dragInput.y) * dragSpeed * Time.deltaTime;

        Vector3 targetPosition = transform.position + panMovement + dragMovement;

        targetPosition.x = targetPosition.x.Clamp(startingOffset.x - offsetLimit.x, startingOffset.x + offsetLimit.x);
        targetPosition.z = targetPosition.z.Clamp(startingOffset.z - offsetLimit.y, startingOffset.z + offsetLimit.y);

        transform.position = targetPosition;
    }

    private void HandleScroll()
    {
        float scrollInput = playerInput.Camera.Scroll.ReadValue<float>();

        // Reduce unnecessary computation if already at/past scroll ranges
        if (transform.position.y <= scrollRange.x && scrollInput > 0) return;
        if (transform.position.y >= scrollRange.y && scrollInput < 0) return;

        // Limit scrolling within bounds to prevent foward/backward movement when
        //   scrolling past bounds by calculating last valid point in path.
        Vector3 targetPosition = transform.position + transform.forward * scrollInput * scrollSpeed * 0.1f * Time.deltaTime;
        if (targetPosition.y < scrollRange.x || targetPosition.y > scrollRange.y)
        {
            Plane limitPlane = new Plane(Vector3.up, new Vector3(0, scrollInput > 0 ? scrollRange.x : scrollRange.y, 0));
            Ray limitRay = new Ray(transform.position, targetPosition - transform.position);
            float rayDistance;
            if (limitPlane.Raycast(limitRay, out rayDistance) || rayDistance == 0)
            {
                targetPosition = limitRay.GetPoint(rayDistance);
            }
        }

        transform.position = targetPosition;
    }

    /// <summary>
    /// Reset camera position
    /// </summary>
    public void ResetPosition()
    {
        mainCamera.transform.localPosition = Vector3.zero;
        mainCamera.transform.localRotation = Quaternion.identity;

        transform.position = startingOffset;
        transform.rotation = Quaternion.Euler(startingRotation);
    }
    #endregion


    #region Input Actions
    private void HandleReset(InputAction.CallbackContext ctx)
    {
        ResetPosition();
    }
    #endregion
}
