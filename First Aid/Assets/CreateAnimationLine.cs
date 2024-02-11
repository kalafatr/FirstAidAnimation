using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class CreateAnimationLine : MonoBehaviour
{
    [SerializeField] GameObject prefabToCreate;
    [SerializeField] Transform firstPointTransform;
    [SerializeField] Transform middlePointTransform;
    [SerializeField] Transform lastPointTransform;

    [SerializeField] Transform leftHand;
    [SerializeField] Transform rightHand;
    [SerializeField] Transform nowrandomhandle;

    [SerializeField] private InputActionProperty leftGrip;
    [SerializeField] private InputActionProperty rightGrip;
    public string AnimationName;
    public Animator animator;
    [SerializeField] bool animatorTrigger = false;
    public float proximity;

    [SerializeField] bool averageX;
    [SerializeField] bool averageY;
    [SerializeField] bool averageZ;


    [Range(0f, 1f)]
    public float AnimationValue = 0f;

    Transform AnimationHandle()
    {
        if (rightHand != null && Vector3.Distance(firstPointTransform.position, rightHand.position)<2 && rightGrip.action.ReadValue<float>() == 1)
        {
            return rightHand;
        }
        if(leftHand != null && Vector3.Distance(firstPointTransform.position, leftHand.position) < 2 && leftGrip.action.ReadValue<float>() == 1)
        {
            return leftHand;
        }
        return nowrandomhandle;
    }
    private void OnEnable()
    {
        if (averageX)
        {
            middlePointTransform.position = new Vector3((firstPointTransform.position.x + lastPointTransform.position.x) / 2, middlePointTransform.position.y, middlePointTransform.position.z);
        }
        if (averageY)
        {
            middlePointTransform.position = new Vector3(middlePointTransform.position.x, (firstPointTransform.position.y + lastPointTransform.position.y) / 2, middlePointTransform.position.z);
        }
        if (averageZ)
        {
            middlePointTransform.position = new Vector3(middlePointTransform.position.x, middlePointTransform.position.y, (firstPointTransform.position.z + lastPointTransform.position.z) / 2);
        }
        InstantiateObjectWithArc(firstPointTransform.position, middlePointTransform.position, lastPointTransform.position);
        
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(firstPointTransform.position, 0.5f);
        Gizmos.DrawWireSphere(lastPointTransform.position, 0.5f);
        Gizmos.DrawWireSphere(middlePointTransform.position, 0.25f);
        Gizmos.DrawLine(firstPointTransform.position, middlePointTransform.position);
        Gizmos.DrawLine(middlePointTransform.position, lastPointTransform.position);
    }

    private void Update()
    {
        //if (AnimationValue == 1) return;
        float distanceToFirstPoint = Vector3.Distance(AnimationHandle().position, firstPointTransform.position);
        float distanceToMiddlePoint = Vector3.Distance(AnimationHandle().position, middlePointTransform.position);
        float distanceToLastPoint = Vector3.Distance(AnimationHandle().position, lastPointTransform.position);

        if (distanceToFirstPoint <= proximity || distanceToMiddlePoint <= proximity || distanceToLastPoint <= proximity)
        {
            float totalDistanceFirstHalf = Vector3.Distance(firstPointTransform.position, middlePointTransform.position);
            float totalDistanceSecondHalf = Vector3.Distance(middlePointTransform.position, lastPointTransform.position);

            if (distanceToFirstPoint <= distanceToLastPoint)
            {
                float ratio = distanceToFirstPoint / totalDistanceFirstHalf;
                AnimationValue = ratio / 2;
            }
            else
            {
                float ratio = distanceToMiddlePoint / totalDistanceSecondHalf;
                AnimationValue = 0.5f + ratio / 2;
            }

            animator.SetFloat(AnimationName, AnimationValue);
            Debug.Log(AnimationValue);
        }
    }

    public void InstantiateObjectWithArc(Vector3 start, Vector3 middle, Vector3 end)
    {
        float distance1 = Vector3.Distance(start, middle);
        float distance2 = Vector3.Distance(middle, end);
        int pointCount = Mathf.CeilToInt(distance1)+ Mathf.CeilToInt(distance2);

        for (int i = 0; i < pointCount; i++)
        {
            float t = i / (float)(pointCount - 1);
            Vector3 point = CalculateArcPoint(start, middle, end, t);
            GameObject prefab = Instantiate(prefabToCreate, point, Quaternion.identity);
            prefab.transform.SetParent(transform);
            if (i < Mathf.CeilToInt(distance1))
            {
                Vector3 direction = (middle - start).normalized;
                prefab.transform.rotation = Quaternion.LookRotation(direction);
            }
            else
            {
                Vector3 direction = (end - middle).normalized;
                prefab.transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }

    private Vector3 CalculateArcPoint(Vector3 start, Vector3 middle, Vector3 end, float t)
    {
        Vector3 point = Vector3.zero;
        float distance1 = Vector3.Distance(start, middle);
        float distance2 = Vector3.Distance(middle, end);

        float totalDistance = distance1 + distance2;
        float normalizedT = t * totalDistance;

        if (normalizedT <= distance1)
        {
            point = Vector3.Lerp(start, middle, t / (distance1 / totalDistance));
        }
        else
        {
            point = Vector3.Lerp(middle, end, (t - distance1 / totalDistance) / (distance2 / totalDistance));
        }

        return point;
    }
}