using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerTracker : MonoBehaviour
{
    public static PlayerTracker instance;
    [SerializeField]
    private GameObject playerPrefab, specialItemPrefab;
    [SerializeField]
    [Tooltip("Base spawn height for the player, will be added on top of world's base height")]
    private float spawnHeight;
    public int seed;
    #region Callbacks
    private void Awake()
    {
        instance = this;
        Debug.Log("Tracker Awake");
        if (seed == 0)
        {
            seed = Random.Range(-999999, 999999);
        }
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
        Debug.Log("Tracker Started");
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    #endregion
    #region Procedures
    public void SpawnPlayer(GameObject spawnPoint)
    {
        spawnHeight += GameManager.instance.worldBaseHeight;
        //Spawns a player prefab
        // Sets up spawn location around the "World Center" object
        var randomPos = Utilities.SpawnSphereOnEdgeRandomly3D(spawnPoint, GameManager.instance.playerSpawnRadius);
        randomPos.y = spawnHeight;
        var direction = spawnPoint.transform.position;
        direction.y = spawnHeight;
        playerPrefab = Instantiate(playerPrefab, randomPos, Quaternion.LookRotation(direction - randomPos));
        PlayerController.instance.SetUpPlayer(playerPrefab);
        playerPrefab.SetActive(false);
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
