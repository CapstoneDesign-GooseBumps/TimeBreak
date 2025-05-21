using System.Collections;
using UnityEngine;

public class RocketLauncher : MonoBehaviour
{
    [Header("References")]
    public GameObject rocketPrefab;
    public Transform  firePoint;

    [Header("Ammo Settings")]
    public int magazineCapacity = 4;   // 탄창 총알 수
    public int reserveAmmo      = 20;  // 예비 탄약
    int currentMagazine;              // 현재 탄창에 남은 수
    bool isReloading = false;         // 재장전 중인가
    bool canFire     = true;          // 발사 딜레이 중인가?
    
    [Header("Timing")]
    public float firstReloadTime = 0.92f;  // 초탄 재장전 시간
    public float regularReloadTime = 0.8f; // 그 이후 재장전/발사 간격

    void Start()
    {
        currentMagazine = magazineCapacity;
    }

    void Update()
    {
        if (isReloading) return;

        // 발사
        if (canFire && Input.GetButtonDown("Fire1") && currentMagazine > 0)
        {
            StartCoroutine(HandleFire());
        }
        // 수동 재장전 (R 키), 자동 재장전은 탄창 소진 시 HandleFire에서 트리거해도 됩니다.
        else if (Input.GetKeyDown(KeyCode.R) && currentMagazine < magazineCapacity && reserveAmmo > 0)
        {
            StartCoroutine(ReloadMagazine());
        }
    }

    IEnumerator HandleFire()
    {
        canFire = false;

        // 1) 로켓 생성
        Instantiate(rocketPrefab, firePoint.position, firePoint.rotation);
        // 2) 탄약 차감
        currentMagazine--;

        // 3) 다음 발사까지 대기
        //    첫 발사(탄창을 막 채워졌을 때)인지 판정
        float delay = (currentMagazine == magazineCapacity - 1)
            ? firstReloadTime
            : regularReloadTime;
        yield return new WaitForSeconds(delay);

        // 4) 탄창 비면 자동 재장전
        if (currentMagazine <= 0 && reserveAmmo > 0)
        {
            StartCoroutine(ReloadMagazine());
        }
        else
        {
            canFire = true;
        }
    }

    IEnumerator ReloadMagazine()
    {
        isReloading = true;
        canFire     = false;

        // 한 발씩 채우는 방식: 첫 로드에는 firstReloadTime, 그 뒤엔 regularReloadTime
        float delay = firstReloadTime;
        while (currentMagazine < magazineCapacity && reserveAmmo > 0)
        {
            yield return new WaitForSeconds(delay);
            currentMagazine++;
            reserveAmmo--;
            delay = regularReloadTime;
        }

        isReloading = false;
        canFire     = true;
    }
}