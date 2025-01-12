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
    private GameObject[] enemyTypes;
    [SerializeField, Foldout("Enemies")]
    [Tooltip("Must correspond to the EnemyTypes table above!\nMin: 0, Max: 1.0")]
    private float[] enemyChances;
    [SerializeField, Foldout("Enemies")]
    private float spawnRadius, spawnInterval;

    [SerializeField, Foldout("Player")]
    private int currentPoint;
    [Foldout("Player")]
    public float playerSpawnRadius;
    [SerializeField, Foldout("Player")]
    private GameObject worldCenter;
    [Foldout("UI/UX")]
    public UIManager hudController;
    [SerializeField, Foldout("UI/UX")]

    private GameObject waitTxt, loadingDoneTxt, gameCanvas, freeLookTxt;
    private Distribution[] enemyDistributions;
    //private List<GameObject> enemyPool;
    

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
        }
    }
    
    #region CallBacks
    private void Awake()
    {
        instance = this;
        enemyDistributions = new Distribution[enemyTypes.Length];
        Debug.Log("Game manager Awake");
        for (int i = 0; i < enemyTypes.Length; i++)
        {
            // Clamps the chance values: 0.0 (0%) -> 1.0 (100%)
            enemyChances[i] = Mathf.Clamp(enemyChances[i], 0, 1);
            // Keeps a table of distribution for performance
            enemyDistributions[i] = enemyTypes[i].GetComponent<Distribution>();
            //Debug.Log(enemyDistributions[i]);
        }
        // Tries updating the text elements
        if (waitTxt == null || loadingDoneTxt == null) return;
        waitTxt.SetActive(true);
        loadingDoneTxt.SetActive(false);
        freeLookTxt.SetActive(false);
    }
    // Start is called before the first frame update
    void Start()
    {
        GamePhase = false;
        currentPoint = 0;
        if (PlayerTracker.instance == null || PlayerTracker.instance.seed == 0) return;
        else Random.InitState(PlayerTracker.instance.seed);
        UpdatePlayerPrefs();
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
        currentPoint += point;
    }
    public int CurrentPoint()
    {
        return currentPoint;
    }
    public void SubtractPoint(int value)
    {
        currentPoint -= value;
    }
    public void SubtractEnemies()
    {
        currentEnemies--;
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
    private void SpawnProcedure()
    {
        for (int i = 0; i < enemyTypes.Length; i++)
        {
            
            if (enemyDistributions[i].ProbabilityCheck((enemyDefeated / maxEnemies), enemyChances[i]))
            {
                
                // Passed the probability check, spawns enemy type now
                var randomPos = Random.insideUnitSphere * spawnRadius;
                randomPos += WorldCenter.transform.position;
                randomPos.y = 5f + worldBaseHeight;
                Instantiate(enemyTypes[i], randomPos, enemyDistributions[i].OverrideRotation());
                currentEnemies++;
            }
        }
    }
    
    private IEnumerator SpawnEnemiesWithDelay(float delay)
    {
        if (enemyDefeated < maxEnemies)
        {
            if (currentEnemies < maxEnemiesAtOnce)
            {
                //Start spawn procedure
                SpawnProcedure();
            }
        }
        else
        {
            // Maximum enemies reached, stops the spawning cycle
            StopCoroutine(SpawnEnemiesWithDelay(delay));
        }
        yield return new WaitForSeconds(delay);
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
