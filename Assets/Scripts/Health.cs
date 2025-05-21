using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;            // 최대 체력
    [SerializeField] private float currentHealth;

    [Header("Events")]
    public UnityEvent<float> OnTakeDamage;    // 파라미터: 남은 체력
    public UnityEvent<float> OnHeal;          // 파라미터: 남은 체력
    public UnityEvent OnDie;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    /// <summary>
    /// 외부에서 데미지를 줄 때 호출합니다.
    /// </summary>
    public void TakeDamage(float amount)
    {
        if (currentHealth <= 0f) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0f);

        OnTakeDamage?.Invoke(currentHealth);

        if (currentHealth == 0f)
            Die();
    }

    /// <summary>
    /// 체력을 회복할 때 호출합니다.
    /// </summary>
    public void Heal(float amount)
    {
        if (currentHealth <= 0f) return;

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        OnHeal?.Invoke(currentHealth);
    }

    void Die()
    {
        OnDie?.Invoke();
        // 기본 사망 처리: 오브젝트 비활성화
        gameObject.SetActive(false);

        // 필요 시 여기에 리스폰, 애니메이션, 사운드 등을 추가하세요.
    }

    /// <summary>
    /// 외부에서 현재 체력을 조회할 수 있게 합니다.
    /// </summary>
    public float GetCurrentHealth()
    {
        return currentHealth;
    }
}