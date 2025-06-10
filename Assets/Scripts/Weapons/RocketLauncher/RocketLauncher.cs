using System.Collections;
using UnityEngine;

public class RocketLauncher : MonoBehaviour
{
    [Header("References")]
    public GameObject rocketPrefab;
    public Transform firePoint;
    public AmmoUIManager uiManager;

    [Header("Ammo Settings")]
    [SerializeField] private int magazineCapacity = 4;
    [SerializeField] private int reserveAmmo = 20;
    [SerializeField] private int currentMagazine;
    [SerializeField] private int currentAmmo;

    private bool isReloading = false;
    private bool isInFireDelay = false;
    private Coroutine reloadCoroutine = null;

    [Header("Timing")]
    public float fireDelay = 0.8f;
    public float firstReloadTime = 0.92f;
    public float regularReloadTime = 0.8f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip fireClip;
    public AudioClip reloadClip;
    public AudioClip deployClip;

    [Header("Weapon UI")]
    public GameObject rocketImage; // RocketImage 오브젝트 참조

    void Start()
    {
        currentMagazine = magazineCapacity;
        currentAmmo = reserveAmmo;
        UpdateAmmoUI();
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
    
    void OnEnable()
    {
        if (audioSource && deployClip)
            audioSource.PlayOneShot(deployClip);

        if (uiManager != null)
        {
            uiManager.Show();
            uiManager.UpdateAmmo(currentMagazine, currentAmmo);
        }

        if (rocketImage != null)
            rocketImage.SetActive(true); // 🔹 이미지 표시
    }

    void OnDisable()
    {
        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            reloadCoroutine = null;
        }

        isReloading = false;
        isInFireDelay = false;

        if (uiManager != null)
            uiManager.Hide();

        if (rocketImage != null)
            rocketImage.SetActive(false); // 🔹 이미지 숨김
    }

    void TryStartReload()
    {
        if (isReloading || currentMagazine >= magazineCapacity || currentAmmo <= 0)
            return;

        reloadCoroutine = StartCoroutine(ReloadMagazine());
    }

    IEnumerator HandleFire()
    {
        isInFireDelay = true;

        // 🔸 장전 중이면 중단
        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            reloadCoroutine = null;
            isReloading = false;
        }

        // 🔸 발사
        Vector3 spawnPos = firePoint.position + firePoint.forward * 0.6f;
        GameObject rocketObj = Instantiate(rocketPrefab, spawnPos, firePoint.rotation);

        var rocket = rocketObj.GetComponent<Rocket>();
        if (rocket != null)
            rocket.Initialize(transform.position, transform.root.gameObject);

        currentMagazine--;
        UpdateAmmoUI();

        if (audioSource && fireClip)
            audioSource.PlayOneShot(fireClip);

        yield return new WaitForSeconds(fireDelay);
        isInFireDelay = false;
    }

    IEnumerator ReloadMagazine()
    {
        isReloading = true;
        float delay = firstReloadTime;
        bool isFirstShell = true;

        while (currentMagazine < magazineCapacity && currentAmmo > 0)
        {
            yield return new WaitForSeconds(delay);

            if (currentMagazine >= magazineCapacity || currentAmmo <= 0)
                break;

            currentMagazine++;
            currentAmmo--;
            UpdateAmmoUI();

            if (audioSource && reloadClip)
                audioSource.PlayOneShot(reloadClip);

            if (isFirstShell)
            {
                Debug.Log("[Rocket] 초탄 장전됨");
                isFirstShell = false;
            }
            else
            {
                Debug.Log("[Rocket] 차탄 장전됨");
            }

            delay = regularReloadTime;
        }

        isReloading = false;
        reloadCoroutine = null;
    }

    void UpdateAmmoUI()
    {
        if (uiManager != null)
            uiManager.UpdateAmmo(currentMagazine, currentAmmo);
    }

    public void ResetAmmo()
    {
        currentMagazine = magazineCapacity;
        currentAmmo = reserveAmmo;
        UpdateAmmoUI();
    }

}