using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    int leftFingerId, rightFingerId;
    float halfScreenWidth;
    // finger tracking
    public Transform cameraTransform;
    public CharacterController characterController;
    public float cameraSensitivit;
    //player settings
    public float moveSpeed;
    public float moveInputDeadZone;
    //movement settings
    Vector2 moveTouchStartPosition;
    Vector2 moveInput;
    [Header("Gravity & Jumping")]
    public float stickToGroundForce = 10;
    public float gravity = 10;
    public float jumpForce = 10;

    private float verticalVelocity;

    [Header("Ground Check")]
    public Transform groundCheck;
    public LayerMask groundLayers;
    public float groundCheckRadius;

    private bool grounded;

    //camera controle
    Vector2 lookInput;
    float cameraPitch;
    void Start()
    {
        leftFingerId = -1;
        rightFingerId = -1;
        halfScreenWidth = Screen.width / 2;
        moveInputDeadZone = Mathf.Pow(Screen.height / moveInputDeadZone, 2);
    }

    private void FixedUpdate()
    {
        grounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayers);
    }
    void Update()
    {
        GetTouchInput();

        if(rightFingerId != -1)
        {
            LookAround();
        }
        if(leftFingerId != -1)
        {
            Move();
        }
        VerticalMove();
        
    }

    void GetTouchInput()
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);

            switch (t.phase)
            {
                case TouchPhase.Began:
                    if (t.position.x < halfScreenWidth && leftFingerId == -1)
                    {
                        leftFingerId = t.fingerId;
                        moveTouchStartPosition = t.position;
                    }
                    else if (t.position.x > halfScreenWidth && rightFingerId == -1)
                    {
                        rightFingerId = t.fingerId;
                        Debug.Log("Tracking right finger");

                    }
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (t.fingerId == leftFingerId)
                    {
                        leftFingerId = -1;
                        Debug.Log("Stopped tracking left finger");
                    }
                    else if (t.fingerId == rightFingerId)
                    {
                        rightFingerId = -1;
                        Debug.Log("Stopped tracking right finger");
                    }
                    break;
                case TouchPhase.Moved:
                    if(t.fingerId == rightFingerId)
                    {
                        // get input for looking around
                        lookInput = t.deltaPosition * cameraSensitivit * Time.deltaTime;
                    }
                    else if(t.fingerId == leftFingerId)
                    {
                        moveInput = t.position - moveTouchStartPosition;
                    }

                    break;
                case TouchPhase.Stationary:
                    if(t.fingerId == rightFingerId)
                    {
                        // set the look input to zero, if finger is still
                        lookInput = Vector2.zero;
                    }
                    break;
            }
        }
    }
    void LookAround()
    {
        // vertical rotation
        cameraPitch = Mathf.Clamp(cameraPitch - lookInput.y, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0, 0);

        // horizontal rotation
        transform.Rotate(transform.up, lookInput.x);
    }
    void Move()
    {
        if (moveInput.sqrMagnitude <= moveInputDeadZone) return;
        {
            Vector2 movementDirection = moveInput.normalized * moveSpeed * Time.deltaTime;
            characterController.Move(transform.right * movementDirection.x + transform.forward * movementDirection.y);
        }
    }
    void VerticalMove()
    {
        //claculate y (vertical) movement
        if (grounded && verticalVelocity <= 0) verticalVelocity = -stickToGroundForce * Time.deltaTime;
        else verticalVelocity -= gravity * Time.deltaTime;

        Vector3 verticalMovement = transform.up * verticalVelocity;
        characterController.Move(verticalMovement * Time.deltaTime);
    }
    public void Jump()
    {
        if (grounded) verticalVelocity = jumpForce;
    }
}
