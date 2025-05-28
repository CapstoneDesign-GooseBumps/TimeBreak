using UnityEngine;

public class HideOwnBody : MonoBehaviour
{
    public bool isLocalPlayer; // 당신의 네트워크 시스템에 따라 이 값은 다르게 설정합니다

    void Start()
    {
        if (isLocalPlayer)
        {
            foreach (var renderer in GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = false;
            }
        }
    }
}