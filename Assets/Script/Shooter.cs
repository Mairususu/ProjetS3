using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Shooter : MonoBehaviour
{
    [SerializeField] public float shootDelay = 0.5f;
    [SerializeField] public int lifePoints;
    [SerializeField] public int maxLifePoints;
    [SerializeField] public int damage;
    [SerializeField] public float bullSpeed;
}
