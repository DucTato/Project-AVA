using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerTracker : MonoBehaviour
{
    [SerializeField]
    private GameObject playerPrefab;
    [SerializeField]
    [Tooltip("By default (offset=0), the player will spawn at 60f")]
    private float heightOffsetOverride;
    private GameManager gameManager;
    #region Callbacks
    private void Awake()
    {
        gameManager = GameManager.instance.GetComponent<GameManager>();
        gameManager.OnGetWorldCenter += PlayerTracker_OnGetWorldCenter;
        gameManager.OnStartGame += GameManager_OnStartGame;
    }
    private void OnEnable()
    {
        
        
    }

    private void GameManager_OnStartGame(object sender, EventArguments e)
    {
        playerPrefab.SetActive(true);
        if (e.OverrideFCSRange())
        {
            playerPrefab.GetComponent<FCS>().lockRange = Mathf.Infinity;
        }
    }

    private void PlayerTracker_OnGetWorldCenter(object sender, EventArguments e)
    {
        //Spawns a player prefab
        // Sets up spawn location around the "World Center" object
        var randomPos = Random.insideUnitSphere * GameManager.instance.playerSpawnRadius;
        randomPos += e.GetPosition();
        randomPos.y = 60f + heightOffsetOverride;
        playerPrefab = Instantiate(playerPrefab, randomPos, Quaternion.LookRotation(e.GetPosition() + new Vector3(0f, 60f, 0f) - randomPos));
        playerPrefab.SetActive(false);
        PlayerController.instance.SetUpPlayer(playerPrefab);
    }

    private void OnDisable()
    {
        gameManager.OnGetWorldCenter -= PlayerTracker_OnGetWorldCenter;
        gameManager.OnStartGame -= GameManager_OnStartGame;
    }
    
    // Start is called before the first frame update
    private void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    

    #endregion
}
