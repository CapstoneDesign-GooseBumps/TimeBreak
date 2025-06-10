using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public Transform[] player1SpawnPoints = new Transform[3];
    public Transform[] player2SpawnPoints = new Transform[3];

    public int deathsToLose = 3;
    public float respawnDelay = 2f;

    public static int player1Deaths = 0;
    public static int player2Deaths = 0;

    private Health healthP1;
    private Health healthP2;

    public static string winnerText = "";
    public static bool gameOver = false;

    void Start()
    {
        var p1 = GameObject.FindGameObjectWithTag("Player1");
        var p2 = GameObject.FindGameObjectWithTag("Player2");

        healthP1 = p1.GetComponent<Health>();
        healthP2 = p2.GetComponent<Health>();

        healthP1.OnDie.AddListener(() => HandleDeath(1, p1));
        healthP2.OnDie.AddListener(() => HandleDeath(2, p2));
    }

    private void HandleDeath(int playerNumber, GameObject player)
    {
        if (playerNumber == 1)
        {
            player1Deaths++;
            if (player1Deaths >= deathsToLose)
            {
                EndGame("Player 2 Wins!");
                return;
            }

            StartCoroutine(RespawnRoutine(player, player1SpawnPoints));
        }
        else
        {
            player2Deaths++;
            if (player2Deaths >= deathsToLose)
            {
                EndGame("Player 1 Wins!");
                return;
            }

            StartCoroutine(RespawnRoutine(player, player2SpawnPoints));
        }
    }

    private IEnumerator RespawnRoutine(GameObject player, Transform[] spawns)
    {
        yield return new WaitForSeconds(respawnDelay);

        int idx = Random.Range(0, spawns.Length);
        player.transform.position = spawns[idx].position;

        player.SetActive(true);

        // 체력 초기화
        var health = player.GetComponent<Health>();
        health.ResetHealth();

        // 무기 초기화
        var grenade = player.GetComponentInChildren<GrenadeLauncher>();
        if (grenade != null) grenade.ResetAmmo();

        var rocket = player.GetComponentInChildren<RocketLauncher>();
        if (rocket != null) rocket.ResetAmmo();
    }

    private void EndGame(string winner)
    {
        winnerText = winner;
        gameOver = true;
        StartCoroutine(RestartSceneAfterDelay(5f));
    }

    private IEnumerator RestartSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        player1Deaths = 0;
        player2Deaths = 0;
        gameOver = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}