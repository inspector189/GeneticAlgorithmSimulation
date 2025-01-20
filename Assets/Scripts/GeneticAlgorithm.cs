using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneticAlgorithm : MonoBehaviour
{
    private List<GeneticAgent> agents;
    [SerializeField, Min(4)]
    private int populationSize = 100;
    [SerializeField, Min(1)]
    private int epochsNum = 10;
    [SerializeField]
    private GameObject wolfPrefab;
    [SerializeField]
    private GridManager gridManager;
    private float timer = 0f;
    [SerializeField]
    private float mutationChance = 0.1f;
    [SerializeField]
    private float actionTime = 5f;
    [SerializeField]
    private int numberActions = 5;
    private SortedSet<Vector2Int> positions = new SortedSet<Vector2Int>(new Vector2IntComparator()); 

    private void Start()
    {
        AddPositions();
        InitializePopulation();
        StartCoroutine(StartEpochs());
    }
    private IEnumerator StartEpochs()
    {
        while (true)
        {
            for(int i = 0; i < numberActions; i++)
            {
                Debug.Log("Numer wykonywanej akcji: " + i);
                StartWolfAction(actionTime);
                yield return new WaitForSeconds(actionTime);
            }
            NextEpoch();
            Debug.Log("Następna epoka liczba agentów: " + agents.Count);
        }
    }
    private void InitializePopulation()
    {
        agents = new List<GeneticAgent>();  
        for(int i = 0; i < populationSize; i++)
        {           
            GameObject wolf = Instantiate(wolfPrefab);
            GeneticAgent agent = wolf.GetComponent<GeneticAgent>();
            wolf.transform.position = SetPosition();
            agent.RandomizeGenes();           
            agent.EvaluateFitness();
            agents.Add(agent);
        }
    }
    private void AddPositions()
    {
        for (int x = 0; x < 10; x++)
        {
            for (int y = 0; y < 10; y++)
            {
                positions.Add(new Vector2Int(x, y));
            }
        }
    }
    private Vector3 SetPosition()
    {
        int index = Random.Range(0, positions.Count);
        foreach (Vector2Int position in positions)
        {
            if (index == 0)
            {
                positions.Remove(position);
                return gridManager.Grid.CellToWorld(new Vector3Int(position.x, 0, position.y));
            }
            index--;
        }
        return default;
    }
    private void WolvesFitness()
    {
        foreach (GeneticAgent agent in agents)
        {
            agent.EvaluateFitness();
        }
    }
    private void SelectParents(List<GeneticAgent> parents)
    {
        agents.Sort((a, b) => b.Fitness.CompareTo(a.Fitness));

        int bestHalfAgentsCount = agents.Count / 2;
        for (int i = bestHalfAgentsCount; i < agents.Count; i++)
        {
            RemoveWolf(i);
        }
        parents.AddRange(agents);
    }
    private void Crossover()
    {
        Debug.Log("CrossOver rozpoczety!");
        List<GeneticAgent> newGeneration = new List<GeneticAgent>();
        List<GeneticAgent> parents = new List<GeneticAgent>();
        SelectParents(parents);
        int childrenToCreate = agents.Count / 2;
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
    private GeneticAgent CreateChild(GeneticAgent firstParent, GeneticAgent secondParent)
    {
        GameObject firstWolf = Instantiate(wolfPrefab);
        GeneticAgent childWolf = firstWolf.GetComponent<GeneticAgent>();
        childWolf.InheritGenes(firstParent, secondParent);
        childWolf.MutateGenes();
        childWolf.EvaluateFitness();
        firstWolf.transform.position = SetPosition();
        return childWolf;
    }
    private GeneticAgent SelectParentFromPool(List<GeneticAgent> selectedParents)
    {
        int randomIndex = Random.Range(0, selectedParents.Count);
        GeneticAgent selectedParent = selectedParents[randomIndex];
        selectedParents.RemoveAt(randomIndex);
        return selectedParent;
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
        Debug.Log("Liczba zmutowanych osobników: " + counter);
    }
    private void RemoveWolf(int index)
    {      
        Vector3 worldPosition = agents[index].transform.position;
        Vector3Int gridPosition = gridManager.Grid.WorldToCell(worldPosition);
        Vector2Int position = new Vector2Int(gridPosition.x, gridPosition.z);
        Destroy(agents[index].gameObject);
        agents.RemoveAt(index);
        positions.Add(position);       
    }
    private void NextEpoch()
    {
        WolvesFitness();
        Crossover();
        Mutate();
        epochsNum--;
    }
    private void StartWolfAction(float actionTime)
    {
        foreach(GeneticAgent agent in agents)
        {
           agent.GetComponent<Wolf>().StartAction(actionTime);
        }
    }
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
}

/*
 1. Wilk moze isc w miejsce gdzie ma movementRange - szukamy zakresu np do 2 (czyli 1, 2)
 2. Movemnt Order - jezeli dane pole jest wolne i dwa wilki chca na nie sie dostac to sprawdzamy ktory ma wiekszy ten movementOrder
 3. UI  - przyspieszenie czasu
 4. Bug - liczba agentow sie chb nie zgadza
 5. ten prefab
 */