using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System;

public class SmallCatController : MonoBehaviour
{
    [SerializeField] float maxDistance;
    [SerializeField] float stepTime;
    [SerializeField] AnimationCurve footCurve;
    [SerializeField] float speed;
    [SerializeField] float rotationSpeed;
    [SerializeField] float targetRotationMult = 1;
    [SerializeField] float footMaxHeight = 1;
    [SerializeField] Transform targetPlayer;
    float currentRotationSpeed;
    float currentSpeed;
    float verticalRotationSpeed = 100f;

    [SerializeField] float groundedOffsetMax = 3;
    [SerializeField] float groundedOffsetMin = 1;
    float offsetCurrent;
    float offsetVelocity;

    [SerializeField] Transform targetParent;
    Rigidbody targetRB;
    [SerializeField] CatLeg leg_FR;
    [SerializeField] CatLeg leg_FL;
    [SerializeField] CatLeg leg_BR;
    [SerializeField] CatLeg leg_BL;
    [SerializeField] Transform body;
    [SerializeField] Transform bodyFront;
    [SerializeField] Transform bodyBack;

    [SerializeField] AudioClip explosionSound;
    [SerializeField] float explosionRange = 2f;
    [SerializeField] GameObject explosionPrefab;




    int limbCount = 4;
    bool cut_FR;
    bool cut_FL;
    bool cut_BR;
    bool cut_BL;

    [SerializeField] SliceableObject[] deathSlices;
    private void Start()
    {
        targetPlayer = FindObjectOfType<PlayerController>().transform;

        currentSpeed = speed;
        currentRotationSpeed = rotationSpeed;
        offsetCurrent = groundedOffsetMax;

        leg_FR.Init();
        leg_FL.Init();
        leg_BR.Init();
        leg_BL.Init();

        leg_FR.SetExclusiveLegs(new CatLeg[2] { leg_FL, leg_BR });
        leg_BL.SetExclusiveLegs(new CatLeg[2] { leg_FL, leg_BR });
        leg_FL.SetExclusiveLegs(new CatLeg[2] { leg_FR, leg_BL });
        leg_BR.SetExclusiveLegs(new CatLeg[2] { leg_FR, leg_BL });

        leg_FR.onLimbDestroyed += () => LimbCut(ref cut_FR);
        leg_FL.onLimbDestroyed += () => LimbCut(ref cut_FL);
        leg_BR.onLimbDestroyed += () => LimbCut(ref cut_BR);
        leg_BL.onLimbDestroyed += () => LimbCut(ref cut_BL);

        foreach (var slice in deathSlices)       
            slice.onSliced += Die;

        
    }
    void Die()
    {
        foreach (var slice in deathSlices)        
            slice.onSliced -= Die;
        
        var slices = GetComponentsInChildren<SliceableObject>();
        var toBeSlices = GetComponentsInChildren<ToBeSliceable>();
        foreach (var item in toBeSlices)
        {
            var slice = item.AddComponent<SliceableObject>();
            slice.Initialize(slices[0]);
            slice.transform.parent = null;
            slice.useAnchor = false;
        }
        foreach (var slice in slices)
        {
            slice.Initialize(slice);
            slice.rigidBody.isKinematic = false;
            slice.transform.parent = null;
            slice.useAnchor = false;
            slice.Initialize(slice);
            slice.Explode(slice);
        }
        Destroy(transform.parent.gameObject);
    }
    //used to damage the player
    void SelfDestruct()
    {
        AudioManager.Play(explosionSound, 0.025f, 1f, transform.position);
        Instantiate(explosionPrefab, transform.position, Quaternion.identity, null);
        Die();
    }
    void Update()
    {
        HandleLeg(leg_FR);
        HandleLeg(leg_BL);
        HandleLeg(leg_FL);
        HandleLeg(leg_BR);

        CheckAmputation();
        CheckAmputationPair(leg_FL, leg_FR, ref bodyFront);
        CheckAmputationPair(leg_BL, leg_BR, ref bodyBack);
        BodyVerticalRotation();
        BodyHorizontalRotation();

        transform.position += transform.forward * currentSpeed * Time.deltaTime;

        Gravity();
        HandleTargetRotation();

        if (Vector3.Distance(targetPlayer.position, transform.position) < explosionRange)
            SelfDestruct();
    }
    void BodyHorizontalRotation()
    {
        Vector3 lookPos = new Vector3(targetPlayer.position.x, transform.position.y, targetPlayer.position.z);
        //lerp from current rotation to look at the target
        var currentRotation = transform.rotation;
        transform.LookAt(lookPos);
        transform.rotation = Quaternion.RotateTowards(currentRotation, transform.rotation, currentRotationSpeed * Time.deltaTime);
    }
    void HandleTargetRotation()
    {
        //rotation of the leg IK targets
        if (targetRB == null)
            targetRB = targetPlayer.GetComponent<Rigidbody>();
        Vector3 predictedPosition = targetRB.position + targetRB.velocity * stepTime;
        var lookPos = new Vector3(predictedPosition.x, transform.position.y, predictedPosition.z);
        var currentRotation = transform.rotation;
        targetParent.LookAt(lookPos);
        targetParent.rotation = Quaternion.RotateTowards(currentRotation, targetParent.rotation, currentRotationSpeed * targetRotationMult * Time.deltaTime);

        targetParent.transform.position = new Vector3(body.position.x, groundedOffsetMax, body.position.z);
    }
    void Gravity()
    {
        offsetCurrent = (bodyFront.position.y + bodyBack.position.y) / 2;

        offsetVelocity += -9.81f * Time.deltaTime;
        body.position += Vector3.up * offsetVelocity * Time.deltaTime;
        if (body.position.y < offsetCurrent)
        {
            body.position = new Vector3(body.position.x, offsetCurrent, body.position.z);
            offsetVelocity = 0;
        }
    }
    void HandleLeg(CatLeg _leg)
    {
        _leg.CheckFootTarget(maxDistance);
        _leg.LerpPosition(stepTime, footCurve, footMaxHeight);
    }
    void BodyVerticalRotation()
    {
        var current = body.rotation;
        body.LookAt(bodyFront);
        body.rotation = Quaternion.RotateTowards(current, body.rotation, verticalRotationSpeed);
    }
    void CheckAmputation()
    {
        int chainCount = leg_BL.chainLength + leg_BR.chainLength + leg_FL.chainLength + leg_FR.chainLength;
        currentSpeed = speed * chainCount / 8;

        if (limbCount <= 0)
            currentRotationSpeed = rotationSpeed * 0;
        else if (limbCount == 1)
            currentRotationSpeed = rotationSpeed * 0.2f;
        else if (limbCount == 2)
            currentRotationSpeed = rotationSpeed * 0.5f;
        else if (limbCount == 3)
            currentRotationSpeed = rotationSpeed * 0.7f;
    }
    void CheckAmputationPair(CatLeg _left, CatLeg _right, ref Transform _bodyTarget)
    {
        int chainLeft = _left.chainLength;
        int chainRight = _right.chainLength;

        //both fully cut
        if (chainLeft == 0 && chainRight == 0)
        {
            float y = Mathf.Lerp(groundedOffsetMin, groundedOffsetMax, 0);
            _bodyTarget.position = new Vector3(_bodyTarget.position.x, y, _bodyTarget.position.z);
        }
        //both half cut
        else if (chainLeft == 1 && chainRight == 1)
        {
            float y = Mathf.Lerp(groundedOffsetMin, groundedOffsetMax, 0.3f);
            _bodyTarget.position = new Vector3(_bodyTarget.position.x, y, _bodyTarget.position.z);
        }
        //one fully cut and one half cut
        else if (chainLeft == 0 && chainRight == 1 || chainRight == 0 && chainLeft == 1)
        {
            float y = Mathf.Lerp(groundedOffsetMin, groundedOffsetMax, 0.3f);
            _bodyTarget.position = new Vector3(_bodyTarget.position.x, y, _bodyTarget.position.z);
        }
        else
        {
            float y = Mathf.Lerp(groundedOffsetMin, groundedOffsetMax, 1f);
            _bodyTarget.position = new Vector3(_bodyTarget.position.x, y, _bodyTarget.position.z);
        }
    }
    private void OnDrawGizmos()
    {
        float radius = 0.1f;
        Gizmos.color = Color.red;
        DrawLeg(leg_FR, radius);
        DrawLeg(leg_BL, radius);
        DrawLeg(leg_FL, radius);
        DrawLeg(leg_BR, radius);
    }
    void DrawLeg(CatLeg _leg, float _radius)
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(_leg.target.position, _radius);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(_leg.foot.position, _radius);
    }
    void LimbCut(ref bool _bool)
    {
        _bool = true;
        limbCount--;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PlayerProjectile"))
        {
            var proj = other.GetComponent<Projectile>();
            if (proj != null)
            {
                Die();
                proj.Die();
            }
        }
    }
}