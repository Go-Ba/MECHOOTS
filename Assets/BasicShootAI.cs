using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicShootAI : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] GameObject projectile;
    [SerializeField] Transform shootPos;
    [SerializeField] float fireRate;
    float lastFireTime;

    Rigidbody rb;
    void Start()
    {
        target = FindObjectOfType<PlayerController>().transform;
        rb = target.GetComponent<Rigidbody>();
    }

    void Update()
    {
        transform.LookAt(target);
        if (Time.time - lastFireTime > 1 / fireRate)
            Shoot();
    }

    void Shoot()
    {
        var proj = Instantiate(projectile, shootPos.position, transform.rotation, null);

        float seconds = Random.Range(0, 2f);
        //Vector3 predictedPosition = target.position + rb.velocity * seconds;
        Vector3 predictedPosition = target.position;


        proj.transform.LookAt(predictedPosition);
        lastFireTime = Time.time;
    }
}
