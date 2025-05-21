using System.Collections;
using UnityEngine;

public class RocketLauncher : MonoBehaviour
{
    [Header("References")]
    public GameObject rocketPrefab;
    public Transform firePoint;

    [Header("Ammo Settings")]
    public int magazineCapacity = 4;
    public int reserveAmmo = 20;

    [SerializeField]
    private int currentMagazine;

    [SerializeField]
    private int currentAmmo;

    private bool isReloading = false;
    private bool canFire = true;

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
    }

    void Update()
    {
        if (Input.GetButton("Fire1") && currentMagazine > 0 && canFire)
        {
            // 장전 중이면 중단
            if (isReloading)
                return;

            StartCoroutine(HandleFire());
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            TryStartReload();
        }
    }

    void TryStartReload()
    {
        // 조건을 철저히 검사
        if (isReloading) return;
        if (currentMagazine >= magazineCapacity) return;
        if (currentAmmo <= 0) return;

        StartCoroutine(ReloadMagazine());
    }

    IEnumerator HandleFire()
    {
        canFire = false;

        Instantiate(rocketPrefab, firePoint.position, firePoint.rotation);
        currentMagazine--;

        if (audioSource && fireClip)
            audioSource.PlayOneShot(fireClip);

        float delay = (currentMagazine == magazineCapacity - 1) ? firstReloadTime : regularReloadTime;
        yield return new WaitForSeconds(delay);

        canFire = true;

        if (currentMagazine <= 0 && currentAmmo > 0)
        {
            TryStartReload();
        }
    }

    IEnumerator ReloadMagazine()
    {
        isReloading = true;
        canFire = false;

        float delay = firstReloadTime;

        while (currentMagazine < magazineCapacity && currentAmmo > 0)
        {
            yield return new WaitForSeconds(delay);

            currentMagazine = Mathf.Min(currentMagazine + 1, magazineCapacity);
            currentAmmo--;

            if (audioSource && reloadClip)
                audioSource.PlayOneShot(reloadClip);

            delay = regularReloadTime;
        }

        isReloading = false;
        canFire = true;
    }
}