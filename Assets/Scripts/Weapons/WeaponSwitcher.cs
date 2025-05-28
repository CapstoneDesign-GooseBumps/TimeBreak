using UnityEngine;

public class WeaponSwitcher : MonoBehaviour
{
    public GameObject rocketLauncher;
    public GameObject grenadeLauncher;

    private bool usingRocket = true;

    void Start()
    {
        UpdateWeaponState();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            usingRocket = !usingRocket;
            UpdateWeaponState();
        }
    }

    void UpdateWeaponState()
    {
        if (rocketLauncher != null)
            rocketLauncher.SetActive(usingRocket);

        if (grenadeLauncher != null)
            grenadeLauncher.SetActive(!usingRocket);
    }
}