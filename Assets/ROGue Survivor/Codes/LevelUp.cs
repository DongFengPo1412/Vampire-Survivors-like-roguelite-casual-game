using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelUp : MonoBehaviour
{
    RectTransform rect;
    Item[] items;
    void Awake()
    {
        rect = GetComponent<RectTransform>();
        items = GetComponentsInChildren<Item>(true);
    }

    public void Show()
    {
        Next();
        rect.localScale = Vector3.one;
        GameManager.instance.StopGame();
        AudioManager.instance.PlaySfx(AudioManager.Sfx.LevelUp);
        AudioManager.instance.FilterBgm(true);
    }

    public void Hide()
    {
        rect.localScale = Vector3.zero;
        GameManager.instance.ResumeGame();
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
        AudioManager.instance.FilterBgm(false);
    }

    public void Select(int index)
    {
        items[index].OnClick();
    }

    void Next()
    {
        // 1. Disable all items
        foreach (Item item in items)
        {
            item.gameObject.SetActive(false);
        }

        // Filter before sampling: exhausted upgrades do not collapse into the
        // same heal button. If fewer than three choices remain, show each once.
        List<Item> candidates = new List<Item>();
        foreach (Item item in items)
        {
            if (item.CanUpgrade) candidates.Add(item);
        }
        int count = Mathf.Min(3, candidates.Count);
        for (int choice = 0; choice < count; choice++)
        {
            int index = Random.Range(0, candidates.Count);
            candidates[index].gameObject.SetActive(true);
            candidates.RemoveAt(index);
        }
    }
}
