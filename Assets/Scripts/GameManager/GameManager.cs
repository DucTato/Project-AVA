using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using Random = UnityEngine.Random;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [Tooltip("Base height of world, spawnables won't spawn below this value")]
    public float worldBaseHeight;

    [SerializeField, Foldout("Enemies")]
    private int currentEnemies, maxEnemies, enemyDefeated;
    [SerializeField, Foldout("Enemies")]
    private int maxEnemiesAtOnce = 32;
    [SerializeField, Foldout("Enemies")]
    private GameObject[] Enemies;
    [SerializeField, Foldout("Enemies")]
    private float spawnRadius, spawnInterval;

    [SerializeField, Foldout("Player")]
    private int currentPoint, maxPoint;
    [Foldout("Player")]
    public float playerSpawnRadius;
    [SerializeField, Foldout("Player")]
    private GameObject worldCenter;
    [Foldout("UI/UX")]
    public UIManager hudController;
    [SerializeField, Foldout("UI/UX")]

    private GameObject waitTxt, loadingDoneTxt, gameCanvas, freeLookTxt;

    private List<GameObject> enemyPool;
    

    private bool BossPhase;
   
    public bool GamePhase { get; set; }
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
        freeLookTxt.SetActive(false);
    }
    // Start is called before the first frame update
    void Start()
    {
        GamePhase = false;
        CurrentPoint = 0;
        if (PlayerTracker.instance == null || PlayerTracker.instance.seed == 0) return;
        else Random.InitState(PlayerTracker.instance.seed);
        
        UpdatePlayerPrefs();
    }

    // Update is called once per frame
    void Update()
    {

        //Debug.Log(PlayerController.instance.playerInput.currentActionMap);
    }
    #endregion

    #region Procedures
    public void SetFogValues(float startDistance, float endDistance)
    {
        // Duration of all fog change operations is 5 seconds :>
        DOTween.To(() => RenderSettings.fogStartDistance, x => RenderSettings.fogStartDistance = x, startDistance, 5f);
        DOTween.To(() => RenderSettings.fogEndDistance, x => RenderSettings.fogEndDistance = x, endDistance, 5f);

    }
    private void SetCurrentProgress()
    {
        hudController.SetProgressBar(enemyDefeated, maxEnemies);
    }
    public void AddPoint(int point)
    {
        CurrentPoint += point;
    }
    public void AddEnemiesDefeated()
    {
        enemyDefeated++;
        if (enemyDefeated == maxEnemies && !BossPhase)
        {
            // Switch to BOSS phase
            BossPhase = true;
        }
        SetCurrentProgress();
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
    public void TurnOnFreeLook()
    {
        freeLookTxt.SetActive(true);
    }
    private void InitializeSpawns()
    {
        for (int i = 0; i < maxEnemies; i++)
        {
            // Sets up spawn location around the "World Center" object
            var randomPos = Random.insideUnitSphere * spawnRadius;
            randomPos += WorldCenter.transform.position;
            randomPos.y = 5f + worldBaseHeight;
            // Spawns objects            
            enemyPool.Add(Instantiate(Enemies[Random.Range(0, Enemies.Length - 1)], randomPos, Quaternion.Euler(-35f, 0f, 0f)));
        }
    }
    private IEnumerator SpawnEnemiesWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        for (int i = 0; i < enemyPool.Count; i++)
        {
            if (currentEnemies >= maxEnemiesAtOnce)
            {
                //Debug.Log("Break");
                break; 
            }

            if (!enemyPool[i].activeInHierarchy)
            {
                enemyPool[i].SetActive(true);
                currentEnemies++;
                enemyPool.Remove(enemyPool[i]);
            }
        }

        StartCoroutine(SpawnEnemiesWithDelay(delay));
    }
    /// <summary>
    /// Update all the player's current preferences upon loading a new scene
    /// </summary>
    private void UpdatePlayerPrefs()
    {
        // Update Graphical Settings
        GameObject.FindGameObjectWithTag("PPVolume").GetComponent<Volume>().enabled = PlayerTracker.instance.PostProcessing();
    }
    #endregion
    #region Input Handler
    public void OnFreeLook(InputAction.CallbackContext ctx)
    {
        if (ctx.phase == InputActionPhase.Performed)
        {
            PlayerController.instance.playerInput.actions.FindActionMap("Gameplay").Disable();
        }
    }
    public void OnStartButton(InputAction.CallbackContext ctx)
    {
        if (ctx.phase == InputActionPhase.Performed)
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
                freeLookTxt.SetActive(false);
                gameCanvas.SetActive(true);
                // Sets up new fog value for in-game phase
                SetFogValues(1000f, 3000f);
                GamePhase = true;
                SetCurrentProgress();
            }
            else if (GamePhase)
            {
                Debug.Log("Game manager called");
                // The START button can also be used to turn off the pause menu
                hudController.TogglePause();
            }
        }
    }
    public void OnCancelButton(InputAction.CallbackContext ctx)
    {
        if (ctx.phase == InputActionPhase.Performed)
        {
            hudController.OnEastButton();
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
}
