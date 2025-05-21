using UnityEngine;

[RequireComponent(typeof(Health), typeof(Rigidbody))]
public class Target : MonoBehaviour
{
    public float knockbackMultiplier = 0.1f;

    private Rigidbody rb;
    private Vector3 lastExplosionOrigin;
    private float lastDamage;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        var health = GetComponent<Health>();
        if (health != null)
            health.OnTakeDamage.AddListener(OnTakeDamage);
    }

    public void RecordExplosion(Vector3 origin, float damage)
    {
        lastExplosionOrigin = origin;
        lastDamage = damage;
    }

    void OnTakeDamage(float _)
    {
        if (rb == null) return;

        Vector3 dir = (transform.position - lastExplosionOrigin).normalized;
        float force = lastDamage * knockbackMultiplier;
        rb.AddForce(dir * force, ForceMode.Impulse);
    }
}
