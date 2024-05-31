using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spore : MonoBehaviour
{
    private bool isAlive = true;

    [SerializeField] private float baseDamage = 10;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Rigidbody2D>().AddForce(Vector3.up * 100);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isAlive)
        {
            IDamageable dmg;
            ITarget t; 
            if (collision.collider.TryGetComponent<ITarget>(out t))
            {
                collision.collider.GetComponent<IDamageable>().ApplyDamage(baseDamage);
            }
            isAlive = false;
            StartCoroutine(Explode());
        }
    }

    IEnumerator Explode()
    {
        GetComponent<Animator>().SetTrigger("Explode");
        yield return new WaitForSeconds(2);
        Destroy(gameObject);
    }
}
