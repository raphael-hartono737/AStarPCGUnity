using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Movement settings
    public float moveSpeed = 4.0f;
    public float sprintMultiplier = 2.0f;
    public float acceleration = 10.0f;
    public float deceleration = 10.0f;
    public float jumpForce = 5.0f;
    public float groundCheckDistance = 0.3f;

    // Screen shake settings
    public float sprintShakeIntensity = 0.1f;
    public float sprintShakeDuration = 0.1f;
    private Coroutine shakeCoroutine;

    // Components and references
    public static PlayerMovement Instance { get; private set; }
    [SerializeField] private bool isGrounded = false;
    [SerializeField] private float currentSpeed;
    [SerializeField] private Rigidbody rb;
    public LayerMask groundLayer;

    [SerializeField] public Player playerGO; 

    // Smooth velocity for better control
    private Vector3 smoothVelocity = Vector3.zero;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Ensure only one instance exists
        }
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        ConfigureRigidbody();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void ConfigureRigidbody()
    {
        rb.mass = 1.0f;
        rb.drag = 0.1f; // Optional: Add some drag to reduce sliding
        rb.angularDrag = 0.05f;
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    private void Update()
    {
        Move();
        Jump();
    }

    private void Move()
    {
        float moveX = Input.GetAxis("Horizontal"); // A/D or Left/Right arrow keys
        float moveZ = Input.GetAxis("Vertical");   // W/S or Up/Down arrow keys

        // Create a movement vector
        Vector3 moveDirection = transform.right * moveX + transform.forward * moveZ;

        // Apply acceleration and deceleration
        if (moveDirection.magnitude > 0)
        {
            smoothVelocity = Vector3.Lerp(smoothVelocity, moveDirection.normalized * currentSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            smoothVelocity = Vector3.Lerp(smoothVelocity, Vector3.zero, deceleration * Time.deltaTime);
        }

        // Sprinting
        currentSpeed = isGrounded && Input.GetButton("Sprint") ? moveSpeed * sprintMultiplier : moveSpeed;

        // Apply velocity to Rigidbody
        rb.velocity = new Vector3(smoothVelocity.x, rb.velocity.y, smoothVelocity.z);

        // Trigger screen shake when sprinting
        if (isGrounded && Input.GetButton("Sprint") && moveDirection.magnitude > 0)
        {
            if (shakeCoroutine == null)
            {
                shakeCoroutine = StartCoroutine(ScreenShake(sprintShakeIntensity, sprintShakeDuration));
            }
        }
        else
        {
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
                shakeCoroutine = null;
            }
        }
    }

    private void Jump()
    {
        // If the player is grounded and presses the jump key (space)
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    private IEnumerator ScreenShake(float intensity, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Generate random offsets for the camera
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;
            float z = Random.Range(-1f, 1f) * intensity;

            // Apply the shake to the camera's position
            Camera.main.transform.localPosition += new Vector3(x, y, z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Reset the camera position after shaking
        Camera.main.transform.localPosition = Vector3.zero;
        shakeCoroutine = null;
    }
}