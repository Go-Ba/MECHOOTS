using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatDroneController : MonoBehaviour
{
    Transform target;
    [SerializeField] float acceleration = 5;
    [SerializeField] float maxVelocity = 15;
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] Transform projectileShootPos;
    [SerializeField] float timeBetweenProjectiles;
    [SerializeField] AudioClip deathSound;

    Rigidbody rb;

    Vector3 velocity;
    float timeOfProjectile;
    void Start()
    {
        target = FindObjectOfType<PlayerController>().transform;
        rb = GetComponent<Rigidbody>();

        Vector3 targetPos = new Vector3(target.position.x, transform.position.y, target.position.z);
        var dir = (targetPos - transform.position).normalized;
        rb.velocity = dir * maxVelocity;
    }

    void Update()
    {
        Vector3 targetPos = new Vector3(target.position.x, transform.position.y, target.position.z);

        var dir = (targetPos - transform.position).normalized;

        velocity += dir * acceleration * Time.deltaTime;
        if (velocity.magnitude > maxVelocity)
            velocity = velocity.normalized * maxVelocity;

        rb.velocity = velocity;
        //transform.position += velocity * Time.deltaTime;
        transform.LookAt(transform.position + dir);

        if (Time.time - timeOfProjectile > timeBetweenProjectiles)
        {
            timeOfProjectile = Time.time;
            Instantiate(projectilePrefab, projectileShootPos.position, Quaternion.identity, null);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PlayerProjectile"))
            SelfDestruct();
    }
    void SelfDestruct()
    {
        var toBeSlices = GetComponentsInChildren<ToBeSliceable>();
        foreach (var item in toBeSlices)
        {
            var slice = item.gameObject.AddComponent<SliceableObject>();
            slice.Initialize(slice);
            slice.transform.parent = null;
            slice.useAnchor = false;
        }
        AudioManager.Play(deathSound, 0.1f, 1f, transform.position);
        Destroy(gameObject);
    }
}
