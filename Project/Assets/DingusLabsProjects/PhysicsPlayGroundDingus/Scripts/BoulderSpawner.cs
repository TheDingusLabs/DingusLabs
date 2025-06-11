using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoulderSpawner : MonoBehaviour
{
    public float spawnRate;
    public float spawnOffset;
    public float spawnCount;

    public float horizontalLaunchForce;
    public GameObject boulderPrefab;
    private List<GameObject> spawnedBoulders;

    // Start is called before the first frame update
    void Start()
    {
        spawnedBoulders = new List<GameObject>();
        spawnCount = -spawnOffset;
    }

    // Update is called once per frame
    void Update()
    {
        spawnCount += Time.deltaTime;

        if(spawnCount >= spawnRate){
            SpawnBoulder();
            spawnCount = 0f;
        }
    }

    private void SpawnBoulder(){
        var boulder = Instantiate(boulderPrefab, this.transform.position, this.transform.rotation, this.transform);
        boulder.GetComponent<Rigidbody>().AddForce(new Vector3(0,0,-horizontalLaunchForce));
        spawnedBoulders.Add(boulder);
    }

    public void ResetSpawner(){
        foreach(var boulder in spawnedBoulders){
            Destroy(boulder);
        }
        spawnCount = -spawnOffset;
    }
}
