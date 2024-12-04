using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaveFunctionCollapse : MonoBehaviour
{
    [SerializeField]
    private int dimensions;
    [SerializeField]
    private Tile[] tileObjects;
    [SerializeField]
    private List<Cell> gridComponents;
    [SerializeField]
    private Cell cellObject;
    [SerializeField]
    private Tile backupTile;

    private int iteration;

    #region CallBacks
    private void Awake()
    {
        gridComponents = new List<Cell>();
        InitializeGrid();
    }
    #endregion
    private void InitializeGrid()
    {
        // Populate grid with all the possible tiles
        for (int y = 0; y < dimensions; y++)
        {
            for (int x = 0; x < dimensions; x++)
            {
                Cell newCell = Instantiate(cellObject, new Vector3(x * 9.6f, 0, y * 9.6f), Quaternion.identity);
                newCell.CreateCell(false, tileObjects);
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
        else Debug.Log("Done building!");
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

}