using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeSkillManager : MonoBehaviour
{
    [Header("Skill Values")]
    public float slowdownAmount = -0.5f;    // 감속 시 곱해질 값 (빼는 형태)
    public float speedupAmount   =  0.2f;   // 가속 시 더해질 값
    public float slowdownDuration = 5f;     // 감속 지속시간
    public float speedupDuration  = 5f;     // 가속 지속시간
    public float stopDuration     = 3f;     // 정지 지속시간

    // 내부 구조체: 각 스킬 효과의 남은 시간과 값
    private struct SkillEffect
    {
        public float value;
        public float remaining;
        public SkillEffect(float v, float r) { value = v; remaining = r; }
    }

    private List<SkillEffect> effects = new List<SkillEffect>();
    private bool isStopped = false;
    private float stopTimeRemaining = 0f;

    /// <summary>
    /// 외부에서 읽어다 사용할 최종 배속값
    /// </summary>
    public float TimeMultiplier { get; private set; } = 1f;

    void Update()
    {
        // 1,2,3 키 입력으로 스킬 발동
        if (Input.GetKeyDown(KeyCode.Alpha1))
            StartCoroutine(ApplySkill(slowdownAmount, slowdownDuration, isStopSkill: false));
        if (Input.GetKeyDown(KeyCode.Alpha2))
            StartCoroutine(ApplySkill(speedupAmount, speedupDuration, isStopSkill: false));
        if (Input.GetKeyDown(KeyCode.Alpha3))
            StartCoroutine(ApplySkill(0f, stopDuration, isStopSkill: true));

        // 모든 효과 지속시간 감소
        for (int i = effects.Count - 1; i >= 0; i--)
        {
            var e = effects[i];
            e.remaining -= Time.deltaTime;
            if (e.remaining <= 0f)
                effects.RemoveAt(i);
            else
                effects[i] = e;
        }

        // 정지 스킬 시간 감소
        if (isStopped)
        {
            stopTimeRemaining -= Time.deltaTime;
            if (stopTimeRemaining <= 0f)
                isStopped = false;
        }

        // 최종 multiplier 계산
        if (isStopped)
        {
            TimeMultiplier = 0f;
        }
        else
        {
            float sum = 0f;
            foreach (var e in effects) sum += e.value;
            TimeMultiplier = 1f + sum;
        }
    }

    private IEnumerator ApplySkill(float value, float duration, bool isStopSkill)
    {
        if (isStopSkill)
        {
            // 즉시 정지
            isStopped = true;
            stopTimeRemaining = duration;
        }
        else
        {
            // 신규 스킬 효과 추가 (가감속)
            effects.Add(new SkillEffect(value, duration));
        }

        // duration 후 자동 종료 (Update()에서 처리)
        yield return new WaitForSeconds(duration);
    }
}