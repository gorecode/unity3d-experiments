using UnityEngine;
using UnityEngineExt;
using System.Collections;

public class Activist : Creature2 {
    public enum AliveState {
        RUNNING, START_RUNNING_WITHOUT_SEAL, RUNNING_WITHOUT_SEAL
    }

    enum DropDownState {
        NONE, IN_PROGRESS, DONE
    }

    public GameObject sealPrefab;

    public AudioClip[] soundsOfDyingSeal1;
    public AudioClip[] soundsOfDyingSeal2;

    private FSM<AliveState> aliveState;

    private DropDownState dropDownState;

    protected new void Awake()
    {
        base.Awake();

        aliveState = new FSM<AliveState>();
        aliveState.AllowTransitionChain(AliveState.RUNNING, AliveState.START_RUNNING_WITHOUT_SEAL, AliveState.RUNNING_WITHOUT_SEAL);
        aliveState.RegisterState(AliveState.RUNNING);
        aliveState.RegisterState(AliveState.START_RUNNING_WITHOUT_SEAL,
                                 Action_PlayAnimation("StartRunWithDeadSeal"),
                                 Action_AdvanceAfterAnimation(aliveState, AliveState.RUNNING_WITHOUT_SEAL));
        aliveState.RegisterState(AliveState.RUNNING_WITHOUT_SEAL, OnBecomeRunWithDeadSeal);
    }

    public override void Damage(float damage)
    {
        if (GetCurrentState() != State.Alive) return;

        base.Damage(damage);

        if (health > 0 && health <= maxHealth / 2)
        {
            if (aliveState.Advance(AliveState.START_RUNNING_WITHOUT_SEAL))
            {
                AudioCenter.PlayRandomClipAtMainCamera(soundsOfDyingSeal1);
                AudioCenter.PlayRandomClipAtMainCamera(soundsOfDyingSeal2);
            }
        } else if (health <= 0)
        {
            switch (aliveState.GetCurrentState())
            {
                case AliveState.RUNNING:
                    // Die and drop seal with 50% chance.
                    if (Random2.NextBool())
                    {
                        if (Advance(State.Dying, Random2.RandomArrayElement("DieAndDropDown1", "DieAndDropDown2")))
                        {
                            dropDownState = DropDownState.IN_PROGRESS;
                        }
                    }
                    // Die with seal with 50% chance.
                    else
                    {
                        Advance(State.Dying, Random2.RandomArrayElement("DieWithoutSeal1", "DieWithoutSeal2"));
                    }
                    break;
                case AliveState.RUNNING_WITHOUT_SEAL:
                case AliveState.START_RUNNING_WITHOUT_SEAL:
                    Advance(State.Dying, Random2.RandomArrayElement("DieWithoutSeal1", "DieWithoutSeal2"));
                    break;
            }
        }
    }

    protected override void OnBecomeAlive(object param)
    {
        base.OnBecomeAlive(param);

        runningSpeed = UnityEngine.Random.Range(defaultRunningSpeed, defaultRunningSpeed * 2);
        currentSpeed = runningSpeed;

        aliveState.ForceEnterState(AliveState.RUNNING);

        myAnimation["Run"].speed = currentSpeed / defaultRunningSpeed;
        myAnimation.PlayImmediately("Run");

        mySpriteAnimator.Update();

        dropDownState = DropDownState.NONE;
    }

    protected void OnBecomeRunWithDeadSeal(object param)
    {
        myAnimation.Play("RunWithDeadSeal");
    }

    protected override void OnAlive()
    {
        aliveState.Update();

        myParent.position += Vector3.right * Time.deltaTime * currentSpeed;
    }

    protected override void OnDying()
    {
        base.OnDying();

        if (dropDownState == DropDownState.IN_PROGRESS)
        {
            int sheet = (int)mySpriteAnimator.sheet;

            if ((sheet == 3 || sheet == 4) && ((int)mySpriteAnimator.index >= 4))
            {
                Vector3 position = myParent.transform.position;
                position.x += 0.14f;

                GameObjectPool.Instance.Instantiate(sealPrefab, position, Quaternion.identity);

                dropDownState = DropDownState.DONE;
            }
        }
    }
}
