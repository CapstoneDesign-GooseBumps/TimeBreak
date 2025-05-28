using System.Collections;
using UnityEngine;

public class RocketLauncher : MonoBehaviour
{
    [Header("References")]
    public GameObject rocketPrefab;
    public Transform firePoint;
    public AmmoUIManager uiManager; // 🔹 UI 연결

    [Header("Ammo Settings")]
    [SerializeField] private int magazineCapacity = 4;
    [SerializeField] private int reserveAmmo = 20;
    [SerializeField] private int currentMagazine;
    [SerializeField] private int currentAmmo;

    private bool isReloading = false;
    private bool isInFireDelay = false;

    [Header("Timing")]
    public float firstReloadTime = 0.92f;
    public float regularReloadTime = 0.8f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip fireClip;
    public AudioClip reloadClip;

    void Start()
    {
        currentMagazine = magazineCapacity;
        currentAmmo = reserveAmmo;
        UpdateAmmoUI(); // 🔸 시작 시 UI 초기화
    }

    void Update()
    {
        if (Input.GetButton("Fire1") && currentMagazine > 0 && !isInFireDelay)
        {
            StartCoroutine(HandleFire());
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            TryStartReload();
        }
    }

    void TryStartReload()
    {
        if (isReloading || currentMagazine >= magazineCapacity || currentAmmo <= 0)
            return;

        StartCoroutine(ReloadMagazine());
    }

    IEnumerator HandleFire()
    {
        isInFireDelay = true;

        Vector3 spawnPos = firePoint.position + firePoint.forward * 0.6f;
        GameObject rocketObj = Instantiate(rocketPrefab, spawnPos, firePoint.rotation);

        var rocket = rocketObj.GetComponent<Rocket>();
        if (rocket != null)
            rocket.Initialize(transform.position, transform.root.gameObject);

        currentMagazine--;
        UpdateAmmoUI(); // 🔸 탄약 감소 시 UI 반영

        if (audioSource && fireClip)
            audioSource.PlayOneShot(fireClip);

        float delay = (currentMagazine == magazineCapacity - 1) ? firstReloadTime : regularReloadTime;
        yield return new WaitForSeconds(delay);

        isInFireDelay = false;

        if (currentMagazine <= 0 && currentAmmo > 0 && !isReloading)
        {
            TryStartReload();
        }
    }

    IEnumerator ReloadMagazine()
    {
        isReloading = true;

        float delay = firstReloadTime;
        while (currentMagazine < magazineCapacity && currentAmmo > 0)
        {
            yield return new WaitForSeconds(delay);

            currentMagazine++;
            currentAmmo--;

            UpdateAmmoUI(); // 🔸 장전 시 UI 반영

            if (audioSource && reloadClip)
                audioSource.PlayOneShot(reloadClip);

            delay = regularReloadTime;
        }

        isReloading = false;
    }

    void UpdateAmmoUI()
    {
        if (uiManager != null)
        {
            uiManager.UpdateAmmo(currentMagazine, currentAmmo); // 💡 직접 전달
        }
    }
}