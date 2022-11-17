using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{

    public int SpawnAmountAtStart = 30;
    public GameObject EnemyPrefab;

    public float TerrainXBoundary = 500f;
    public float TerrainZBoundary = 500f;

    public Vector3 RandomPosAtTerrain;
    public float GivenHeight;

    // Start is called before the first frame update
    void Start()
    {

        SpawnAtStart(SpawnAmountAtStart);

    }

    float GetHeightAtPointOfTerrain(Vector3 givenPos)
    {
        return Terrain.activeTerrain.SampleHeight(givenPos);
    }

    void SpawnAtStart(int spawnAmount)
    {
        for (int i = 0; i < spawnAmount; i++)
        {
            float RandomX = Random.Range(0f, TerrainXBoundary);
            float RandomZ = Random.Range(0f, TerrainZBoundary);

            GivenHeight = GetHeightAtPointOfTerrain(RandomPosAtTerrain);

            RandomPosAtTerrain = new Vector3(RandomX, GivenHeight, RandomZ);

            Instantiate(EnemyPrefab, RandomPosAtTerrain, Quaternion.identity);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
