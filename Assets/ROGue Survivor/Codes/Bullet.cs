using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage;
    public int perforation;
    Rigidbody2D rigid;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    
    }
    public void Init(float damage, int perforation, Vector3 direction)
    {
        this.damage = damage;
        this.perforation = perforation;

        if (perforation > -1)
        {
            rigid.velocity = direction * 10f;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.CompareTag("Enemy") || perforation == -1 || rigid == null) return;

            perforation--;

            if (perforation == -1) 
            {
                rigid.velocity = Vector2.zero;
                gameObject.SetActive(false);
            }
        }
    
    void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Area")) return;
        gameObject.SetActive(false);
    }
}
