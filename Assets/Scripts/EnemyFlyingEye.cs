using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFlyingEye : Enemy
{
    [SerializeField] private float defaultSpeed = 5;
    [SerializeField] private float hp = 50;
    [SerializeField] private float damage = 10;
    [SerializeField] private float baseSpeed = 10;
    [SerializeField] private float baseDamage = 10;
    [SerializeField] private GameObject attackPoint;

    [SerializeField] private AudioClip attackClip;
    [SerializeField] private AudioClip biteClip;
    [SerializeField] private AudioClip hitClip;

    Animator anim;
    AudioSource audio;


    protected override void Initialize()
    {
        anim = GetComponent<Animator>();
        audio = GetComponent<AudioSource>();

        maxHitPoints = hp;
        hitPoints = hp;

        Roam roamState = new Roam(this);
        Chase chaseState = new Chase(this);
        Die dieState = new Die(this);

        roamState.AddTransition(new State.Transition(IsDead, dieState));
        roamState.AddTransition(new State.Transition(() => {
            LayerMask mask = LayerMask.NameToLayer("Character");
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 10);
            foreach(Collider2D c in colliders)
            {
                ITarget t;
                if(c.TryGetComponent<ITarget>(out t))
                {
                    chaseState.SetTarget(c.transform);
                    return true;
                }
            }

            return false;
        }, chaseState));

        chaseState.AddTransition(new State.Transition(IsDead, dieState));
        chaseState.AddTransition(new State.Transition(chaseState.IsTargetOutOfReach, roamState));
        
        currentState = roamState;
        roamState.OnEnter();
    }

    public void Attack()
    {
        Collider2D other = Physics2D.OverlapCircle(attackPoint.transform.position, 1f);
        IDamageable dmg;
        audio.PlayOneShot(attackClip);
        if(other != null && other.TryGetComponent<IDamageable>(out dmg))
        {
            audio.PlayOneShot(biteClip);
            print(other.name);
            dmg.ApplyDamage(baseDamage);
        }
    }

    public override void ApplyDamage(float damage)
    {
        if(!IsDead())
        {
            audio.PlayOneShot(hitClip);
            base.ApplyDamage(damage);
            Move(Vector3.zero);
            anim.SetTrigger("Hit");
        }
    }
    #region FlyingEye State Machine

    private class Roam : State
    {
        EnemyFlyingEye controller;
        Vector2 direction;
        Coroutine directionCoroutine;

        public Roam(EnemyFlyingEye controller)
        {
            this.controller = controller;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            directionCoroutine = controller.StartCoroutine(ChangeDirection());
        }

        public override void OnUpdate()
        {
            controller.Move(direction * controller.baseSpeed, false);

            if(direction.x < 0)
            {
                controller.transform.rotation = Quaternion.Euler(0, 180, 0);
            }
            else if(direction.x > 0)
            {
                controller.transform.rotation = Quaternion.Euler(0, 0, 0);
            }

            if(Physics2D.Raycast(controller.transform.position, direction, 3))
            {
                direction = Random.insideUnitCircle.normalized;
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            controller.StopCoroutine(directionCoroutine);
        }

        IEnumerator ChangeDirection()
        {
            while(true)
            {
                direction = Random.insideUnitCircle.normalized;
                yield return new WaitForSeconds(3);
            }
        }
    }

    private class Chase : State
    {
        EnemyFlyingEye controller;
        Transform target;
        Vector2 direction;

        Animator anim;
        bool isAttacking = false;

        public Chase(EnemyFlyingEye controller)
        {
            this.controller = controller;
            anim = controller.GetComponent<Animator>();
        }

        public void SetTarget(Transform target)
        {
            this.target = target;
        }

        public override void OnUpdate()
        {
            direction = (target.position - controller.transform.position).normalized;

            if(direction.x < 0)
            {
                controller.transform.rotation = Quaternion.Euler(0, 180, 0);
            }
            else if(direction.x > 0)
            {
                controller.transform.rotation = Quaternion.Euler(0, 0, 0);
            }

            if(Vector2.Distance(target.position, controller.transform.position) < 2)
            {
                controller.Move(Vector3.zero);
                if(!isAttacking)
                {
                    isAttacking = true;
                    controller.StartCoroutine(Attack());
                }
            }
            else
            {
                controller.Move(direction * controller.baseSpeed, false);
            }
        }

        private IEnumerator Attack()
        {
            anim.SetTrigger("Attack");
            AnimatorStateInfo info;
            do
            {
                info = anim.GetCurrentAnimatorStateInfo(0);
                controller.Move(Vector3.zero);
                yield return null;
            }
            while (info.IsName("Eye@Attack"));
            isAttacking = false;
        }

        public bool IsTargetOutOfReach()
        {
            return Vector2.Distance(controller.transform.position, target.position) > 10;
        }
    }
    #endregion
}
