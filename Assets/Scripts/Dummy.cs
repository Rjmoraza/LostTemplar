using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dummy : MonoBehaviour, IDamageable, ITarget
{
    SpriteRenderer sr;

    // Start is called before the first frame update
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void IDamageable.ApplyDamage(float damage)
    {
        StartCoroutine(ShowDamage());
    }

    void IDamageable.Heal(float damage)
    {
        throw new System.NotImplementedException();
    }

    IEnumerator ShowDamage()
    {
        sr.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        sr.color = Color.white;
    }
}
