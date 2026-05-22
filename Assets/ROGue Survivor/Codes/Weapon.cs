using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public int id;
    public int prefabId;
    public float damage;
    public int count;
    public float speed;
    float timer; // timer for shooting delay
    Player player;

    void Awake()
    {
        player = GameManager.instance.player;
    }

    void Update()
    {
        if (!GameManager.instance.isLive) 
            return;
            
        switch(id)
        {
            case 0:
                transform.Rotate(Vector3.back * speed * Time.deltaTime);
                break;
            case 1:
                timer += Time.deltaTime;
                if (Input.GetMouseButton(0) && timer > speed)
                {
                    timer = 0f;
                    Fire();
                }
                break;
            default:
                break;
        }

        
        // Test Code 
        if (Input.GetButtonDown("Jump"))
        {
            LevelUp(10, 1);
        }
    }

    public void LevelUp(float damage, int count)
    {
        this.damage = damage;
        this.count += count;

        if (id == 0)
            Batch();
        
        player.BroadcastMessage("ApplyGear", SendMessageOptions.DontRequireReceiver);
    }

    public void Init(ItemData data)
    {
        // Basic set
        name = "Weapon " + data.itemId;
        transform.parent = player.transform;
        transform.localPosition = Vector3.zero;

        // Property set
        id = data.itemId;
        damage = data.baseDamage;
        count = data.baseCount;
        
        for (int index = 0; index < GameManager.instance.objectManager.prefabs.Length; index++)
        {
            if (data.projectile == GameManager.instance.objectManager.prefabs[index])
            {
                prefabId = index;
                break;
            }
        }

        switch(id)
        {
            case 0:
                speed = 150;
                Batch();
                break;
            case 1:
                speed = 0.5f;
                break;
            default:
                break;
        }

        // Hand set
        Hand hand = player.hands[(int)data.itemType];
        hand.sprite.sprite = data.hand;
        hand.gameObject.SetActive(true);

        player.BroadcastMessage("ApplyGear", SendMessageOptions.DontRequireReceiver);
    }

    // Batch bullets
    void Batch()
    {
        for (int index = 0; index < count; index++)
        {
            // Get bullet from object pool
            Transform bullet;
            
            if (index < transform.childCount)
            {
                bullet = transform.GetChild(index);
            }
            else 
            {
                bullet = GameManager.instance.objectManager.Get(prefabId).transform;
                bullet.parent = transform;
            }
            bullet.parent = transform;
            

            // rotation using local coordinates, not world coordinates
            bullet.localPosition = Vector3.zero;
            bullet.localRotation = Quaternion.identity;

            // Rotate crowbar
            Vector3 rotateVector = Vector3.forward * (360 / count) * index;
            bullet.Rotate(rotateVector);
            bullet.Translate(bullet.up * 1.8f, Space.World);

            // Init crowbar
            bullet.GetComponent<Bullet>().Init(damage, -1, Vector3.zero); // -1 is infinity perforation
        }
    }

    public void Fire()
    {
        // Debug.Log("Fire started");
        //if (!player.scanner.nearestTarget) return;

        //Vector3 targetPos = player.scanner.nearestTarget.position;
        //Vector3 dir = targetPos - transform.position;
        //dir = dir.normalized;
        //bullet.rotation = Quaternion.FromToRotation(Vector3.up, dir);
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Camera.main.transform.position.z - transform.position.z;
        Vector3 targetPos3D = Camera.main.ScreenToWorldPoint(mousePosition);
        Vector2 targetPos = new Vector2(targetPos3D.x, targetPos3D.y);
        Vector2 position2D = new Vector2(transform.position.x, transform.position.y);
        Vector2 dir = (targetPos - position2D).normalized;
        
        Transform bullet = GameManager.instance.objectManager.Get(prefabId).transform;
        bullet.position = transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        bullet.rotation = Quaternion.Euler(0, 0, angle - 90); // Подстройка на 90 градусов, если нужно
        bullet.GetComponent<Bullet>().Init(damage, count, dir);

        AudioManager.instance.PlaySfx(AudioManager.Sfx.Pistol);
    }
}
