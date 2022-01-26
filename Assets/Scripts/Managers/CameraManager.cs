using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraManager : MonoBehaviour
{
    #region Attributes
    [Header("Positioning")]
    [InfoBox("Positioning values are applied when game starts and during camera resets.")]
    [DisableInPlayMode]
    [SerializeField]
    private Vector3 _startingOffset;
    [DisableInPlayMode]
    [SerializeField]
    private Vector3 _startingRotation;

    [Header("Scroll")]
    [SerializeField]
    [Range(0f, 50f)]
    private float _scrollSpeed = 10f;
    [SerializeField]
    private Vector2 _scrollRange = new Vector2(8, 15);

    [Header("Movement")]
    /// <summary>
    /// Maximum camera movement/offset limits
    /// </summary>
    [SerializeField]
    private Vector2 _offsetLimit;
    [Range(5f, 25f)]
    [SerializeField]
    private float _dragSpeed = 10f;
    [Range(5f, 25f)]
    [SerializeField]
    private float _panSpeed = 10f;
    [SerializeField]
    private bool _borderPan = true;
    [Range(0, 20)]
    [DisableIf("@!_borderPan")]
    [SerializeField]
    private int _borderPanWidth = 10;
    #endregion


    #region Properties
    #endregion


    private Camera _mainCamera;
    private PlayerInput _playerInput;


    #region Unity Methods
    void Awake()
    {
        _mainCamera = Camera.main;
        _playerInput = new PlayerInput();

        ResetPosition();
    }

    void Update()
    {

    }

    private void LateUpdate()
    {
        HandleMovement();
        HandleScroll();
    }

    private void OnEnable()
    {
        _playerInput.Camera.Enable();
        _playerInput.Camera.Reset.performed += HandleReset;
    }

    private void OnDisable()
    {
        _playerInput.Camera.Disable();
        _playerInput.Camera.Reset.performed -= HandleReset;
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
        bool dragging = _playerInput.Camera.Drag.ReadValue<float>() > 0;
        Vector2 dragInput = dragging ? -_playerInput.Camera.MouseDelta.ReadValue<Vector2>() : Vector2.zero;

        Vector3 mouseInput = new Vector3(0, 0, 0);
        if (_borderPan && !dragging)
        {
            Vector2 mousePosition = _playerInput.Camera.Mouse.ReadValue<Vector2>();
            if (mousePosition.x <= _borderPanWidth)
                mouseInput.x = -1;
            if (mousePosition.x >= Screen.width - _borderPanWidth)
                mouseInput.x = 1;
            if (mousePosition.y >= Screen.height - _borderPanWidth)
                mouseInput.z = 1;
            if (mousePosition.y <= _borderPanWidth)
                mouseInput.z = -1;
        }

        Vector2 keyboardInput = !dragging ? _playerInput.Camera.Movement.ReadValue<Vector2>() : Vector2.zero;

        Vector3 panMovement = new Vector3(keyboardInput.x, 0, keyboardInput.y) + mouseInput;
        panMovement = panMovement.normalized * _panSpeed * Time.deltaTime;
        Vector3 dragMovement = new Vector3(dragInput.x, 0, dragInput.y) * _dragSpeed * Time.deltaTime;

        Vector3 targetPosition = transform.position + panMovement + dragMovement;

        targetPosition.x = targetPosition.x.Clamp(_startingOffset.x - _offsetLimit.x, _startingOffset.x + _offsetLimit.x);
        targetPosition.z = targetPosition.z.Clamp(_startingOffset.z - _offsetLimit.y, _startingOffset.z + _offsetLimit.y);

        transform.position = targetPosition;
    }

    private void HandleScroll()
    {
        float scrollInput = _playerInput.Camera.Scroll.ReadValue<float>();

        // Reduce unnecessary computation if already at/past scroll ranges
        if (transform.position.y <= _scrollRange.x && scrollInput > 0) return;
        if (transform.position.y >= _scrollRange.y && scrollInput < 0) return;

        // Limit scrolling within bounds to prevent foward/backward movement when
        //   scrolling past bounds by calculating last valid point in path.
        Vector3 targetPosition = transform.position + transform.forward * scrollInput * _scrollSpeed * 0.1f * Time.deltaTime;
        if (targetPosition.y < _scrollRange.x || targetPosition.y > _scrollRange.y)
        {
            Plane limitPlane = new Plane(Vector3.up, new Vector3(0, scrollInput > 0 ? _scrollRange.x : _scrollRange.y, 0));
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
        _mainCamera.transform.localPosition = Vector3.zero;
        _mainCamera.transform.localRotation = Quaternion.identity;

        transform.position = _startingOffset;
        transform.rotation = Quaternion.Euler(_startingRotation);
    }
    #endregion


    #region Input Actions
    private void HandleReset(InputAction.CallbackContext ctx)
    {
        ResetPosition();
    }
    #endregion
}
