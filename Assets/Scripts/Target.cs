using UnityEngine;
using Fragsurf.Movement;

[RequireComponent(typeof(Health))]
public class Target : MonoBehaviour
{
    public float knockbackMultiplier = 0.1f;

    private Vector3 _lastExplosionOrigin;
    private float _lastDamage;

    private void Awake()
    {
        var health = GetComponent<Health>();
        if (health != null)
        {
            // OnTakeDamage가 float → currentHealth일 경우, Ignore해도 됨
            // 핵심은 데미지 직후 호출
            health.OnTakeDamage.AddListener(OnTakeDamage);
        }
    }

    /// <summary>
    /// Rocket.cs에서 폭발 발생 직전에 호출됨
    /// </summary>
    public void RecordExplosion(Vector3 origin, float damage)
    {
        _lastExplosionOrigin = origin;
        _lastDamage = damage;
    }

    /// <summary>
    /// Health.TakeDamage()가 실행된 후 자동 호출됨
    /// </summary>
    private void OnTakeDamage(float currentHealth)
    {
        var surf = GetComponentInParent<SurfCharacter>();
        if (surf == null) return;

        Vector3 dir = (transform.position - _lastExplosionOrigin).normalized;
        float force = _lastDamage * knockbackMultiplier;

        surf.AddExternalVelocity(dir * force);

        // 디버그용 로그
        Debug.Log($"[Target] Knockback applied to SurfCharacter: {force} in direction {dir}");
    }
}