using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AIAgent : MonoBehaviour
{
    private GridManager gridManager;
    private void Start()
    {  
        gridManager = FindFirstObjectByType<GridManager>();
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
    public void Move(Vector2Int direction)
    {
        if (gridManager != null)
        {
            Vector3Int newPosition = gridManager.Grid.WorldToCell(transform.position + new Vector3(direction.x, 0, direction.y));
            Vector2Int newPosVec2 = new(newPosition.x, newPosition.z);
            if (gridManager.IsPositionEmpty(newPosVec2))
            {
                gridManager.UpdateObjectOnGrid(this, newPosVec2);
                transform.position = new Vector3(newPosition.x, transform.position.y, newPosition.z);
            }
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
}
