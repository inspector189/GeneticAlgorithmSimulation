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
    private float mutationChance = 0.1f;
    private GridManager gridManager;
    private float timer = 0f;
    private float interval = 10f;
    private SortedSet<Vector2Int> positions = new SortedSet<Vector2Int>(new Vector2IntComparer()); 
    private int index = 0;

    private void Start()
    {
        gridManager = Object.FindFirstObjectByType<GridManager>();
        AddPositions();
        InitializePopulation();
    }
    private void Update()
    {
        timer += Time.deltaTime;
        if(timer >= interval && epochsNum > 0)
        {
            NextEpoch();
            timer = 0f;
        }
    }
    private void InitializePopulation()
    {
        agents = new List<GeneticAgent>();  
        for(int i = 0; i < populationSize; i++)
        {           
            GameObject wolf = Instantiate(wolfPrefab);
            GeneticAgent agent = wolf.GetComponent<GeneticAgent>();
            wolf.transform.position = SetPosition(wolf);
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
    private Vector3 SetPosition(GameObject wolf)
    {
        Vector2Int position = GetRandomPosition();
        positions.Remove(position);
        return gridManager.Grid.CellToWorld(new Vector3Int(position.x, 0, position.y));
    }
    private Vector2Int GetRandomPosition()
    {
        int index = Random.Range(0, positions.Count);
        foreach(Vector2Int position in positions)
        {           
            if (index-- == 0)
            {
                return position;
            }
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
    private List<GeneticAgent> SelectParents()
    {
        agents.Sort((a, b) => b.Fitness.CompareTo(a.Fitness));
        int bestHalfAgentsCount = agents.Count / 2;
        RemoveWolves(bestHalfAgentsCount);
        return new List<GeneticAgent>(agents);
    }
    private void Crossover()
    {
        List<GeneticAgent> newGeneration = new List<GeneticAgent>();
        List<GeneticAgent> selectedParents = SelectParents();
        int childrenToCreate = selectedParents.Count / 2;
        for (int i = 0; i < childrenToCreate; i++)
        {
            if (selectedParents.Count < 2) break;
            GeneticAgent parent1 = SelectParentFromPool(selectedParents);
            GeneticAgent parent2 = SelectParentFromPool(selectedParents);

            GeneticAgent firstChild = CreateChild(parent1, parent2);
            GeneticAgent secondChild = CreateChild(parent1, parent2);
            newGeneration.Add(firstChild);
            newGeneration.Add(secondChild);
            Debug.Log("Ogarniete");
        }
        agents.AddRange(newGeneration);
    }
    private GeneticAgent CreateChild(GeneticAgent firstParent, GeneticAgent secondParent)
    {
        GameObject firstWolf = Instantiate(wolfPrefab);
        GeneticAgent childWolf = firstWolf.GetComponent<GeneticAgent>();
        childWolf.InheritGenes(firstParent, secondParent);
        childWolf.MutateGenes(mutationChance);
        childWolf.EvaluateFitness();
        firstWolf.transform.position = SetPosition(firstWolf);
        Debug.Log(firstWolf.transform.position);
        return childWolf;

    }
    private GeneticAgent SelectParentFromPool(List<GeneticAgent> parentsPool)
    {
        if (parentsPool.Count == 0)
        {
            throw new System.InvalidOperationException("Parents pool is empty. Cannot select a parent.");
        }

        int randomIndex = Random.Range(0, parentsPool.Count);
        GeneticAgent selectedParent = parentsPool[randomIndex];
        parentsPool.RemoveAt(randomIndex);
        return selectedParent;
    }
    private void Mutate()
    {
        foreach (GeneticAgent agent in agents)
        {
            agent.MutateGenes(mutationChance);
        }
    }
    private void RemoveWolves(int count)
    {
        agents.Sort((a, b) => b.Fitness.CompareTo(a.Fitness));

        for (int i = 0; i < count; i++)
        {
            if (agents.Count == 0) break;

            Vector3 worldPosition = agents[agents.Count - 1].transform.position;
            Vector3Int gridPosition = gridManager.Grid.WorldToCell(worldPosition);
            Vector2Int position = new Vector2Int(gridPosition.x, gridPosition.z);
            Destroy(agents[agents.Count - 1].gameObject);
            agents.RemoveAt(agents.Count - 1);
            positions.Add(position);
        }

    }
    private void NextEpoch()
    {
        WolvesFitness();
        Crossover();
        epochsNum--;
    }
    public class Vector2IntComparer : IComparer<Vector2Int>
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

//TO DO: HashSet na SortedSet 
//TO DO: RemoveWolves usuwa jednego pojedynczego wilka, a selectParent polowe
