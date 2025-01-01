using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Vector3 velocity;
    private Vector3 playerMovementInput;
    private Vector2 playerMouseInput;
    private float xRotation;

    [SerializeField] private Transform playerCamera;
    [SerializeField] private CharacterController controller;

    [Space]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float sensitivity = 2f;

    void Update()
    {
        // Get player movement input
        playerMovementInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
        playerMouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        // Handle movement and camera rotation
        MovePlayer();
        MovePlayerCamera();
    }

    private void MovePlayer()
    {
        // Convert input to world space movement
        Vector3 moveVector = transform.TransformDirection(playerMovementInput);

        // Vertical movement (e.g., flying up/down)
        if (Input.GetKey(KeyCode.Space))
        {
            velocity.y = 1f;
        }
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            velocity.y = -1f;
        }
        else
        {
            velocity.y = 0f; // Reset vertical velocity when neither key is pressed
        }

        // Apply movement to the CharacterController
        controller.Move(moveVector * speed * Time.deltaTime);
        controller.Move(velocity * speed * Time.deltaTime);
    }

    private void MovePlayerCamera()
    {
        if (Input.GetMouseButton(1))
        {
            xRotation -= playerMouseInput.y * sensitivity;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Prevent flipping

            transform.Rotate(0f, playerMouseInput.x * sensitivity, 0f);

            playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
        
     
    }
}
