using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;



public class PlayerMovement : MonoBehaviour {
    public ReferenceManager rm;

    public float friction = 50;

    Vector2 moveInput;  // Use Vector2 to look at / augment character movement  //          If we are going left, right, up, down we will be storing it in ' moveInput'
    Vector2 throwInput;
    Vector2 throwDirection;

    bool onJump, releaseJump, onFire, releaseFire;

    Rigidbody2D playerRigidBody;
    Animator playerAnimator;
    CapsuleCollider2D playerCapsuleCollider;

    public bool grounded, throwCharging, pullCharging;

    float chargeTime, pullChargeTime;

    [SerializeField] float maxSwingSpeed = 8;
    [SerializeField] float maxWalkSpeed = 2;
    [SerializeField] float walkSpeed = 10f;
    [SerializeField] float jumpSpeed = 5f;
    [SerializeField] float pullSpeed = 2f;

    [SerializeField] InputActionReference move, grapple, fire, jump;

    int soundCue;


    void Start() {

        playerRigidBody = GetComponent<Rigidbody2D>();  //  the GetComponent<>(); method will hold the unity component we are trying to access, hence our playerRigidBody which is of the RigidBody2D class will be held as a parameter in GetComponent<>(); as GetComponent<RigidBody2D>();
        playerAnimator = GetComponent<Animator>();  //  these variables are global because we will be accessing them throughout the program
        playerCapsuleCollider = GetComponent<CapsuleCollider2D>();  //  Set up a reference to alter the players referenced capsule collider
    }

    private void Update() {

        GetInputs();

        if (onJump && grounded) {

            playerRigidBody.AddForce(Vector2.up * jumpSpeed, ForceMode2D.Impulse);
            rm.audioSrc.PlayOneShot(rm.sounds[0]);
        }

        else if (onJump && !grounded && rm.hookThrow.attached) {

            playerRigidBody.AddForce(playerRigidBody.velocity*0.5f, ForceMode2D.Impulse);
            rm.grappleHook.ResetRope();
            rm.hookThrow.ResetThrow();
        }

        if(grounded && moveInput.x == 0) {

            playerRigidBody.AddForce(Vector2.right * friction * Mathf.Clamp(-playerRigidBody.velocity.x, -0.2f, 0.2f), ForceMode2D.Force);
        }

        if(throwCharging) {

            //if(chargeTime - Time.time < )
        }


        if (onFire) {

            if (rm.hookThrow.attached) {

                pullCharging = true;
                pullChargeTime = Time.time;
                rm.grappleHook.pull = true;
            }
            else if (!throwCharging) {

                throwCharging = true;
                chargeTime = Time.time;
                rm.audioSrc.PlayOneShot(rm.sounds[5]);
                soundCue = 1;
            }
        }

        else if (releaseFire) {

            if (throwCharging) {

                throwCharging = false;
                rm.grappleHook.ResetRope();
                rm.hookThrow.ResetThrow();
                rm.hookThrow.Throw(Mathf.Clamp(Time.time - chargeTime,0.1f, 1f), throwDirection);
            }

            else if (pullCharging && rm.hookThrow.attached) {

                pullChargeTime = Mathf.Clamp(Time.time - pullChargeTime, 0.1f, 0.5f);
                rm.grappleHook.Pull(pullChargeTime * pullSpeed);
            }

            else if (pullCharging)
                pullCharging = false;
        }


        if(rm.hookThrow.attached) {

            if (moveInput.y > 0) {
                if (rm.grappleHook.GetDistance(true) < rm.grappleHook.maxLength + 0.1f)
                    rm.grappleHook.maxLength -= 0.02f;
            }
            else if (moveInput.y < 0) {
                if (rm.grappleHook.maxLength - rm.grappleHook.GetDistance(true) < 0.1f)
                    rm.grappleHook.maxLength += 0.02f;

            }
        }
    }

    void FixedUpdate() {

        CheckGrounded();

        Move();

        FlipSprite();
    }


    void GetInputs() {

        moveInput = move.action.ReadValue<Vector2>();
        throwInput = grapple.action.ReadValue<Vector2>();

        onFire = grapple.action.WasPressedThisFrame();
        releaseFire = grapple.action.WasReleasedThisFrame();

        onJump = jump.action.WasPressedThisFrame();
        releaseJump = jump.action.WasReleasedThisFrame();

        if (throwInput.y > 0.3f)
            throwDirection = new Vector2(0,1.2f);
        else if (throwInput.y < -0.3f)
            throwDirection = new Vector2(0, -0.5f);
        else if (throwInput.x > 0.3f)
            throwDirection = new Vector2(0.6f, 0.8f);
        else if (throwInput.x < -0.3f)
            throwDirection = new Vector2(-0.6f, 0.8f);
    }


    void CheckGrounded() {

        RaycastHit2D boxCast = Physics2D.BoxCast(this.transform.position, new Vector2(0.5f,0.16f), 0, Vector2.down, 0.5f, rm.terrainLayer);
        //Debug.Log(boxCast.collider != null);
        grounded = (boxCast.collider != null);
    }


    void Pull() {

        if (!grounded && rm.hookThrow.attached) {

            playerRigidBody.AddForce((Vector2.up * jumpSpeed) + new Vector2(0, -playerRigidBody.velocity.y * playerRigidBody.mass) * 0.8f, ForceMode2D.Impulse);
            rm.grappleHook.ResetRope();
            rm.hookThrow.ResetThrow();
        }
    }


    void Move() {

        bool valid = true;

        if(!grounded && rm.hookThrow.attached) {
            if (Mathf.Abs(playerRigidBody.velocity.magnitude) > maxSwingSpeed) {
                if ((playerRigidBody.velocity.x > 0 && moveInput.x > 0) || (playerRigidBody.velocity.x < 0 && moveInput.x < 0)) {
                    valid = false;
                }
            }
            if (valid)
                playerRigidBody.AddForce(Vector2.right * moveInput.x * walkSpeed * (Mathf.Clamp(playerRigidBody.velocity.x, 0.005f, 0.08f) + Mathf.Clamp(Mathf.Abs(playerRigidBody.velocity.y), 0.005f, 0.08f)));
        } else {

            if (Mathf.Abs(playerRigidBody.velocity.x) > maxWalkSpeed) {
                if ((playerRigidBody.velocity.x > 0 && moveInput.x > 0) || (playerRigidBody.velocity.x < 0 && moveInput.x < 0)) {
                    valid = false;
                }
            }
            if (valid)
                playerRigidBody.AddForce(Vector2.right * moveInput.x * walkSpeed);
        }

        if (moveInput.x != 0)                                //  Instructor did this differently in Video 83 of TileVania //     bool playerHasHorizontalSpeed = Mathf.Abs(playerRigidBody.velocity.x) > Mathf.Epsilon; followed by :     playerAnimator.SetBool("isRunning", true);
            playerAnimator.SetBool("isRunning", true);
        else
            playerAnimator.SetBool("isRunning", false);
    }


    void FlipSprite() {

        bool playerHasHorizontalSpeed = Mathf.Abs(playerRigidBody.velocity.x) > Mathf.Epsilon;  //  playerRigidBody by default faces to the right side, so if the value of x < 0 (or epsilon) than the character will instead be facing left                                                                                          
                                                                                                // The value of the boolean will automatically be false, so in the following if statement the characer should be facing left whenever the value of x is negative // when the player presses the corresponding key to move left
        if(playerHasHorizontalSpeed) 
        {
            transform.localScale = new Vector2(Mathf.Sign(playerRigidBody.velocity.x), 1f); //  if the x value of playerRigidBody is greater than Mathf.Sign() the player will face right, and conversly playerRigidBody will face left. //     Mathf.Sign(); return the value of 
        }      
    }
}
