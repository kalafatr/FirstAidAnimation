using System.Collections.Generic;
using UnityEngine;

public class CreateAnimationLine : MonoBehaviour
{
    public GameObject prefabToCreate;
    public Transform firstPointTransform;
    public Transform lastPointTransform;
    public PositionHandle positionHandle;
    public string AnimationName;
    public Animator animator;

    [Range(0f, 1f)]
    public float AnimationValue = 0f;

    public Transform AnimationHandle;

    private readonly List<GameObject> spawnedLinePoints = new List<GameObject>();

    private void OnEnable()
    {
        InstantiateObjectWithLine(firstPointTransform, lastPointTransform);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(firstPointTransform.position, 0.25f);
        Gizmos.DrawWireSphere(lastPointTransform.position, 0.25f);
        Gizmos.DrawLine(firstPointTransform.position, lastPointTransform.position);
    }

    private void Update()
    {
        if (positionHandle == null || animator == null || string.IsNullOrEmpty(AnimationName)) return;
        AnimationValue = positionHandle.ratio;
        animator.SetFloat(AnimationName, AnimationValue);
    }

    public void InstantiateObjectWithLine(Transform start, Transform end)
    {
        foreach (GameObject point in spawnedLinePoints)
        {
            if (point != null) Destroy(point);
        }
        spawnedLinePoints.Clear();

        if (prefabToCreate == null) return;

        int pointCount = (int)(Mathf.Max(Mathf.CeilToInt(Vector3.Distance(start.position, end.position)), 1) * 1.5f);
        if (pointCount < 2) return;

        Vector3 direction = (end.position - start.position).normalized;
        Quaternion rotation = Quaternion.LookRotation(direction);

        for (int i = 0; i < pointCount; i++)
        {
            float t = i / (float)(pointCount - 1);
            Vector3 point = Vector3.Lerp(start.position, end.position, t);
            GameObject prefab = Instantiate(prefabToCreate, point, Quaternion.identity);
            prefab.transform.SetParent(transform);
            prefab.transform.rotation = rotation;
            spawnedLinePoints.Add(prefab);
        }
    }
}
