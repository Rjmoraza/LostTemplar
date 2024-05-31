using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathPlane : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collider)
    {
        IDamageable dmg;
        if(collider.TryGetComponent<IDamageable>(out dmg))
        {
            dmg.ApplyDamage(1000);
        }
    }
}
