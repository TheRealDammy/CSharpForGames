using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/* public class Map : MonoBehaviour
{
    public BiomePreset[] biomes;
    public GameObject tilePrefab;

    [Header("Map Settings")]
    public int width = 50;
    public int height = 50;
    public float scale = 1.0f;
    public Vector2 offset;

    [Header("Heigt Map Settings")]
    public Wave[] heightWaves;
    public float[,] heightMap;

    [Header("Moisture Map Settings")]
    public Wave[] moistureWaves;
    public float[,] moistureMap;

    [Header("Heat Map Settings")]
    public Wave[] heatWaves;
    public float[,] heatMap;

    public void GenerateMap()
    {
        //height map
        heightMap = NoiseGenerator.Generate(width, height, heightWaves, scale, offset);
        //moisture map
        moistureMap = NoiseGenerator.Generate(width, height, moistureWaves, scale, offset);
        //heat map
        heatMap = NoiseGenerator.Generate(width, height, heatWaves, scale, offset);

        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                float heightValue = heightMap[x, y];
                float moistureValue = moistureMap[x, y];
                float heatValue = heatMap[x, y];
                BiomePreset selectedBiome = null;
                foreach(BiomePreset biome in biomes)
                {
                    if(biome.MatchConditions(heightValue, moistureValue, heatValue))
                    {
                        selectedBiome = biome;
                        break;
                    }
                }
                if(selectedBiome != null)
                {
                    GameObject tile = GameObject.Instantiate(tilePrefab, new Vector3(x, y, 0), Quaternion.identity);
                    SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
                    sr.sprite = selectedBiome.GetTileSprite();
                }
            }
        }
    }

    void start()
    {
        GenerateMap();
    }

    BiomePreset GetBiome(float height, float moisture, float heat)
    {
        BiomePreset biomeToReturn = null;
        List<BiomeTempData> biomeTemp = new List<BiomeTempData>();

        foreach (BiomePreset biome in biomes)
        {
            if (biome.MatchConditions(height, moisture, heat))
            {
                biomeTemp.Add(new BiomeTempData(biome));
            }
        }

        float curVal = 0.0f;

        foreach (BiomeTempData biome in biomeTemp)
        {
            if (biomeToReturn == null)
            {
                biomeToReturn = biome.biome;
                curVal = biome.GetDiffValue(height, moisture, heat);
            }
            else
            {
                if (biome.GetDiffValue(height, moisture, heat) < curVal)
                {
                    biomeToReturn = biome.biome;
                    curVal = biome.GetDiffValue(height, moisture, heat);
                }
            }
        }

        if (biomeToReturn == null)
        {
            biomeToReturn = biomes[0];
            Debug.LogError("No biome found for height: " + height + " moisture: " + moisture + " heat: " + heat);
        }

        return biomeToReturn;
    }
}

public class BiomeTempData
{
    public BiomePreset biome;
    public BiomeTempData (BiomePreset preset)
    {
        biome = preset;
    }
    public float GetDiffValue(float height, float moisture, float heat)
    {
        float diff = 0f;
        diff += Mathf.Abs(biome.minHeight - height);
        diff += Mathf.Abs(biome.minMoisture - moisture);
        diff += Mathf.Abs(biome.minHeat - heat);
        return diff;
    }
} */
