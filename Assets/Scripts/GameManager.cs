using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [SerializeField]
    private UIManager hudController;
    [SerializeField]
    private int currentPoint, maxPoint;

    public int CurrentPoint
    {
        get { return currentPoint; }
        set 
        { 
            currentPoint = value;
            SetCurrentProgress();
        }
    }
    #region CallBacks
    private void Awake()
    {
        instance = this; 
    }
    // Start is called before the first frame update
    void Start()
    {
        hudController = GameObject.FindGameObjectWithTag("UICanvas").GetComponent<UIManager>();
        CurrentPoint = 0;
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    #endregion
    private void SetCurrentProgress()
    {
        hudController.SetProgressBar(CurrentPoint, maxPoint);
    }
    public void AddPoint(int point)
    {
        CurrentPoint += point;
    }
}
