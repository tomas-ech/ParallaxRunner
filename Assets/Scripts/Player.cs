using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    //Components
    private Rigidbody2D rb;
    private Animator playerAnim;

    [Header("Speed Info")]
    [SerializeField] private float maxSpeed;
    [SerializeField] private float speedMultiplier;
    [SerializeField] private float milestoneIncreaser;
    private float speedMilestone;

    [Header("Move Info")]
    [SerializeField] private bool playerUnlocked;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float doubleJumpForce;
    private bool canDoubleJump;

    [Header("Slide Info")]
    [SerializeField] private float slideSpeed;
    [SerializeField] private float slideTimer;
    [SerializeField] private float slideCooldown;
    private float slideCooldownCounter;
    private float slideTimerCounter;
    private bool isSliding;

    [Header("Collision Info")]
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private float ceilingCheckDistance;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private Vector2 wallCheckSize;
    private bool isGrounded;
    private bool isWallDetected;
    private bool ceilingDetected; 
    [HideInInspector] public bool ledgeDetected;

    [Header("Ledge Info")]
    [SerializeField] private Vector2 offset1; //beforeClimb
    [SerializeField] private Vector2 offset2; //AfterClimb

    private Vector2 climbBegunPosition;
    private Vector2 climbOverPosition;

    private bool canGrabLedge = true;
    private bool canClimb; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerAnim = GetComponent<Animator>();

        speedMilestone = milestoneIncreaser;
    }

    void Update()
    {
        CheckCollision();
        AnimatorControllers();
        SpeedController();
        CheckForSlide();
        CheckForLedge();
        CheckInput();

        slideTimerCounter -= Time.deltaTime;
        slideCooldownCounter -= Time.deltaTime;

        if (playerUnlocked)
        {
            PlayerMovement();
        }

        if (isGrounded)
        {
            canDoubleJump = true;
        }
    }

    private void CheckForSlide()
    {
        if (slideTimerCounter < 0 && !ceilingDetected)
        {
            isSliding = false;
        }
    }

    private void CheckForLedge()
    {
        if (ledgeDetected && canGrabLedge)
        {
            canGrabLedge = false;

            Vector2 ledgePosition = GetComponentInChildren<LedgeDetection>().transform.position;

            climbBegunPosition = ledgePosition + offset1;
            climbOverPosition = ledgePosition + offset2;

            canClimb = true;
        }

        if (canClimb)
        {
            transform.position = climbBegunPosition;
        }
    }

    private void SpeedController()
    {
        if (movementSpeed == maxSpeed)
        {
            return;
        }

        if (transform.position.x > speedMilestone)
        {
            speedMilestone += milestoneIncreaser;

            movementSpeed *= speedMultiplier;
            milestoneIncreaser *= speedMultiplier;

            if (movementSpeed > maxSpeed)
            {
                movementSpeed = maxSpeed;
            }
        }
    }

    private void PlayerMovement()
    {

        if (isWallDetected)
        {
            return;
        }

        if (isSliding)
        {
            rb.velocity = new Vector2(slideSpeed, rb.velocity.y);
        }
        else
        {
            rb.velocity = new Vector2(movementSpeed, rb.velocity.y);
        }
    }

    private void LedgeClimbMechanic()
    {
        canClimb = false;
        transform.position = climbOverPosition;
        //Invoke nos permite usar una funcion con cierto delay para empezar
        Invoke("AllowLedgeGrab", 0.1f);
    }

    //Esta funcion por ser muy corta se puede usar => indicando que va a retornar un solo valor, en este caso un booleano
    private void AllowLedgeGrab() => canGrabLedge = true;

    private void SlideMechanic()
    {
        if (rb.velocity.x != 0  && slideCooldownCounter < 0)
        {
            isSliding = true;
            slideTimerCounter = slideTimer;
            slideCooldownCounter = slideCooldown;
        }
    }
    private void JumpMechanic()
    {

        if (isSliding)
        {
            return; //Al poner return en este condicional, los demas condicionales
            //en esta misma funcion no se van a activar
        }

        if (isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
        else if (canDoubleJump)
        {
            canDoubleJump = false;
            rb.velocity = new Vector2(rb.velocity.x, doubleJumpForce);
        }
    }
    private void CheckInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            JumpMechanic();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            SlideMechanic();
        }
    }
    private void AnimatorControllers()
    {
        playerAnim.SetFloat("xVelocity", rb.velocity.x);
        playerAnim.SetFloat("yVelocity", rb.velocity.y);

        playerAnim.SetBool("isGrounded", isGrounded);
        playerAnim.SetBool("canDoubleJump", canDoubleJump);
        playerAnim.SetBool("isSliding", isSliding);
        playerAnim.SetBool("canClimb", canClimb);
    }
    private void CheckCollision()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround);

        isWallDetected = Physics2D.BoxCast(wallCheck.position, wallCheckSize, 0f, Vector2.zero, 0f, whatIsGround);

        ceilingDetected = Physics2D.Raycast(transform.position, Vector2.up, ceilingCheckDistance, whatIsGround);
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y - groundCheckDistance));

        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y + ceilingCheckDistance));

        Gizmos.DrawWireCube(wallCheck.position, wallCheckSize);
    }
}
