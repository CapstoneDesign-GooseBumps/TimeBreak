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
    
    [SerializeField, Tooltip("현재 장전된 탄 수 (0 ~ magazineCapacity)")]
    private int currentMagazine;

    [SerializeField, Tooltip("예비 탄약 (장탄 제외한 나머지)")]
    private int currentAmmo;
    private bool isReloading = false;
    private bool canFire = true;
    private Coroutine reloadCoroutine;

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
        if (Input.GetButtonDown("Fire1") && currentMagazine > 0)
        {
            // 🔸 장전 중단
            if (isReloading && reloadCoroutine != null)
            {
                StopCoroutine(reloadCoroutine);
                isReloading = false;
                canFire = true;
            }

            if (canFire)
                StartCoroutine(HandleFire());
        }
        else if (Input.GetKeyDown(KeyCode.R) && currentMagazine < magazineCapacity && currentAmmo > 0)
        {
            // 🔸 재장전 중이 아니라면 시작
            if (!isReloading)
                reloadCoroutine = StartCoroutine(ReloadMagazine());
        }
    }

    IEnumerator HandleFire()
    {
        canFire = false;

        Instantiate(rocketPrefab, firePoint.position, firePoint.rotation);
        currentMagazine--;

        // 🔸 발사음
        if (audioSource && fireClip)
            audioSource.PlayOneShot(fireClip);

        float delay = (currentMagazine == magazineCapacity - 1) ? firstReloadTime : regularReloadTime;
        yield return new WaitForSeconds(delay);

        if (currentMagazine <= 0 && currentAmmo > 0)
        {
            reloadCoroutine = StartCoroutine(ReloadMagazine());
        }
        else
        {
            canFire = true;
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

            currentMagazine++;
            currentAmmo--;

            // 장전음
            if (audioSource && reloadClip)
                audioSource.PlayOneShot(reloadClip);

            // 이후 장탄부터는 일반 시간
            delay = regularReloadTime;
        }

        isReloading = false;
        canFire = true;
        reloadCoroutine = null;
    }
}