using UnityEngine;

public class PositionHandle : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;
    public Vector3[] positions;
    private int closestPointIndex;
    public float ratio;

    void OnEnable()
    {
        transform.position = startPoint.localPosition;
        positions = CalculatePositionsOnLine(startPoint.position, endPoint.position, numberOfPoints());
    }

    void Update()
    {
        Grabbing();
    }

    void Grabbing()
    {
        closestPointIndex = FindClosestPointIndex(transform.position);
        transform.position = positions[closestPointIndex];
        transform.rotation = Quaternion.identity;

        float distanceToFirstPoint = Vector3.Distance(transform.position, startPoint.position);
        float totalDistance = Vector3.Distance(startPoint.position, endPoint.position);
        ratio = totalDistance > 0f ? Mathf.Clamp01(distanceToFirstPoint / totalDistance) : 0f;
    }

    Vector3[] CalculatePositionsOnLine(Vector3 start, Vector3 end, int numPoints)
    {
        Vector3[] linePositions = new Vector3[numPoints];
        for (int i = 0; i < numPoints; i++)
        {
            float t = i / (float)(numPoints - 1);
            linePositions[i] = Vector3.Lerp(start, end, t);
        }
        return linePositions;
    }

    int numberOfPoints()
    {
        return (int)((Mathf.Max(Vector3.Distance(startPoint.position, endPoint.position), 1)) * 100);
    }

    int FindClosestPointIndex(Vector3 targetPosition)
    {
        int closestIndex = 0;
        float closestDistance = Mathf.Infinity;
        for (int i = 0; i < positions.Length; i++)
        {
            float distance = Vector3.Distance(targetPosition, positions[i]);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }
        return closestIndex;
    }
}
