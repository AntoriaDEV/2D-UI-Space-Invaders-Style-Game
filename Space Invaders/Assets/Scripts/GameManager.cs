using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

//Handles background calculations and general gameplay
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
        //intializing the serializefield values and classes
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
        //calls functions which call more specific functions
        playerLogic.HandleInput();
        enemySpawner.HandleSpawning();
    }

    public void AddScore(int amount)
    {
        //every enemy hit add score
        score += amount;
        UpdateScore();
    }

    void UpdateScore()
    {
        //display updated score text on each call
        scoreText.text = "Score: " + score;
    }

    public void GameOver()
    {
        //pulls up game over menu, freezes the game in the background
        gameOverText.SetActive(true);
        restartButton.gameObject.SetActive(true);
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        //reloads the base game state and reverts time to normal rate
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

//handles player specific interactions and functions
class PlayerLogic
{
    GameObject player;
    GameObject projectilePrefab;
    Transform firePoint;

    //base movespeed
    float moveSpeed = 8f;

    public PlayerLogic(GameObject playerObj, GameObject projectile, Transform fire)
    {
        //setting serializefield items to variables, accessable by other classes/functions
        player = playerObj;
        projectilePrefab = projectile;
        firePoint = fire;
    }
    //called on game manager update for player movements and fire
    public void HandleInput()
    {
        Move();
        Shoot();
    }

    void Move()
    {
        float move = 0f; //Base movement speed is 0

        if (UnityEngine.InputSystem.Keyboard.current.aKey.isPressed)//basic movement based on A or D
            move = -1f;
        if (UnityEngine.InputSystem.Keyboard.current.dKey.isPressed)// ^
            move = 1f;
        if ((UnityEngine.InputSystem.Keyboard.current.leftShiftKey.isPressed) && (UnityEngine.InputSystem.Keyboard.current.aKey.isPressed)) //increased movementspeed if shift is also pressed with A/D
            move = -2f;
        if ((UnityEngine.InputSystem.Keyboard.current.leftShiftKey.isPressed) && (UnityEngine.InputSystem.Keyboard.current.dKey.isPressed)) // ^
            move = 2f;
        player.transform.Translate(Vector2.right * move * moveSpeed * Time.deltaTime); //handles actual movement of player object based on speed variable
    }

    void Shoot() // calls the projectile class, and initializes the projectile with all information needed from unity
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
    //variable initialization
    float speed = 10f;
    float lifeTime = 3f;

    void Start()
    {
        //removes the projectile after lifeTime (in seconds)
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        //Moves the projectile upwards based on speed
        transform.Translate(Vector2.up * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other) // if projectile collides with an enemy objec that has a 2D collider on it, then it gets destroyed. also references the enemy class for all info needed
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.DestroyEnemy();
            Destroy(gameObject); // only destroys the projectile
        }
    }
}

class Enemy : MonoBehaviour
{
    //variable initialization
    float speed = 3f;
    int scoreValue = 10;

    void Update()
    {
        transform.Translate(Vector2.down * speed * Time.deltaTime); // movement handler

        if (transform.position.y < -6f) // lower bound for enemy to reach in order for game to end
        {
            GameManager.Instance.GameOver();
            Destroy(gameObject);
        }
    }

    public void DestroyEnemy() //adds score if an enemy is destroyed and destroys the enemy
    {
        GameManager.Instance.AddScore(scoreValue);
        Destroy(gameObject);
    }
}

class EnemySpawner
{

    //variable initialization and object initialization
    GameObject enemyPrefab;

    float spawnInterval;
    float minX;
    float maxX;
    float spawnY;

    float timer;

    public EnemySpawner(GameObject prefab, float interval, float min, float max, float y) //initialize specific variables
    {
        enemyPrefab = prefab;
        spawnInterval = interval;
        minX = min;
        maxX = max;
        spawnY = y;
    }

    public void HandleSpawning() // has a timer that increments, if the time reaches the variable then calls spawn enemy and resets timer
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnEnemy();
            timer = 0f;
        }
    }

    void SpawnEnemy() // handles the spawning of each enemy based on the serializefield variable bounds
    {
        float randomX = Random.Range(minX, maxX);
        Vector3 pos = new Vector3(randomX, spawnY, 0);

        GameObject enemy = GameObject.Instantiate(enemyPrefab, pos, Quaternion.identity);
        enemy.AddComponent<Enemy>();
    }
}