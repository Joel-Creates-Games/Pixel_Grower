using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpHeight = 2f;

    [Header("Fly-Cam Settings")]
    [SerializeField] private float cameraFollowSpeed = 10f;
    [SerializeField] private float flySpeed = 10f;
    [SerializeField] private float lookSensitivity = 1f;

    [Header("References")]
    [SerializeField] private Transform cameraParentTransform;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask floorMask;
    [SerializeField] private GameObject seedPrefab;
    [SerializeField] private ShopController shopController;
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerInventory playerInventory;
    [Tooltip("Parent object containing disabled seed instances")]
    [SerializeField] private Transform seedContainer;
    [Tooltip("The bone at the player's right hand")]
    [SerializeField] private Transform rightHandBone;

    [Header("Flower Interaction")]
    [SerializeField] private LayerMask interactMask;    // include layers for your flower triggers





    [Header("Animations")]
    private bool hasSownThisCycle = false;


    private CharacterController controller;
    private Vector3 velocity;

    // Input Actions
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction buyAction;
    private InputAction rightClickAction;
    private InputAction leftClickAction;
    private InputAction mousePosAction;
    private InputAction toggleFlyAction;
    public InputAction ascendAction;
    public InputAction descendAction;


    // Fly-cam bookkeeping
    private bool isFlyMode = false;
    private Transform camOriginalParent;
    private Vector3 camOriginalLocalPos;
    private Quaternion camOriginalLocalRot;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        // Cache original camera parent & local transform
        camOriginalParent = transform;
        camOriginalLocalPos = mainCamera.transform.localPosition;
        camOriginalLocalRot = mainCamera.transform.localRotation;

        // Movement & look
        moveAction = new InputAction("Move", InputActionType.Value);
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        lookAction = new InputAction("Look", InputActionType.Value);
        lookAction.AddBinding("<Mouse>/delta");

        // Vertical fly (space = up, left ctrl = down)
        ascendAction = new InputAction("Ascend", InputActionType.Value, "<Keyboard>/space");
        descendAction = new InputAction("Descend", InputActionType.Value, "<Keyboard>/leftCtrl");

        // Other controls
        jumpAction = new InputAction("Jump", InputActionType.Button, "<Keyboard>/space");
        buyAction = new InputAction("Buy", InputActionType.Button, "<Keyboard>/e");
        rightClickAction = new InputAction("RightClick", InputActionType.Button, "<Mouse>/rightButton");
        leftClickAction = new InputAction("LeftClick", InputActionType.Button, "<Mouse>/leftButton");
        mousePosAction = new InputAction("MousePos", InputActionType.Value, "<Pointer>/position");
        toggleFlyAction = new InputAction("ToggleFly", InputActionType.Button, "<Keyboard>/q");
    }

    private void OnEnable()
    {
        moveAction.Enable();
        lookAction.Enable();
        ascendAction.Enable();
        descendAction.Enable();
        jumpAction.Enable();
        buyAction.Enable();
        rightClickAction.Enable();
        leftClickAction.Enable();
        mousePosAction.Enable();
        toggleFlyAction.Enable();

        toggleFlyAction.performed += _ => ToggleFlyMode();
    }

    private void OnDisable()
    {
        toggleFlyAction.performed -= _ => ToggleFlyMode();
        moveAction.Disable();
        lookAction.Disable();
        jumpAction.Disable();
        buyAction.Disable();
        rightClickAction.Disable();
        leftClickAction.Disable();
        mousePosAction.Disable();
        toggleFlyAction.Disable();
    }

    private void Start()
    {
        mainCamera = cameraParentTransform.GetChild(0).GetComponent<Camera>();
        camOriginalParent = cameraParentTransform;
        camOriginalLocalPos = mainCamera.transform.localPosition;
        camOriginalLocalRot = mainCamera.transform.localRotation;
    }


    private void Update()
    {
        if (isFlyMode)
        {
            HandleCameraFly();
        }
        else
        {
            HandleMovement();
            HandleJump();
            HandleRotationAndAiming();
            HandleBuy();
            HandleSow();
            HandleEndGrowth();   // ← new call
            FollowPlayerCamera();
        }
    }



    private void HandleMovement()
    {
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

        // Move in world space
        controller.Move(moveDirection * moveSpeed * Time.deltaTime);

        // Animation flags
        bool isWalking = moveDirection.sqrMagnitude > 0.01f;
        animator.SetBool("IsWalking", isWalking);

        bool isAiming = rightClickAction.IsPressed();
        animator.SetBool("IsAiming", isAiming);

        // Determine sidestep based on movement relative to character's facing direction
        if (isWalking && isAiming)
        {
            Vector3 localMoveDir = transform.InverseTransformDirection(moveDirection);
            float sidewaysAmount = Mathf.Abs(localMoveDir.x);
            float forwardAmount = Mathf.Abs(localMoveDir.z);

            bool isSidestepping = sidewaysAmount > forwardAmount;
            animator.SetBool("IsSidestepping", isSidestepping);
        }
        else
        {
            animator.SetBool("IsSidestepping", false);
        }

        // Rotate toward movement direction if not aiming
        if (isWalking && !isAiming)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }
    }




    private void HandleJump()
    {
        bool groundedBefore = controller.isGrounded;

        if (jumpAction.triggered && groundedBefore)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
            animator.SetTrigger("Jump");
            animator.SetBool("IsJumping", true);
        }

        velocity.y += Physics.gravity.y * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if (!groundedBefore && controller.isGrounded)
            animator.SetBool("IsJumping", false);
    }

    private void HandleRotationAndAiming()
    {
        bool aiming = rightClickAction.IsPressed();
        animator.SetBool("IsAiming", aiming);
        if (!aiming) return;

        Vector2 mPos = mousePosAction.ReadValue<Vector2>();
        Ray ray = mainCamera.ScreenPointToRay(mPos);
        if (Physics.Raycast(ray, out var hit, Mathf.Infinity, floorMask))
        {
            Vector3 aimPoint = hit.point;
            aimPoint.y = transform.position.y;
            Vector3 toAim = (aimPoint - transform.position).normalized;
            if (toAim.sqrMagnitude > 0.01f)
            {
                float yaw = Quaternion.LookRotation(toAim).eulerAngles.y;
                transform.rotation = Quaternion.Euler(0, yaw, 0);
            }
        }
    }

    private void HandleBuy()
    {
        if (buyAction.triggered)
        {
            shopController?.BuySeeds();
            animator.SetTrigger("BuySeed");
        }
    }

    private void HandleSow()
    {
        bool rmb = rightClickAction.IsPressed();
        bool lmbPressed = leftClickAction.triggered;

        if (!rmb || !lmbPressed)
            return;

        // We know there's at least one seed, so start sow animation
        hasSownThisCycle = false;
        animator.SetTrigger("SowSeed");
    }



    // Called via Animation Event at frame 10 of “SowSeed” clip
    public void OnSowAnimationEvent()
    {
        if (hasSownThisCycle)
            return;

        if (playerInventory == null || playerInventory.seedCount <= 0)
            return;

        // Raycast to floor under mouse cursor
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask))
            return;

        // Get an inactive seed from the pool
        GameObject seedGO = GetPooledSeed();
        if (seedGO == null)
        {
            Debug.LogWarning("No available seeds in pool!");
            return;
        }

        // Activate seed at the hand position
        seedGO.transform.position = rightHandBone.position;
        seedGO.transform.rotation = transform.rotation;
        seedGO.SetActive(true);

        // Call ThrowSeed(origin) with the hand bone's world position
        var seedCtrl = seedGO.GetComponent<SeedController>();
        if (seedCtrl != null)
        {
            seedCtrl.ThrowSeed();
        }
        else
        {
            Debug.LogError("SeedController missing on pooled seed.");
        }

        // Consume one seed from inventory
        playerInventory.seedCount--;
        hasSownThisCycle = true;
    }


    private GameObject GetPooledSeed()
    {
        foreach (Transform t in seedContainer)
        {
            if (!t.gameObject.activeInHierarchy)
                return t.gameObject;
        }
        return null;
    }

    private void HandleEndGrowth()
    {
        // Don’t fire while aiming or if LMB wasn’t just pressed
        if (rightClickAction.IsPressed() || !leftClickAction.triggered)
            return;

        // Raycast from camera through the mouse cursor
        Vector2 mousePos = mousePosAction.ReadValue<Vector2>();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        // Include trigger colliders in the raycast
        if (Physics.Raycast(
                ray,
                out RaycastHit hit,
                Mathf.Infinity,
                interactMask,
                QueryTriggerInteraction.Collide))
        {
            // If we hit a flower trigger, call EndGrowth()
            if (hit.collider.CompareTag("flower"))
            {
                PixelPlant plant = hit.collider.GetComponent<PixelPlant>();
                if (plant != null)
                    plant.EndGrowth();
                playerInventory.gold += 11;
            }
        }

    }

    private void FollowPlayerCamera()
    {
        //if (mainCamera.transform.parent == null)
        // {
        // Calculate target position based on original offset
        Vector3 targetPos = transform.position;// + camOriginalParent.TransformPoint(camOriginalLocalPos) - camOriginalParent.position;
            cameraParentTransform.transform.position = Vector3.Lerp(cameraParentTransform.transform.position, targetPos, cameraFollowSpeed * Time.deltaTime);

            // Keep original rotation (or set a fixed one)
            //mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation, camOriginalLocalRot, cameraFollowSpeed * Time.deltaTime);
       // }
    }



    private void ToggleFlyMode()
    {
        isFlyMode = !isFlyMode;

        if (isFlyMode)
        {
            // Detach camera for free flight
            mainCamera.transform.SetParent(null);
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = false;
        }
        else
        {
            // Re-attach camera back to player
            mainCamera.transform.SetParent(camOriginalParent);
            mainCamera.transform.localPosition = camOriginalLocalPos;
            mainCamera.transform.localRotation = camOriginalLocalRot;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Reset vertical velocity so character doesn't fall while flying
        velocity.y = 0f;
    }

    private void HandleCameraFly()
    {
        // Mouse look
        Vector2 lookDelta = lookAction.ReadValue<Vector2>() * lookSensitivity;
        mainCamera.transform.Rotate(Vector3.up, lookDelta.x, Space.World);
        mainCamera.transform.Rotate(Vector3.right, -lookDelta.y, Space.Self);

        // Horizontal movement
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        Vector3 fwd = mainCamera.transform.forward;
        Vector3 right = mainCamera.transform.right;
        fwd.y = 0; right.y = 0;
        fwd.Normalize(); right.Normalize();
        Vector3 horizontalMove = (right * moveInput.x + fwd * moveInput.y);

        // Vertical movement
        float ascend = ascendAction.ReadValue<float>();
        float descend = descendAction.ReadValue<float>();
        float verticalInput = ascend - descend;
        Vector3 verticalMove = Vector3.up * verticalInput;

        // Combine and apply
        Vector3 fullMove = (horizontalMove + verticalMove) * flySpeed * Time.deltaTime;
        mainCamera.transform.position += fullMove;
    }

}