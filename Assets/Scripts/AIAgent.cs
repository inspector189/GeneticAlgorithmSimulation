using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class AIAgent : MonoBehaviour
{
    private GridManager gridManager;
    private void Start()
    {  
        gridManager = FindObjectOfType<GridManager>();
        if (gridManager != null)
        {
            gridManager.AddObjectToGrid(this);
        }
    }
    private void OnDestroy()
    {
        if (gridManager != null)
        {
            gridManager.RemoveObjectFromGrid(this);
        }
    }
    public void Move()
    {
        gridManager = FindObjectOfType<GridManager>();
        Vector2Int currentGridPos = new(gridManager.Grid.WorldToCell(transform.position).x, gridManager.Grid.WorldToCell(transform.position).z);
        Vector2Int targetPosition = gridManager.FindNearestFreePosition(currentGridPos, GetComponent<Wolf>());
        if (targetPosition != currentGridPos)
        {
            gridManager.GetPositions().Add(currentGridPos);
            gridManager.GetOccupiedPositions().Add(targetPosition);
            GetComponent<Wolf>().SetTargetPosition(targetPosition, gridManager.GetOccupiedPositions());
            gridManager.GetPositions().Remove(targetPosition);
        }
    }
    public Vector2Int GetPosition()
    {
        if (gridManager != null && gridManager.Grid != null)
        {
            Vector3Int currentPosition = gridManager.Grid.WorldToCell(transform.position);
            return new Vector2Int(currentPosition.x, currentPosition.z);
        }

        return Vector2Int.zero;
    }
    public HashSet<Vector2Int> GetOccupiedPosition()
    {
        return gridManager.GetOccupiedPositions();
    }
    public GridManager GetGrid()
    {
        return gridManager;
    }
    public Vector3 GetTargetWorldPosition(Vector2Int targetPosition)
    {
        return GetGrid().Grid.CellToWorld(new Vector3Int(targetPosition.x, 0, targetPosition.y));
    }
}
