using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillUIManager : MonoBehaviour
{
    public Image[] skillFillImages;          // Foreground: 밝은 이미지 (Filled)
    public float skillCooldownTime = 10f;
    private float[] currentCooldowns = new float[4];
    private bool[] isCooling = new bool[4];

    public TextMeshProUGUI gameTimerText;
    public float totalTime = 100f;

// SkillUIManager.cs 안에 아래 코드 추가
public TMPro.TextMeshProUGUI ammoText;

public void UpdateAmmo(int current, int max)
{
    ammoText.text = $"{current} / {max}";
}


    void Start()
    {
        for (int i = 0; i < 4; i++)
        {
            skillFillImages[i].fillAmount = 1f;   // 처음엔 꽉 찬 밝은 아이콘
            isCooling[i] = false;
        }
    }

    void Update()
    {
        for (int i = 0; i < 4; i++)
        {
            if (isCooling[i])
            {
                currentCooldowns[i] -= Time.unscaledDeltaTime;

                if (currentCooldowns[i] <= 0f)
                {
                    currentCooldowns[i] = 0f;
                    isCooling[i] = false;
                    skillFillImages[i].fillAmount = 1f; // 원래대로 다시 채움
                }
                else
                {
                    float ratio = currentCooldowns[i] / skillCooldownTime;
                    skillFillImages[i].fillAmount = ratio;
                }
            }
        }

        if (totalTime > 0f)
        {
            totalTime -= Time.unscaledDeltaTime;
            if (totalTime < 0f) totalTime = 0f;
            gameTimerText.text = Mathf.CeilToInt(totalTime).ToString();
        }
    }

    public void TriggerSkill(int index)
    {
        currentCooldowns[index] = skillCooldownTime;
        isCooling[index] = true;
        skillFillImages[index].fillAmount = 1f; // Radial 채우기 시작
    }

    public bool IsSkillOnCooldown(int index)
    {
        return isCooling[index];
    }
}
