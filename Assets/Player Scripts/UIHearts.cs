using UnityEngine;
using UnityEngine.UI;

public class UIHearts : MonoBehaviour
{
    public Image[] circles;       // drag your circle UI Images here
    public Color fullColor = Color.red;
    public Color emptyColor = Color.black;

    public void UpdateHearts(int current, int max)
    {
        for (int i = 0; i < circles.Length; i++)
        {
            if (i < current)
                circles[i].color = fullColor; // full health = red
            else
                circles[i].color = emptyColor; // empty = black/grey

            circles[i].enabled = (i < max);
        }
    }
}
