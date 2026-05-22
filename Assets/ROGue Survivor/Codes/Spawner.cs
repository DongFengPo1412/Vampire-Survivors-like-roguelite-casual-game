using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{    
    public SpawnData[] spawnData;
    public Transform[] spawnPoint;
    public float levelTime;

    float timer;
    int level;

    void Awake() // get all spawn points when the game starts
    {
        spawnPoint = GetComponentsInChildren<Transform>();
    }

    void Update() 
    {
        if (!GameManager.instance.isLive) 
            return;
            
        timer += Time.deltaTime;
        level = Mathf.Min(Mathf.FloorToInt(GameManager.instance.gameTime / 60f), spawnData.Length - 1);

        if (timer > spawnData[level].spawnTime) // spawn enemies related to level
        {
            timer = 0;
            Spawn();
            //Debug.Log("level == " + level);
        }   
    }

  
    void Spawn()  // spawn enemies randomly at spawn points
    {
        GameObject enemy = GameManager.instance.objectManager.Get(0);
        enemy.transform.position = spawnPoint[UnityEngine.Random.Range(1, spawnPoint.Length)].position;
        enemy.GetComponent<EnemyLogic>().Init(spawnData[level]);
    }
}

[Serializable]
public class SpawnData
{
    public float spawnTime;
    public int spriteType;
    public int health;
    public float speed;
}
