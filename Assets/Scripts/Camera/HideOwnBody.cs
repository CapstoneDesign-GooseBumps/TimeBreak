using UnityEngine;

public class HideOwnBody : MonoBehaviour
{
    public bool isLocalPlayer; // 네트워크 플레이어 여부 (예: Mirror, Photon 등과 연동 가능)

    void Start()
    {
        if (isLocalPlayer)
        {
            foreach (var renderer in GetComponentsInChildren<Renderer>())
            {
                // "Weapon" 태그가 붙은 오브젝트는 숨기지 않음
                if (renderer.CompareTag("Weapon"))
                    continue;

                renderer.enabled = false;
            }
        }
    }
}