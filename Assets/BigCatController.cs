using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System;
public class BigCatController : MonoBehaviour
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
    [SerializeField] SliceableObject tail;

    [SerializeField] AudioClip deathSound;


    int limbCount = 4;
    bool cut_FR;
    bool cut_FL;
    bool cut_BR;
    bool cut_BL;
    bool cut_Tail;

    public float spawnRange = 50;

    float timeOfChange;
    float timeBetweenChange = 7f;

    private void Start()
    {
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

        targetPlayer = new GameObject("cat target").transform;
        targetPlayer.AddComponent<Rigidbody>();
        SetRandomTargetPosition(10);

        tail.onSliced += OnTailSliced;
    }
    void OnTailSliced()
    {
        cut_Tail = true;
        Invoke("SelfDestruct", 10);
    }
    void SetRandomTargetPosition(float _spawnRange)
    {
        targetPlayer.transform.position = new Vector3(UnityEngine.Random.Range(-_spawnRange, _spawnRange), 0, UnityEngine.Random.Range(-_spawnRange, _spawnRange));
        timeOfChange = Time.time;
        //check distance from 0,0 is within spawn range
        if (targetPlayer.transform.position.magnitude > _spawnRange)
            SetRandomTargetPosition(_spawnRange);
    }
    void SelfDestruct()
    {
        var slices = GetComponentsInChildren<SliceableObject>();
        var toBeSlices = GetComponentsInChildren<ToBeSliceable>();
        foreach (var item in toBeSlices)
        {
            var slice = item.AddComponent<SliceableObject>();
            slice.Initialize(slices[0]);
            slice.transform.parent = null;
            slice.useAnchor = false;
        }
        foreach (var item in slices)
        {
            item.Initialize(item);
            item.rigidBody.isKinematic = false;
            item.useAnchor = false;
            item.transform.parent = null;
        }
        AudioManager.Play(deathSound, 0.1f, 1f, transform.position);
        Destroy(transform.parent.gameObject);
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

        if (Vector3.Distance(targetPlayer.position, transform.position) < 15)
            SetRandomTargetPosition(spawnRange);
        if (Time.time - timeOfChange > timeBetweenChange)
            SetRandomTargetPosition(spawnRange);

        if (cut_BL && cut_BR && cut_FL && cut_FR && cut_Tail)
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

        //if most limbs are cut and tail is gone, destruct after time
        if (chainCount <= 2 && cut_Tail)
            Invoke("SelfDestruct", 5f);
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
}
[System.Serializable]
public class CatLeg
{
    [DoNotSerialize] private CatLeg[] excludeLegs = new CatLeg[0]; //Don't step while these are stepping
    [field: SerializeField] public Transform foot { get; private set; }
    [field: SerializeField] public Transform target { get; private set; }
    [field: SerializeField] public BoneLeafController finalLeaf { get; private set; }
    float stepStartTime;
    public bool stepping { get; private set; }

    Vector3 stepStartPos; 
    Vector3 stepGoalPos;

    [field: SerializeField] public BoneLeafController[] chain { get; private set; }
    public int chainLength { get { return GetChainLength(); } }

    public event Action onLimbDestroyed;
    public void SetExclusiveLegs(CatLeg[] _legs)
    {
        excludeLegs = _legs;
    }
    public void Init()
    {
        stepStartPos = foot.position;
        stepGoalPos = foot.position;

        if (finalLeaf != null)
            finalLeaf.onLimbDestroyed += LimbDestroyed;
    }
    int GetChainLength()
    {
        int count = 0;
        for (int i = 0; i < chain.Length; i++)       
            if (chain[i] != null) count++;
        
        return count;
    }
    void LimbDestroyed()
    {
        Debug.Log("Limb Destroyed");
        finalLeaf.onLimbDestroyed -= LimbDestroyed;
        onLimbDestroyed?.Invoke();
    }
    public void CheckFootTarget(float _maxDistance)
    {
        if (stepping) return;
        float distance = Vector3.Distance(foot.position, target.position);
        if (distance > _maxDistance && !OtherLegsStepping())
            StartStep();
    }
    public void StartStep()
    {
        stepping = true;
        stepGoalPos = target.position;
        stepStartTime = Time.time;
        stepStartPos = foot.position;
    }
    public void LerpPosition(float _stepTime, AnimationCurve _curve, float _footMaxHeight)
    {
        if (!stepping) return;
        
        float progress = (Time.time - stepStartTime) / _stepTime;
        progress = Mathf.Clamp(progress, 0, 1);
        var groundPosition = Vector3.Lerp(stepStartPos, stepGoalPos, progress);
        foot.position = groundPosition + Vector3.up * _curve.Evaluate(progress) * _footMaxHeight;

        if (Time.time - stepStartTime > _stepTime)
            EndStep();
    }
    void EndStep()
    {
        foot.position = stepGoalPos;
        stepping = false;
    }
    bool OtherLegsStepping()
    {
        if (excludeLegs.Length == 0)
            return false;

        foreach (var leg in excludeLegs)        
            if (leg.stepping) return true;
        
        return false;
    }
}
