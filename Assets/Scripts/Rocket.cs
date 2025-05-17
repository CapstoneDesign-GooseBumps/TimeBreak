using UnityEngine;

public class Rocket : MonoBehaviour
{
    float speed = 30;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject, 3); //3초 후에 오브젝트 삭제
    }

    // Update is called once per frame
    void Update()
    {
        float amtMove = speed * Time.deltaTime;
        transform.Translate(Vector3.forward * amtMove);
    }

    // 충돌 처리
    private void OnCollisionEnter(Collision other){
        Destroy(other.gameObject); //상대방 제거
        Destroy(gameObject); // 자신제거
    }

}
