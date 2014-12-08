using UnityEngine;
using System.Collections;

public class Creature : FSMBehaviour<Creature.State> {
    public enum State { Alive, Dying, Dead, Recycled }

    protected new void Awake()
    {
        base.Awake();

        AllowTransitionChain(State.Alive, State.Dying, State.Dead, State.Recycled);
    }

    public OnEnter delegatePlayAnimation(string clipName)
    {
        return delegate(object prop)
        {
            if (prop == null) prop = clipName;

            myAnimation.Play((string)prop);
        };
    }

    public OnUpdate delegateAdvanceAfterAnimation<T>(FSM<T> fsm, T nextState)
    {
        return delegate
        {
            if (!myAnimation.isPlaying) fsm.Advance(nextState);
        };
    }
}
