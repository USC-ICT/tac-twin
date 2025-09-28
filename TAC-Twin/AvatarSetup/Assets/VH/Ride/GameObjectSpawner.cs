using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ride.VirtualHumans;


public class GameObjectSpawner
{
    const float MAX_DIST = 10;
    const int MAX_ITERATIONS = 1000;
    const float SPAWN_RADIUS = 0.48f;

    Dictionary<string, Vector3> m_spawnPositions = new Dictionary<string, Vector3>();
    LayerMask m_floorLayerMask;
    float m_spawnRadius = 1;

    public GameObjectSpawner(float spawnRadius, LayerMask floorLayerMask)
    {
        m_spawnRadius = spawnRadius;
        m_floorLayerMask = floorLayerMask;
    }

    bool DoesSpawnOverlap(Vector3 originalPosition)
    {
        originalPosition.y = 0;
        foreach (var position in m_spawnPositions)
        {
            if (Vector3.Distance(originalPosition, position.Value) < SPAWN_RADIUS) return true;
        }
        return false;
    }

    public GameObject SpawnGameObjectInView(string name, GameObject template, Vector3 targetPosition, Vector3 viewDir)
    {
        Vector3 viewDirProjected = viewDir;
        viewDirProjected.y = 0;
        viewDirProjected.Normalize();

        var spawnCenter = targetPosition + viewDirProjected * m_spawnRadius;

        Vector3 leftVector = (new Vector3(viewDirProjected.z, 0, -viewDirProjected.x)).normalized;

        Vector3 spawnLeft = spawnCenter + leftVector;
        Vector3 spawnRight = spawnCenter - leftVector;



        int count = 0;
        Vector3 spawnPosition = Vector3.Lerp(spawnLeft, spawnRight, 0.5f);

        while (DoesSpawnOverlap(spawnPosition) && count < MAX_ITERATIONS)
        {
            spawnPosition = Vector3.Lerp(spawnLeft, spawnRight, Random.Range(0.0f, 1.0f));

            Vector3 limit = leftVector * (m_spawnRadius * (float)count / MAX_ITERATIONS * 2);
            spawnLeft = spawnCenter + limit;
            spawnRight = spawnCenter - limit;

            count++;
        }

        return SpawnGameObject(name, template, spawnPosition, -viewDirProjected);
    }

    public GameObject SpawnGameObjectInProximity(string name, GameObject template, Vector3 targetPosition, Vector3 forward)
    {
        Vector3 center = targetPosition;

        Vector3 xy = Random.insideUnitCircle * m_spawnRadius;
        var spawnPosition = center + xy;

        return SpawnGameObject(name, template, spawnPosition, forward);
    }

    public GameObject SpawnGameObject(string name, GameObject template, Vector3 spawnPosition, Vector3 forward)
    {
        AddGameObjectPosition(name, spawnPosition);
        spawnPosition.y = Ride.Globals.api.terrainSystem.GetTerrainHeight(spawnPosition);

        GameObject gameObject = GameObject.Instantiate(template, spawnPosition, Quaternion.identity);
        gameObject.transform.forward = forward;


        return gameObject;
    }

    public void DeleteGameObject(GameObject gameObject)
    {
        m_spawnPositions.Remove(gameObject.name);
        GameObject.Destroy(gameObject);
    }

    public void AddGameObjectPosition(string name, Vector3 position)
    {
        m_spawnPositions.Add(name, position);
    }
}

