using System;
using UnityEditor.Animations;
using UnityEditor.U2D;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class VATAnimator : MonoBehaviour
{
    [field: SerializeField]
    public Animator Target { get; private set; }

    private void Awake()
    {
        Initialize();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        Initialize();
    }
#endif

    private void Initialize()
    {
        Target = GetComponent<Animator>();
    }
    
    [ContextMenu("Bake Animation States")]
    private void BakeAnimationStates()
    {
        if (!Target)
        {
            Initialize();
            return;
        }

        var animatorController = Target.runtimeAnimatorController as AnimatorController;
        foreach (var layer in animatorController.layers)
        {
            var states = layer.stateMachine.states;
            foreach (var state in states)
            {
                // state.state.
            }
        }
    }
}