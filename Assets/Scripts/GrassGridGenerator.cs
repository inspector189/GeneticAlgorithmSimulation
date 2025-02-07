using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassGridGenerator : MonoBehaviour
{
    [SerializeField]
    private GameObject grassBlockPrefab;
    [SerializeField]
    private GridManager gridManager;
    private int gridWidth;
    private int gridHeight;
    [SerializeField]
    private float cellSize;
    [SerializeField]
    private Transform parentTerrain;
    void Start()
    {
        gridWidth = gridManager.GetGridWidth();
        gridHeight = gridManager.GetGridHeight();
        GenerateGrid();
        grassBlockPrefab.SetActive(false);
    }

    void GenerateGrid()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                Vector3 spawnPosition = new Vector3(x * cellSize, -1f, z * cellSize);
                Instantiate(grassBlockPrefab, spawnPosition, Quaternion.identity, parentTerrain);
            }
        }
    }
    private void OnDestroy()
    {
        grassBlockPrefab.SetActive(true);
    }
}
