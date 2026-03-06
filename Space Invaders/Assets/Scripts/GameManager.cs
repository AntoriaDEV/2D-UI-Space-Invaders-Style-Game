using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    //Formatted fields
    [Header("UI")]
    [SerializeField] TMP_Text scoreText;
    [SerializeField] GameObject gameOverText;
    [SerializeField] Button restartButton;

    [Header("Player")]
    [SerializeField] GameObject player;
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] Transform firePoint;

    [Header("Enemy")]
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] float spawnInterval = 2f;
    [SerializeField] float minX = -8f;
    [SerializeField] float maxX = 8f;
    [SerializeField] float spawnY = 6f;

    int score = 0;

    PlayerLogic playerLogic;
    EnemySpawner enemySpawner;
    
    //Sets Instance before anything can use it so it doesnt throw a null reference error
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        playerLogic = new PlayerLogic(player, projectilePrefab, firePoint);
        enemySpawner = new EnemySpawner(enemyPrefab, spawnInterval, minX, maxX, spawnY);

        UpdateScore();

        //hide game over text and restart button
        gameOverText.SetActive(false);
        restartButton.gameObject.SetActive(false);

        //call restart game on button press
        Button btn = restartButton.GetComponent<Button>();
        btn.onClick.AddListener(RestartGame);
    }

    void Update()
    {
        playerLogic.HandleInput();
        enemySpawner.HandleSpawning();
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateScore();
    }

    void UpdateScore()
    {
        scoreText.text = "Score: " + score;
    }

    public void GameOver()
    {
        gameOverText.SetActive(true);
        restartButton.gameObject.SetActive(true);
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

class PlayerLogic
{
    GameObject player;
    GameObject projectilePrefab;
    Transform firePoint;

    float moveSpeed = 8f;

    public PlayerLogic(GameObject playerObj, GameObject projectile, Transform fire)
    {
        player = playerObj;
        projectilePrefab = projectile;
        firePoint = fire;
    }

    public void HandleInput()
    {
        Move();
        Shoot();
    }

    void Move()
    {
        float move = 0f;

        if (UnityEngine.InputSystem.Keyboard.current.aKey.isPressed)
            move = -1f;
        if (UnityEngine.InputSystem.Keyboard.current.dKey.isPressed)
            move = 1f;
        if ((UnityEngine.InputSystem.Keyboard.current.leftShiftKey.isPressed) && (UnityEngine.InputSystem.Keyboard.current.aKey.isPressed))
            move = -2f;
        if ((UnityEngine.InputSystem.Keyboard.current.leftShiftKey.isPressed) && (UnityEngine.InputSystem.Keyboard.current.dKey.isPressed))
            move = 2f;
        player.transform.Translate(Vector2.right * move * moveSpeed * Time.deltaTime);
    }

    void Shoot()
    {
        if (UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            GameObject bullet = GameObject.Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            bullet.AddComponent<Projectile>();
        }
    }
}

class Projectile : MonoBehaviour
{
    float speed = 10f;
    float lifeTime = 3f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(Vector2.up * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.DestroyEnemy();
            Destroy(gameObject);
        }

        Debug.Log("Hit: " + other.name);
    }
}

class Enemy : MonoBehaviour
{
    float speed = 3f;
    int scoreValue = 10;

    void Update()
    {
        transform.Translate(Vector2.down * speed * Time.deltaTime);

        if (transform.position.y < -6f)
        {
            GameManager.Instance.GameOver();
            Destroy(gameObject);
        }
    }

    public void DestroyEnemy()
    {
        GameManager.Instance.AddScore(scoreValue);
        Destroy(gameObject);
    }
}

class EnemySpawner
{
    GameObject enemyPrefab;

    float spawnInterval;
    float minX;
    float maxX;
    float spawnY;

    float timer;

    public EnemySpawner(GameObject prefab, float interval, float min, float max, float y)
    {
        enemyPrefab = prefab;
        spawnInterval = interval;
        minX = min;
        maxX = max;
        spawnY = y;
    }

    public void HandleSpawning()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnEnemy();
            timer = 0f;
        }
    }

    void SpawnEnemy()
    {
        float randomX = Random.Range(minX, maxX);
        Vector3 pos = new Vector3(randomX, spawnY, 0);

        GameObject enemy = GameObject.Instantiate(enemyPrefab, pos, Quaternion.identity);
        enemy.AddComponent<Enemy>();
    }
}