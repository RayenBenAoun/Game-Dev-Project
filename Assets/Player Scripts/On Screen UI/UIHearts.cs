using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIHearts : MonoBehaviour
{
    [Header("References")]
    public Image heartTemplate;          // assign a disabled child Image
    public Sprite fullHeartSprite;       // full heart image
    public Sprite emptyHeartSprite;      // empty heart image (or tinted/outline)

    [Header("Layout")]
    public Vector2 heartSize = new Vector2(48, 48);

    private readonly List<Image> hearts = new();

    void Awake()
    {
        if (heartTemplate == null)
            Debug.LogWarning("[UIHearts] HeartTemplate not assigned.", this);
    }

    public void Build(int max)
    {
        if (heartTemplate == null) return;

        // clear everything except the template
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var c = transform.GetChild(i);
            if (c != heartTemplate.transform) DestroyImmediate(c.gameObject);
        }
        hearts.Clear();

        // create hearts
        for (int i = 0; i < max; i++)
        {
            var img = Instantiate(heartTemplate, transform);
            img.rectTransform.sizeDelta = heartSize;
            img.sprite = fullHeartSprite != null ? fullHeartSprite : heartTemplate.sprite;
            img.gameObject.SetActive(true);
            hearts.Add(img);
        }
    }

    public void UpdateHearts(int current, int max)
    {
        if (hearts.Count != max)
            Build(max);

        for (int i = 0; i < hearts.Count; i++)
        {
            hearts[i].sprite = (i < current)
                ? (fullHeartSprite != null ? fullHeartSprite : heartTemplate.sprite)
                : (emptyHeartSprite != null ? emptyHeartSprite : heartTemplate.sprite);

            hearts[i].enabled = (i < max);
        }
    }
}
