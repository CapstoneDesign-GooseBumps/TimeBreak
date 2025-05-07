using UnityEngine;

public class RocketLauncher : MonoBehaviour
{
    public GameObject rocketPrefab;      // 프리팹 연결
    public Transform firePoint;          // 발사 위치
    public float rocketSpeed = 30f;      // 속도

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // 마우스 왼쪽 클릭
        {
            FireRocket();
        }
    }

    private void FireRocket()
    {
        GameObject rocket = Instantiate(rocketPrefab, firePoint.position,Quaternion.Euler(0, 0, -90));

        Rigidbody rb = rocket.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = transform.up * rocketSpeed;
        }
        
    }
}