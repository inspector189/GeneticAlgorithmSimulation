using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneticAlgorithm : MonoBehaviour
{
    private List<GeneticAgent> agents;
    private List<Sheep> sheeps = new();
    [SerializeField, Min(4)]
    private int populationSize = 100;
    [SerializeField, Min(1)]
    private int epochsNum = 10;
    [SerializeField, Min(4)]
    private int sheepPopulationSize = 10;
    [SerializeField]
    private GameObject wolfPrefab;
    [SerializeField]
    private GameObject sheepPrefab;
    [SerializeField]
    private GridManager gridManager;
    [SerializeField]
    private float mutationChance = 0.1f;
    [SerializeField]
    private float actionTime = 5f;
    [SerializeField]
    private int numberActions = 5;

    private void Start()
    {
        gridManager.AddPositions();
        InitializePopulation();
        SpawnSheeps();
        StartCoroutine(StartEpochs());
    }
    #region ManagingWolves
    private void InitializePopulation()
    {
        agents = new List<GeneticAgent>();
        for (int i = 0; i < populationSize; i++)
        {
            GameObject wolf = Instantiate(wolfPrefab);
            GeneticAgent agent = wolf.GetComponent<GeneticAgent>();
            wolf.transform.position = gridManager.SetPosition();
            agent.RandomizeGenes();
            agent.EvaluateFitness();
            agents.Add(agent);
        }
        wolfPrefab.GetComponent<Renderer>().enabled = false;
    }
    private void SelectParents(List<GeneticAgent> parents)
    {
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
        GameObject firstWolf = Instantiate(firstParent.gameObject);
        GeneticAgent childWolf = firstWolf.GetComponent<GeneticAgent>();
        childWolf.InheritGenes(firstParent, secondParent);
        childWolf.MutateGenes();
        childWolf.EvaluateFitness();

        Vector2Int randomGridPos = gridManager.GetRandomAvailablePosition();
        gridManager.GetPositions().Remove(randomGridPos);
        gridManager.GetOccupiedPositions().Add(randomGridPos);

        Wolf wolf = childWolf.GetComponent<Wolf>();
        Vector2Int nearestFreePos = gridManager.FindNearestFreePosition(randomGridPos, wolf);

        firstWolf.transform.position = gridManager.Grid.CellToWorld(new Vector3Int(nearestFreePos.x, 0, nearestFreePos.y));
        gridManager.GetPositions().Remove(nearestFreePos);
        gridManager.GetOccupiedPositions().Add(nearestFreePos);
        return childWolf;
    }
    private GeneticAgent SelectParentFromPool(List<GeneticAgent> selectedParents)
    {
        int randomIndex = Random.Range(0, selectedParents.Count);
        GeneticAgent selectedParent = selectedParents[randomIndex];
        selectedParents.RemoveAt(randomIndex);
        return selectedParent;
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
    private void NextEpoch()
    {
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
    #region Sheep
    private void CreateSheepOnGrid()
    {
        Vector2Int randomPosition = gridManager.GetRandomAvailablePosition();
        gridManager.GetPositions().Remove(randomPosition);
        GameObject sheep = Instantiate(sheepPrefab);
        sheep.transform.position = new Vector3(randomPosition.x, 0, randomPosition.y);
        sheeps.Add(sheep.GetComponent<Sheep>());
        gridManager.GetOccupiedPositions().Add(randomPosition);
    }
    private void SpawnSheeps()
    {
        for(int i = sheeps.Count; i < sheepPopulationSize; i++)
        {
            CreateSheepOnGrid();
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


/*
    - wilki przeszukuja w swoim ViewRange czy jest owca jakas do której moga doskoczyc
        - jezeli tak doskakuja do niej i ja zjadaja -> wzrasta nam Fitness i maleje z racji ze zrobily ruch o x kratek
        - jezeli nie znajduja to sobie chodza w random miejscach 
 */