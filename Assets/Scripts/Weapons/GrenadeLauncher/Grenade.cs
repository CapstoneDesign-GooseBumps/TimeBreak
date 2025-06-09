using UnityEngine;
using System.Collections;

public class Grenade : MonoBehaviour
{
    public float explosionRadius = 3f;
    public float countdown = 2.3f;

    public float directDamage = 100f;
    public float splashMinDamage = 30f;
    public float splashMaxDamage = 60f;

    public GameObject explosionEffect;
    public AudioClip explosionSound;
    public float explosionVolume = 1f;

    private GameObject shooter;
    private Collider lastDirectHit;
    private bool exploded = false;

    void Start()
    {
        StartCoroutine(CountdownToExplode());
    }

    public void Initialize(GameObject shooterObj)
    {
        shooter = shooterObj;
        Collider myCol = GetComponent<Collider>();
        foreach (var col in shooterObj.GetComponentsInChildren<Collider>())
        {
            Physics.IgnoreCollision(myCol, col, true);
        }
    }

    IEnumerator CountdownToExplode()
    {
        yield return new WaitForSeconds(countdown);
        Explode();
    }

    void OnCollisionEnter(Collision col)
    {
        if (exploded) return;

        GameObject targetObj = col.collider.transform.root.gameObject;

        if (targetObj.layer == LayerMask.NameToLayer("Player") && targetObj != shooter)
        {
            var hp = col.collider.GetComponent<Health>();
            if (hp != null)
            {
                Debug.Log($"[Grenade Direct Hit] {targetObj.name} took {directDamage} damage");
                hp.TakeDamage(Mathf.FloorToInt(directDamage));
                lastDirectHit = col.collider;
            }

            Explode();
        }
        // else: 굴러다님. 아무 일도 하지 않음.
    }

    void Explode()
    {
        if (exploded) return;
        exploded = true;

        // 1) 파티클 생성 & 자동 삭제
        if (explosionEffect != null)
        {
            var effect = Instantiate(explosionEffect, transform.position, Quaternion.identity);

            // 자식 포함 모든 파티클 시스템의 최대 재생 시간 계산
            var psList = effect.GetComponentsInChildren<ParticleSystem>();
            float maxDuration = 0f;
            foreach (var ps in psList)
            {
                var main = ps.main;
                float dur = main.duration + main.startLifetime.constantMax;
                if (dur > maxDuration) maxDuration = dur;
            }
            // 재생 종료 후 오브젝트 정리
            Destroy(effect, maxDuration);
        }

        // 2) 사운드
        if (explosionSound != null)
            AudioSource.PlayClipAtPoint(explosionSound, transform.position, explosionVolume);

        // 3) 데미지 처리
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var c in hits)
        {
            if (c == null || c == lastDirectHit) continue;

            var hp = c.GetComponent<Health>();
            if (hp != null)
            {
                float dist = Vector3.Distance(transform.position, c.transform.position);
                float t = Mathf.Clamp01(dist / explosionRadius);
                float dmg = Mathf.Lerp(splashMaxDamage, splashMinDamage, t);
                hp.TakeDamage(Mathf.FloorToInt(dmg));
            }
        }

        // 4) 파편(수류탄) 오브젝트 제거
        Destroy(gameObject);
    }


#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
#endif
}
