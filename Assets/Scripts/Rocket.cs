using UnityEngine;
using System.Collections;

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
    private GameObject shooterObject;
    private Collider lastDirectHit;
    private bool ignoreFirstFrame = true;

    void Start()
    {
        // 초기 발사 속도 적용 (Translate 대신 물리 기반)
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = transform.forward * speed;
        }

        // 첫 프레임만 충돌 무시
        StartCoroutine(EnableCollisionNextFrame());
    }

    IEnumerator EnableCollisionNextFrame()
    {
        yield return null;
        ignoreFirstFrame = false;
    }

    /// <summary>
    /// 발사자 정보 세팅
    /// </summary>
    public void Initialize(Vector3 shooterPos, GameObject shooterObj)
    {
        shooterPosition = shooterPos;
        shooterObject = shooterObj;

        // 발사자의 모든 Collider와 충돌 무시
        var myCol = GetComponent<Collider>();
        if (myCol != null)
        {
            foreach (var col in shooterObject.GetComponentsInChildren<Collider>())
            {
                Physics.IgnoreCollision(myCol, col, true);
            }
        }
    }

    void OnCollisionEnter(Collision col)
    {
        // 첫 프레임 딜레이 혹은 발사자 자신과의 충돌은 무시
        if (ignoreFirstFrame || col.gameObject == shooterObject)
            return;

        // 직격 피해 처리
        var hp = col.collider.GetComponent<Health>();
        if (hp != null)
        {
            float distShooter = Vector3.Distance(shooterPosition, transform.position);
            float dmg = ComputeDirectDamage(distShooter);
            Debug.Log($"[Direct] {col.collider.name} took {Mathf.FloorToInt(dmg)} damage");

            var tgt = col.collider.GetComponent<Target>();
            if (tgt != null)
                tgt.RecordExplosion(transform.position, dmg);

            hp.TakeDamage(dmg);
            lastDirectHit = col.collider;
        }

        Explode();
    }

    void Explode()
    {
        // 이펙트
        if (explosionParticlePrefab != null)
            Instantiate(explosionParticlePrefab, transform.position, Quaternion.identity);
        if (explosionSoundClip != null)
            AudioSource.PlayClipAtPoint(explosionSoundClip, transform.position, explosionSoundVolume);

        // 스플래시 피해
        var hits = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var c in hits)
        {
            // 발사자 콜라이더 전체, 그리고 직격 대상은 제외
            if (c == null || c.gameObject == shooterObject || c == lastDirectHit)
                continue;

            var hp = c.GetComponent<Health>();
            if (hp != null)
            {
                float distExpl = Vector3.Distance(transform.position, c.transform.position);
                float directDmg = ComputeDirectDamage(Vector3.Distance(shooterPosition, transform.position));
                float splashDmg = ComputeSplashDamage(directDmg, distExpl);
                Debug.Log($"[Splash] {c.name} took {Mathf.FloorToInt(splashDmg)} damage");

                var tgt = c.GetComponent<Target>();
                if (tgt != null)
                    tgt.RecordExplosion(transform.position, splashDmg);

                hp.TakeDamage(Mathf.FloorToInt(splashDmg));
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