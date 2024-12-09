using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ObjectInfo : MonoBehaviour
{
    [SerializeField]
    private Sprite _thumbnail;
    [SerializeField]
    private string _description;

    public string Description()
    {
        return _description;
    }
    public Sprite Thumbnail()
    {
        return _thumbnail;
    }
}
