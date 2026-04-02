using UnityEngine;
using UnityEngine.AI;

public enum NPC_ANIM_TYPES
{
    NONE,
    WALKING_ONLY,
    EXAMPLE_FOR_MORE_COMPLICATED_BEHAVIOR,
}

[RequireComponent( typeof(NavMeshAgent), typeof (NPC_Behavior), typeof(Animator))]
public class NPCAnimationBrain : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] NPC_ANIM_TYPES animType;

    NavMeshAgent agent;
    Animator animator;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        UpdateAnimator();
    }

    private void UpdateAnimator()
    {
        switch (animType)
        {
            case NPC_ANIM_TYPES.NONE:
                //Do nothing!
                break;

            case NPC_ANIM_TYPES.WALKING_ONLY:
                if (agent.isStopped)
                {
                    animator.SetFloat("moveSpeed", 0);
                }
                else
                {
                    animator.SetFloat("moveSpeed", agent.speed/agent.velocity.magnitude);
                }
                break;

            case NPC_ANIM_TYPES.EXAMPLE_FOR_MORE_COMPLICATED_BEHAVIOR:
                //we can do stuff here!
                break;
        }
    }
}
