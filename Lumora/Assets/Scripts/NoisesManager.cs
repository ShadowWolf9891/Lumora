using System;
using UnityEngine;

public class NoisesManager : MonoBehaviour
{
    //Noises Manager, to be attached to Noises Manager in tools, spawns in 'noise' objects 

    [SerializeField]
    GameObject genericNoiseObject;
    private void Start()
    {
        GameContext.Instance.OnGenericNoise += RaiseNoise;
    }


    private void RaiseNoise(Vector3 position, float maxSize)
    {
        GameObject newNoise = Instantiate(genericNoiseObject, position, new Quaternion(0, 0, 0, 0));
        newNoise.GetComponent<NoiseBehaviors>().SpawnNoisePing(maxSize);
    }
}
