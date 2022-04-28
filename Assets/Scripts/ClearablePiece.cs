using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearablePiece : MonoBehaviour
{
    private bool isBeingCleared = false;
    public bool IsBeingCleared
    {
        get { return isBeingCleared; }
    }

    public void Clear()
    {
        isBeingCleared = true;
        StartCoroutine(ClearCoroutine());
    }

    private IEnumerator ClearCoroutine()
    {
        Animator animator = GetComponent<Animator>();

        if (animator)
        {
            animator.SetBool("IsDestroy", true);
            float timeOfAnimation = GetComponent<Animator>().runtimeAnimatorController
                .animationClips[0].length;
            yield return new WaitForSeconds(timeOfAnimation);
            Destroy(gameObject);

        }
    }
}
