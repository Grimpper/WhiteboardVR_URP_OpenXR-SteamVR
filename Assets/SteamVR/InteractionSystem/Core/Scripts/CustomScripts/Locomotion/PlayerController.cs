using System.Collections;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class PlayerController : MonoBehaviour
{
    // OPTIONS
    public bool allowAirMovement;
    
    // OBJECTS
    private CharacterController characterController;
    public SteamVR_Action_Vector2 joystick;
    public Transform head;
    public RoomCollider roomCollider;
    
    // INPUTS
    private Vector2 input;
    
    // CAMERA
    private Vector3 headForward;
    private Vector3 headRight;
    
    // GRAVITY
    public bool grounded;
    private float gravity = 10;
    
    // PHYSICS
    private Vector3 direction;
    private Vector3 velocity;
    private Vector3 velocityXZ;
    private float speed = 5;
    public float acceleration = 5;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        //roomCollider = GameObject.Find("RoomCollider").GetComponent<RoomCollider>();
    }

    private void Update()
    {
        DoInput();
        CalculateCamera();
        CalculateGround();
        DoMove();
        DoGravity();
        SetCharacterHeight();
        DecideMovementBehaviour();
    }

    void DoInput()
    {
        input = new Vector2(joystick.axis.x, joystick.axis.y);
        input = Vector2.ClampMagnitude(input, 1);
    }

    void CalculateCamera()
    {
        headForward = head.forward;
        headRight = head.right;

        headForward.y = 0;
        headRight.y = 0;

        headForward = headForward.normalized;
        headRight = headRight.normalized;
    }

    void CalculateGround()
    {
        // RAYCAST IMPLEMENTATION
        /*
        int layerMask = 1 << 8;
        float rayLength = 0.2f;
        RaycastHit hit;
        Ray ray = new Ray(head.position - head.localPosition.y * Vector3.up, Vector3.down);

        grounded = Physics.Raycast(ray, out hit, rayLength, ~layerMask);
        */
        // RAYCAST IMPLEMENTATION
        
        grounded = characterController.isGrounded;
        
        
    }
    
    void DoMove()
    {
        direction = headForward * input.y + headRight * input.x;

        velocityXZ = velocity;
        velocityXZ.y = 0;
        
        velocityXZ = Vector3.Lerp(velocityXZ, speed * direction, acceleration * Time.deltaTime);

        velocity = new Vector3(velocityXZ.x, velocity.y, velocityXZ.z);

    }

    void DoGravity()
    {
        if (grounded)
        {
            velocity.y = -0.5f;
        }
        else
        {
            velocity.y -= gravity * Time.deltaTime;
            velocity.y = Mathf.Clamp(velocity.y, -10, 10);

            if (!allowAirMovement)
            {
                velocity.x = 0;
                velocity.z = 0;
            }
        }
    }
    
    void SetCharacterHeight()
    {
        characterController.height = Mathf.Max(head.transform.localPosition.y, characterController.radius);
        characterController.center = new Vector3(characterController.center.x,
            characterController.height / 2, characterController.center.z);
    }

    void DecideMovementBehaviour()
    {
        if (Teleport.instance.teleportAction.lastStateUp)
        {
            StartCoroutine(WaitForTeleport());
        }

        if (joystick.axis.magnitude > 0)
        {
            SetCharacterTransform();
        }

        if (characterController.enabled && !roomCollider.insideCollider)
        {
            characterController.Move(velocity * Time.deltaTime); 
        }
        else if(roomCollider.insideCollider && joystick.axis.magnitude > 0)
        {
            characterController.enabled = false;
            SetPlayerTransform();
            roomCollider.insideCollider = false;
            characterController.enabled = true;
        }  
    }
    
    IEnumerator WaitForTeleport()
    {
        characterController.enabled = false;
        yield return new WaitForSeconds(0.1f);
        characterController.enabled = true;
    }
    
    void SetCharacterTransform()
    {
        characterController.center = Vector3.ProjectOnPlane(head.localPosition, Vector3.up) +
                                     characterController.center.y * Vector3.up;
    }

    void SetPlayerTransform()
    {
        int layerMask = 1 << 8;
        float rayLength = 100;
        var ray = new Ray(roomCollider.lastCollisionTransform, Vector3.down);

        Physics.Raycast(ray, out var raycastHit, rayLength, ~layerMask);
        
        
        Debug.DrawRay(roomCollider.lastCollisionTransform,rayLength * Vector3.down, Color.red, duration: 10000, false);

        transform.position = raycastHit.point - Vector3.ProjectOnPlane(head.position - transform.position, Vector3.up);
        
        // DEBUGGING //
        /*
        Debug.Log("RAYCAST POINT: " + raycastHit.point);
        Debug.Log("PLAYER TO: " + transform.position);
        Debug.Log("RAYCAST: " + head.position);
        Debug.Log("CAMERA IS ON: " + head.position);
        */
    }
}
