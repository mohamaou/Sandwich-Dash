using System.Collections.Generic;
using UnityEngine;

public class Grade : MonoBehaviour
{
    [SerializeField] private bool showGizmos;
    [SerializeField] [Range(0,14)]private int with, lenght;
    [SerializeField] private LayerMask groundLayer;
    public Head head;
    private List<Vector3> gradePoints = new List<Vector3>();
   
   


    private void Start()
    {
        SetGradePoint();
    }
    public void SetGradePoint()
    {
        gradePoints.Clear();
        for (int x = 0; x < with; x++) 
        { 
            for (int z = 0; z < lenght; z++)
            {
                var point = new Vector3(x + 0.5f - with / 2, 0, z + 0.5f - lenght / 2) + transform.position;
                if(!IsFull(point)) gradePoints.Add(point);
            }
        }
    }
    private bool IsFull(Vector3 point)
    {
        var results = new Collider[10];
        var size = Physics.OverlapSphereNonAlloc(point +Vector3.up * 0.5f, 0.25f, results, groundLayer);
        return size != 0;
    }
    public Vector3 GetClosestPoint(Vector3 point)
    {
        var minDist = 1000f;
        var closestPoint = Vector3.back;
        for (int i = 0; i < gradePoints.Count; i++)
        {
            var dist = Vector3.Distance(point,gradePoints[i]);
            if (dist < minDist)
            {
                closestPoint = gradePoints[i];
                minDist = dist;
            }
        } 
        return new Vector3(closestPoint.x,point.y,closestPoint.z);
    }

    public void RemovePoint(Vector3 point)
    {
        gradePoints.Remove(gradePoints[GetClosestPointIndex(point)]);
    }

    
    private int GetClosestPointIndex(Vector3 point)
    {
        var minDist = 1f;
        var index = 100000;
        for (int i = 0; i < gradePoints.Count; i++)
        {
            var dist = Vector3.Distance(point,gradePoints[i]);
            if (dist < minDist)
            {
                index = i;
                minDist = dist;
            }
        }
        return index;
    }
    private void OnDrawGizmos()
    {
        if (!showGizmos) return; 
        SetGradePoint();
        Gizmos.color = Color.green;
        for (int i = 0; i < gradePoints.Count; i++)
        {
            Gizmos.DrawSphere(gradePoints[i],0.3f);
        }
    }
}
