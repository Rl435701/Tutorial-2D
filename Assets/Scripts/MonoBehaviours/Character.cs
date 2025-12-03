using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HitPoint
{
    public int value;
}

public class Character : MonoBehaviour
{
    public int maxHitPoints = 10;
    public HitPoint hitPoints;

    protected virtual void Start()
    {
        if (hitPoints == null)
        {
            hitPoints = new HitPoint();
        }
        hitPoints.value = 5;
    }
}