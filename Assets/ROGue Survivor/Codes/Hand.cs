using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
    public bool isLeft;
    public SpriteRenderer sprite;

    SpriteRenderer player;
    Vector3 rightPos = new Vector3(0.5f, 0, 0);
    Vector3 rightPosReversed = new Vector3(-0.5f, 0, 0);
    Quaternion leftRotate = Quaternion.Euler(0, 0, -120);
    Quaternion leftRotateReversed = Quaternion.Euler(0, 0, -240);

    void Awake()
    {
        player = GetComponentsInParent<SpriteRenderer>()[1];
    }

    void LateUpdate()
    {
        bool isReverse = player.flipX;

        if (isLeft)
        {
            transform.localRotation = isReverse ? leftRotateReversed : leftRotate;
            sprite.flipY = isReverse;
            sprite.sortingOrder = isReverse ? 4 : 6;
        }
        else
        {
            transform.localPosition = isReverse ? rightPosReversed : rightPos;
            sprite.flipX = isReverse;
            sprite.sortingOrder = isReverse ? 6 : 4;
        }
    }
}
