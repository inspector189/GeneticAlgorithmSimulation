using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(GeneticAgent))]
public class Wolf : AIAgent
{
    private GeneticAgent agent;
    private Vector2Int targetPosition;
    private int foodNum;
    private void Awake()
    {
        agent = GetComponent<GeneticAgent>();
        SetFoodNum();
    }
    public int GetFoodNum()
    {
        return foodNum;
    }
    private void SetFoodNum()
    {
        foodNum = agent.Fitness;
    }
    public void SetTargetPosition(Vector2Int position, HashSet<Vector2Int> occupiedPositions)
    {
        occupiedPositions.Remove(targetPosition);
        targetPosition = position;
        occupiedPositions.Add(targetPosition);
    }
    public void StartAction(float timeBudget)
    {
        Move();
        StartCoroutine(MoveOverTime(GetTargetWorldPosition(targetPosition), timeBudget, GetOccupiedPosition()));
    }
    private IEnumerator MoveOverTime(Vector3 targetPosition, float timeBudget, HashSet<Vector2Int> occupiedPositions)
    {
        Vector3 startPosition = transform.position;
        float elapsedTime = 0f;
        Vector2Int previousPosition = new(Mathf.RoundToInt(startPosition.x), Mathf.RoundToInt(startPosition.z));

        while (elapsedTime < timeBudget)
        {
            elapsedTime += Time.deltaTime;
            transform.position = Vector3.Lerp(startPosition, targetPosition, Mathf.SmoothStep(0, 1, elapsedTime / timeBudget));
            yield return null;
        }

        Vector2Int newPosition = new(Mathf.RoundToInt(targetPosition.x), Mathf.RoundToInt(targetPosition.z));

        occupiedPositions.Remove(previousPosition);
        occupiedPositions.Add(newPosition);       
    }
    public void CountFitnessAfterMove()
    {
        int cellsWalkedOver = CountCellsWolfWalkedOver(new(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z)), targetPosition);
        if(cellsWalkedOver > 0)
        {
            foodNum -= cellsWalkedOver;
        }    
    }

    public void CountFitnessAfterAttack()
    {
        int energyEff = agent.GetEnergyEfficiencyValue();
        int percentageIncrease = energyEff / 100;
        foodNum += foodNum * percentageIncrease;
    }
}
