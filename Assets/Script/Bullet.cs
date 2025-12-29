using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private int damage = 3;
    [SerializeField] private float speed = 5f;
    [SerializeField] private ParticleSystem explosionPrefab;

    private void FixedUpdate()
    {
        transform.position += transform.forward * (speed * Time.fixedDeltaTime);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.TryGetComponent<PlayerController>(out var damageablePlayer))
        {
            damageablePlayer.ApplyDamage(damage);
        }
        else if (other.gameObject.TryGetComponent<PlayerController>(out var damageableEnemy))
        {
            damageableEnemy.ApplyDamage(damage);
        }

        var explosion = Instantiate(explosionPrefab, other.contacts[0].point, Quaternion.identity);
        explosion.gameObject.SetActive(true);
        explosion.Play();
        Destroy(explosion.gameObject, explosion.main.duration);

        Destroy(gameObject);
    }

    public void Initialize(int damage, float speed)
    {
        this.damage = damage;
        this.speed = speed;
        Destroy(gameObject, 5f);
    }
}
