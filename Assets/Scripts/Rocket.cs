using UnityEngine;

public class Rocket : MonoBehaviour
{
    [Header("Motion")]
    public float speed = 25f;

    [Header("Direct Damage Settings")]
    [Tooltip("Point-blank max damage (at 0m)")]
    public float directMaxDamage    = 112f;
    [Tooltip("Base damage (at 3m)")]
    public float baseDamage         =  90f;
    [Tooltip("Damage at medium-range end (at 30m)")]
    public float midMinDamage       =  50f;
    [Tooltip("Damage beyond falloff end (constant)")]
    public float falloffMinDamage   =  48f;
    [Tooltip("Point-blank range (m)")]
    public float pointBlankRange    =   3f;
    [Tooltip("Falloff end range (m)")]
    public float falloffEndDistance =  30f;

    [Header("Splash Damage Settings")]
    [Tooltip("Explosion radius for splash damage")]
    public float explosionRadius    =  2.8f;
    [Tooltip("Ratio at edge of explosion (0.5 = 50%)")]
    [Range(0f,1f)]
    public float minSplashRatio     =  0.5f;  // 50%

    [Header("Knockback")]
    [Tooltip("Knockback = damage × multiplier")]
    public float knockbackMultiplier = 0.1f;

    [Header("VFX / SFX")]
    public GameObject explosionParticlePrefab;
    public AudioClip  explosionSoundClip;
    [Range(0f,1f)]
    public float explosionSoundVolume = 1f;

    // 내부상태
    Vector3 shooterPosition;
    Collider directHitCollider;

    /// <summary>
    /// 런처에서 Instantiate 직후 호출할 것
    /// </summary>
    public void Initialize(Vector3 shooterPos)
    {
        shooterPosition = shooterPos;
    }

    void Update()
    {
        // 단순 전진 (라이프타임 제거)
        transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);
    }

    void OnCollisionEnter(Collision col)
    {
        directHitCollider = null;

        // 1) 직격 피해
        var hp = col.collider.GetComponent<Health>();
        if (hp != null)
        {
            float distShooter = Vector3.Distance(shooterPosition, transform.position);
            float dmg = ComputeDirectDamage(distShooter);
            hp.TakeDamage(dmg);
            ApplyKnockback(col.rigidbody, transform.position, dmg);
            directHitCollider = col.collider;
        }

        // 2) 폭발 처리
        Explode();
    }

    void Explode()
    {
        // 파티클
        if (explosionParticlePrefab != null)
            Instantiate(explosionParticlePrefab, transform.position, Quaternion.identity);

        // 사운드
        if (explosionSoundClip != null)
            AudioSource.PlayClipAtPoint(explosionSoundClip, transform.position, explosionSoundVolume);

        // 스플래시 피해
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var c in hits)
        {
            if (c == directHitCollider) continue;
            var hpSplash = c.GetComponent<Health>();
            if (hpSplash != null)
            {
                // 직격 피해량 계산
                float distShooter = Vector3.Distance(shooterPosition, transform.position);
                float directDmg    = ComputeDirectDamage(distShooter);

                // 폭발 반경 내 거리
                float distExpl = Vector3.Distance(transform.position, c.transform.position);
                float splashDmg = ComputeSplashDamage(directDmg, distExpl);

                hpSplash.TakeDamage(splashDmg);
                ApplyKnockback(c.attachedRigidbody, transform.position, splashDmg);
            }
        }

        Destroy(gameObject);
    }

    /// <summary>
    /// 발사자↔착탄 지점 거리로 계산하는 직격 피해
    /// </summary>
    float ComputeDirectDamage(float dist)
    {
        if (dist <= pointBlankRange)
        {
            // 0~3m: 112 → 90
            float t = dist / pointBlankRange;
            return Mathf.Lerp(directMaxDamage, baseDamage, t);
        }
        else if (dist <= falloffEndDistance)
        {
            // 3~30m: 90 → 50
            float t = (dist - pointBlankRange) / (falloffEndDistance - pointBlankRange);
            return Mathf.Lerp(baseDamage, midMinDamage, t);
        }
        else
        {
            // >30m: 48 고정
            return falloffMinDamage;
        }
    }

    /// <summary>
    /// 직격 피해량(directDmg)과 폭발↔대상 거리로 스플래시 피해 계산
    /// 0m에서 100% direct, radius에서 50% direct
    /// </summary>
    float ComputeSplashDamage(float directDmg, float distExpl)
    {
        float t     = Mathf.Clamp01(distExpl / explosionRadius);
        float ratio = Mathf.Lerp(1f, minSplashRatio, t);
        return directDmg * ratio;
    }

    void ApplyKnockback(Rigidbody rb, Vector3 center, float damage)
    {
        if (rb == null) return;
        Vector3 dir = (rb.position - center).normalized;
        rb.AddForce(dir * damage * knockbackMultiplier, ForceMode.Impulse);
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