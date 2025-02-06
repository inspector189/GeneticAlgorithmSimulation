using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(GeneticAgent))]
public class Wolf : AIAgent
{
    private GeneticAgent agent;
    private Vector2Int targetPosition;
    private int foodNum;
    private GeneticAlgorithm algorithm;
    private int attacksThisEpoch = 0;
    private Color wolfColor;
    private void Start()
    {
        wolfColor = CalculateColor();
        Renderer wolfRenderer = GetComponent<Renderer>();
        wolfRenderer.material.color = wolfColor;
    }
    private void Awake()
    {
        algorithm = FindObjectOfType<GeneticAlgorithm>();
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
        if (cellsWalkedOver > 0)
        {
            foodNum = Mathf.Max(foodNum - cellsWalkedOver, 0);
        }
    }

    public void CountFitnessAfterAttack()
    {
        foodNum += 50;
    }
    public Color GetWolfColor()
    {
        return wolfColor;
    }
    public void AttackSheep(Vector2Int wolfPosition)
    {
        if (!CanAttack())
            return;
        if (!gridManager.IsSheepCaught(GetComponent<Wolf>())) return;

        List<Sheep> allSheep = new List<Sheep>(FindObjectsOfType<Sheep>());

        foreach (Sheep sheep in allSheep)
        {
            Vector2Int sheepPosition = new(gridManager.Grid.WorldToCell(sheep.transform.position).x,
                                           gridManager.Grid.WorldToCell(sheep.transform.position).z);

            if (sheepPosition == wolfPosition)
            {
                sheep.IsUnderAttack = true;
                algorithm.RemoveSheep(sheep);
                GetComponent<Wolf>().CountFitnessAfterAttack();
                attacksThisEpoch++;
                break;
            }
            sheep.IsUnderAttack = false;
        }
    }
    public bool CanAttack()
    {
        return attacksThisEpoch < agent.GetFrequencyAggressive();
    }
    public void ResetAttackCounter()
    {
        attacksThisEpoch = 0;
    }
    private float Lerp(float a, float b, float t)
    {
        return (1 - t) * a + t * b;
    }
    private int getFValue(float genMin, float genMax, float genValue)
    {
        float t = (genValue - genMin) / (genMax - genMin);

        return(int)(Lerp(0, 16, t));
    }
    private Color CalculateColor()
    {
        int[] colorArray = new int[6];
        int index = 0;
        int color = 0;
        foreach(Gene gene in agent.GetGenes())
        {
            float minValue = gene.getMin();
            float maxValue = gene.getMax();
            float currentValue = gene.CurrentValue;
            colorArray[index] = getFValue(minValue, maxValue, currentValue);
            color += (int)(colorArray[index] * Mathf.Pow(16, index));
            index++;
        }
        float r = ((color >> 16) & 0xFF)/ 255f;
        float g = ((color >> 8) & 0xFF) / 255f;
        float b = (color & 0xFF) / 255f;
        return new Color(r, g, b);
    }
}

/* def lerp(a: float, b: float, t: float) -> float:
    """Linear interpolate on the scale given by a to b, using t as the point on that scale.
    Examples
    --------
        50 == lerp(0, 100, 0.5)
        4.2 == lerp(1, 5, 0.8)
    """
    return (1 - t) * a + t * b
    
def getFValue(genMin : float, genMax : float, genValue: float) -> int:
    t = (genValue - genMin) / (genMax - genMin)
    return int(lerp(0, 16, t))*/
