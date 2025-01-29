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
    public void UpdateObjectOnGrid(AIAgent obj, Vector2Int newPosition)
    {
        Vector2Int lastPosition = obj.GetPosition();
        if(gridObjects.ContainsKey(lastPosition) && gridObjects[lastPosition] == obj && !gridObjects.ContainsKey(newPosition))
        {
            RemoveObjectFromPosition(lastPosition);
            AddObjectToGrid(obj);
        }
    }
    public void RemoveObjectFromPosition(Vector2Int position)
    {
        if (gridObjects.ContainsKey(position))
        {
            gridObjects.Remove(position);
        }
    }
    public void RemoveObjectFromGrid(AIAgent obj)
    {
        gridObjects.Remove(obj.GetPosition());
    }
    public bool IsPositionEmpty(Vector2Int position)
    {
        return !gridObjects.ContainsKey(position);
    }
    public SortedSet<Vector2Int> GetPositions()
    {
        return positions;
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