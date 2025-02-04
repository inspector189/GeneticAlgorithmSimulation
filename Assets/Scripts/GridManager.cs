using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static GeneticAlgorithm;

public class GridManager : MonoBehaviour
{
    private Dictionary<Vector2Int, AIAgent> gridObjects = new();
    [SerializeField]
    private Grid grid;
    public Grid Grid => grid;
    private SortedSet<Vector2Int> positions = new(new Vector2IntComparator());
    private HashSet<Vector2Int> occupiedPositions = new();
    [SerializeField]
    private int gridWidth = 10;
    [SerializeField]
    private int gridHeight = 10;

    public bool AddObjectToGrid(AIAgent obj)
    {
        Vector3 worldPos = obj.transform.position;
        Vector3Int cellPos = grid.WorldToCell(worldPos);
        Vector2Int gridPos = new(cellPos.x, cellPos.y);
        if(!gridObjects.ContainsKey(gridPos))
        {
            gridObjects[gridPos] = obj;
            return true;
        }
        return false;
    }
    public Vector2Int GetRandomAvailablePosition()
    {
        int index = Random.Range(0, positions.Count);
        foreach (Vector2Int position in positions)
        {
            if (index == 0)
            {
                return position;
            }
            index--;
        }

        return Vector2Int.zero;
    }
    public void RemoveObjectFromGrid(AIAgent obj)
    {
        gridObjects.Remove(obj.GetPosition());
    }
    public SortedSet<Vector2Int> GetPositions()
    {
        return positions;
    }
    public Vector2Int FindNearestPosition(Vector2Int currentPos, Wolf wolf)
    {
        int viewRange = wolf.GetComponent<GeneticAgent>().GetViewRangeValue();

        Vector2Int sheepPosition = FindNearestSheep(currentPos, viewRange);

        if (sheepPosition != Vector2Int.zero)
        {
            return sheepPosition;
        }
        return FindNearestFreePosition(currentPos, wolf);
    }
    public bool IsSheepCaught(Wolf wolf)
    {
        Vector2Int wolfPosition = new(grid.WorldToCell(wolf.transform.position).x,
                                      grid.WorldToCell(wolf.transform.position).z);

        Sheep[] allSheep = FindObjectsOfType<Sheep>(); 

        foreach (Sheep sheep in allSheep)
        {
            Vector2Int sheepPosition = new(grid.WorldToCell(sheep.transform.position).x,
                                           grid.WorldToCell(sheep.transform.position).z);

            if (sheepPosition == wolfPosition) 
            {
                return true;
            }
        }
        return false;
    }
    private Vector2Int FindNearestSheep(Vector2Int currentPos, int viewRange)
    {
        Sheep[] allSheep = FindObjectsOfType<Sheep>(); 

        Vector2Int nearestSheepPos = Vector2Int.zero;
        int minDistance = int.MaxValue; 

        foreach (Sheep sheep in allSheep)
        {
            Vector3Int cellPos = grid.WorldToCell(sheep.transform.position);
            Vector2Int sheepGridPos = new(cellPos.x, cellPos.z);

            int distance = Mathf.Abs(currentPos.x - sheepGridPos.x) + Mathf.Abs(currentPos.y - sheepGridPos.y);

            if (distance <= viewRange && distance < minDistance)
            {
                nearestSheepPos = sheepGridPos;
                minDistance = distance;
            }
        }
        return nearestSheepPos;
    }

    public Vector2Int FindNearestFreePosition(Vector2Int currentPos, Wolf wolf)
    {
        int movementRange = wolf.GetComponent<GeneticAgent>().GetMovementRangeValue();

        Queue<Vector2Int> searchQueue = new();
        HashSet<Vector2Int> visited = new() { currentPos };
        searchQueue.Enqueue(currentPos);

        while (searchQueue.Count > 0)
        {
            Vector2Int position = searchQueue.Dequeue();

            if (positions.Contains(position) && !occupiedPositions.Contains(position))
                return position;

            foreach (Vector2Int neighbor in GetNeighbors(position, movementRange))
            {
                if (visited.Add(neighbor) && positions.Contains(neighbor))
                    searchQueue.Enqueue(neighbor);
            }
        }
        return currentPos;
    }
    private Vector2Int[] GetNeighbors(Vector2Int position, int movementRange)
    {
        return new Vector2Int[]
        {
            new(position.x + movementRange, position.y),
            new(position.x - movementRange, position.y),
            new(position.x, position.y + movementRange),
            new(position.x, position.y - movementRange)
        };
    }
    public HashSet<Vector2Int> GetOccupiedPositions()
    {
        return occupiedPositions;
    }
    public void AddPositions()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                positions.Add(new Vector2Int(x, y));
            }
        }
    }
    public Vector3 SetPosition()
    {
        Vector2Int randomGridPos = GetRandomAvailablePosition();
        positions.Remove(randomGridPos);
        return Grid.CellToWorld(new Vector3Int(randomGridPos.x, 0, randomGridPos.y));
    }
}