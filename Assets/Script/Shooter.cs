using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] public float shootDelay = 0.5f;
    [SerializeField] private int lifePoints;
    [SerializeField] private int maxLifePoints;
    [SerializeField] private int damage;
    [SerializeField] private float bullSpeed;
    
    public void ApplyDamage(int value)
    {
        lifePoints -= value;

        if (lifePoints <= 0)
        {
            // Game Over
            StartCoroutine(DyeCoroutine());
        }
    }

    IEnumerator DyeCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }

    public void ApplyHeal(int value)
    {
        lifePoints += value;
    }

    public void Shoot()
    {
        var direction = transform.rotation * Vector3.forward;
        Bullet bull = Instantiate(bulletPrefab, transform.position + direction * 0.5f+Vector3.up, transform.rotation).GetComponent<Bullet>();
        bull.Initialize(damage, bullSpeed);
    }

}
