using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationLineManager : MonoBehaviour
{
    public List<CreateAnimationLine> createAnimationLines;
    public Animator animator;
    public GameObject[] silinecekler;

    private void Start()
    {
        for (int i = 1; i < createAnimationLines.Count; i++)
        {
            createAnimationLines[i].gameObject.SetActive(false);
        }
    }

    public bool nextline()
    {
        if (createAnimationLines.Count != 0) {
            if (createAnimationLines[0].AnimationValue >= 1)
            {
                animator.SetFloat("multiplier", -1);
                return true;
            }
            else { return false; }
        }
        else { return false; }

    }

    void Update()
    {
        if (nextline()) {
            Destroy(createAnimationLines[0].prefabToCreate.gameObject);
            Destroy(createAnimationLines[0].lastPointTransform.gameObject);
            Destroy(createAnimationLines[0].firstPointTransform.gameObject);
            Destroy(createAnimationLines[0].positionHandle.gameObject);
            Destroy(createAnimationLines[0].gameObject);
            createAnimationLines.RemoveAt(0);
            animator.SetFloat("multiplier", 1);

            if (0 < createAnimationLines.Count)
            {
                createAnimationLines[0].gameObject.SetActive(true);
                animator.SetTrigger("Gec");
            }
        }
    }
}
