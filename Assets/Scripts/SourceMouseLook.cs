using UnityEngine;

public class SourceMouseLook : MonoBehaviour {

    [Header("Mouse Settings (Source style)")]
    public float sensitivity = 2.0f;  // Source Engine 기준 m_yaw/m_pitch 보정용 감도
    public float maxPitch = 89f;

    private Transform cam;
    private float pitch = 0f;

    void Start() {
        cam = Camera.main.transform;

        // Lock & hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update() {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Y축 회전: 수직 카메라 회전
        pitch -= mouseY * sensitivity * 0.022f; // Source 감도 보정
        pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);
        cam.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        // X축 회전: 플레이어 본체 회전
        float yaw = mouseX * sensitivity * 0.022f;
        transform.Rotate(Vector3.up * yaw);
    }
}