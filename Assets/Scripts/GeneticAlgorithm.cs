
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GeneticAlgorithm : MonoBehaviour
{
    private List<GeneticAgent> agents;
    [SerializeField, Min(4)]
    private int populationSize = 100;
    [SerializeField, Min(1)]
    private int epochsNum = 10;
    [SerializeField, Min(4)]
    private int sheepInitPopulationSize = 10;
    [SerializeField]
    private GameObject wolfPrefab;
    [SerializeField]
    private GridManager gridManager;
    [SerializeField]
    private float mutationChance = 0.1f;
    [SerializeField]
    private float actionTime = 5f;
    [SerializeField]
    private int numberActions = 5;
    private List<Sheep> sheeps = new();
    [SerializeField]
    private GameObject sheepPrefab;
    [SerializeField]
    private Transform wolvesParent;
    [SerializeField]
    private Transform sheepsParent;

    private void Start()
    {
        gridManager.AddPositions();
        InitializePopulation();
        StartCoroutine(StartEpochs());
    }
    #region ManagingWolves
    private void InitializePopulation()
    {
        agents = new List<GeneticAgent>();
        for (int i = 0; i < populationSize; i++)
        {
            GameObject wolf = Instantiate(wolfPrefab, wolvesParent);
            GeneticAgent agent = wolf.GetComponent<GeneticAgent>();
            wolf.transform.position = gridManager.SetPosition();
            agent.RandomizeGenes();
            agent.EvaluateFitness();
            agents.Add(agent);
            gridManager.AddObjectToGrid(wolf.GetComponent<AIAgent>());
        }
        SpawnSheeps();
        wolfPrefab.GetComponent<Renderer>().enabled = false;
    }
    private void SpawnSheeps()
    {
        for (int i = sheeps.Count; i < sheepInitPopulationSize; i++)
        {
            CreateSheepOnGrid();
        }
    }
    public void CreateSheepOnGrid()
    {
        Vector2Int randomPosition = gridManager.GetRandomAvailablePosition();
        if (randomPosition != Vector2Int.zero)
        {
            gridManager.GetPositions().Remove(randomPosition);
            sheepPrefab.SetActive(false);
            GameObject sheep = Instantiate(sheepPrefab, sheepsParent);
            sheep.SetActive(true);
            sheep.transform.position = new Vector3(randomPosition.x, 0, randomPosition.y);
            Sheep sheepComponent = sheep.GetComponent<Sheep>();
            sheeps.Add(sheepComponent);
            gridManager.GetOccupiedPositions().Add(randomPosition);
            gridManager.AddObjectToGrid(sheepComponent);
        }
    }
    private void SelectParents(List<GeneticAgent> parents)
    {
        agents = agents.OrderBy(a => Random.value).ToList();
        agents.Sort((a, b) => b.Fitness.CompareTo(a.Fitness));

        int bestHalfAgentsCount = agents.Count / 2;
        for (int i = agents.Count - 1; i >= bestHalfAgentsCount; i--)
        {
            RemoveWolf(i);
        }
        parents.AddRange(agents);
    }

    private GeneticAgent CreateChild(GeneticAgent firstParent, GeneticAgent secondParent)
    {
        GameObject firstWolf = Instantiate(firstParent.gameObject, wolvesParent);
        GeneticAgent childWolf = firstWolf.GetComponent<GeneticAgent>();
        childWolf.InheritGenes(firstParent, secondParent);
        childWolf.MutateGenes();
        childWolf.EvaluateFitness();
        CalculateEnergyForChildren(firstParent, secondParent, childWolf);
        Vector2Int randomGridPos = gridManager.GetRandomAvailablePosition();
        gridManager.GetPositions().Remove(randomGridPos);
        gridManager.GetOccupiedPositions().Add(randomGridPos);

        Wolf wolf = childWolf.GetComponent<Wolf>();
        Vector2Int nearestFreePos = gridManager.FindNearestFreePosition(randomGridPos, wolf);

        firstWolf.transform.position = gridManager.Grid.CellToWorld(new Vector3Int(nearestFreePos.x, 0, nearestFreePos.y));
        gridManager.GetPositions().Remove(nearestFreePos);
        gridManager.GetOccupiedPositions().Add(nearestFreePos);
        gridManager.AddObjectToGrid(wolf);
        return childWolf;
    }
    private GeneticAgent SelectParentFromPool(List<GeneticAgent> selectedParents)
    {
        int randomIndex = Random.Range(0, selectedParents.Count);
        GeneticAgent selectedParent = selectedParents[randomIndex];
        selectedParents.RemoveAt(randomIndex);
        return selectedParent;
    }
    
    private void CalculateEnergyForChildren(GeneticAgent firstParent, GeneticAgent secondParent, GeneticAgent childWolf)
    {
        int energyFromFirstParent = firstParent.GetPercentOfEnergryFromParent();
        int energyFromSecondParent = secondParent.GetPercentOfEnergryFromParent();
        int childEnergy = (energyFromFirstParent + energyFromSecondParent)/2;
        energyFromFirstParent -= childEnergy;
        energyFromSecondParent -= childEnergy;
        childWolf.Fitness = childEnergy;
    }
    private void RemoveWolfThroughFitness()
    {
        for (int i = agents.Count - 1; i >= 0; i--)
        {
            if (!agents[i].IsWolfFitnessPositive())
            {
                RemoveWolf(i);
            }
        }
    }
    private void RemoveWolf(int index)
    {
        Vector3 worldPosition = agents[index].transform.position;
        Vector3Int gridPosition = gridManager.Grid.WorldToCell(worldPosition);
        Vector2Int position = new(gridPosition.x, gridPosition.z);
        Destroy(agents[index].gameObject);
        gridManager.GetPositions().Add(position);
        gridManager.GetOccupiedPositions().Remove(position);
        agents.RemoveAt(index);
    }
    public void RemoveSheep(Sheep sheep)
    {
        Vector3Int gridPos = gridManager.Grid.WorldToCell(sheep.transform.position);
        Vector2Int position = new(gridPos.x, gridPos.z);
        gridManager.GetOccupiedPositions().Remove(position);
        gridManager.GetPositions().Add(position);
        gridManager.RemoveObjectFromGrid(sheep);
        sheeps.Remove(sheep);
        Destroy(sheep.gameObject);
    }

    private void StartWolfAction(float actionTime)
    {
        SortAgentsByMovementOrder(agents);
        foreach (GeneticAgent agent in agents)
        {

            agent.GetComponent<Wolf>().StartAction(actionTime);
        }
    }
    private void SortAgentsByMovementOrder(List<GeneticAgent> agents)
    {
        agents.Sort((a, b) => b.GetGene("Movement Order").CurrentValue.CompareTo(a.GetGene("Movement Order").CurrentValue));
    }
    #endregion

    #region Epochs
    private IEnumerator StartEpochs()
    {
        while (true)
        {
            for (int i = 0; i < numberActions; i++)
            {
                StartWolfAction(actionTime);
                yield return new WaitForSeconds(actionTime);
                RemoveWolfThroughFitness();
                yield return new WaitForSeconds(3f);
            }
            NextEpoch();
        }
    }
    private void WolvesFitness()
    {
        foreach (GeneticAgent agent in agents)
        {
            agent.EvaluateFitness();
        }
    }
    private void Crossover()
    {
        List<GeneticAgent> geneticAgents = new();
        List<GeneticAgent> newGeneration = geneticAgents;
        List<GeneticAgent> parents = new();
        SelectParents(parents);
        int childrenToCreate = parents.Count / 2;
        for (int i = 0; i < childrenToCreate; i++)
        {
            GeneticAgent parent1 = SelectParentFromPool(parents);
            GeneticAgent parent2 = SelectParentFromPool(parents);

            GeneticAgent firstChild = CreateChild(parent1, parent2);
            GeneticAgent secondChild = CreateChild(parent1, parent2);
            newGeneration.Add(firstChild);
            newGeneration.Add(secondChild);
        }
        agents.AddRange(newGeneration);
    }
    private void Mutate()
    {
        int counter = 0;
        foreach (GeneticAgent agent in agents)
        {
            if(Random.Range(0f, 1f) < mutationChance)
            {
                agent.MutateGenes();
                counter++;
            }
        }
    }
    private void ResetAttackCounterForWolf()
    {
        foreach(GeneticAgent agent in agents)
        {
            Wolf wolf = agent.GetComponent<Wolf>();
            wolf.ResetAttackCounter();
        }
    }
    private void NextEpoch()
    {
        gridManager.GetReservedSheeps().Clear();
        gridManager.GetReservedCells().Clear();
        ResetAttackCounterForWolf();
        if(epochsNum >= 0)
        {
            WolvesFitness();
            SpawnSheeps();
            Crossover();
            Mutate();
            epochsNum--;
        }
        else
        {
            Time.timeScale = 0f;
            Application.Quit();
        }
    }
    #endregion

    #region Comparator
    public class Vector2IntComparator : IComparer<Vector2Int>
    {
        public int Compare(Vector2Int a, Vector2Int b)
        {
            if (a.x == b.x)
            {
                return a.y.CompareTo(b.y);
            }
            return a.x.CompareTo(b.x);
        }
    }
    #endregion
}
