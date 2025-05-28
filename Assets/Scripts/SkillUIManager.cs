using UnityEngine;

public class SkillUIManager : MonoBehaviour
{
    public SkillCooldown skill1, skill2, skill3;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && skill1.IsReady) skill1.Trigger();
        if (Input.GetKeyDown(KeyCode.Alpha2) && skill2.IsReady) skill2.Trigger();
        if (Input.GetKeyDown(KeyCode.Alpha3) && skill3.IsReady) skill3.Trigger();
    }
}
