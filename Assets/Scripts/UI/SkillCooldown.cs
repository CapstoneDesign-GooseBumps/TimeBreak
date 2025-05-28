using UnityEngine;
using UnityEngine.UI;

public class SkillCooldown : MonoBehaviour
{
    public Image brightImage;
    public float cooldownDuration = 5f;

    float timer = 0f;
    bool cooling = false;

    public void Trigger()
    {
        brightImage.fillAmount = 1f;
        timer = cooldownDuration;
        cooling = true;
    }

    void Update()
    {
        if (!cooling) return;

        timer -= Time.deltaTime;
        brightImage.fillAmount = Mathf.Clamp01(timer / cooldownDuration);

        if (timer <= 0f)
        {
            brightImage.fillAmount = 1f;
            cooling = false;
        }
    }

    public bool IsReady => !cooling;
}
