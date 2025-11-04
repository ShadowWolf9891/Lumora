using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Static class for handling events related to NPC's in the game. Load must be called in awake to subscribe properly.
/// </summary>
public static class NPCManager
{
    private static Dictionary<string, GameObject> _npcCache = new();

    private static bool _isLoaded = false;

    public static void Load()
    {
        _npcCache.Clear();
        GameEvents<NPCMovementEvent>.Subscribe(MoveNPC);
        Debug.Log("Loaded NPCManager");
        _isLoaded = true;
    }

    public static void MoveNPC(NPCMovementEvent e)
    {
        if (!_isLoaded) { Load(); }

        if (!_npcCache.TryGetValue(e.NPCToMove, out GameObject npc))
        {
            npc = GameObject.Find(e.NPCToMove);
            if(npc != null)
            {
                _npcCache[e.NPCToMove] = npc;
            }
            else
            {
                Debug.LogWarning($"Cannot find {e.NPCToMove} in the scene! Check spelling and visibility.");
                return;
            }
        }

        if(_npcCache[e.NPCToMove].TryGetComponent(out NavMeshAgent agent))
        {
            Vector3 targetLoc = _npcCache[e.NPCToMove].transform.position + e.TargetLocation;
            agent.SetDestination(targetLoc);
        }
	}


}
