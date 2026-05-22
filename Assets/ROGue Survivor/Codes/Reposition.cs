using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;

// map reposition script
public class Reposition : MonoBehaviour
{
    Collider2D enemyCollider;

    void Awake()
    {
        enemyCollider = GetComponent<Collider2D>();
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        // if collides not with Area object, then return
        if (!collision.CompareTag("Area") || GameManager.instance.health <= 0) return;

        // get playerPosition and position of Area
        UnityEngine.Vector3 playerPos = GameManager.instance.player.transform.position;
        UnityEngine.Vector3 myPos = transform.position;

        // get difference between player and Tilemap pos
        float diffX = Mathf.Abs(playerPos.x - myPos.x);
        float diffY = Mathf.Abs(playerPos.y - myPos.y);

        // get direction of player
        UnityEngine.Vector3 playerDir = GameManager.instance.player.inputVec;
        float dirX = playerDir.x < 0 ? -1 : 1; 
        float dirY = playerDir.y < 0 ? -1 : 1;

        // switch to check the source of collision
        switch(transform.tag)
        {
            // moving area if player collides with the borders of area
            case "Ground": 
                //Debug.Log(gameObject.name + "\n" + "diffX = " + diffX + ", diffY =" + diffY);
                if (diffX > 20 && diffY > 20) // prevent from moving area too far
                {
                    //Debug.Log("Translated " + gameObject.name + " for both directions");
                    transform.Translate(UnityEngine.Vector3.up * dirY * 40);
                    break;
                }
                else
                {
                    if (diffX > diffY)
                        transform.Translate(UnityEngine.Vector3.right * dirX * 56);
                    else if (diffX < diffY)
                        transform.Translate(UnityEngine.Vector3.up * dirY * 40);
                    break;
                }
            // moving enemies if player went too far from enemies
            case "Enemy":
                if (enemyCollider.enabled)
                {
                    transform.Translate(playerDir * 30 + new UnityEngine.Vector3(UnityEngine.Random.Range(-5f, 5f), UnityEngine.Random.Range(-5f, 5f), 0f));
                }
                break;
            
            default: break;
        }
    }
}
        
    
    

