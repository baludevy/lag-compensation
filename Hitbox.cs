using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    private int id; // refers to the player id who triggered the creation of this hitbox
                    // use this to make sure that the player who shot this hitbox is the one who created it
    private int targetId; // refers to the player id that the hitbox recalls
                          // use this to recall to the player that got shot

    private void Start()
    {
        Invoke("DestroySelf", 1f);
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }

    public void Initialize(int id, int targetId)
    {
        this.id = id;
        this.targetId = targetId;
    }

    public bool CheckOwnership(int id)
    {
        return id == this.id;
    }

    public int GetTargetPlayerId()
    {
        return targetId;
    }
}
