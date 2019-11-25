using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    public enum GenerationType
    {
        RANDOM, PERLINNOISE
    }
    public GenerationType generationType;
    public int mapWidth;
    public int mapHeight;
    public float noiseScale;
    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;
    public bool autoUpdate;
    public int seed;
    public Vector2 offset;
    public Tilemap tilemap;

    public TerrainType[] regions;
    public TerrainType[] dirtRegions;
    public TerrainType[] oreRegions;

    public void GenerateMap()
    {
        if (generationType == GenerationType.PERLINNOISE)
        {
            GenerateMapWithNoise();
        }
        else if (generationType == GenerationType.RANDOM)
        {
            GenerateMapWithRandom();
        }
    }

    private void GenerateMapWithRandom()
    {
        TileBase[] customTilemap = new TileBase[mapWidth * mapHeight];
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float rnd = UnityEngine.Random.Range(0f, 1f);
                customTilemap[y * mapWidth + x] = FindTileFromRegion(rnd);
            }
        }
        SetTileMap(customTilemap);
    }

    private void SetTileMap(TileBase[] customTilemap)
    {
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                tilemap.SetTile(new Vector3Int(x, y, 0), customTilemap[y * mapWidth + x]);
            }
        }
    }

    private TileBase FindTileFromRegion(float rnd)
    {
        for (int i = 0; i < regions.Length; i++)
        {
            if (rnd <= regions[i].height)
            {
                return regions[i].tile;
            }
        }
        return regions[0].tile;
    }

    private TileBase FindDirtTileFromRegion(float rnd)
    {
        for (int i = 0; i < dirtRegions.Length; i++)
        {
            if (rnd <= dirtRegions[i].height)
            {
                return dirtRegions[i].tile;
            }
        }
        return regions[0].tile;
    }

    private TileBase FindTileFromRegion(float oreRnd,float dirtRnd,float oreTypeRnd,float caveRnd)
    {
        if(caveRnd <= 0.6f)
        {
            if (oreRnd >= 0.8f)
            {
                for (int i = 0; i < oreRegions.Length; i++)
                {
                    if (oreTypeRnd <= oreRegions[i].height)
                    {
                        return oreRegions[i].tile;
                    }
                }
            }
            return FindDirtTileFromRegion(dirtRnd);
        }
        return null;
    }

    private void GenerateMapWithNoise()
    {
        float[,] dirtMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);
        float[,] oreMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed*2, noiseScale, octaves, persistance, lacunarity, offset);
        float[,] oreTypeMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed * 3, noiseScale, octaves, persistance, lacunarity, offset);
        float[,] caveMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed * 4, noiseScale*2, octaves, persistance, lacunarity, offset);
        TileBase[] customTilemap = new TileBase[mapWidth * mapHeight];
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                customTilemap[y * mapWidth + x] = FindTileFromRegion(oreMap[x, y],dirtMap[x,y],oreTypeMap[x,y],caveMap[x,y]);
            }
        }
        SetTileMap(customTilemap);
    }

    private void OnValidate()
    {
        if (mapHeight < 1)
        {
            mapHeight = 1;
        }
        if (mapWidth < 1)
        {
            mapWidth = 1;
        }
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }
        if (octaves < 1)
        {
            octaves = 1;
        }
    }
}
[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public TileBase tile;
}