using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using EasyBehaviorTree;
using UnityEngine.AI;

namespace EasyBehaviorTreeExample
{
    public class ExampleBehavior : MonoBehaviour
    {
        public BehaviorTree bt; //Assign this to your created behavior tree in the unity editor 
        public TextMeshProUGUI textToChange; //Text to change when something happens.
        public List<GameObject> patrolPoints; //A list of points that the npc will move between while patrolling
        public GameObject hidePoint; //A point where the npc will go to hide when it sees the player.
        public GameObject player; //A reference to the player to avoid.

        private BTBlackboard bb; //A reference to the blackboard attached to the behavior tree. Use GetBlackboard().
        private NavMeshAgent navMeshAgent; //A reference to the NavMeshAgent component attached to the npc for pathfinding.

        private int currentPatrolPoint = 0; //Which patrol point the npc is currently headed towards.
        private bool hasSetPath = false; //Helper variable to make sure the path is only being set once, (instead of each frame).
        private float waitedTime = 0; //How long the npc has waited while hiding from the player.
        // Start is called before the first frame update
        void Start()
        {
            if (bt == null)
            {
                Debug.LogError("You forgot to assign the behavior tree in the editor!");
            }

            bb = bt.rootNode.GetBlackboard(); //Get the blackboard from the behavior tree
            navMeshAgent = GetComponent<NavMeshAgent>();

            //Reset each blackboard variable when the game starts, since the blackboard values would otherwise persist between play sessions.
			bb.Set<bool>("InPlayerSight", false);
			bb.Set<bool>("IsHidden", false);
			bb.Set<bool>("DoneWaiting", false);

		}

        // Update is called once per frame
        void Update()
        {
            bt.Tick(this.gameObject); //Tick the behavior tree and set scripts attached to this object as the context.

            if(CheckDistance(player) <= 2 ) //Check if the npc is near the player.
            {
                hasSetPath = false;
                textToChange.text = "Fleeing...";
				bb.Set<bool>("InPlayerSight", true); //Set the bool blackboard variable to true.
			}
        }
        /// <summary>
        /// Patrol is triggered by an action node in the behavior tree. It moves the NPC between set points in the game.
        /// Note: There are 0 references, showing that this method is not called anywhere in the code, and is instead called
        /// from the behavior tree.
        /// </summary>
        void Patrol()
        {
            if(!hasSetPath) //Set first destination once.
            {
                hasSetPath = true;
                navMeshAgent.SetDestination(patrolPoints[0].transform.position);
                textToChange.text = "Patroling...";
            }
            else
            {
                if(navMeshAgent.remainingDistance <= 0.1f) //If the npc is close to its destination, move to next point.
                {
                    currentPatrolPoint++;
                    currentPatrolPoint %= patrolPoints.Count;
                    navMeshAgent.SetDestination(patrolPoints[currentPatrolPoint].transform.position);
                }
                else 
                {
                    navMeshAgent.Move(Vector3.zero); //Move the npc.
                }
            }
        }
        /// <summary>
        /// Hide is triggered by an action node in the behavior tree. It moves the npc towards the hide point set in the editor.
        /// </summary>
        void Hide()
        {
            if(!hasSetPath) 
            {
                navMeshAgent.SetDestination(hidePoint.transform.position);
                hasSetPath = true;

                //Note: Instead of a hide point, you could use raycasts to find a suitable location out of sight of the player.

            }
            if(navMeshAgent.remainingDistance > 0.1f)
            {
                navMeshAgent.Move(Vector3.zero);
            }
            else
            {
                //Set blackboard variables to show that the npc is now hidden from the player.
                bb.Set<bool>("InPlayerSight", false);
                bb.Set<bool>("IsHidden", true);
                textToChange.text = "Waiting...";
            }
           
        }
        /// <summary>
        /// Wait is triggered by an action node, and makes the npc wait a certain amount of time before continuing on.
        /// </summary>
        void Wait()
        {
            //This shows how you can get the blackboard values you have set and perform operations on them.
            //In the future I may add nodes for comparing blackboard values if there is enough interest in doing that in the editor.
            if(waitedTime >= bb.Get<float>("WaitTime"))
            {
                waitedTime = 0;
                bb.Set<bool>("DoneWaiting", true);
            }
            else
            {
                waitedTime += Time.deltaTime;
            }
        }
        /// <summary>
        /// Stop hiding is triggered by an action node and just resets the blackboard values so that the NPC goes back to patrolling.
        /// </summary>
        void StopHiding()
        {
            hasSetPath = false;
			bb.Set<bool>("IsHidden", false);
			bb.Set<bool>("DoneWaiting", false); 
			textToChange.text = "Resuming Patrol...";
		}

        /// <summary>
        /// Helper function to check the distance between this object and another object.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>Distance between <paramref name="this"/> and <paramref name="other"/></returns>
		private float CheckDistance(GameObject other)
        {
            return Vector3.Distance(this.transform.position, other.transform.position);
        }



	}
}