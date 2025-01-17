using UnityEngine;
using UnityEngineExt;
using System.Collections;

public class Pinguin : Creature2
{
    public enum Alive_SubState { Walking, Sliding }

    public float friction = 0.02f;
    public float delayBeforeSliding = 1;
    public float slidingSpeed;

    private Vector3 direction = Vector3.right;

    private FSM<Alive_SubState> aliveState;

    public Alive_SubState GetAliveState()
    {
        return Alive_SubState.Sliding;
    }

    public override MobType GetMobType()
    {
        return MobType.Pinguin;
    }

    public override void Damage(float damage, Limb target)
    {
        base.Damage(damage, target);

        if (health > 0) return;

        switch (aliveState.GetCurrentState())
        {
            case Alive_SubState.Walking:
                Advance(State.Dying, Random2.RandomArrayElement("Bodyshot", "Headshot"));
                break;
            case Alive_SubState.Sliding:
                Advance(State.Dying, Random2.RandomArrayElement("Corpse1", "Corpse2", "Corpse_RotatingWithoutHead1", "Corpse_RotatingWithoutHead2"));
                break;
        }
    }

    public new void Awake()
    {
        base.Awake();

        aliveState = new FSM<Alive_SubState>();
        aliveState.AllowTransitionChain(Alive_SubState.Walking, Alive_SubState.Sliding);
        aliveState.RegisterState(Alive_SubState.Walking, OnBecomeWalking);
        aliveState.RegisterState(Alive_SubState.Sliding, OnBecomeSliding);
    }

    protected override void OnBecomeAlive(object param)
    {
        base.OnBecomeAlive(param);

        direction = Vector3.right;

        aliveState.ForceEnterState(Alive_SubState.Walking);

        direction = Quaternion.Euler(0, 0, Random.Range(-30, 30)) * Vector3.right;
        direction.Normalize();

        mySpriteAnimator.Update();
    }

    protected override void OnAlive()
    {
        myParent.position += direction * currentSpeed * Time.deltaTime;

        aliveState.Update();
    }

    private void OnBecomeWalking(object param)
    {
        myAnimation.PlayImmediately("Walk");

        currentSpeed = walkingSpeed;

        StopCoroutine("SlideAfterDelayCoroutine");
        StartCoroutine(SlideAfterDelayCoroutine(delayBeforeSliding));
    }

    private IEnumerator SlideAfterDelayCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (State.Alive.Equals(GetCurrentState())) aliveState.Advance(Alive_SubState.Sliding);

        yield return null;
    }

    private void OnBecomeSliding(object param)
    {
        slidingSpeed = currentSpeed = walkingSpeed * 4f;

        direction = Quaternion.Euler(0, 0, Random.Range(-30, 30)) * direction;
        direction.Normalize();

        myAnimation.Play("StartSlide");
    }

    protected override void OnBecomeDying(object param)
    {
        base.OnBecomeDying(param);

        StopAllCoroutines();
    }

    protected override void OnDying()
    {
        switch (aliveState.GetCurrentState())
        {
            case Alive_SubState.Walking:
                if (!myAnimation.isPlaying) Advance(State.Dead);
                break;
            case Alive_SubState.Sliding:
                myParent.position += direction * currentSpeed * Time.deltaTime;

                currentSpeed = Mathf.Max(0.0f, currentSpeed - friction * Time.deltaTime);

                AnimationState animationState = myAnimation.GetCurrentAnimationState();

                if (animationState != null)
                {
                    animationState.speed = 1.0f - (slidingSpeed - currentSpeed) / slidingSpeed;
                }

                if (currentSpeed < 0.01f) Advance(State.Dead);

                break;
        }
    }
}
