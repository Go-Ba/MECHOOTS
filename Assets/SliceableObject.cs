using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SliceableObject : MonoBehaviour
{
    const int debrisLifetime = 10;
    const int maxSlices = 5;
    public int sliceCount { get; private set; } //the amount of slices that have been done to get to this point
    public bool canBeSliced { get { return sliceCount < maxSlices; } }

    public MeshCollider meshCollider { get; private set; }
    public Rigidbody rigidBody { get; private set; }
    public MeshFilter meshFilter { get; private set; }
    public MeshRenderer meshRenderer { get; private set; }

    public event Action onSliced;

    public bool useAnchor = false;
    public Transform anchor;

    bool dontDie;


    private void Start()
    {
        if (meshCollider == null) gameObject.GetComponent<MeshCollider>();
        if (rigidBody == null) gameObject.GetComponent<Rigidbody>();
        if (meshFilter == null) gameObject.GetComponent<MeshFilter>();
        if (meshRenderer == null) gameObject.GetComponent<MeshRenderer>();
    }
    public void Initialize(SliceableObject _parent)
    {
        sliceCount = _parent.sliceCount + 1;
        if (meshCollider == null) meshCollider = gameObject.AddComponent<MeshCollider>();
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        meshCollider.sharedMesh = meshFilter.sharedMesh;
        meshCollider.convex = true;

        if (rigidBody == null) rigidBody = gameObject.AddComponent<Rigidbody>();

        for (int j = 1; j < meshRenderer.materials.Length; j++)
            meshRenderer.materials[j] = meshRenderer.materials[0];

        if (!useAnchor)
            Invoke("Despawn", debrisLifetime);
    }
    public void CancelDeathTimer()
    {
        dontDie = true;
    }
    public void HandleChildren(GameObject[] _slices)
    {
        for (int i = 0; i < _slices.Length; i++)
        {
            _slices[i].transform.parent = null;
            var s = _slices[i].AddComponent<SliceableObject>();
            s.Initialize(this);
            s.transform.rotation = transform.rotation;
            s.transform.localScale = transform.lossyScale;
            Explode(s);
        }

        if (useAnchor) HandleAnchor(_slices);

        onSliced?.Invoke();
        Destroy(gameObject);
    }
    public void Explode(SliceableObject _s)
    {
        _s.rigidBody.AddExplosionForce(100, transform.position + new Vector3(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value), 5f);
    }
    public void HandleAnchor(GameObject[] _slices)
    {
        //find the closest slice to the anchor
        GameObject closest = null;
        float closestDistance = float.MaxValue;
        for (int i = 0; i < _slices.Length; i++)
        {
            var center = _slices[i].GetComponent<MeshRenderer>().bounds.center;
            var dis = Vector3.Distance(center, anchor.position);
            if (dis < closestDistance)
            {
                closest = _slices[i];
                closestDistance = dis;
            }
        }

        //keep the closest slice connected to the original parent and disable physics
        closest.transform.parent = transform.parent;
        closest.GetComponent<Rigidbody>().isKinematic = true;
        closest.transform.localScale = transform.localScale;
        closest.transform.localRotation = transform.localRotation;
        //give the anchor to the closest object
        if (anchor.parent == transform)
            anchor.parent = closest.transform;
        var sliceable = closest.GetComponent<SliceableObject>();
        sliceable.useAnchor = true;
        sliceable.anchor = anchor;

        sliceable.rigidBody.velocity = Vector3.zero;
        sliceable.CancelDeathTimer();
    }
    void Despawn()
    {
        if (dontDie) return;
        Destroy(gameObject);
    }

}
