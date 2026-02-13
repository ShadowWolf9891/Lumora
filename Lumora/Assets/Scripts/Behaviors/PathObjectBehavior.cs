using System.Collections.Generic;
using UnityEngine;

public class PathObjectBehavior : MonoBehaviour
{
    [SerializeField] List<GameObject> pathObjects = new List<GameObject>();

    private List<WaypointPath> paths = new List<WaypointPath>();
    int currentPath = 0;
    int currentPoint = 0;
    void LoadPath()
    {
        for(int i = 0; i < pathObjects.Count; i++) 
        {
            paths.Add(pathObjects[i].GetComponent<WaypointPath>());
        }
    }
    public bool HasPath() => paths.Count > 0;
    /// <summary>
    /// Go back to the start of the first path in the list.
    /// </summary>
    /// <returns>The position of the first point of the first path.</returns>
    public Vector3 RestartPath()
    {
        if(paths.Count == 0) LoadPath();
        currentPath = 0;
        currentPoint = 0;
        ErrorCheck();
		return paths[0].GetPointWorld(0);
	}
    /// <summary>
    /// Gets the next point along the current path.
    /// </summary>
    /// <returns>The world location of the next point.</returns>
    public Vector3 GetNextPoint()
    {
		if (paths.Count == 0) LoadPath();
		ErrorCheck();
        if(currentPoint >= paths[currentPath].points.Count - 1)
        {
			if (paths[currentPath].loop)  currentPoint = 0;
			return paths[currentPath].GetPointWorld(currentPoint);
		}
        currentPoint++;
        return paths[currentPath].GetPointWorld(currentPoint);
	}

    /// <summary>
    /// Check if the object has finished moving to the end of the path or not.
    /// </summary>
    /// <param name="currentLocation"> The current location of the object</param>
    /// <returns>If the object is within 0.05 units of the last point on the path.</returns>
    public bool IsDonePath(Vector3 currentLocation, float threshold = 1f)
    {
		if (paths.Count == 0) LoadPath();
		if (currentPoint < paths[currentPath].points.Count - 1 || paths[currentPath].loop) return false;
        
        return IsAtPoint(currentLocation, threshold);
    }
    /// <summary>
    /// Check if the location is close to a point within a threshold.
    /// </summary>
    /// <param name="currentLocation">Location of the object to check</param>
    /// <param name="threshold">Tolerence for how far away it can be. Fairly big in case it is high off the ground.</param>
    /// <returns>True if object is at the point.</returns>
    public bool IsAtPoint(Vector3 currentLocation, float threshold = 1f) 
    {
		if (paths.Count == 0) LoadPath();
		return Mathf.Abs((currentLocation - paths[currentPath].GetPointWorld(currentPoint)).magnitude) < threshold;
	}
    /// <summary>
    /// Gets the current path and point the attached object should be at, or moving towards.
    /// </summary>
    /// <returns>The world position of the point on a path.</returns>
    public (int,int) GetCurrentPathAndPoint()
    {
        return (currentPath,currentPoint);
    }
	/// <summary>
	/// Go to the next path in the list. Will throw an error if invalid.
	/// </summary>
	/// <returns>First point along the path</returns>
	public Vector3 GoToNextPath()
    {
		if (paths.Count == 0) LoadPath();
        if (paths.Count > currentPath + 1) currentPath++;
        return GoToPath(currentPath);
    }
	/// <summary>
	/// Go to previous path in the list. Will throw an error if invalid.
	/// </summary>
	/// <returns>First point along the path</returns>
	public Vector3 GoToPreviousPath()
	{
		if (paths.Count == 0) LoadPath();
		if (currentPath > 0) currentPath--;
		return GoToPath(currentPath);
	}
	/// <summary>
	/// Gets the location of a point along a path and make that the current target location. Default pointIndex is first point.
	/// </summary>
	/// <param name="pathIndex">Which path from the list to go to. Can be same as current.</param>
	/// <param name="pointIndex">Which point along the path to move to.</param>
	/// <returns>World location of the point in the path.</returns>
	public Vector3 GoToPath(int pathIndex, int pointIndex = 0)
    {
		if (paths.Count == 0) LoadPath();
		currentPath = pathIndex;
        currentPoint = pointIndex;
        ErrorCheck();
        return paths[currentPath].GetPointWorld(currentPoint);
	}
    /// <summary>
    /// Check if path and points are valid for debugging. Doesn't stop code.
    /// </summary>
    private void ErrorCheck()
    {
		if (paths[currentPath] == null) Debug.LogError($"Invalid path at index {currentPath} of pathObjects list. Assign in inspector.");
		if (paths[currentPath].points[currentPoint] == null) Debug.LogError($"Invalid point at index {currentPoint} of path {currentPath} list.");
	}
}
