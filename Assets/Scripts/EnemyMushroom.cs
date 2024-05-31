using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMushroom : Enemy
{
    [SerializeField] private GameObject spore;
    [SerializeField] private float defaultSpeed = 5;
    [SerializeField] private float hp = 20;

    [SerializeField] private AudioClip hitClip;

    private float speed;
    private Animator anim;

    private AudioSource audio;

    protected override void Initialize()
    {
        hitPoints = hp;
        maxHitPoints = hp;
        speed = defaultSpeed;
        anim = GetComponent<Animator>();
        audio = GetComponent<AudioSource>();

        Roam roamState = new Roam(this);
        Panic panicState = new Panic(this, spore);
        Die dieState = new Die(this);

        roamState.AddTransition(new State.Transition(IsDead, dieState));
        roamState.AddTransition(new State.Transition(roamState.CheckHitPoints, panicState));

        panicState.AddTransition(new State.Transition(IsDead, dieState));
        panicState.AddTransition(new State.Transition(() => {
            return !panicState.isPanicking;
        }, roamState));

        currentState = roamState;
        roamState.OnEnter();
    }

    public override void ApplyDamage(float damage)
    {
        Move(Vector2.zero);
        if (!IsDead())
        {
            base.ApplyDamage(damage);
            anim.SetTrigger("Hit");
            audio.PlayOneShot(hitClip);
        }
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
        platformHit = Physics2D.Raycast(transform.position + Vector3.down + transform.right * 1f, Vector2.down, 2f, mask);
        wallHit = Physics2D.Raycast(transform.position + Vector3.down, transform.right, 2, mask);

        return platformHit && !wallHit;
    }

    #region Mushroom State Machine
    private class Roam : State
    {
        EnemyMushroom controller;
        Animator anim;
        float hitPoints;

        public Roam(EnemyMushroom controller) : base()
        {
            this.controller = controller;
            anim = controller.GetComponent<Animator>();
        }

        public override void OnEnter()
        {
            base.OnEnter();
            controller.speed = controller.defaultSpeed;
            hitPoints = controller.hitPoints;
        }

        public override void OnUpdate()
        {
            if (controller.CheckForward())
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
        /// Checks if there is a change of hitpoints
        /// </summary>
        /// <returns>true if the recorded hitpoints are different from the current hitpoints</returns>
        public bool CheckHitPoints()
        {
            return hitPoints != controller.hitPoints;
        }

    }

    private class Panic : State
    {
        EnemyMushroom controller;
        GameObject spore;
        Animator anim;
        Coroutine panicCoroutine;

        public bool isPanicking = false;

        public Panic(EnemyMushroom controller, GameObject spore) : base()
        {
            this.controller = controller;
            this.spore = spore;
            anim = controller.GetComponent<Animator>();
        }

        public override void OnEnter()
        {
            base.OnEnter();
            controller.speed = controller.defaultSpeed * 2;
            panicCoroutine = controller.StartCoroutine(PanicCoroutine());
            isPanicking = true;
        }
        public override void OnUpdate()
        {
            if (controller.CheckForward())
            {
                controller.Move(controller.transform.right * controller.speed);
            }
            else
            {
                controller.transform.Rotate(0, 180, 0);
            }
            anim.SetFloat("Speed", controller.speed / controller.defaultSpeed);
        }

        public override void OnExit()
        {
            base.OnExit();
            controller.StopCoroutine(panicCoroutine);
        }

        IEnumerator PanicCoroutine()
        {
            for(int i = 0; i < 20; ++i)
            {
                Instantiate(spore, controller.transform.position + Vector3.up * 1.5f, Quaternion.identity);
                yield return new WaitForSeconds(0.5f);
            }
            isPanicking = false;
        }
    }

    #endregion
}
