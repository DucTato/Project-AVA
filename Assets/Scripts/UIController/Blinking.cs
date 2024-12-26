
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Blinking : MonoBehaviour
{
    [SerializeField]
    private float animationSpdMult;
    [SerializeField]
    private List<Graphic> textGraphics;
    private Color tempColor;
    private bool StopBlinking;
    // Start is called before the first frame update
    void Awake()
    {
        //GetComponent<Animator>().speed = animationSpdMult;
        tempColor = textGraphics[0].color;
        StopBlinking = false;
    }
    private void Update()
    {
        if (StopBlinking) return;
        foreach (Graphic g in textGraphics)
        {
            tempColor.a = (Mathf.Sin(Time.time * animationSpdMult) + 1.0f) / 2.0f;
            g.color = tempColor;
        }
    }
    public void ToggleBlink()
    {
        StopBlinking = !StopBlinking;
    }
    public void StopBlink()
    {
        StopBlinking = true;
        foreach (Graphic g in textGraphics)
        {
            tempColor.a = 1f;
            g.color = tempColor;
        }
    }
}
