using UnityEngine;
using System.Collections;

public class Creature : FSMBehaviour<Creature.State> {
    public enum State { Alive, Dying, Dead, Recycled }

    protected SpriteRenderer mySpriteRenderer;
    protected Animator myAnimator;
    protected Transform myParent;
    protected Animation myAnimation;

    protected void Awake()
    {
        AllowTransitionChain(State.Alive, State.Dying, State.Dead, State.Recycled);

        myAnimator = GetComponent<Animator>();
        mySpriteRenderer = GetComponent<SpriteRenderer>();
        myParent = transform.parent;
        myAnimation = animation;
    }
}
