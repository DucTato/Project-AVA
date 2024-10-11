using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class IndexedArray<T> where T : struct
{
    // This is a container class for voxel dictionary, to be used with compute shading
    private bool initialized = false;
    [SerializeField]
    [HideInInspector] 
    public T[] array;

    [SerializeField]
    [HideInInspector]
    private Vector2Int size;
    public int Count
    {
        get
        {
            return size.x * size.y * size.x;
        }
    }
    public T[] GetData
    {
        get
        {
            return array;
        }
    }
    public IndexedArray() 
    {
        // Constructor
        Create(WorldManager.WorldSettings.containerSize, WorldManager.WorldSettings.maxHeight);
    }
    public void Create(int sizeX, int sizeY)
    {
        size = new Vector2Int(sizeX + 3, sizeY + 1);
        array = new T[Count];
        initialized = true;
    }
    public void Clear()
    {
        if (!initialized)
        {
            return;
        }
        else
        {
            for (int x = 0; x < size.x; x++)
                for (int y = 0; y < size.y; y++) 
                    for (int z = 0; z < size.x; z++)
                        array[x +(y* size.x) + (z * size.x * size.y)] = default(T);           
        }
    }
    public int IndexFromCoord(Vector3 index)
    {
        return Mathf.RoundToInt(index.x) + (Mathf.RoundToInt(index.y) * size.x) + (Mathf.RoundToInt(index.z) * size.x * size.y);
    }
    public T this[Vector3 coord]
    {
        get
        {
            if (coord.x < 0 || coord.x > size.x ||
                coord.y < 0 || coord.y > size.y ||
                coord.z < 0 || coord.z > size.x)
            {
                Debug.Log("!Coordinates out of bounds: " + coord);
                return default(T);
            }
            return array[IndexFromCoord(coord)];
        }
        set
        {
            if (coord.x < 0 || coord.x >= size.x ||
                coord.y < 0 || coord.y >= size.y ||
                coord.z < 0 || coord.z >= size.x)
            {
                Debug.Log("!Coordinates out of bounds: " + coord);
                return;
            }
            array[IndexFromCoord(coord)] = value;
        }
    }
}
