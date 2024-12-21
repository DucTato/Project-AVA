using Cinemachine;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class WaveFunctionCollapse : MonoBehaviour
{
    [Foldout("World Gen")]
    public GameObject Terrain;
    [SerializeField, Foldout("World Gen")]
    private GameObject Water;
    [SerializeField, Foldout("World Gen")]
    private int dimensions;
    [SerializeField, Foldout("World Gen")]
    private Tile[] tileObjects;
    [SerializeField, Foldout("World Gen")]
    private List<Cell> gridComponents;
    [SerializeField, Foldout("World Gen")]
    private Cell cellObject;
    [SerializeField, Foldout("World Gen")]
    private Tile backupTile;
    private int iteration;
    [SerializeField, Foldout("UI Handler")]
    private CinemachineVirtualCamera virtualCam;

    private bool RaiseWater;
    #region CallBacks
    private void Awake()
    {
        gridComponents = new List<Cell>();
        //InitializeGrid();
        Terrain = new GameObject("Terrain");
        StartCoroutine(WaitAndDo(8f));
    }
    private void Start()
    {
        InitializeGrid();
    }
    private void Update()
    {
        if (RaiseWater)
        {
            Water.transform.position = Vector3.MoveTowards(Water.transform.position, new Vector3(3200f, 40f, 3200f), Time.deltaTime * 9.5f);   //40f is water height
            if (Water.transform.position.y >= 20f) Destroy(gameObject);
        }
    }
    #endregion
    #region World Generation
    private void InitializeGrid()
    {
        // Populate grid with cells that contain tiles
        for (int y = 0; y < dimensions; y++)
        {
            for (int x = 0; x < dimensions; x++)
            {
                Cell newCell = Instantiate(cellObject, new Vector3(x * 320f, -50, y * 320f), Quaternion.identity);
                newCell.CreateCell(false, tileObjects);
                if (x == dimensions / 2 && x == y)
                {
                    newCell.GetComponentInChildren<CinemachineFreeLook>().Priority = 2;
                    newCell.gameObject.name = "WorldCenter";
                    if (GameManager.instance != null)   GameManager.instance.SetWorldCenter(newCell.gameObject);
                }
                else
                {
                    newCell.transform.parent = transform ;
                }
                gridComponents.Add(newCell);
            }
        }
        StartCoroutine(CheckEntropy());
    }
    private IEnumerator CheckEntropy()
    {
        // Reorganize the list so that the 1st item is the one w/ lowest entropy
        List<Cell> tempGrid = new List<Cell>(gridComponents);
        tempGrid.RemoveAll(c => c.collapsed);
        tempGrid.Sort((a, b) => a.tileOptions.Length - b.tileOptions.Length);
        tempGrid.RemoveAll(a => a.tileOptions.Length != tempGrid[0].tileOptions.Length);
        yield return null;
        CollapseCell(tempGrid);
    }
    private void CollapseCell(List<Cell> tempGrid)
    {
        // Build the tile
        int randIndex = UnityEngine.Random.Range(0, tempGrid.Count);
        Cell cell2Collapse = tempGrid[randIndex];
        cell2Collapse.collapsed = true;
        try
        {
            Tile selectedTile = cell2Collapse.tileOptions[UnityEngine.Random.Range(0, cell2Collapse.tileOptions.Length)];
            cell2Collapse.tileOptions = new Tile[] { selectedTile };
        }
        catch
        {
            Debug.Log("Using back up Tile");
            Tile selectedTile = backupTile;
            cell2Collapse.tileOptions = new Tile[] { selectedTile };
        }

        Tile foundTile = cell2Collapse.tileOptions[0];
        Instantiate(foundTile, cell2Collapse.transform.position, foundTile.transform.rotation);
        UpdateGeneration();
    }
    private void UpdateGeneration()
    {
        List<Cell> newGenCell = new List<Cell>(gridComponents); // Make a copy to edit the current cells
        for (int y = 0; y < dimensions; y++)
        {
            for (int x = 0; x < dimensions; x++)
            {
                int index = x + y * dimensions;
                if (gridComponents[index].collapsed) newGenCell[index] = gridComponents[index];
                else
                {
                    List<Tile> options = new List<Tile>();
                    foreach (Tile t in tileObjects)
                    {
                        options.Add(t);
                    }
                    // Go up to check the down neighbours
                    if (y > 0)
                    {
                        Cell up = gridComponents[x + (y - 1) * dimensions];
                        List<Tile> validOptions = new List<Tile>();
                        foreach (Tile possibleOptions in up.tileOptions)
                        {
                            var validOption = Array.FindIndex(tileObjects, obj => obj == possibleOptions);
                            var valid = tileObjects[validOption].downNeighbours;
                            validOptions = validOptions.Concat(valid).ToList();
                        }
                        CheckValidity(options, validOptions);
                    }
                    // Go left to check the right neighbours
                    if (x < dimensions - 1)
                    {
                        Cell left = gridComponents[x + 1 + y * dimensions];
                        List<Tile> validOptions = new List<Tile>();
                        foreach (Tile possibleOption in left.tileOptions)
                        {
                            var validOption = Array.FindIndex(tileObjects, obj => obj == possibleOption);
                            var valid = tileObjects[validOption].rightNeighbours;
                            validOptions = validOptions.Concat(valid).ToList();
                        }
                        CheckValidity(options, validOptions);
                    }
                    // Go right to check the left neighbours
                    if (x > 0)
                    {
                        Cell right = gridComponents[x - 1 + y * dimensions];
                        List<Tile> validOptions = new List<Tile>();
                        foreach (Tile possibleOption in right.tileOptions)
                        {
                            var validOption = Array.FindIndex(tileObjects, obj => obj == possibleOption);
                            var valid = tileObjects[validOption].leftNeighbours;
                            validOptions = validOptions.Concat(valid).ToList();
                        }
                        CheckValidity(options, validOptions);
                    }
                    // Go down to check the up neighbours
                    if (y < dimensions - 1)
                    {
                        Cell down = gridComponents[x + (y + 1) * dimensions];
                        List<Tile> validOptions = new List<Tile>();
                        foreach (Tile possibleOptions in down.tileOptions)
                        {
                            var validOption = Array.FindIndex(tileObjects, obj => obj == possibleOptions);
                            var valid = tileObjects[validOption].upNeighbours;
                            validOptions = validOptions.Concat(valid).ToList();
                        }
                        CheckValidity(options, validOptions);
                    }
                    Tile[] newTileList = new Tile[options.Count];
                    for (int i = 0; i < options.Count; i++)
                    {
                        newTileList[i] = options[i];
                    }
                    newGenCell[index].ReCreateCell(newTileList);
                }

            }
        }
        gridComponents = newGenCell;
        if (++iteration < dimensions * dimensions) StartCoroutine(CheckEntropy());
        else 
        { 
            
            MergeTile();
        }
    }
    private void CheckValidity(List<Tile> optionList, List<Tile> validOption)
    {
        for (int x = optionList.Count - 1; x >= 0; x--)
        {
            var element = optionList[x];
            if (!validOption.Contains(element))
            {
                optionList.RemoveAt(x);
            }
        }
    }
    private void MergeTile()
    {
        foreach (GameObject builtTile in GameObject.FindGameObjectsWithTag("Terrain"))
        {
            builtTile.transform.parent = Terrain.transform;
        }
        Debug.Log("Done building!");
        RaiseWater = true;
        // Done building world, start placing down enemies and updating the ui status
        if (GameManager.instance == null) return;
        GameManager.instance.StartPlacingEnemies();
        GameManager.instance.UpdateLoadingScreen();
    }
    #endregion
    #region UI/UX Handler
    private IEnumerator WaitAndDo(float time)
    {
        yield return new WaitForSeconds(time);
        virtualCam.Priority = 0;
        GameManager.instance.SetFogValues(5000f, 10000f);
    }
    #endregion
}