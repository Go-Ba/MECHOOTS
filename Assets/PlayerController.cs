using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] SettingsObject settings;
    Rigidbody rb;
    PlayerState currentState;

    [Header("Camera")]
    [SerializeField] Transform camHoriAxis;
    [SerializeField] Transform camVertAxis;
    [SerializeField] Camera playerCam;
    [SerializeField] int slideFoVDelta = 15;
    [Range(-1, 0f)][SerializeField] float slideCamOffset = -0.5f;
    [SerializeField] float FoVSmoothtime;
    [SerializeField] float camOffsetSmoothtime;
    int targetFoV;
    float camTargetOffset;
    float camCurrentOffset;
    [SerializeField] Transform hands;
    [Range(0, 1)][SerializeField] float handRotationLerp = 1f;
    [Range(0, 1)][SerializeField] float handVerticalSmoothtime = 1f;
    [SerializeField] float handVerticalMax = 1f;
    float handVertVelocity;

    [Header("Walking")]
    [SerializeField] float crouchSpeed = 5;
    [SerializeField] float crouchAcceleration = 5;
    [SerializeField] float crouchDeceleration = 5;
    [SerializeField] float walkSpeed = 10;
    [SerializeField] float walkAcceleration = 10;
    [SerializeField] float walkDeceleration = 10;
    [SerializeField] float runSpeed = 20;
    [SerializeField] float runAcceleration = 20;
    [SerializeField] float runDeceleration = 20;
    [SerializeField] float slideMaxSpeed = 30;
    [SerializeField] float slideDeceleration = 20;
    [SerializeField] float slideCooldown = 1f;
    [SerializeField] CapsuleCollider walkingCollider;
    [SerializeField] CapsuleCollider slidingCollider;

    [Header("Jumping")]
    [SerializeField] float jumpForce = 5;
    [SerializeField] float airStrafeForce;
    [SerializeField] float sideBoostSpeed = 5;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] Transform footPosition;
    [SerializeField] float rayDistance = 0.3f;

    [Header("Combat")]
    [SerializeField] int maxHealth = 10;
    int currentHealth;
    [SerializeField] float parryCooldown = 0.5f;
    [SerializeField] float attackCooldown = 0.5f;
    float slideTime;
    float parryTime;
    float attackTime;
    [SerializeField] Transform slicePlane;
    [SerializeField] float planeRotationSpeed = 5;
    [SerializeField] Transform parryCube;
    [SerializeField] float sliceWidth;
    [SerializeField] GameObject sliceParticles;
    bool isDead;

    float jumpStartTime;
    bool hasDoubleJumped;
    bool hasSideBoosted;


    [Header("Audio")]
    [SerializeField] AudioClip sliceSound;
    [SerializeField] AudioClip sliceSuccessSound;
    [SerializeField] AudioClip[] footstepSound;
    [SerializeField] float footstepsPerMetre;
    float timeOfFootstep;
    [SerializeField] AudioClip windLoop;
    [SerializeField] AudioClip slideLoop;
    [SerializeField] AudioClip jumpSound;
    [SerializeField] AudioClip doubleJumpSound;
    [SerializeField] AudioClip parrySound;
    [SerializeField] AudioClip parryWhiffSound;
    [SerializeField] AudioClip deathSound;


    [SerializeField] float sliceVolume = 0.5f;
    [SerializeField] float sliceSuccessVolume = 0.5f;
    [SerializeField] float footstepVolume = 0.4f;
    [SerializeField] float slideVolume = 0.82f;
    [SerializeField] float jumpVolume = 1;
    [SerializeField] float doubleJumpVolume = 1;
    [SerializeField] float parryVolume = 1;
    [SerializeField] float parryWhiffVolume = 1;
    [SerializeField] float deathVolume = 1;




    public static event Action<float> onHealthUpdated;
    public static event Action<float> onSwordAngleChange;
    public static event Action onAttack;
    public static event Action onParry;
    public static event Action onPlayerDeath;


    enum PlayerState
    {
        Walking,
        Running,
        Jumping,
        Sliding,
        Crouching
    }

    // Start is called before the first frame update
    void Start()
    {
        slidingCollider.enabled = false;
        walkingCollider.enabled = true;
        currentHealth = maxHealth;
        targetFoV = settings.FoV;
        rb = GetComponent<Rigidbody>();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    #region State Machine
    // Update is called once per frame
    void Update()
    {
        if (isDead) return;

        Ctrl.HandleInput();

        switch (currentState)
        {
            case PlayerState.Walking:
                WalkingUpdate();
                break;
            case PlayerState.Running:
                RunningUpdate();
                break;
            case PlayerState.Jumping:
                JumpingUpdate();
                break;
            case PlayerState.Sliding:
                SlidingUpdate();
                break;
            case PlayerState.Crouching:
                CrouchingUpdate();
                break;
            default:
                break;
        }
        HandleHandSway();
    }
    void StartRunOrWalk()
    {
        float horizontalSpeed = Vector3.Scale(rb.velocity, new Vector3(1, 0, 1)).magnitude;
        if (settings.autoRun) StartRunning();

        //if above the right speed and not facing opposite your velocity, start running
        if (horizontalSpeed > walkSpeed && CamHorizontalDotVelocity() >= -0.2f)
            StartRunning();
        else
            StartWalking();
    }
    void StartWalking()
    {
        currentState = PlayerState.Walking;
    }
    void WalkingUpdate()
    {
        if (settings.autoRun)
            StartRunning();

        targetFoV = settings.FoV;

        HandleHorizontalMovement(Ctrl.WASD, walkSpeed, walkAcceleration, walkDeceleration);
        HandleCamera();
        HandleSlicePlane();
        CheckFalling();
        if (Ctrl.LMBDown) DoSlice();
        if (Ctrl.RMBDown) DoParry();

        //jump
        if (Ctrl.jumpDown)
            StartJumping();
        if (Ctrl.sprint)
            StartRunning();
        if (Ctrl.crouchDown)
            StartCrouching();

        HandleFootsteps();
    }

    void StartRunning()
    {
        currentState = PlayerState.Running;
    }
    void RunningUpdate()
    {
        //if input is in the opposite direction of facing direction, stop running
        if (!settings.autoRun)
            if (CamHorizontalDotInput() < 0 || Ctrl.WASD.magnitude == 0)
                StartWalking();

        targetFoV = settings.FoV;

        HandleHorizontalMovement(Ctrl.WASD, runSpeed, runAcceleration, runDeceleration);
        HandleCamera();
        HandleSlicePlane();
        CheckFalling();
        if (Ctrl.LMBDown) DoSlice();
        if (Ctrl.RMBDown) DoParry();

        //jump
        if (Ctrl.jumpDown)
            StartJumping();
        if (Ctrl.crouch)
            StartSliding();

        HandleFootsteps();
    }
    void StartCrouching()
    {
        slidingCollider.enabled = true;
        walkingCollider.enabled = false;
        currentState = PlayerState.Crouching;
        camTargetOffset = slideCamOffset;
    }
    void CrouchingUpdate()
    {
        HandleHorizontalMovement(Ctrl.WASD, crouchSpeed, crouchAcceleration, crouchDeceleration);
        HandleCamera();
        HandleSlicePlane();
        CheckFalling();
        if (Ctrl.LMBDown) DoSlice();
        if (Ctrl.RMBDown) DoParry();

        //jump
        if (Ctrl.jumpDown)
        {
            StopCrouching();
            StartJumping();
        }
        if (!Ctrl.crouch)
        {
            StopCrouching();
            StartRunOrWalk();
        }
    }
    void StopCrouching()
    {
        slidingCollider.enabled = false;
        walkingCollider.enabled = true;
        camTargetOffset = 0;
    }
    void StartFalling()
    {
        currentState = PlayerState.Jumping;
        hasDoubleJumped = false;
    }
    void StartJumping()
    {
        rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
        jumpStartTime = Time.time;
        currentState = PlayerState.Jumping;
        hasDoubleJumped = false;
        hasSideBoosted = false;
        AudioManager.Play(jumpSound, 0.1f, jumpVolume);
    }
    void JumpingUpdate()
    {
        HandleCamera();
        HandleSlicePlane();
        if (Ctrl.LMBDown) DoSlice();
        if (Ctrl.RMBDown) DoParry();
        bool jumpBufferEnded = Time.time - jumpStartTime > 0.250f;
        if (jumpBufferEnded && IsGrounded())
        {
            StopJumping();
            if (Ctrl.crouch)
                StartSliding();
            else
                StartRunOrWalk();
        }
        if (!hasDoubleJumped && Ctrl.jumpDown)
        {
            if (Ctrl.WASD.magnitude > 0)
                SetSpeedInputDirection();

            rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
            hasDoubleJumped = true;
            hasSideBoosted = false;

            AudioManager.Play(doubleJumpSound, 0.1f, doubleJumpVolume);
        }
        if (!hasSideBoosted)
            CheckSideBoost();

        AirStrafing();
    }
    void StopJumping()
    {
        AudioManager.StopLoop("windLoop");
    }
    //a boost you can use in the air to push yourself to one side
    //can only be used once per jump, but is useful for adjusting angle
    void CheckSideBoost()
    {
        if (!Ctrl.leftDown && !Ctrl.rightDown)
            return;

        var boostDir = Ctrl.rightDown ? camHoriAxis.right : -camHoriAxis.right;

        //mult = 1 if boostDir is perpendicular to velocity direction
        //mult = 0 if boostDir is parallel with velocity direction
        //this is to stop the side boost from being used to boost forward
        var dot = Vector3.Dot(boostDir.normalized, rb.velocity.normalized);
        float mult = 1 - Mathf.Abs(dot);

        rb.velocity += boostDir * sideBoostSpeed * mult;

        hasSideBoosted = true;

    }
    void StartSliding()
    {
        if (Time.time - slideTime < slideCooldown)
            return;

        currentState = PlayerState.Sliding;
        targetFoV = settings.FoV + slideFoVDelta;
        camTargetOffset = slideCamOffset;

        if (Ctrl.WASD.magnitude > 0)
            SetSpeedInputDirection(slideMaxSpeed);
        else if (GetFlatVelocity().magnitude > 0)
            SetSpeed(slideMaxSpeed);
        else
            SetSpeedFacing(slideMaxSpeed);

        AudioManager.StartLoop(slideLoop, slideVolume, "slideLoop");

        slidingCollider.enabled = true;
        walkingCollider.enabled = false;
    }
    void SlidingUpdate()
    {
        HandleCamera();
        HandleSlicePlane();
        ApplyDrag(slideDeceleration);
        if (Ctrl.LMBDown) DoSlice();
        if (Ctrl.RMBDown) DoParry();
        if (!Ctrl.crouch)
        {
            StopSliding();
            StartRunOrWalk();
        }
        if (Ctrl.jumpDown)
        {
            StopSliding();
            StartJumping();
        }
        if (GetFlatVelocity().magnitude <= crouchSpeed)
        {
            StopSliding();
            StartCrouching();
        }
    }
    void StopSliding()
    {
        targetFoV = settings.FoV;
        camTargetOffset = 0;
        slideTime = Time.time;
        AudioManager.StopLoop("slideLoop");
        slidingCollider.enabled = false;
        walkingCollider.enabled = true;
    }
    #endregion
    #region Utility Methods
    void HandleHandSway()
    {
        var camPos = playerCam.transform.position;
        float vert = Mathf.SmoothDamp(hands.position.y, camPos.y, ref handVertVelocity, handVerticalSmoothtime);
        vert = Mathf.Clamp(vert, camPos.y - handVerticalMax, camPos.y + handVerticalMax);
        hands.position = new Vector3(camPos.x, vert, camPos.z);

        hands.rotation = Quaternion.Lerp(hands.rotation, playerCam.transform.rotation, handRotationLerp);
    }
    void CheckFalling()
    {
        if (IsGrounded() == false)
            StartFalling();
    }
    void AirStrafing()
    {
        float currentSpeed = GetFlatVelocity().magnitude;
        Vector3 strafeForce = airStrafeForce * GetFacingInput();
        Vector3 newVelocity = GetFlatVelocity() + strafeForce * Time.deltaTime;
        //if going faster than walk speed and new velocity gave player extra speed, limit it
        if (currentSpeed > walkSpeed && newVelocity.magnitude > currentSpeed)
                newVelocity = newVelocity.normalized * currentSpeed;
        //if going slower than walk speed, allow the player to speed up until walk speed
        else if(currentSpeed < walkSpeed && newVelocity.magnitude > walkSpeed)
                newVelocity = newVelocity.normalized * walkSpeed;
        
        rb.velocity = new Vector3(newVelocity.x, rb.velocity.y, newVelocity.z);
    }
    void DoSlice()
    {
        if (Time.time - attackTime < attackCooldown) return;

        var colliders = Physics.OverlapBox(slicePlane.position, slicePlane.localScale / 2, slicePlane.rotation);
        //SliceCollision() checks for a SliceableObject, so we can call it on anything and it will handle which inputs are valid
        bool success = false;
        //try to slice the objects and record if there were any successes
        foreach (var collider in colliders)
            if (EZSlicer.SliceCollision(collider, slicePlane, sliceParticles))
                success = true;

        attackTime = Time.time;

        AudioManager.Play(sliceSound, 0.1f, sliceVolume);
        if (success)
            AudioManager.Play(sliceSuccessSound, 0.1f, sliceSuccessVolume);

        onAttack?.Invoke();
    }
    void DoParry()
    {
        if (Time.time - parryTime < parryCooldown) return;

        //if collided objects are parryable, parry them
        var colliders = Physics.OverlapBox(parryCube.position, parryCube.localScale, parryCube.rotation);
        bool success = false;
        foreach (var collider in colliders)
        {
            var parryable = collider.GetComponent<IParryable>();
            if (parryable != null)
            {
                parryable.Parry(playerCam.transform.forward);
                success = true;
            }
        }

        if (success)
            AudioManager.Play(parrySound, 0.05f, parryVolume);
        else
            AudioManager.Play(parryWhiffSound, 0.1f, parryWhiffVolume);


        parryTime = Time.time;

        onParry?.Invoke();
    }
    void HandleFootsteps()
    {
        float timeBetweenFootsteps = (1 / GetFlatVelocity().magnitude) / footstepsPerMetre;
        if (Time.time - timeOfFootstep > timeBetweenFootsteps)
        {
            AudioManager.Play(footstepSound, 0.01f, footstepVolume);
            timeOfFootstep = Time.time;
        }
    }
    Vector3 GetFacingInput()
    {
        return (Ctrl.WASD.x * camHoriAxis.right + Ctrl.WASD.y * camHoriAxis.forward).normalized;
    }
    void SetSpeedFacing() => SetSpeedFacing(GetFlatVelocity().magnitude);
    void SetSpeedFacing(float _speed)
    {
        var v = camHoriAxis.forward * _speed;
        rb.velocity = new Vector3(v.x, rb.velocity.y, v.z);
    }
    void SetSpeedInputDirection() => SetSpeedInputDirection(GetFlatVelocity().magnitude);
    void SetSpeedInputDirection(float _speed)
    {
        var v = GetFacingInput() * _speed;
        rb.velocity = new Vector3(v.x, rb.velocity.y, v.z);
    }
    void SetSpeed(float _speed)
    {
        var v = rb.velocity.normalized * _speed;
        rb.velocity = new Vector3(v.x, rb.velocity.y, v.z);
    }
    void ApplyDrag(float _deceleration)
    {
        var v = rb.velocity.normalized * (rb.velocity.magnitude - _deceleration * Time.deltaTime);
        rb.velocity = new Vector3(v.x, rb.velocity.y, v.z);
    }
    void HandleHorizontalMovement(Vector2 _WASD, float _maxSpeed, float _acceleration, float _deceleration)
    {
        var currentVelocity = rb.velocity;
        var inputDirection = camHoriAxis.transform.right * _WASD.x + camHoriAxis.transform.forward * _WASD.y;
        var velocityDirection = GetFlatVelocity().normalized;

        //determine if accelerating or decelerating
        Vector3 deltaV = Vector3.zero;
        if (Ctrl.WASD.magnitude > 0)
            deltaV = inputDirection * _acceleration * Time.deltaTime;
        else
            deltaV = velocityDirection * -_deceleration * Time.deltaTime;

        //apply velocity with max speed limit
        var newVelocity = currentVelocity + deltaV;
        if (newVelocity.magnitude > _maxSpeed)
            newVelocity = newVelocity.normalized * _maxSpeed;

        //if going very slow and no input, just set to 0
        if (Ctrl.WASD.magnitude <= 0.1f && GetFlatVelocity().magnitude < 1)
            rb.velocity = new Vector3(0, rb.velocity.y, 0);

        rb.velocity = newVelocity;
    }
    float VelocityDotInput()
    {
        //compare the horizontal velocity against the horizontal inputs to see the comparison in direction
        return Vector3.Dot(GetFlatVelocity(), GetFacingInput());
    }
    float CamHorizontalDotInput()
    {
        //compare the horizontal velocity against the horizontal inputs to see the comparison in direction
        return Vector3.Dot(camHoriAxis.forward, GetFacingInput());
    }
    float CamHorizontalDotVelocity()
    {
        //compare the horizontal velocity against the horizontal inputs to see the comparison in direction
        return Vector3.Dot(camHoriAxis.forward, GetFlatVelocity());
    }
    void HandleSlicePlane()
    {
        //slice plane
        var scrollDelta = Input.mouseScrollDelta.y;
        slicePlane.Rotate(0, 0, scrollDelta * planeRotationSpeed);
        if (Input.mouseScrollDelta.y > 0)
            onSwordAngleChange?.Invoke(slicePlane.localEulerAngles.z);
    }
    Vector3 GetFlatVelocity() { return Vector3.Scale(rb.velocity, new Vector3(1, 0, 1)); }
    Vector3 GetFlatVelocity(Vector3 _velocity) { return Vector3.Scale(_velocity, new Vector3(1, 0, 1)); }
    float camYOffsetVelocity;
    float camFoVVelocity;
    void HandleCamera()
    {
        camCurrentOffset = Mathf.SmoothDamp(camCurrentOffset, camTargetOffset, ref camYOffsetVelocity, camOffsetSmoothtime);

        var mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        camHoriAxis.transform.Rotate(0, mouseDelta.x * settings.sensitivity, 0);
        camVertAxis.transform.Rotate(-mouseDelta.y * settings.sensitivity, 0, 0);
        camHoriAxis.transform.position = transform.position + camCurrentOffset * Vector3.up;

        playerCam.fieldOfView = Mathf.SmoothDamp(playerCam.fieldOfView, targetFoV, ref camFoVVelocity, FoVSmoothtime);


    }
    bool IsGrounded()
    {
        return Physics.Raycast(footPosition.position, Vector3.down, rayDistance, groundLayer);
    }
    void TakeDamage(int _damage)
    {
        currentHealth -= _damage;
        onHealthUpdated?.Invoke((float)currentHealth / maxHealth);
        if (currentHealth <= 0)
            Die();
    }
    void Die()
    {
        AudioManager.Play(deathSound, 0.05f, 1);
        onPlayerDeath?.Invoke();
        isDead = true;
    }
    #endregion

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Projectile"))
        {
            var proj = other.GetComponent<Projectile>();
            if (proj != null)
            {
                TakeDamage(proj.damage);
                proj.Die();
            }
        }
    }

}
