using EzySlice;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EZSlicer : MonoBehaviour
{
    [SerializeField] GameObject sliceTarget;
    [SerializeField] Transform cutPlane;
    [SerializeField] float planeRadius;
    public static bool SliceCollision(Collider _collider, Transform _cutPlane, GameObject _sliceParticles)
    {
        var sliceable = _collider.GetComponent<SliceableObject>();
        if (sliceable != null && sliceable.canBeSliced)
        {
            var renderer = sliceable.meshRenderer;
            if (renderer == null) renderer = sliceable.GetComponent<MeshRenderer>();
            SliceMesh(sliceable, _cutPlane, _sliceParticles, renderer.sharedMaterial);
            return true;
        }
        return false;
    }

    [ContextMenu("SLICE MESH")]
    static void SliceMesh(SliceableObject _parent, Transform _cutPlane, GameObject _sliceParticles, Material _sliceMaterial)
    {
        GameObject _sliceTarget = _parent.gameObject;

        var slices = SliceInstantiate(_sliceTarget, _cutPlane.position, _cutPlane.up, _sliceMaterial);

        //try flipping the plane upside down and trying again before erroring
        if(slices == null)
            slices = SliceInstantiate(_sliceTarget, _cutPlane.position, -_cutPlane.up, _sliceMaterial);
        if (slices == null)
        {
            Debug.LogWarning("Slices array is null");
            return;
        }

        for (int i = 0; i < slices.Length; i++)        
            slices[i].transform.position = _sliceTarget.transform.position;
        

        //spawn particles
        if (_sliceParticles != null)
            Instantiate(_sliceParticles, _cutPlane.transform.position, _cutPlane.rotation);

        _parent.HandleChildren(slices);
    }

    public static SlicedHull Slice(GameObject _object, Vector3 _planeWorldPosition, Vector3 _planeWorldDirection, Material _material)
    {
        return _object.Slice(_planeWorldPosition, _planeWorldDirection, _material);
    }
    public static GameObject[] SliceInstantiate(GameObject _object, Vector3 _planeWorldPosition, Vector3 _planeWorldDirection, Material _material)
    {
        return _object.SliceInstantiate(_planeWorldPosition, _planeWorldDirection, _material);
    }
}
