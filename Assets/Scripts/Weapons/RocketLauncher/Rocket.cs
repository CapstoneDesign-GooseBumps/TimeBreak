using UnityEngine;
using System.Collections;
using Fragsurf.Movement;

public class Rocket : MonoBehaviour
{
    [Header("Motion")]
    public float speed = 25f;

    [Header("Direct Damage Settings")]
    public float directMaxDamage = 112f;
    public float baseDamage = 90f;
    public float midMinDamage = 50f;
    public float falloffMinDamage = 48f;
    public float pointBlankRange = 5f;
    public float falloffEndDistance = 30f;

    [Header("Splash Damage Settings")]
    public float explosionRadius = 2.8f;
    [Range(0f, 1f)]
    public float minSplashRatio = 0.5f;

    [Header("Knockback")]
    public float knockbackMultiplier = 0.1f;

    [Header("VFX / SFX")]
    public GameObject explosionParticlePrefab;
    public AudioClip explosionSoundClip;
    [Range(0f, 1f)]
    public float explosionSoundVolume = 1f;

    private Vector3 shooterPosition;
    private GameObject shooterObject;
    private Health shooterHealth;
    private Collider lastDirectHit;
    private bool ignoreFirstFrame = true;

    void Start()
    {
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = transform.forward * speed;
        }

        StartCoroutine(EnableCollisionNextFrame());
    }

    IEnumerator EnableCollisionNextFrame()
    {
        yield return null;
        ignoreFirstFrame = false;
    }

    public void Initialize(Vector3 shooterPos, GameObject shooterObj)
    {
        shooterPosition = shooterPos;
        shooterObject = shooterObj;
        shooterHealth = shooterObj.GetComponentInChildren<Health>();

        var myCol = GetComponent<Collider>();
        if (myCol != null)
        {
            foreach (var col in shooterObj.GetComponentsInChildren<Collider>())
            {
                Physics.IgnoreCollision(myCol, col, true);
            }
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (ignoreFirstFrame)
            return;

        GameObject targetObj = col.collider.transform.root.gameObject;
        bool isSelf = targetObj == shooterObject;

        lastDirectHit = col.collider;

        var hp = col.collider.GetComponent<Health>();
        if (hp != null)
        {
            float distShooter = Vector3.Distance(shooterPosition, transform.position);
            float dmg = ComputeDirectDamage(distShooter);

            bool isPlayerLayer = targetObj.layer == LayerMask.NameToLayer("Player");
            float finalDmg = (isPlayerLayer && isSelf) ? dmg * 0.4f : dmg;

            Debug.Log($"[Direct{(isSelf ? " Self" : "")}] {col.collider.name} took {Mathf.FloorToInt(finalDmg)} damage");

            var tgt = col.collider.GetComponent<Target>();
            if (tgt != null)
            {
                tgt.RecordExplosion(transform.position, dmg);
            }

            hp.TakeDamage(Mathf.FloorToInt(finalDmg));
        }

        Explode();
    }

    void Explode()
    {
        // 1) 파티클 생성 & 자동 삭제
        if (explosionParticlePrefab != null)
        {
            var effect = Instantiate(explosionParticlePrefab, transform.position, Quaternion.identity);

            var psList = effect.GetComponentsInChildren<ParticleSystem>();
            float maxDuration = 0f;
            foreach (var ps in psList)
            {
                var main = ps.main;
                float dur = main.duration + main.startLifetime.constantMax;
                if (dur > maxDuration) maxDuration = dur;
            }
            Destroy(effect, maxDuration);
        }

        // 2) 사운드
        if (explosionSoundClip != null)
            AudioSource.PlayClipAtPoint(explosionSoundClip, transform.position, explosionSoundVolume);

        // 3) 데미지 & 넉백 처리
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var c in hits)
        {
            if (c == null || c == lastDirectHit) continue;

            var hp = c.GetComponent<Health>();
            if (hp == null) continue;

            bool isSelf = (hp == shooterHealth);
            float distShooter = Vector3.Distance(shooterPosition, transform.position);
            float directDmg = ComputeDirectDamage(distShooter);
            float distExpl = Vector3.Distance(transform.position, c.transform.position);
            float splashDmg = ComputeSplashDamage(directDmg, distExpl);
            float finalDmg = isSelf ? splashDmg * 0.4f : splashDmg;

            hp.TakeDamage(Mathf.FloorToInt(finalDmg));

            // 넉백
            Vector3 kbDir = (c.transform.position - transform.position).normalized;
            float force = splashDmg * knockbackMultiplier;
            var surfChar = c.GetComponent<SurfCharacter>();
            if (surfChar != null)
            {
                StartCoroutine(ApplyGradualKnockbackWithGravity(surfChar, kbDir * force, 0.2f));
                continue;
            }
            var knockTarget = c.GetComponent<KnockbackTarget>();
            if (knockTarget != null)
                knockTarget.ApplyKnockback(kbDir * force);
        }

        // 4) 로켓 오브젝트 정리
        Destroy(gameObject);
    }

    private IEnumerator ApplyGradualKnockback(SurfCharacter surfChar, Vector3 totalForce, float duration)
    {
        int steps = Mathf.CeilToInt(duration / Time.fixedDeltaTime);
        Vector3 stepForce = totalForce / steps;

        for (int i = 0; i < steps; i++)
        {
            surfChar.AddExternalVelocity(stepForce);
            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator ApplyGradualKnockbackWithGravity(SurfCharacter sc, Vector3 totalForce, float duration)
    {
        sc.MoveData.GravityFactor = 0.85f;
        yield return StartCoroutine(ApplyGradualKnockback(sc, totalForce, duration));
        sc.MoveData.GravityFactor = 1f;
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