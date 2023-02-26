using DitzelGames.FastIK;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System;

public class BoneLeafController : MonoBehaviour
{
    [SerializeField] SliceableObject limb;
    public FastIKFabric IK;
    public event Action onLimbDestroyed;

    private void Start()
    {
        if (IK == null)
            IK = GetComponent<FastIKFabric>();
    }
    private void OnEnable()
    {
        limb.onSliced += OnSlice;
    }
    private void OnDisable()
    {
        limb.onSliced -= OnSlice;
    }
    //if a parent was sliced, then it will call this to be sliced first
    public void ParentSliced()
    {
        OnSlice();
    }
    void OnSlice()
    {
        //if there is no IK connected to this, it means this is a middle bone
        //call slice on the lower bone first before coming back to this one
        if (IK == null)
        {
            var leaves = GetComponentsInChildren<BoneLeafController>();
            if (leaves.Length == 0) return;
            foreach (var leaf in leaves)
            {
                if (leaf == this) continue;
                leaf.ParentSliced();
                break;
            }
        }

        if (IK.ChainLength <= 1)
        {
            onLimbDestroyed?.Invoke();
            Destroy(gameObject);
            return;
        }

        var parent = transform.parent;
        var parentIK = parent.AddComponent<FastIKFabric>();
        parentIK.CopySettings(IK);

        parentIK.ChainLength--;

        var parentLeaf = parent.GetComponent<BoneLeafController>();
        parentLeaf.IK = parentIK;

        MoveChildren();
        Destroy(gameObject);
    }

    void MoveChildren()
    {
        foreach (Transform child in transform)        
            child.transform.parent = transform.parent;
        
    }
}
