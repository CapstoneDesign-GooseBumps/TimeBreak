using UnityEngine;

public class GameUI : MonoBehaviour
{
    public GUIStyle healthBarStyle;
    public GUIStyle vsStyle;
    public GUIStyle lifeBoxStyle;

    private Health healthP1;
    private Health healthP2;

    private float maxHealth = 300f;

    void Start()
    {
        healthP1 = GameObject.FindGameObjectWithTag("Player1").GetComponent<Health>();
        healthP2 = GameObject.FindGameObjectWithTag("Player2").GetComponent<Health>();
    }

    void OnGUI()
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // 체력바 위치
        Rect p1BarRect = new Rect(20, 20, screenWidth / 3f, 25);
        Rect p2BarRect = new Rect(screenWidth - screenWidth / 3f - 20, 20, screenWidth / 3f, 25);
        Rect vsRect = new Rect(screenWidth / 2f - 30, 20, 60, 25);

        // 체력 수치 (0~1로 정규화)
        float p1HealthRatio = Mathf.Clamp01(healthP1.GetCurrentHealth() / maxHealth);
        float p2HealthRatio = Mathf.Clamp01(healthP2.GetCurrentHealth() / maxHealth);

        // 배경
        GUI.color = Color.gray;
        GUI.Box(p1BarRect, GUIContent.none);
        GUI.Box(p2BarRect, GUIContent.none);

        // 체력 바
        GUI.color = Color.red;
        GUI.Box(new Rect(p1BarRect.x, p1BarRect.y, p1BarRect.width * p1HealthRatio, p1BarRect.height), GUIContent.none);
        GUI.color = Color.blue;
        GUI.Box(new Rect(p2BarRect.x + (1 - p2HealthRatio) * p2BarRect.width, p2BarRect.y, p2BarRect.width * p2HealthRatio, p2BarRect.height), GUIContent.none);

        // VS 텍스트
        GUI.color = Color.white;
        GUI.Label(vsRect, "VS", vsStyle);

        // 목숨 표시 (정사각형)
        DrawLives(screenWidth, 50);

        // 승리 텍스트
        if (GameManager.gameOver)
        {
            GUIStyle winStyle = new GUIStyle(GUI.skin.label);
            winStyle.fontSize = 40;
            winStyle.alignment = TextAnchor.MiddleCenter;
            winStyle.normal.textColor = Color.white;

            GUI.Label(new Rect(screenWidth / 2f - 150, screenHeight / 2f - 30, 300, 60), GameManager.winnerText, winStyle);
        }

        // 크로스헤어
        DrawCrosshair();
    }

    void DrawLives(float screenWidth, float top)
    {
        float boxSize = 15f;
        float spacing = 5f;

        // Player 1: 왼쪽 → 중앙 가까운 쪽부터 삭제
        for (int i = 0; i < 3 - GameManager.player1Deaths; i++)
        {
            float x = 20 + (2 - i) * (boxSize + spacing);
            GUI.color = Color.green;
            GUI.Box(new Rect(x, top, boxSize, boxSize), GUIContent.none);
        }

        // Player 2: 오른쪽 → 중앙 가까운 쪽부터 삭제
        for (int i = 0; i < 3 - GameManager.player2Deaths; i++)
        {
            float x = screenWidth - (2 - i) * (boxSize + spacing) - boxSize - 20;
            GUI.color = Color.green;
            GUI.Box(new Rect(x, top, boxSize, boxSize), GUIContent.none);
        }
    }

    void DrawCrosshair()
    {
        float centerX = Screen.width / 2f;
        float centerY = Screen.height / 2f;
        float length = 10f;
        float thickness = 2f;
        float outline = 1f;

        // 아웃라인 (검정색)
        GUI.color = Color.black;

        // 수직 아웃라인
        GUI.DrawTexture(new Rect(centerX - thickness / 2 - outline, centerY - length - outline, thickness + outline * 2, length * 2 + outline * 2), Texture2D.whiteTexture);
        // 수평 아웃라인
        GUI.DrawTexture(new Rect(centerX - length - outline, centerY - thickness / 2 - outline, length * 2 + outline * 2, thickness + outline * 2), Texture2D.whiteTexture);

        // 본체 (초록색)
        GUI.color = Color.green;

        // 수직선
        GUI.DrawTexture(new Rect(centerX - thickness / 2, centerY - length, thickness, length * 2), Texture2D.whiteTexture);
        // 수평선
        GUI.DrawTexture(new Rect(centerX - length, centerY - thickness / 2, length * 2, thickness), Texture2D.whiteTexture);
    }
}