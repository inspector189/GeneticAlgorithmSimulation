using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sheep : AIAgent
{
    private List<Sheep> sheeps = new();
    [SerializeField, Min(4)]
    private int sheepPopulationSize = 10;
    [SerializeField]
    private GameObject sheepPrefab;

    public void SpawnSheeps()
    {
        for (int i = sheeps.Count; i < sheepPopulationSize; i++)
        {
            CreateSheepOnGrid();
        }
    }
    public void CreateSheepOnGrid()
    {
        Vector2Int randomPosition = GetGrid().GetRandomAvailablePosition();
        GetGrid().GetPositions().Remove(randomPosition);
        GameObject sheep = Instantiate(sheepPrefab);
        sheep.transform.position = new Vector3(randomPosition.x, 0, randomPosition.y);
        sheeps.Add(sheep.GetComponent<Sheep>());
        GetGrid().GetOccupiedPositions().Add(randomPosition);
        GetGrid().AddObjectToGrid(sheep.GetComponent<AIAgent>());
    }
    public void RemoveSheep(Sheep sheep)
    {
        Vector3Int gridPos = GetGrid().Grid.WorldToCell(sheep.transform.position);
        Vector2Int position = new(gridPos.x, gridPos.z);

        GetGrid().GetOccupiedPositions().Remove(position);
        GetGrid().GetPositions().Add(position);
        GetGrid().RemoveObjectFromGrid(sheep);
        Destroy(sheep.gameObject);
    }
}
