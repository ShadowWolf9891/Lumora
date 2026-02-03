using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WaypointPath : MonoBehaviour
{
    [HideInInspector]
    public List<Vector3> points = new List<Vector3> ();
    public bool loop = false;

    public Vector3 GetPointWorld(int index)
    {
        return transform.TransformPoint(points[index]);
    }
}