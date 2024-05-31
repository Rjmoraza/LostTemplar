using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySkeleton : Enemy
{
    [SerializeField] private float defaultSpeed = 5;
    [SerializeField] private float hp = 50;
    [SerializeField] private float damage = 10;

    private float speed;
    private Animator anim;
    private AudioSource audio;

    [SerializeField] private AudioClip swishClip;
    [SerializeField] private AudioClip slashClip;
    [SerializeField] private AudioClip hitClip;

    protected override void Initialize()
    {
        maxHitPoints = hp;
        hitPoints = hp;

        anim = GetComponent<Animator>();
        audio = GetComponent<AudioSource>();

        Roam roamState = new Roam(this);
        Chase chaseState = new Chase(this);
        Die dieState = new Die(this);

        roamState.AddTransition(new State.Transition(IsDead, dieState));
        roamState.AddTransition(new State.Transition(() => {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.right, 8);
            ITarget target;
            if(hit.collider != null && hit.collider.TryGetComponent<ITarget>(out target))
            {
                chaseState.SetTarget(hit.collider.transform);
                return true;
            }
            return false;
        }, chaseState));

        chaseState.AddTransition(new State.Transition(IsDead, dieState));
        chaseState.AddTransition(new State.Transition(chaseState.IsTargetOutOfReach, roamState));

        currentState = roamState;
        roamState.OnEnter();
    }

    public void MeleeAttack()
    {
        Collider2D other = Physics2D.OverlapCircle(transform.position + transform.right * 2, 1);
        IDamageable dmg;
        PlaySwishSound();
        if(other != null && other.TryGetComponent<IDamageable>(out dmg))
        {
            dmg.ApplyDamage(damage);
            PlaySlashSound();
        }
    }

    public override void ApplyDamage(float damage)
    {
        if(!IsDead())
        {
            base.ApplyDamage(damage);
            Move(Vector3.zero);
            anim.SetTrigger("Hit");
            audio.PlayOneShot(hitClip);
        }
    }

    public void PlaySwishSound()
    {
        float pitch = Random.Range(0.5f, 0.8f);
        audio.pitch = pitch;
        audio.PlayOneShot(swishClip);
    }

    public void PlaySlashSound()
    {
        float pitch = Random.Range(0.5f, 0.8f);
        audio.pitch = pitch;
        audio.PlayOneShot(slashClip);
    }

    #region Skeleton State Machine

    private class Roam : State
    {
        EnemySkeleton controller;
        Animator anim;

        public Roam(EnemySkeleton controller) : base()
        {
            this.controller = controller;
            anim = controller.GetComponent<Animator>();
        }

        public override void OnEnter()
        {
            controller.speed = controller.defaultSpeed;
        }

        public override void OnUpdate()
        {
            if(CheckForward())
            {
                controller.Move(controller.transform.right * controller.speed);
            }
            else
            {
                controller.transform.Rotate(0, 180, 0);
            }
            anim.SetFloat("Speed", controller.speed / controller.defaultSpeed);
        }

        /// <summary>
        /// Checks if the way forward is clear
        /// </summary>
        /// <returns>true if this character can move forward, false if not</returns>
        private bool CheckForward()
        {
            bool platformHit;
            RaycastHit2D wallHit;
            LayerMask mask = LayerMask.GetMask("Default");
            platformHit = Physics2D.Raycast(controller.transform.position + Vector3.down + controller.transform.right * 1f, Vector2.down, 2f, mask);
            wallHit = Physics2D.Raycast(controller.transform.position + Vector3.down, controller.transform.right, 2, mask);

            return platformHit && !wallHit;
        }
    }

    private class Chase : State
    {
        Transform target;
        EnemySkeleton controller;
        Animator anim;
        bool isAttacking = false;

        public Chase(EnemySkeleton controller) : base()
        {
            this.controller = controller;
            anim = controller.GetComponent<Animator>();
        }

        public void SetTarget(Transform target)
        {
            this.target = target;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            controller.speed = controller.defaultSpeed * 1.5f;
        }

        public override void OnUpdate()
        {
            if(target != null)
            {
                // Check if target is behind the controller
                float deltaX = target.transform.position.x - controller.transform.position.x;
                if ((deltaX * controller.transform.right.x) < 0)
                {
                    controller.transform.Rotate(0, 180, 0);
                }

                // Check if the target is at attack range
                float distance = Vector2.Distance(target.position, controller.transform.position);
                if (distance <= 3f)
                {
                    if(!isAttacking)
                    {
                        isAttacking = true;
                        controller.StartCoroutine(Attack());
                    }
                }
                else
                {
                    controller.Move(controller.transform.right * controller.speed);
                    anim.SetFloat("Speed", controller.speed / controller.defaultSpeed);
                }
            }
        }
    
        private IEnumerator Attack()
        {
            controller.Move(Vector3.zero);
            anim.SetFloat("Speed", 0);
            anim.SetTrigger("Attack");
            yield return null;
            AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);
            while(info.IsName("Skeleton@Attack"))
            {
                yield return null;
                info = anim.GetCurrentAnimatorStateInfo(0);
            }
            yield return new WaitForSeconds(0.2f);
            isAttacking = false;
        }

        public bool IsTargetOutOfReach()
        {
            return target == null || Vector2.Distance(target.position, controller.transform.position) > 8;
        }
    }

    #endregion
}
