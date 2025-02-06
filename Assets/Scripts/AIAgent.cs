using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;


public class AIAgent : MonoBehaviour
{
    public GridManager gridManager;
    private void Awake()
    {
        gridManager = FindObjectOfType<GridManager>();
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
        Vector2Int currentGridPos = GetPosition();
        Vector2Int targetPosition = gridManager.FindNearestPosition(currentGridPos, GetComponent<Wolf>());
        if (gridManager.IsSheepCaught(GetComponent<Wolf>()))
        {
            GetComponent<Wolf>().AttackSheep(currentGridPos);
            targetPosition = gridManager.FindNearestPosition(currentGridPos, GetComponent<Wolf>());
        }
        if (targetPosition != currentGridPos)
        {
            gridManager.GetPositions().Add(currentGridPos);
            gridManager.GetOccupiedPositions().Add(targetPosition);
            GetComponent<Wolf>().SetTargetPosition(targetPosition, gridManager.GetOccupiedPositions());
            gridManager.GetPositions().Remove(targetPosition);
            GetComponent<Wolf>().CountFitnessAfterMove();
        }
    }
    public int CountCellsWolfWalkedOver(Vector2Int currentGridPos, Vector2Int targetPos)
    {
        int cellsWalkedOver = 0;
        while (currentGridPos.x != targetPos.x)
        {
            currentGridPos.x += Math.Sign(targetPos.x - currentGridPos.x);
            cellsWalkedOver++;
        }

        while (currentGridPos.y != targetPos.y)
        {
            currentGridPos.y += Math.Sign(targetPos.y - currentGridPos.y);
            cellsWalkedOver++;
        }

        return cellsWalkedOver;
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
