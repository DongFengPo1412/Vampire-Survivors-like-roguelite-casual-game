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

        // 2. Activate 3 random items from the disabled ones
        int[] randItems = new int[3];
        while (true)
        {
            randItems[0] = Random.Range(0, items.Length);
            randItems[1] = Random.Range(0, items.Length);
            randItems[2] = Random.Range(0, items.Length);

            // checking for same random items (excluding them)
            if (randItems[0] != randItems[1] && randItems[1] != randItems[2] && randItems[0] != randItems[2])
                break;
        }

        for (int index = 0; index < randItems.Length; index++)
        {
            Item ranItem = items[randItems[index]];

            // 3. Replace max-level items with consumable items
            if (ranItem.level == ranItem.data.damages.Length)
            {
                items[4].gameObject.SetActive(true);
            }
            else ranItem.gameObject.SetActive(true);
        }
    }
}
