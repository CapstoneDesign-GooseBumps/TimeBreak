using UnityEngine;
using System.Collections;
public class Rocket : MonoBehaviour
{
    [Header("Motion")]
    public float speed = 25f;

    [Header("Direct Damage Settings")]
    public float directMaxDamage = 112f;
    public float baseDamage = 90f;
    public float midMinDamage = 50f;
    public float falloffMinDamage = 48f;
    public float pointBlankRange = 3f;
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

        // 정확한 Health 컴포넌트 저장
        shooterHealth = shooterObj.GetComponentInChildren<Health>();

        // 발사자와 충돌 무시
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

        // 💥 반드시 먼저 설정
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
        if (explosionParticlePrefab != null)
            Instantiate(explosionParticlePrefab, transform.position, Quaternion.identity);

        if (explosionSoundClip != null)
            AudioSource.PlayClipAtPoint(explosionSoundClip, transform.position, explosionSoundVolume);

        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var c in hits)
        {
            if (c == null || c == lastDirectHit)
                continue;

            var hp = c.GetComponent<Health>();
            if (hp == null)
                continue;

            bool isSelf = (hp == shooterHealth);

            float distShooter = Vector3.Distance(shooterPosition, transform.position);
            float directDmg = ComputeDirectDamage(distShooter);
            float distExpl = Vector3.Distance(transform.position, c.transform.position);
            float splashDmg = ComputeSplashDamage(directDmg, distExpl);
            float finalDmg = isSelf ? splashDmg * 0.4f : splashDmg;

            Debug.Log($"[Splash{(isSelf ? " Self" : "")}] {c.name} took {Mathf.FloorToInt(finalDmg)} damage");

            var tgt = c.GetComponent<Target>();
            if (tgt != null)
                tgt.RecordExplosion(transform.position, splashDmg);

            hp.TakeDamage(Mathf.FloorToInt(finalDmg));

            // ✅ 넉백 처리
            Vector3 knockbackDir = (c.transform.position - transform.position).normalized;
            float force = splashDmg * knockbackMultiplier;

            // 플레이어에게는 SurfCharacter 방식
            var surfChar = c.GetComponent<Fragsurf.Movement.SurfCharacter>();
            if (surfChar != null)
            {
                surfChar.AddExternalVelocity(knockbackDir * force);
            }

            // 연습 타겟에게는 KnockbackTarget 방식
            var knockTarget = c.GetComponent<KnockbackTarget>();
            if (knockTarget != null)
            {
                Vector3 dir = (c.transform.position - transform.position).normalized;
                float knockbackForce = splashDmg * knockbackMultiplier;
                knockTarget.ApplyKnockback(dir * knockbackForce);
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