using UnityEngine;

public class GameStateController : MonoBehaviour
{

    public enum GameState
    {
        Started,
        Title,
        GameOver
    }

    [SerializeField] private GameState _gameState = GameState.Title;

    public GameObject obstacleController;
    public GameObject enemyController;

    public GameObject titleUI;
    public GameObject inGameUI;
    public GameObject gameOverUI;

    public GameObject player;

    public GameState GetState()
    {
        return _gameState;
    }

    public void SetState(GameState value)
    {
        _gameState = value;
        if (value == GameState.Title)
        {
            titleUI.SetActive(true);
            inGameUI.SetActive(false);
            gameOverUI.SetActive(false);

        } else if (value == GameState.Started)
        {
            titleUI.SetActive(false);
            inGameUI.SetActive(true);
            gameOverUI.SetActive(false);

            enemyController.SetActive(true);
            obstacleController.SetActive(true);
            ResetGame();
        } else if (value == GameState.GameOver)
        {
            titleUI.SetActive(false);
            inGameUI.SetActive(false);
            gameOverUI.SetActive(true);

            enemyController.SetActive(false);
            obstacleController.SetActive(false);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if ((GetState() == GameState.Title || GetState()  == GameState.GameOver) && Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SetState(GameState.Started);
            
        }
    }

    void ResetGame()
    {
        foreach (GameObject enemy in  GameObject.FindGameObjectsWithTag("Enemy"))
        {
            Destroy(enemy);
        }
        foreach (GameObject obstacle in GameObject.FindGameObjectsWithTag("Obstacle"))
        {
            if (obstacle.name == "Gate")
            {
                Destroy(obstacle);
            }
        }
        player.SetActive(true);
        player.transform.position = new Vector3(0, 0, player.transform.position.z);
        enemyController.GetComponent<SpawnEnemies>().StartEnemySpawnRoutine();
        obstacleController.GetComponent<SpawnObstacles>().ResetConfig();
    }
}
