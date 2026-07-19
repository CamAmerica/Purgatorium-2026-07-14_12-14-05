using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
	[Header("References")]
	public Camera playerCamera;

	[Header("Movement")]
	public float walkSpeed = 6f;
	public float runSpeed = 12f;
	public float jumpPower = 7f;
	public float gravity = 10f;

	[Header("Look")]
	public float lookSpeed = 2f;
	public float lookXLimit = 45f;

	[Header("Crouch")]
	public float defaultHeight = 2f;
	public float crouchHeight = 1f;
	public float crouchSpeed = 3f;

	private Vector3 moveDirection = Vector3.zero;
	private float rotationX = 0f;
	private CharacterController characterController;

	private bool canMove = true;

	// Cached so crouching doesn't permanently overwrite the inspector values
	// (the original script hardcoded 6f/12f back on release, which broke
	// customizing walkSpeed/runSpeed in the inspector).
	private float defaultWalkSpeed;
	private float defaultRunSpeed;

	// Input System actions, built in code so no separate .inputactions asset is required
	private InputAction moveAction;
	private InputAction lookAction;
	private InputAction jumpAction;
	private InputAction sprintAction;
	private InputAction crouchAction;

	private Vector2 moveInput;
	private Vector2 lookInput;

	void Awake()
	{
		characterController = GetComponent<CharacterController>();

		defaultWalkSpeed = walkSpeed;
		defaultRunSpeed = runSpeed;

		moveAction = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");
		moveAction.AddCompositeBinding("2DVector")
			.With("Up", "<Keyboard>/w")
			.With("Down", "<Keyboard>/s")
			.With("Left", "<Keyboard>/a")
			.With("Right", "<Keyboard>/d");
		moveAction.AddBinding("<Gamepad>/leftStick");

		lookAction = new InputAction("Look", InputActionType.Value, expectedControlType: "Vector2");
		lookAction.AddBinding("<Mouse>/delta");
		lookAction.AddBinding("<Gamepad>/rightStick");

		jumpAction = new InputAction("Jump", InputActionType.Button, "<Keyboard>/space");
		jumpAction.AddBinding("<Gamepad>/buttonSouth");

		sprintAction = new InputAction("Sprint", InputActionType.Button, "<Keyboard>/leftShift");
		sprintAction.AddBinding("<Gamepad>/leftStickPress");

		crouchAction = new InputAction("Crouch", InputActionType.Button, "<Keyboard>/r");
		crouchAction.AddBinding("<Gamepad>/buttonEast");
	}

	void OnEnable()
	{
		moveAction.Enable();
		lookAction.Enable();
		jumpAction.Enable();
		sprintAction.Enable();
		crouchAction.Enable();
	}

	void OnDisable()
	{
		moveAction.Disable();
		lookAction.Disable();
		jumpAction.Disable();
		sprintAction.Disable();
		crouchAction.Disable();
	}

	void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	void Update()
	{
		moveInput = moveAction.ReadValue<Vector2>();
		lookInput = lookAction.ReadValue<Vector2>();
		bool jumpPressed = jumpAction.IsPressed();
		bool sprintHeld = sprintAction.IsPressed();
		bool crouchHeld = crouchAction.IsPressed();

		Vector3 forward = transform.TransformDirection(Vector3.forward);
		Vector3 right = transform.TransformDirection(Vector3.right);

		float curSpeedX = canMove ? (sprintHeld ? runSpeed : walkSpeed) * moveInput.y : 0f;
		float curSpeedY = canMove ? (sprintHeld ? runSpeed : walkSpeed) * moveInput.x : 0f;
		float movementDirectionY = moveDirection.y;
		moveDirection = (forward * curSpeedX) + (right * curSpeedY);

		if (jumpPressed && canMove && characterController.isGrounded)
		{
			moveDirection.y = jumpPower;
		}
		else
		{
			moveDirection.y = movementDirectionY;
		}

		if (!characterController.isGrounded)
		{
			moveDirection.y -= gravity * Time.deltaTime;
		}

		if (crouchHeld && canMove)
		{
			characterController.height = crouchHeight;
			walkSpeed = crouchSpeed;
			runSpeed = crouchSpeed;
		}
		else
		{
			characterController.height = defaultHeight;
			walkSpeed = defaultWalkSpeed;
			runSpeed = defaultRunSpeed;
		}

		characterController.Move(moveDirection * Time.deltaTime);

		if (canMove)
		{
			rotationX += -lookInput.y * lookSpeed * 0.02f;
			rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
			playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
			transform.rotation *= Quaternion.Euler(0, lookInput.x * lookSpeed * 0.02f, 0);
		}
	}
}