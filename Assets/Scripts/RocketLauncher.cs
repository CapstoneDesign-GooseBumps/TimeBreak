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
        if (isReloading) return;
        if (currentMagazine >= magazineCapacity) return;
        if (currentAmmo <= 0) return;

        StartCoroutine(ReloadMagazine());
    }

    IEnumerator HandleFire()
    {
        isInFireDelay = true;

        // 발사 위치를 몸체에서 0.6m 전방으로 오프셋
        Vector3 spawnPos = firePoint.position + firePoint.forward * 0.6f;
        GameObject rocketObj = Instantiate(rocketPrefab, spawnPos, firePoint.rotation);

        // 초기화: 위치 + 플레이어 게임오브젝트
        var rocket = rocketObj.GetComponent<Rocket>();
        if (rocket != null)
        {
            rocket.Initialize(transform.position, gameObject);
        }

        currentMagazine--;

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

            currentMagazine = Mathf.Min(currentMagazine + 1, magazineCapacity);
            currentAmmo--;

            if (audioSource && reloadClip)
                audioSource.PlayOneShot(reloadClip);

            delay = regularReloadTime;
        }

        isReloading = false;
    }
}