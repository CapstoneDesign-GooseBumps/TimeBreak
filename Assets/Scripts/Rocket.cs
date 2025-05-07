using UnityEngine;

public class Rocket : MonoBehaviour
{
    // public float explosionRadius = 5f;
    // public float directMaxDamage = 115f;
    // public float directMinDamage = 54f;
    // public float indirectMaxDamage = 75f;
    // public float indirectMinDamage = 27f;

    public string shooterTag = "Shooter";

    public GameObject explosionEffect;


    private void OnCollisionEnter(Collision collision)
    {
        
        if (collision.gameObject.CompareTag(shooterTag))
        {
            return;
        }
        
        GameObject fx = Instantiate(explosionEffect, transform.position, Quaternion.identity);
        Destroy(fx, 2f);

        Destroy(gameObject);
    }
}
