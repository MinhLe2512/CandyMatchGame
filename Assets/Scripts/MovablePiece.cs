using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovablePiece : MonoBehaviour
{
    private Candy candy;
    private IEnumerator moveCoroutine;

    private void Awake()
    {
        candy = GetComponent<Candy>();
    }
    public void Move(int newX, int newY, float fillTime)
    {
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);


        moveCoroutine = MoveCouroutine(newX, newY, fillTime);
        StartCoroutine(moveCoroutine);
    }


    private IEnumerator MoveCouroutine(int newX, int newY, float time)
    {
        candy.X = newX;
        candy.Y = newY;

        Vector3 startPos = transform.position;
        Vector3 endPos = candy.gridRef.GetWorldPosition(newX, newY);

        //Debug.Log("Move from " + startPos.x + ", " +startPos.y + " to " + newX + ", " + newY);
        for (float t = 0; t <= 1 * time; t+= Time.deltaTime)
        {
            candy.transform.position = Vector3.Lerp(startPos, endPos, t / time);
            yield return 0;
        }

        candy.transform.position = endPos;
        //Grid.instance.IsSwapping = false;
    }

    public void MoveBack(int newX, int newY, float time)
    {
        StartCoroutine(MoveBackCoroutine(newX, newY, time));
        //Grid.instance.IsSwapping = false;
    }

    private IEnumerator MoveBackCoroutine(int newX, int newY, float time)
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = candy.gridRef.GetWorldPosition(newX, newY);

        for (float t = 0; t <= 1; t+= Time.deltaTime)
        {
            candy.transform.position = Vector3.Lerp(startPos, endPos, t / time);
            yield return 0;
        }

        for (float t = 0; t <= 1; t += Time.deltaTime)
        {
            candy.transform.position = Vector3.Lerp(endPos, startPos, t / time);

            yield return 0;
        }
        candy.transform.position = startPos;
    }
}
