using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class Knight : MonoBehaviour, IDamageable, ITarget
{
    private Rigidbody2D rb;
    private Animator anim;

    [SerializeField] private float speed = 10;
    [SerializeField] private float jumpSpeed = 20;
    [SerializeField] private float dashSpeed = 10;

    [SerializeField] private float hitpoints = 100;
    [SerializeField] private float maxHitPoints = 100;

    [SerializeField] private float baseDamage = 10;
    [SerializeField] private GameObject attackPoint;


    private bool isDead = false;    
    private bool canMove = true;
    private bool isDashing = false;

    [SerializeField] private AudioClip stepsSound;
    [SerializeField] private AudioClip swishSound;
    [SerializeField] private AudioClip slashSound;

    private AudioSource audio;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        audio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        // Get input values
        float x = 0;
        if(canMove && !isDead) x = Input.GetAxisRaw("Horizontal");
        bool grounded = IsGrounded();

        // Move
        if(!isDashing) rb.velocity = new Vector3(x * speed, rb.velocity.y);

        // If direction is right, set rotation to identity
        if (x > 0)
        {
            transform.rotation = Quaternion.identity;
        }
        // If direction is left, set rotation to -y
        else if (x < 0)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        
        if(Input.GetButtonDown("Jump") && grounded && canMove)
        {
            rb.AddForce(Vector2.up * jumpSpeed, ForceMode2D.Impulse);
        }
        
        // Update animations
        anim.SetFloat("XSpeed", x);
        anim.SetFloat("YSpeed", rb.velocity.y);
        anim.SetBool("Grounded", grounded);

        if (Input.GetButtonDown("Attack") && !isDashing)
        {
            anim.SetTrigger("Attack");
            canMove = false;
        }

        if(Input.GetButtonDown("Dash") && grounded && canMove)
        {
            rb.velocity = transform.right * dashSpeed;
            //rb.AddForce(transform.right * dashSpeed, ForceMode2D.Impulse);
            anim.SetTrigger("Dash");
            isDashing = true;
        }
    }

    /// <summary>
    /// Checks if this character controller is hitting the ground
    /// Make sure Physics2D.queriesStartInColliders is set to false to ignore self 
    /// </summary>
    /// <returns>true if the capsule is on the ground</returns>
    bool IsGrounded()
    {
        LayerMask mask = LayerMask.GetMask("Default");
        return Physics2D.CircleCast(transform.position + Vector3.up, 1, Vector3.down, 1.1f, mask);
    }

    public void Attack()
    {
        Collider2D other = Physics2D.OverlapCircle(attackPoint.transform.position, 1);
        IDamageable dmg;
        PlaySwishSound();
        
        if(other != null && other.name != gameObject.name && other.TryGetComponent<IDamageable>(out dmg))
        {
            dmg.ApplyDamage(baseDamage);
            PlaySlashSound();
        }
    }

    public void EndAttack()
    {
        canMove = true;
    }

    public void EndDash()
    {
        isDashing = false;
    }

    public bool IsDead()
    {
        return isDead;
    }

    void IDamageable.ApplyDamage(float damage)
    {
        if(!isDashing)
        {
            if (hitpoints < damage)
            {
                hitpoints = 0;
                anim.SetTrigger("Dead");
                isDead = true;
            }
            else
            {
                hitpoints -= damage;
                anim.SetTrigger("Hit");
                canMove = true;
            }
        }
    }

    void IDamageable.Heal(float damage)
    {
        if (hitpoints + damage > maxHitPoints) hitpoints = maxHitPoints;
        else hitpoints = hitpoints + damage;

        // TODO VFX for Heal
    }

    public float GetHPRatio()
    {
        return hitpoints / maxHitPoints;
    }

    public void PlayStepsSound()
    {
        float pitch = Random.Range(1.4f, 1.8f);
        audio.pitch = pitch;
        audio.PlayOneShot(stepsSound);
    }

    public void PlaySwishSound()
    {
        float pitch = Random.Range(1.4f, 1.8f);
        audio.pitch = pitch;
        audio.PlayOneShot(swishSound);
    }

    public void PlaySlashSound()
    {
        float pitch = Random.Range(1.4f, 1.8f);
        audio.pitch = pitch;
        audio.PlayOneShot(slashSound);
    }
}
