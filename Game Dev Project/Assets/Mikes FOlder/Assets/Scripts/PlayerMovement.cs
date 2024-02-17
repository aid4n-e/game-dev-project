using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;



public class PlayerMovement : MonoBehaviour
{
    Vector2 moveInput;  // Use Vector2 to look at / augment character movement  //          If we are going left, right, up, down we will be storing it in ' moveInput'

    Rigidbody2D playerRigidBody;
    Animator playerAnimator;
    CapsuleCollider2D playerCapsuleCollider;
    PolygonCollider2D playerPolygonCollider;


    [SerializeField] float  playerSpeed = 10f;    //  create a variable that is adjustable within the Unity Engine Window
    [SerializeField] float playerJump = 5f;
    [SerializeField] float climbingSpeed = 5f;

    void Start()
    {
        playerRigidBody = GetComponent<Rigidbody2D>();  //  the GetComponent<>(); method will hold the unity component we are trying to access, hence our playerRigidBody which is of the RigidBody2D class will be held as a parameter in GetComponent<>(); as GetComponent<RigidBody2D>();
        
        playerAnimator = GetComponent<Animator>();  //  these variables are global because we will be accessing them throughout the program

        playerCapsuleCollider = GetComponent<CapsuleCollider2D>();  //  Set up a reference to alter the players referenced capsule collider
    }
    void Update()
    {
        Run();
        FlipSprite();
        ClimbLadder();
    }
    void OnMove(InputValue value)       //      take 'value' we recieve from our player input and store it in 'moveInput' // Vector2 
    {
        moveInput = value.Get<Vector2>();   //
       // Debug.Log(moveInput);
    }
     void OnJump(InputValue value)
     {
         // if (playerPolygonCollider.IsTouchingLayers(LayerMask.GetMask("Ground")))        //  Here we are checking if the LayerMask, in this case our "Ground" layer is touching the players capsule collider

         if (playerCapsuleCollider.IsTouchingLayers(LayerMask.GetMask("Ground")) || playerCapsuleCollider.IsTouchingLayers(LayerMask.GetMask("ClimbLadder"))) // Wall stick bug was fixed by adding a second capsule collider holding a physics 2D material with 0 to both stats. then altered to be wider than the original capsule collider we use for jumping.
         {   
             if (value.isPressed)
             {
                 Debug.Log("tried to jump");
                 playerRigidBody.velocity += new Vector2(0f, playerJump);
             }     
         }
     }
    void ClimbLadder()
    {
        if (playerCapsuleCollider.IsTouchingLayers(LayerMask.GetMask("ClimbLadder"))) 
        { 
            Vector2 climbVelocity = new Vector2(playerRigidBody.velocity.x, moveInput.y * climbingSpeed);
            playerRigidBody.velocity = climbVelocity;
            
            if (moveInput.y != 0)// || moveInput.y == 0)                                //  Instructor did this differently in Video 83 of TileVania //     bool playerHasHorizontalSpeed = Mathf.Abs(playerRigidBody.velocity.x) > Mathf.Epsilon; followed by :     playerAnimator.SetBool("isRunning", true);
                playerAnimator.SetBool("isClimbingLadder", true);
        
            else
                playerAnimator.SetBool("isClimbingLadder", false);


        }

        
    }
/*
     void ClimbLadder()
     {
         if (!playerCapsuleCollider.IsTouchingLayers(LayerMask.GetMask("ClimbLadder"))) // Wall stick bug was fixed by adding a second capsule collider holding a physics 2D material with 0 to both stats. then altered to be wider than the original capsule collider we use for jumping.
         { return;  }
             Debug.Log("tried to climb the ladder");

             Vector2 ClimbingVelocity = new Vector2(playerRigidBody.velocity.x ,playerRigidBody.velocity.y * climbingSpeed);    //  When the player presses a key or button that corresponds to the player moving right or left the method will move the character at the corressponding speed which can be adjusted in the Unity engine because 'playerSpeed' is in a [serializefield] variable
             playerRigidBody.velocity = ClimbingVelocity;   
     }
            //  Has a bad reaction with the tilemap colliders trigger on the ladder object

 */
    void Run()
    {
        Vector2 playerVelocity = new Vector2(moveInput.x * playerSpeed,playerRigidBody.velocity.y);    //  When the player presses a key or button that corresponds to the player moving right or left the method will move the character at the corressponding speed which can be adjusted in the Unity engine because 'playerSpeed' is in a [serializefield] variable
        playerRigidBody.velocity = playerVelocity;

        if(moveInput.x != 0)                                //  Instructor did this differently in Video 83 of TileVania //     bool playerHasHorizontalSpeed = Mathf.Abs(playerRigidBody.velocity.x) > Mathf.Epsilon; followed by :     playerAnimator.SetBool("isRunning", true);
            playerAnimator.SetBool("isRunning", true);

        else
            playerAnimator.SetBool("isRunning", false);
        
      //  bool playerHasHorizontalSpeed = Mathf.Abs(playerRigidBody.velocity.y) > Mathf.Epsilon;
       // playerAnimator.SetBool("isRunning", playerHasHorizontalSpeed);
    }
    void FlipSprite()
    {
        bool playerHasHorizontalSpeed = Mathf.Abs(playerRigidBody.velocity.x) > Mathf.Epsilon;  //  playerRigidBody by default faces to the right side, so if the value of x < 0 (or epsilon) than the character will instead be facing left                                                                                          
                                                                                                // The value of the boolean will automatically be false, so in the following if statement the characer should be facing left whenever the value of x is negative // when the player presses the corresponding key to move left
        if(playerHasHorizontalSpeed) 
        {
            transform.localScale = new Vector2(Mathf.Sign(playerRigidBody.velocity.x), 1f); //  if the x value of playerRigidBody is greater than Mathf.Sign() the player will face right, and conversly playerRigidBody will face left. //     Mathf.Sign(); return the value of 
        }      
    }
}
