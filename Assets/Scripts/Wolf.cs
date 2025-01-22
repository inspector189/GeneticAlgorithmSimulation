using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(GeneticAgent))]
public class Wolf : AIAgent
{
    private GeneticAgent agent;
    private Vector2Int targetPosition;

    private void Awake()
    {
        agent = GetComponent<GeneticAgent>();
    }
     public Vector2Int GetTargetPosition()
    {
        return targetPosition;
    }
    public void StartAction(float timeBudget)
    {
        List<Vector2Int> enableMoves = new()
        { 
            new Vector2Int(0, agent.GetMovementRangeValue()), 
            new Vector2Int(0, -agent.GetMovementRangeValue()), 
            new Vector2Int(-agent.GetMovementRangeValue(), 0), 
            new Vector2Int(agent.GetMovementRangeValue(), 0) 
        };
        
           System.Random random = new();
           int selectedIndex = random.Next(enableMoves.Count);
           Vector2Int selectedMove = enableMoves[selectedIndex];
           Vector3 targetPosition = transform.position + new Vector3(selectedMove.x, 0, selectedMove.y);
           StartCoroutine(MoveOverTime(targetPosition, timeBudget));
    }
    private IEnumerator MoveOverTime(Vector3 targetPosition, float timeBudget)
    {
        Vector3 startPosition = new(transform.position.x, transform.position.y, transform.position.z);
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
