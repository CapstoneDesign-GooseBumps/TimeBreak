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

        if (explosionEffect != null)
            Instantiate(explosionEffect, transform.position, Quaternion.identity);

        if (explosionSound != null)
            AudioSource.PlayClipAtPoint(explosionSound, transform.position, explosionVolume);

        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var c in hits)
        {
            if (c == null || c == lastDirectHit)
                continue;

            var hp = c.GetComponent<Health>();
            if (hp != null)
            {
                float dist = Vector3.Distance(transform.position, c.transform.position);
                float t = Mathf.Clamp01(dist / explosionRadius);
                float dmg = Mathf.Lerp(splashMaxDamage, splashMinDamage, t);

                Debug.Log($"[Grenade Splash] {c.name} took {dmg} damage");
                hp.TakeDamage(Mathf.FloorToInt(dmg));
            }
        }

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
