using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationPlayer : MonoBehaviour
{
    [SerializeField] private Animator _animator;

    private readonly string WALK = "Walk";
    private readonly string IDLE = "Idle";

    private void Start()
    {
        _animator = GetComponent<Animator>();
    }

    public void Walk()
    {
        // _animator.SetBool(WALK, true);
        // _animator.SetBool(IDLE, false);
    }

    public void Idle()
    {
        // _animator.SetBool(WALK, false);
        // _animator.SetBool(IDLE, true);
    }
}
