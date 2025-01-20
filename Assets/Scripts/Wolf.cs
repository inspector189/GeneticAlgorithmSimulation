using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wolf : AIAgent
{
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.W))
        {
            Move(new Vector2Int(0, 1));
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            Move(new Vector2Int(0, -1));
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            Move(new Vector2Int(-1, 0));
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            Move(new Vector2Int(1, 0));
        }
    }
    public void StartAction(float timeBudget)
    {
        List<Vector2Int> enableMoves = new List<Vector2Int>
        { 
            new Vector2Int(0, 1), 
            new Vector2Int(0, -1), 
            new Vector2Int(-1, 0), 
            new Vector2Int(1, 0) 
        };

        System.Random random = new System.Random();
        int selectedIndex = random.Next(enableMoves.Count);
        Vector2Int selectedMove = enableMoves[selectedIndex];
        Vector3 targetPosition = transform.position + new Vector3(selectedMove.x, 0, selectedMove.y);
        StartCoroutine(MoveOverTime(targetPosition, timeBudget));
    }
    private IEnumerator MoveOverTime(Vector3 targetPosition, float timeBudget)
    {
        Vector3 startPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        float elapsedTime = 0f;
        while (elapsedTime < timeBudget)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / timeBudget;
            float easedT = Mathf.SmoothStep(0, 1, t);
            transform.position = Vector3.Lerp(startPosition, targetPosition, easedT);
            yield return null;             
        }
        bool isTrue = startPosition.Equals(transform.position);
    }
}
