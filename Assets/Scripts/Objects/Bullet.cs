using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{

    [SerializeField] private float speed = 70f;

    public void ShootForward(GameObject bullet, Transform turret)
    {
        bullet.GetComponent<Rigidbody2D>().AddForce(turret.up * speed, ForceMode2D.Impulse);
    }

    private void HitTarget()
    {
        //Keep this is we want AOE
        /*GameObject effectIns = Instantiate(impactEffect, transform.position, transform.rotation);
        Destroy(effectIns, 5f);
        if (explosionRadius > 0f)
        {
            Explode();
        }*/
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject collider = collision.gameObject;
        if (collider.tag == "Player" || collider.tag == "Enemy"/*collider.GetComponentInParent<PlayerMovement>()*/)
        {
            Destroy(collider);//Target has been hit, if target is player => game over
            Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
