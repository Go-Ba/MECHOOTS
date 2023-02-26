using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [field: SerializeField] public int damage { get; private set; }
    public virtual void Die()
    {
        Destroy(gameObject);
    }
}
public class SlowProjectile : Projectile, IParryable
{
    [SerializeField] float speed;
    [SerializeField] Rigidbody rb;
    [SerializeField] float lifetime;
    float creationTime;
    [SerializeField] AudioClip projectileSound;
    [SerializeField] float projectileVolume = 0.9f;
    [SerializeField] float projectileSoundMaxDistance = 50;



    public void Parry(Vector3 _direction)
    {
        transform.LookAt(transform.position + _direction);
        rb.velocity = transform.forward * speed;
        creationTime = Time.time;
        gameObject.tag = "PlayerProjectile";
    }

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        rb.velocity = transform.forward * speed;
        creationTime = Time.time;
        AudioManager.Play(projectileSound, 0.1f, projectileVolume, transform.position, projectileSoundMaxDistance, AudioRolloffMode.Linear);
    }
    private void Update()
    {
        rb.velocity = transform.forward * speed;
        if (Time.time - creationTime >= lifetime) Destroy(gameObject);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Forcefield"))
            Die();
    }
}
public interface IParryable
{
    public abstract void Parry(Vector3 _direction);
}
