using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using static GeneticAlgorithm;

public class GridManager : MonoBehaviour
{
    private Dictionary<Vector2Int, AIAgent> gridObjects = new();
    [SerializeField]
    private Grid grid;
    private Dictionary<Vector2Int, Wolf> reservedCells = new Dictionary<Vector2Int, Wolf>();
    private Dictionary<Sheep, Wolf> reservedSheep = new Dictionary<Sheep, Wolf>();

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
        Vector2Int gridPos = new(cellPos.x, cellPos.z);
        if (!gridObjects.ContainsKey(gridPos))
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

        Vector2Int sheepPosition = FindNearestSheep(currentPos, viewRange, wolf, viewRange);
        if (sheepPosition != Vector2Int.zero && wolf.CanAttack())
        {
            Vector2Int reserved = TryReserveCell(wolf, sheepPosition);
            if (reserved != Vector2Int.zero)
            {
                return reserved;
            }
        }
        Vector2Int freePos = FindNearestFreePosition(currentPos, wolf);
        Vector2Int reservedFree = TryReserveCell(wolf, freePos);
        if (reservedFree != Vector2Int.zero)
        {
            return reservedFree;
        }
        return freePos;
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
    //przeszukiiwanie owiec w okręgu
    private Vector2Int FindNearestSheep(Vector2Int currentPos, int baseViewRange, Wolf wolf, int effectiveBase)
    {
        Sheep[] allSheep = FindObjectsOfType<Sheep>();

        Vector2Int nearestSheepPos = Vector2Int.zero;
        Sheep candidateSheep = null;
        float minDistance = float.MaxValue;

        foreach (Sheep sheep in allSheep)
        {
            if (sheep.IsUnderAttack)
                continue;

            Vector3Int cellPos = grid.WorldToCell(sheep.transform.position);
            Vector2Int sheepGridPos = new Vector2Int(cellPos.x, cellPos.z);
            float distance = Vector2Int.Distance(currentPos, sheepGridPos);

            if (distance <= baseViewRange && distance < minDistance)
            {
                minDistance = distance;
                candidateSheep = sheep;
                nearestSheepPos = sheepGridPos;
            }
        }

        if (candidateSheep != null)
        {
            ReserveSheep(candidateSheep, wolf, nearestSheepPos);
        }

        return nearestSheepPos;
    }
    //przeszukiwanie wolnych miejsc w prostokącie 
    public Vector2Int FindNearestFreePosition(Vector2Int currentPos, Wolf wolf)
    {
        int movementRange = wolf.GetComponent<GeneticAgent>().GetMovementRangeValue();
        List<Vector2Int> candidates = new List<Vector2Int>();
        HashSet<Vector2Int> visited = new() { currentPos };

        for (int dx = -movementRange; dx <= movementRange; dx++)
        {
            for (int dy = -movementRange; dy <= movementRange; dy++)
            {
                Vector2Int neighbor = new(currentPos.x + dx, currentPos.y + dy);

                if (positions.Contains(neighbor) && !occupiedPositions.Contains(neighbor) && visited.Add(neighbor))
                {
                    candidates.Add(neighbor);
                }
            }
        }
        if (candidates.Count > 0)
        {
            int randomIndex = Random.Range(0, candidates.Count);
            return candidates[randomIndex];
        }
        return currentPos;
    }
    public Dictionary<Sheep, Wolf> GetReservedSheeps()
    {
        return reservedSheep;
    }

    public HashSet<Vector2Int> GetOccupiedPositions()
    {
        return occupiedPositions;
    }
    public Dictionary<Vector2Int, Wolf> GetReservedCells()
    {
        return reservedCells;
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
    private Vector2Int TryReserveCell(Wolf wolf, Vector2Int candidate)
    {
        if(!reservedCells.ContainsKey(candidate))
        {
            reservedCells[candidate] = wolf;
            return candidate;
        }
        else
        {
            Wolf reservedWolf = reservedCells[candidate];
            int currentOrder = wolf.GetComponent<GeneticAgent>().GetGene("Movement Order").CurrentValue;
            int reservedOrder = reservedWolf.GetComponent<GeneticAgent>().GetGene("Movement Order").CurrentValue;
            if(currentOrder > reservedOrder)
            {
                reservedCells[candidate] = wolf;

                return candidate;
            }
            else if(currentOrder == reservedOrder)
            {
                if(Random.value < 0.5f)
                {
                    reservedCells[candidate] = wolf;
                    return candidate;
                }
            }
            return Vector2Int.zero;
        }
    }
    private void ReserveSheep(Sheep candidateSheep, Wolf wolf, Vector2Int nearestSheepPos)
    {
        if (!reservedSheep.ContainsKey(candidateSheep))
        {
            reservedSheep[candidateSheep] = wolf;
        }
        else
        {
            int currentWolfOrder = wolf.GetComponent<GeneticAgent>().GetGene("Movement Order").CurrentValue;
            int reservedWolfOrder = reservedSheep[candidateSheep].GetComponent<GeneticAgent>().GetGene("Movement Order").CurrentValue;

            if (currentWolfOrder > reservedWolfOrder)
            {
                reservedSheep[candidateSheep] = wolf;
            }
            else if (currentWolfOrder == reservedWolfOrder)
            {
                if (Random.value < 0.5f)
                {
                    reservedSheep[candidateSheep] = wolf;
                }
                else
                {
                    candidateSheep = null;
                    nearestSheepPos = Vector2Int.zero;
                }
            }
            else
            {
                candidateSheep = null;
                nearestSheepPos = Vector2Int.zero;
            }
        }
    }
    public int GetGridWidth()
    {
        return gridWidth;
    }
    public int GetGridHeight()
    {
        return gridHeight;
    }
    
}
/*
    UI - tabelka z informacjami o genach danego wilka
     //zrobic to prostokątem a nie przeszukiwaniem wszerz -
    //i z tego prostokatu gridManager zwraca nam liste owiec ktore sie w nim znajduja -
    //i jezeli owca ma boola ze jest atakowana to wtedy szukamy randomowej innej ktora nie ma
 
 */