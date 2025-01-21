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
    [SerializeField]
    private float mutationChance = 0.1f;
    [SerializeField]
    private float actionTime = 5f;
    [SerializeField]
    private int numberActions = 5;
    private SortedSet<Vector2Int> positions = new(new Vector2IntComparator()); 

    private void Start()
    {
        AddPositions();
        InitializePopulation();
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
            wolf.transform.position = SetPosition();
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
        for (int i = bestHalfAgentsCount; i < agents.Count; i++)
        {
            RemoveWolf(i);
        }
        Debug.Log("Usunięto: " + (agents.Count - bestHalfAgentsCount) + " liczbę wilków");
        parents.AddRange(agents);
        foreach(GeneticAgent parent in parents)
        {
            Debug.Log("Parent dodany do listy agentów: " + parent.gameObject.name);
        }
        Debug.Log("Łączna liczba parentów: " + parents.Count);
    }
    private GeneticAgent CreateChild(GeneticAgent firstParent, GeneticAgent secondParent)
    {
        GameObject firstWolf = Instantiate(firstParent.gameObject);
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
    private void RemoveWolf(int index)
    {
        Vector3 worldPosition = agents[index].transform.position;
        Vector3Int gridPosition = gridManager.Grid.WorldToCell(worldPosition);
        Vector2Int position = new(gridPosition.x, gridPosition.z);
        Destroy(agents[index].gameObject);
        agents.RemoveAt(index);
        positions.Add(position);
    }
    private void StartWolfAction(float actionTime)
    {
        foreach (GeneticAgent agent in agents)
        {
            agent.GetComponent<Wolf>().StartAction(actionTime);
        }
    }
    #endregion
    #region PositionsOnTheGrid
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
        int childrenToCreate = agents.Count / 2;
        for (int i = 0; i < childrenToCreate; i++)
        {
            GeneticAgent parent1 = SelectParentFromPool(parents);
            GeneticAgent parent2 = SelectParentFromPool(parents);

            GeneticAgent firstChild = CreateChild(parent1, parent2);
            GeneticAgent secondChild = CreateChild(parent1, parent2);
            newGeneration.Add(firstChild);
            newGeneration.Add(secondChild);
            Debug.Log("Dodano dziecko: " + i + " do nowej generacji: " + firstChild.gameObject.name + " " + secondChild.gameObject.name + " liczba dzieci to utworzenia" + childrenToCreate);
            Debug.Log("Rodzic 1: " + parent1.name + " Rodzic 2: " + parent2.name);
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
        Debug.Log("Liczba zmutowanych osobników: " + counter);
    }

    private void NextEpoch()
    {
        WolvesFitness();
        Crossover();
        Mutate();
        Debug.Log(agents.Count + " Liczba agentów");
        epochsNum--;
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
 2. Movement Order - jezeli dane pole jest wolne i dwa wilki chca na nie sie dostac to sprawdzamy ktory ma wiekszy ten movementOrder
 3. UI  - przyspieszenie czasu
 */