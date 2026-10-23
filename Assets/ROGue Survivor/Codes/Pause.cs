using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause : MonoBehaviour
{
    RectTransform rect;
    public bool IsPaused { get; private set; }


    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    public void Show()
    {
        if (!GameManager.instance.isLive || GameManager.instance.IsEnding) return;
        IsPaused = true;
        rect.localScale = Vector3.one;
        GameManager.instance.StopGame();
        AudioManager.instance.FilterBgm(true);
    }

    public void Hide()
    {
        if (!IsPaused || GameManager.instance.IsEnding) return;
        IsPaused = false;
        rect.localScale = Vector3.zero;
        GameManager.instance.ResumeGame();
        AudioManager.instance.FilterBgm(false);
    }
}
