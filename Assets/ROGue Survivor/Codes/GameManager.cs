using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("# Game Object")]
    public Player player;
    public ObjectManager objectManager;
    public LevelUp uiLevelUp;
    public Pause pause;
    public Result uiResult;
    public GameObject enemyCleaner;

    [Header("# Game Control")]
    public bool isLive;
    public float gameTime;
    public float maxGameTime = 300; // set max time of the 1 game

    [Header("# Player info")]
    public float health;
    public float maxHealth = 100;
    public int level = 0;
    public int kill = 0;
    public int exp = 0;
    public int[] nextExp;

    void Awake()
    {
        instance = this;
    }

    public void GameStart()
    {
        health = maxHealth;
        uiLevelUp.Select(0); // set standart weapon
        ResumeGame();

        AudioManager.instance.PlayBgm(true);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
    }

    public void GameOver()
    {
        StartCoroutine(GameOverRoutine());
    }

    IEnumerator GameOverRoutine()
    {
        isLive = false;

        yield return new WaitForSeconds(0.5f);

        uiResult.gameObject.SetActive(true);
        uiResult.Lose();
        StopGame();

        AudioManager.instance.PlayBgm(false);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Lose);
    }

    public void GameWon()
    {
        StartCoroutine(GameWonRoutine());
    }

    IEnumerator GameWonRoutine()
    {
        isLive = false;
        enemyCleaner.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        uiResult.Win();
        uiResult.gameObject.SetActive(true);
        StopGame();

        AudioManager.instance.PlayBgm(false);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Win);
    }

    public void GameRetry()
    {
        SceneManager.LoadScene(0);
        ResumeGame();
    }

    void Update()
    {
        if (!isLive)
        {
            if (Input.GetKeyDown(KeyCode.Escape) && !uiResult.gameObject.activeSelf && uiLevelUp.gameObject.transform.localScale == Vector3.zero)
                pause.Hide();
            return;
        } 

        gameTime += Time.deltaTime; // update time each frame

        if (gameTime > maxGameTime)
        {
           //Debug.Log("gameTime = " + gameTime);
            gameTime = maxGameTime;
            GameWon();
        }
        
        if (Input.GetKeyDown(KeyCode.Escape))
            pause.Show();
    }

    public void GetExp()
    {
        if (!isLive) 
            return;

        exp++;

        if (exp == nextExp[Mathf.Min(level, nextExp.Length - 1)])
        {
            level++;
            exp = 0;
            uiLevelUp.Show();
        }
    }

    // when leveling up / menu use pause game function 
    public void StopGame()
    {
        isLive = false;
        Time.timeScale = 0;
    }

    // unpause game
    public void ResumeGame()
    {
        isLive = true;
        Time.timeScale = 1;
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}