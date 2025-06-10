using System.Collections;
using UnityEngine;

public class GrenadeLauncher : MonoBehaviour
{
    [Header("References")]
    public GameObject grenadePrefab;
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
    public float fireDelay = 0.6f;
    public float firstReloadTime = 1.24f;
    public float regularReloadTime = 0.6f;

    public float fireForce = 15f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip fireClip;
    public AudioClip reloadInsertClip;
    public AudioClip reloadOpenClip;
    public AudioClip reloadCloseClip;
    public AudioClip deployClip;

    [Header("Weapon UI")]
    public GameObject grenadeImage; // GrenadeImage 오브젝트 참조

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

        if (grenadeImage != null)
            grenadeImage.SetActive(true); // 🔹 이미지 표시
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

        if (grenadeImage != null)
            grenadeImage.SetActive(false); // 🔹 이미지 숨김
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
        Vector3 spawnPos = firePoint.position + firePoint.forward * 0.5f;
        GameObject grenadeObj = Instantiate(grenadePrefab, spawnPos, Quaternion.identity);
        Rigidbody rb = grenadeObj.GetComponent<Rigidbody>();
        if (rb != null)
            rb.linearVelocity = firePoint.forward * fireForce;

        var grenade = grenadeObj.GetComponent<Grenade>();
        if (grenade != null)
            grenade.Initialize(transform.root.gameObject);

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
        bool isFirstShell = true;

        // 실린더 오픈
        if (audioSource && reloadOpenClip)
            audioSource.PlayOneShot(reloadOpenClip);

        yield return new WaitForSeconds(0.4f); // 오픈음 길이에 맞게 조정

        while (currentMagazine < magazineCapacity && currentAmmo > 0)
        {
            float delay = isFirstShell ? firstReloadTime : regularReloadTime;
            yield return new WaitForSeconds(delay);

            if (currentMagazine >= magazineCapacity || currentAmmo <= 0)
                break;

            if (audioSource && reloadInsertClip)
                audioSource.PlayOneShot(reloadInsertClip);

            currentMagazine++;
            currentAmmo--;
            UpdateAmmoUI();

            if (isFirstShell)
            {
                Debug.Log("[Grenade] 초탄 장전됨");
                isFirstShell = false;
            }
            else
            {
                Debug.Log("[Grenade] 차탄 장전됨");
            }
        }

        // 실린더 클로즈
        if (audioSource && reloadCloseClip)
            audioSource.PlayOneShot(reloadCloseClip);

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