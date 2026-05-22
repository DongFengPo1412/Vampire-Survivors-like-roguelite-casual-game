using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyLogic : MonoBehaviour
{
    public float speed;
    public float health;
    public float maxHealth;
    public RuntimeAnimatorController[] animController;
    public Rigidbody2D target;
    public bool isALive;
    Rigidbody2D rigid;
    Animator anim;
    Collider2D coll;
    SpriteRenderer sprite;
    WaitForFixedUpdate wait;

    // Start is called before the first frame update
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        wait = new WaitForFixedUpdate();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!GameManager.instance.isLive) 
            return;

        if (!isALive || anim.GetCurrentAnimatorStateInfo(0).IsName("Monster_hit")) 
            return;

        UnityEngine.Vector2 directionalVector = target.position - rigid.position;
        UnityEngine.Vector2 nextVector = directionalVector.normalized * Time.fixedDeltaTime * speed;
        rigid.MovePosition(rigid.position + nextVector);
        rigid.velocity = UnityEngine.Vector2.zero;
    }

    void LateUpdate()
    {
        if (!GameManager.instance.isLive) 
            return;
            
        if (!isALive) return;

        sprite.flipX = target.position.x < rigid.position.x;
    }

    void OnEnable() // setting parameters of enemy when it's spawned
    {
        target = GameManager.instance.player.GetComponent<Rigidbody2D>();
        isALive = true;
        coll.enabled = true;
        rigid.simulated = true;
        sprite.sortingOrder = 2;
        anim.SetBool("Dead", false);
        health = maxHealth;
    }

    public void Init(SpawnData data)
    {
        anim.runtimeAnimatorController = animController[data.spriteType];
        speed = data.speed;
        maxHealth = data.health;
        health = data.health;
    }

    void OnTriggerEnter2D(Collider2D collision) // damage taking and damage dealing logic
    {
        if (!collision.CompareTag("Bullet") || !isALive) return;

        health -= collision.GetComponent<Bullet>().damage;
        StartCoroutine(Knock()); // knocking enemy


        if (health > 0) // if enemy is alive we're playing hit animation
        {
            anim.SetTrigger("Hit");
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Hit);
        }
        else // if enemy is dead we're disabling it and it parameters and give exp to player
        {
            isALive = false;
            coll.enabled = false;
            rigid.simulated = false;
            sprite.sortingOrder = 1;
            anim.SetBool("Dead", true);
            GameManager.instance.kill++;
            GameManager.instance.GetExp();

            if (GameManager.instance.isLive)
                AudioManager.instance.PlaySfx(AudioManager.Sfx.Dead);
        }
    }

    IEnumerator Knock() // knockback logic
    {
        yield return wait; // wait for 1 frame
        UnityEngine.Vector3 playerPos = GameManager.instance.player.transform.position;
        UnityEngine.Vector3 knockVector = transform.position - playerPos;
        rigid.AddForce(knockVector.normalized * 3, ForceMode2D.Impulse);
    }
    
    void Dead()
    {
        gameObject.SetActive(false);
    }
}
