using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour, IDamageable
{
    protected float maxHitPoints;
    protected float hitPoints;
    protected State currentState;
    protected Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        currentState = currentState.Update();
    }

    protected abstract void Initialize();

    public virtual void ApplyDamage(float damage)
    {
        if (hitPoints < damage) hitPoints = 0;
        else hitPoints = hitPoints - damage;
    }

    public virtual void Heal(float damage)
    {
        if (hitPoints + damage > maxHitPoints) hitPoints = maxHitPoints;
        else hitPoints = hitPoints + damage;
    }

    public bool IsDead()
    {
        return hitPoints <= 0;
    }

    public virtual void Move(Vector2 direction, bool useGravity = true)
    {
        float y = useGravity ? rb.velocity.y : 0;
        rb.velocity = new Vector2(direction.x, y + direction.y);
    }
}
