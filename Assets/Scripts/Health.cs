// Health.cs 수정
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 300f;            
    [SerializeField] private float currentHealth;

    [Header("Events")]
    public UnityEvent<float> OnTakeDamage;    
    public UnityEvent<float> OnHeal;          
    public UnityEvent OnDie;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (currentHealth <= 0f) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0f);

        OnTakeDamage?.Invoke(currentHealth);

        if (currentHealth == 0f)
            Die();
    }

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
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 리스폰 시 체력을 완전히 회복시키고 OnHeal 이벤트를 발생시킵니다.
    /// </summary>
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        OnHeal?.Invoke(currentHealth);
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }
}