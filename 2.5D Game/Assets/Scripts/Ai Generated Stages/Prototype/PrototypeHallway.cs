using UnityEngine;
using System.Collections.Generic;

public class PrototypeHallway : MonoBehaviour
{
    private Vector2Int startPos;
    private Vector2Int endPos;
    private float cellSize;
    private GeneratorPrototype generatorPrototype;

    public GameObject passagePrefab;
    public int hallwayWidth = 1;

    private List<Vector2Int> pathTiles = new List<Vector2Int>();

    public void SetupHallway(Vector2Int start, Vector2Int end, float cellSize, GeneratorPrototype generator)
    {
        this.startPos = start;
        this.endPos = end;
        this.cellSize = cellSize;
        this.generatorPrototype = generator;
        GenerateHallwayPath();
        GenerateHallwayPrefabs();
        AttachDoorsToWalls();
    }

    private void GenerateHallwayPath()
    {
        // Simple straight path: first x, then y
        Vector2Int current = startPos;
        pathTiles.Add(current);
        while (current != endPos)
        {
            if (current.x != endPos.x)
                current.x += (endPos.x > current.x) ? 1 : -1;
            else if (current.y != endPos.y)
                current.y += (endPos.y > current.y) ? 1 : -1;
            pathTiles.Add(current);
        }
    }

    private void GenerateHallwayPrefabs()
    {
        foreach (var tile in pathTiles)
        {
            Vector3 worldPos = generatorPrototype.GridToWorld(tile);
            Instantiate(passagePrefab, worldPos, Quaternion.identity, this.transform);
        }
    }

    private void AttachDoorsToWalls()
    {
        // Only attach doors to walls at the endpoints
        if (generatorPrototype.GetTileTypeAt(startPos) == TileType.Wall)
            generatorPrototype.SetTileTypeAt(startPos, TileType.Door);
        if (generatorPrototype.GetTileTypeAt(endPos) == TileType.Wall)
            generatorPrototype.SetTileTypeAt(endPos, TileType.Door);
    }

    public List<Vector2Int> GetPathTiles()
    {
        return pathTiles;
    }
} 