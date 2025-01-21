using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    private Dictionary<Vector2Int, AIAgent> gridObjects = new();
    [SerializeField]
    private Grid grid;
    public Grid Grid => grid;

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
    public bool IsPositionEmpty(AIAgent obj)
    {
        return !gridObjects[obj.GetPosition()];
    }
}