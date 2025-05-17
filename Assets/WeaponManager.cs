using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public int[] maxAmmo = { 6, 4 };
    private int[] currentAmmo = { 6, 4 };
    private int currentWeapon = 0;

    private bool isReloading = false;

    public SkillUIManager uiManager;

    void Start()
    {
        UpdateUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            currentWeapon = 1 - currentWeapon;
            UpdateUI();
        }

        if (Input.GetMouseButtonDown(0) && !isReloading)
        {
            if (currentAmmo[currentWeapon] > 0)
            {
                currentAmmo[currentWeapon]--;
                UpdateUI();
            }
        }

        if (Input.GetKeyDown(KeyCode.R) && !isReloading)
        {
            StartCoroutine(Reload());
        }
    }

    System.Collections.IEnumerator Reload()
    {
        isReloading = true;
        yield return new WaitForSeconds(2f);
        currentAmmo[currentWeapon] = maxAmmo[currentWeapon];
        isReloading = false;
        UpdateUI();
    }

    void UpdateUI()
    {
        uiManager.UpdateAmmo(currentAmmo[currentWeapon], maxAmmo[currentWeapon]);
    }
}
