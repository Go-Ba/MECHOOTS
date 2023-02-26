using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionProjectile : Projectile
{
    [SerializeField] float explosionSpeed;
    [SerializeField] float explosionDeceleration;
    [SerializeField] float explosionLifetime;
    float startTime;
    void Start()
    {
        startTime = Time.time;
    }
    void Update()
    {
        transform.localScale += explosionSpeed * Time.deltaTime * Vector3.one;
        explosionSpeed -= explosionDeceleration * Time.deltaTime;
        explosionSpeed = Mathf.Clamp(explosionSpeed, 0, float.MaxValue);
        if (Time.time - startTime > explosionLifetime)
            Destroy(gameObject);
    }
    public override void Die()
    {
        //do nothing
    }
}
