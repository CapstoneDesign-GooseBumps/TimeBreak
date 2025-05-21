using UnityEngine;

public class Rocket : MonoBehaviour
{
    [Header("Motion")]
    public float speed = 25f;

    [Header("Direct Damage Settings")]
    public float directMaxDamage    = 112f;
    public float baseDamage         = 90f;
    public float midMinDamage       = 50f;
    public float falloffMinDamage   = 48f;
    public float pointBlankRange    = 3f;
    public float falloffEndDistance = 30f;

    [Header("Splash Damage Settings")]
    public float explosionRadius    = 2.8f;
    [Range(0f, 1f)]
    public float minSplashRatio     = 0.5f;

    [Header("Knockback")]
    public float knockbackMultiplier = 0.1f;

    [Header("VFX / SFX")]
    public GameObject explosionParticlePrefab;
    public AudioClip explosionSoundClip;
    [Range(0f, 1f)] public float explosionSoundVolume = 1f;

    private Vector3 shooterPosition;
    private Collider directHitCollider;

    public void Initialize(Vector3 shooterPos)
    {
        shooterPosition = shooterPos;
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);
    }

    void OnCollisionEnter(Collision col)
    {
        directHitCollider = null;

        var hp = col.collider.GetComponent<Health>();
        if (hp != null)
        {
            float distShooter = Vector3.Distance(shooterPosition, transform.position);
            float dmg = ComputeDirectDamage(distShooter);

            // 🔹 넉백 정보를 미리 전달
            var dummy = col.collider.GetComponent<Target>();
            if (dummy != null)
                dummy.RecordExplosion(transform.position, dmg);

            hp.TakeDamage(dmg);
            directHitCollider = col.collider;
        }

        Explode();
    }

    void Explode()
    {
        if (explosionParticlePrefab != null)
            Instantiate(explosionParticlePrefab, transform.position, Quaternion.identity);

        if (explosionSoundClip != null)
            AudioSource.PlayClipAtPoint(explosionSoundClip, transform.position, explosionSoundVolume);

        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var c in hits)
        {
            if (c == directHitCollider) continue;

            var hp = c.GetComponent<Health>();
            if (hp != null)
            {
                float distShooter = Vector3.Distance(shooterPosition, transform.position);
                float directDmg = ComputeDirectDamage(distShooter);

                float distExpl = Vector3.Distance(transform.position, c.transform.position);
                float splashDmg = ComputeSplashDamage(directDmg, distExpl);

                // 🔹 넉백 정보를 미리 전달
                var dummy = c.GetComponent<Target>();
                if (dummy != null)
                    dummy.RecordExplosion(transform.position, splashDmg);

                hp.TakeDamage(splashDmg);
            }
        }

        Destroy(gameObject);
    }

    float ComputeDirectDamage(float dist)
    {
        if (dist <= pointBlankRange)
        {
            float t = dist / pointBlankRange;
            return Mathf.Lerp(directMaxDamage, baseDamage, t);
        }
        else if (dist <= falloffEndDistance)
        {
            float t = (dist - pointBlankRange) / (falloffEndDistance - pointBlankRange);
            return Mathf.Lerp(baseDamage, midMinDamage, t);
        }
        else
        {
            return falloffMinDamage;
        }
    }

    float ComputeSplashDamage(float directDmg, float distExpl)
    {
        float t = Mathf.Clamp01(distExpl / explosionRadius);
        float ratio = Mathf.Lerp(1f, minSplashRatio, t);
        return directDmg * ratio;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(shooterPosition, falloffEndDistance);
    }
#endif
}