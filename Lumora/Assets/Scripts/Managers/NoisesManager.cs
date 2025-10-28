using System;
using UnityEngine;

public class NoisesManager : MonoBehaviour
{
    //Noises Manager, to be attached to Noises Manager in tools, spawns in 'noise' objects 

    [SerializeField]
    GameObject genericNoiseObject;
    [SerializeField]
    GameObject sprintNoiseObject;
    private void Start()
    {
        GameEvents<SpawnVisibleNoiseEvent>.Subscribe(RaiseNoise);
        //GameContext.Instance.OnGenericNoise += RaiseNoise;
    }


    private void RaiseNoise(SpawnVisibleNoiseEvent e)
    {
        if (e.IsPlayerSpecificNoise)
        {
            GameObject newNoise = Instantiate(sprintNoiseObject, e.Position, new Quaternion(0, 0, 0, 0));
            newNoise.GetComponent<NoiseBehaviors>().SpawnNoisePing(e.MaxSize, true);
        }
        else
        {
            GameObject newNoise = Instantiate(genericNoiseObject, e.Position, new Quaternion(0, 0, 0, 0));
            newNoise.GetComponent<NoiseBehaviors>().SpawnNoisePing(e.MaxSize, false);
        }
    }
}
