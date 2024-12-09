using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerTracker : MonoBehaviour
{
    public static PlayerTracker instance;
    [SerializeField]
    private GameObject playerPrefab, specialItemPrefab;
    [SerializeField]
    [Tooltip("By default (offset=0), the player will spawn at 60f")]
    private float heightOffsetOverride;
    #region Callbacks
    private void Awake()
    {
        instance = this;

    }
    private void OnEnable()
    {
        gameObject.transform.parent = null;
        DontDestroyOnLoad(gameObject);
        
    }
    private void OnDisable()
    {
        
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
    #region Procedures
    public void SpawnPlayer(GameObject spawnPoint)
    {
        //Spawns a player prefab
        // Sets up spawn location around the "World Center" object
        var randomPos = Utilities.SpawnSphereOnEdgeRandomly3D(spawnPoint, GameManager.instance.playerSpawnRadius);
        randomPos.y = 60f + heightOffsetOverride;
        playerPrefab = Instantiate(playerPrefab, randomPos, Quaternion.LookRotation(spawnPoint.transform.position + new Vector3(0f, 60f + heightOffsetOverride, 0f) - randomPos));
        playerPrefab.SetActive(false);
        PlayerController.instance.SetUpPlayer(playerPrefab);
    }

    public void PlaceDownPlayer(bool OverrideFCS)
    {
        playerPrefab.SetActive(true);
        if (OverrideFCS)
        {
            playerPrefab.GetComponent<FCS>().lockRange = Mathf.Infinity;
        }
    }
    public void SetAircraft(GameObject aircraft, GameObject item)
    {
        playerPrefab = aircraft;
        specialItemPrefab = item;
    }
    #endregion
}
