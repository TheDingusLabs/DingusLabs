using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TrashSpawner : MonoBehaviour
{
    public List<GameObject> trashPrefabs;
    public GameObject SpawnerSon;
    public GameObject agent;
    Bounds m_SpawnAreaBounds;

    public float countdown = 4f;
    public float counter = 0f;
    public float difficultyOverTimeMod = 0.0004f;
    public float difficulty = 1f;

    public int failCondition = 10;

    private void Start()
    {
        CreateTrash();
    }

    public void ResetTrashSpawner()
    {
        DestroyTheChildren();
        counter = 0;
        difficulty = 1f;
        //CreateTrash();
        CreateStarterTrash();
    }

    public Vector3 GetEdgeSpawnPos()
    {
        m_SpawnAreaBounds = SpawnerSon.GetComponent<Collider>().bounds;
        var randSide = Random.Range(0, 2);
        var randomPosX = Random.Range(-m_SpawnAreaBounds.extents.x+ 3, -m_SpawnAreaBounds.extents.x + 4);
        if (randSide == 0)
        {
            randomPosX = Random.Range(m_SpawnAreaBounds.extents.x - 3, m_SpawnAreaBounds.extents.x - 4);
        }
        randSide = Random.Range(0, 2);
        var randomPosZ = Random.Range(-m_SpawnAreaBounds.extents.z + 3, -m_SpawnAreaBounds.extents.z + 4);

        if (randSide == 0)
        {
            randomPosZ = Random.Range(m_SpawnAreaBounds.extents.z - 3, m_SpawnAreaBounds.extents.z - 4);
        }

        var randomSpawnPos = this.transform.position +
            new Vector3(randomPosX, 0.5f, randomPosZ);
        return randomSpawnPos;
    }

    public Vector3 GetRandomSpawnPos()
    {
        m_SpawnAreaBounds = SpawnerSon.GetComponent<Collider>().bounds;
        var randomPosX = Random.Range(-m_SpawnAreaBounds.extents.x, m_SpawnAreaBounds.extents.x);
        var randomPosZ = Random.Range(-m_SpawnAreaBounds.extents.z, m_SpawnAreaBounds.extents.z);

        var randomSpawnPos = this.transform.position +
            new Vector3(randomPosX, 0.5f, randomPosZ);
        return randomSpawnPos;
    }

    public void CreateStarterTrash()
    {
        var child = Instantiate(trashPrefabs[2], GetRandomSpawnPos(), Quaternion.Euler(new Vector3(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f))) );
        child.transform.parent = this.transform;
    }

    public void CreateTrash()
    {
        var kids = this.transform.GetComponentsInChildren<Transform>();
        int kidsCount = 0;
        foreach(var kid in kids)
        {
            if (kid.gameObject.tag == "squareTrash" || kid.gameObject.tag == "cylinderTrash" || kid.gameObject.tag == "sphereTrash")
            {
                kidsCount++;
            }
        }
        if (kidsCount <= failCondition)
        {
            var trashNo = Random.Range(0, trashPrefabs.Count);
            if(trashNo == 1)
            {
                var child = Instantiate(trashPrefabs[trashNo], GetRandomSpawnPos(), Quaternion.identity);
                child.transform.parent = this.transform;
            }
            else
            {
                var child = Instantiate(trashPrefabs[trashNo], GetRandomSpawnPos(), Quaternion.Euler(new Vector3(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f))));
                child.transform.parent = this.transform;
            }

        }
        else{
            agent.GetComponent<TrashManAgent>().failed = true;
        }
    }

    public void DestroyTheChildren()
    {
        foreach (var child in this.transform.GetComponentsInChildren<Transform>())
        {
            if (child.gameObject.tag == "squareTrash" || child.gameObject.tag == "cylinderTrash" || child.gameObject.tag == "sphereTrash")
            {
                Destroy(child.gameObject);
            }
        }
    }

    public void FixedUpdate()
    {
        counter += Time.deltaTime * difficulty;
        difficulty += difficultyOverTimeMod;


        if (counter >= countdown)
        {
            counter = 0;
            CreateTrash();
        }
    }
}
