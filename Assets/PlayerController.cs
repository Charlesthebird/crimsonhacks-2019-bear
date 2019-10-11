using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    [Header("References")]
    public Animator bearAnim;

    [Header("Character Settings")]
    public float speed = 2;
    public float maxSpeed = 3;
    public bool squareInputMove = true;


    Rigidbody rb;
    public static PlayerController instance;
	void Awake() {
        instance = this;
        rb = GetComponent<Rigidbody>();
	}

    public bool useGoodInput = true;
    [HideInInspector]
    public Vector3 inputRequest;
	void Update ()
    {
        GroundCheck();
        ProcessInput();
        // Debug.Log(inputRequest.magnitude);
    }
    [Range(0,1)]
    public float xzDecayFactor = .1f;
    public float xzMoveFactorJumping = .5f;
    public float extraFallVel = 3;
    public float fastSpeedMultiplier = 1.5f;
    private void FixedUpdate()
    {
        // get the current xy velocity and the additional velocity we can add to it this frame
        var xzVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        // ---------- ANIMATE ----------- //
        // update the animator
        SetAnimatorProperties(xzVel.magnitude);
        // ---------- MOVE ----------- //
        var additionalVelocity = (inputRequest * speed);
        // check if the new velocity goes over the maximum velocity
        if(xzVel.magnitude + additionalVelocity.magnitude > maxSpeed)
        {
            // only add what will get us to the maximum velocity
            // clamp at 0 to allow external factors pushing us
            additionalVelocity = xzVel * Mathf.Max(0, xzVel.magnitude - maxSpeed);
        }
        // add the requested force (the derivative of the additional velocity)
        var xyForce = additionalVelocity / Time.fixedDeltaTime;
        // ---------- JUMP ----------- //
        if(jumpRequest)
        {
            rb.velocity = Vector3.zero;
            rb.isKinematic = true;
            rb.isKinematic = false;
            rb.AddForce((Vector3.up * jumpVel / Time.fixedDeltaTime));// - additionalVelocity / Time.fixedDeltaTime);
            jumpRequest = false;
        }
        if (jumpHalt)
        {
            rb.AddForce(Vector3.down * (extraFallVel / Time.fixedDeltaTime));
            jumpHalt = false;
        }
        // ---------- DRAG ----------- //
        // add drag to xz
        rb.AddForce((-xzVel / Time.fixedDeltaTime) * (xzDecayFactor));
        if (fastSpeed)
        {
            xyForce *= fastSpeedMultiplier;
        }
        // ---------- IN-AIR BEHAVIOUR ----------- //
        if (!isOnGround)
        {
            rb.AddForce(xyForce * xzMoveFactorJumping);
        }
        else {
            rb.AddForce(xyForce);
        }
    }
    void SetAnimatorProperties(float characterSpeed)
    {
        // Debug.Log(characterSpeed, gameObject);
        bearAnim.SetFloat("Speed", characterSpeed);
        bearAnim.SetBool("IsJumping", isJumping);
        bearAnim.SetBool("IsOnGround", isOnGround);
    }
    bool fastSpeed = false;
    void ProcessInput()
    {
        // get the movement input from the joystick or arrow keys
        ProcessInputMoveRequest();
        // get the jump
        if (Input.GetButtonDown("Fire1")) { TryToJump(); }
        else if (Input.GetButtonUp("Fire1")) { jumpHalt = true; }


        if (Input.GetButton("Fire3")) { fastSpeed = true; }
        else { fastSpeed = false; }
    }

    [Header("Jump Properties")]
    public LayerMask groundCheckMask;
    public float groundCheckDist = .6f; // player's collider sphere has radius of 1
    public bool isOnGround = false;
    public bool isJumping = false;
    public float jumpVel = 10;
    bool jumpRequest = false;
    bool jumpHalt = false;
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(.5f,.2f,1,.8f);
        Gizmos.DrawWireSphere(transform.position + (Vector3.down * .2f), .4f);
    }
    void GroundCheck()
    {
        if (Input.GetButtonDown("Fire2")) {
            isJumping = false;
            isOnGround = true;
            jumpRequest = true;
            return;
        }
        /*
        RaycastHit hit;
        Physics.Raycast(transform.position, Vector3.down * groundCheckDist, out hit,
            groundCheckDist,
            groundCheckMask,
            QueryTriggerInteraction.Ignore);
        */
        var overlaps = Physics.OverlapSphere(transform.position + (Vector3.down * .2f), 
            .4f, groundCheckMask, QueryTriggerInteraction.Ignore);
        // save whether or not we are on the ground
        // if(hit.collider == null) {
        if (overlaps.Length == 0) {
            isOnGround = false;
            // if we jumped and were still on the ground, we finally left the ground, 
            // so we can say that we're not jumping now for real
            if(isJumping) { isJumping = false; }
        }
        else { isOnGround = true; }
    }
    void TryToJump()
    {
        // check if jumping already or not on the ground
        if(isJumping || !isOnGround) { return; }
        // jump on the next fixedUpdate
        jumpRequest = true;
        isJumping = true;
    }
    void ProcessInputMoveRequest()
    {
        if (!useGoodInput)
        {
            inputRequest = new Vector3( Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            return;
        }
        // inputRequest.x and inputRequest.y are both in the range 0..1
        // this means that inputRequest.magnitude can be greater than 1 when x=1 and y=1
        inputRequest = new Vector3(
            Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        if (inputRequest.x == 0 || inputRequest.z == 0) return;
        // changes inputRequest.magnitude from a square to circle input space with magnitude=1
        //Debug.Log("input normalized: " + inputRequest.normalized);
        float angle = Mathf.Acos(Mathf.Abs(inputRequest.normalized.x) / inputRequest.normalized.magnitude);
        //Debug.Log("angle: " + angle * Mathf.Rad2Deg);
        // multiply sine and cosine by hypotenuse here for more accuracy
        // sin = y / hypotenuse, and cos = x / hypotenuse
        // this is the UNIT circle, but the hypotenuse is not 1 at all points!!!
        var processedRequest = new Vector3(
            Mathf.Lerp(0, Mathf.Cos(angle), Mathf.Abs(inputRequest.x)),
            0,
            Mathf.Lerp(0, Mathf.Sin(angle), Mathf.Abs(inputRequest.z)));
        //Debug.Log("cos theta " + Mathf.Cos(angle));
        inputRequest.x = (inputRequest.x < 0) ? -processedRequest.x : processedRequest.x;
        inputRequest.z = (inputRequest.z < 0) ? -processedRequest.z : processedRequest.z;
        //Debug.Log("Processed: " + processedRequest);
        //Debug.Log("Final Input: " + inputRequest);
        // inputRequest *= 1f - (inputRequest.magnitude - 1f);


        if (squareInputMove)
        {
            inputRequest.x = (inputRequest.x < 0) ? -(inputRequest.x * inputRequest.x) : (inputRequest.x * inputRequest.x);
            inputRequest.y = (inputRequest.y < 0) ? -(inputRequest.y * inputRequest.y) : (inputRequest.y * inputRequest.y);
            inputRequest.z = (inputRequest.z < 0) ? -(inputRequest.z * inputRequest.z) : (inputRequest.z * inputRequest.z );
        }
    }
}
