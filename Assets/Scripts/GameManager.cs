using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField, Foldout("Enemies")]
    private int currentEnemies, maxEnemies;
    [SerializeField, Foldout("Enemies")]
    private GameObject[] Enemies;
    [SerializeField, Foldout("Enemies")]
    private float spawnRadius, spawnInterval;
    [SerializeField, Foldout("Enemies")]
    [Tooltip("By default (offset=0), enemies will spawn at 5f")]
    private float spawnHeightOffset;
    [SerializeField, Foldout("Player")]
    private int currentPoint, maxPoint;
    [Foldout("Player")]
    public float playerSpawnRadius;
    [SerializeField, Foldout("Player")]
    private GameObject worldCenter;
    [Foldout("UI/UX")]
    public UIManager hudController;
    [SerializeField, Foldout("UI/UX")]

    private GameObject waitTxt, loadingDoneTxt, gameCanvas;
   
    private List<GameObject> enemyPool;
    

    private bool BossPhase;
    private const int maxEnemiesAtOnce = 32;
    
    public GameObject WorldCenter
    {
        get 
        {
            return worldCenter;
        }
        private set 
        {
            if (value == null) return;
            worldCenter = value;
            PlayerTracker.instance.SpawnPlayer(worldCenter);
            InitializeSpawns();
        }
    }
    public int CurrentPoint
    {
        get { return currentPoint; }
        set 
        { 
            currentPoint = value;
            SetCurrentProgress();
            if (currentPoint == maxPoint) StartWinProcedure();
        }
    }
    public int CurrentEnemies
    {
        get { return currentEnemies; }
        private set 
        { 
            currentEnemies = value; 
            if (currentEnemies == 0 && !BossPhase)
            {
                // Switch to BOSS phase
                BossPhase = true;
            }
        }
    }
    #region CallBacks
    private void Awake()
    {
        instance = this;
        enemyPool = new List<GameObject>(maxEnemies);
        Debug.Log(Random.seed);
        // Tries adding a default world center
        
        if (waitTxt == null || loadingDoneTxt == null) return;
        waitTxt.SetActive(true);
        loadingDoneTxt.SetActive(false);
    }
    // Start is called before the first frame update
    void Start()
    {
        
        CurrentPoint = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
        //Debug.Log(PlayerController.instance.playerInput.currentActionMap);
    }
    #endregion

    #region Procedures
    private void SetCurrentProgress()
    {
        hudController.SetProgressBar(CurrentPoint, maxPoint);
    }
    public void AddPoint(int point)
    {
        CurrentPoint += point;
    }
    public void StartWinProcedure()
    {
        hudController.SetWinScreen(true); 
    }
    public void StartDeathProcedure()
    {
        hudController.SetDeathScreen(true);
    }
    public void SetWorldCenter(GameObject center)
    {
        WorldCenter = center;
    }
    
    public void SubtractCurrentEnemies()
    {
        currentEnemies--;
    }
    public void StartPlacingEnemies()
    {
        StartCoroutine(SpawnEnemiesWithDelay(spawnInterval));
    }
    public void UpdateLoadingScreen()
    {
        waitTxt.SetActive(false);
        loadingDoneTxt.SetActive(true);
    }
    private void InitializeSpawns()
    {
        for (int i = 0; i < maxEnemies; i++)
        {
            // Sets up spawn location around the "World Center" object
            var randomPos = Random.insideUnitSphere * spawnRadius;
            randomPos += WorldCenter.transform.position;
            randomPos.y = 5f + spawnHeightOffset;
            // Spawns objects            
            enemyPool.Add(Instantiate(Enemies[Random.Range(0, Enemies.Length - 1)], randomPos, Quaternion.Euler(-35f, 0f, 0f)));
        }
    }
    private IEnumerator SpawnEnemiesWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        for(int i = 0; i < enemyPool.Count; i++)
        {
            if (currentEnemies >= maxEnemiesAtOnce) break;
            if (!enemyPool[i].activeInHierarchy)
            {
                enemyPool[i].SetActive(true);
                currentEnemies++;
                enemyPool.Remove(enemyPool[i]);
            }
        }
        
        StartCoroutine(SpawnEnemiesWithDelay(delay));
    }
    #endregion
    #region Input Handler
    public void OnStartButton()
    {
        // Checks if this is the loading screen phase or not
        if (loadingDoneTxt.activeInHierarchy)
        {
            PlayerTracker.instance.PlaceDownPlayer(false);  // Places down the player, no FCS override
            PlayerController.instance.playerInput.actions.FindActionMap("UI").Disable();
            PlayerController.instance.SetCurrentInputMap("Gameplay");
            Debug.Log("Switched to gameplay map");
            hudController.ToggleCanvas(true);
            loadingDoneTxt.SetActive(false);
            gameCanvas.SetActive(true);
            
        }
        else
        {
            // The START button can also be used to turn off the pause menu
        }
    }
    #endregion
}
public class EventArguments : EventArgs
{
    private GameObject point;
    public EventArguments(GameObject point)
    {
        this.point = point;
    }
    public GameObject GetPoint()
    {
        return point;
    }
    private bool _fcsOverride;
    public EventArguments(bool fcsOverride)
    {
        this._fcsOverride = fcsOverride;
    }
    public bool OverrideFCSRange()
    {
        return _fcsOverride;
    }
}
