using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public Transform rocket; //로켓 프리팹
    public Transform launch; //로켓 발사 위치

    float speedMove = 8; //이동속도
    float speedTurn = 90; //회전속도
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        MovePlayer();
        if (Input.GetButtonDown("Fire1")){ //Left crtl, 마우스 왼쪽 클릭
            LaunchRocket();
        }
    }

    //로켓 발사
    private void LaunchRocket(){
        // launch의 위치에, launch의 방향으로 rocket 배치
        Instantiate(rocket, launch.position, launch.rotation);
    }

    //이동 및 회전
    void MovePlayer()
    {
        //이동
        float keyV = Input.GetAxis("Vertical");
        float amtMove = keyV * speedMove * Time.deltaTime;
        transform.Translate(Vector3.forward * amtMove);

        //회전
        float keyH = Input.GetAxis("Horizontal");
        float amtTurn = keyH * speedTurn * Time.deltaTime;
        transform.Rotate(Vector3.up * amtTurn);

    }
}
