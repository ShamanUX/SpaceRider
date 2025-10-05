using TMPro;
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
    public GameObject enemyControllerObject;

    public GameObject titleUI;
    public GameObject inGameUI;
    public GameObject gameOverUI;

    public GameObject player;
    public int levelNumber = 1;

    public int GetLevelNumber()
    {
        return levelNumber;
    }

    public void SetLevelNumber(int levelNumber)
    {
        this.levelNumber = levelNumber;
    }
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

            enemyControllerObject.SetActive(true);
            ResetGame();
        } else if (value == GameState.GameOver)
        {
            titleUI.SetActive(false);
            inGameUI.SetActive(false);
            gameOverUI.SetActive(true);

            GameObject.Find("LevelReached").GetComponent<TextMeshPro>().text = $"You reached Level {levelNumber}";
            enemyControllerObject.SetActive(false);
            obstacleController.GetComponent<SpawnObstacles>().StopAllCoroutines();
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if ((GetState() == GameState.Title || GetState()  == GameState.GameOver) && Input.GetKeyDown(KeyCode.Return))
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
        SetLevelNumber(1);
        player.SetActive(true);
        player.transform.position = new Vector3(0, 0, player.transform.position.z);
        SpawnEnemies enemyController = enemyControllerObject.GetComponent<SpawnEnemies>();
        enemyController.SetMaxForceMultiplier(1);
        enemyController.SetMaxSpeedMultiplier(1);
        enemyController.StartEnemySpawnRoutine();

        FindFirstObjectByType<AmmoGauge>().RefillAmmoFully(); 

        obstacleController.GetComponent<SpawnObstacles>().ResetConfig();
    }
}
