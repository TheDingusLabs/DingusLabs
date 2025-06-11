using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class FuckYou{
    /// <summary>
    /// Shuffles the element order of the specified list.
    /// </summary>
    public static void Shuffle<T>(this IList<T> ts) {
        var count = ts.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i) {
            var r = UnityEngine.Random.Range(i, count);
            var tmp = ts[i];
            ts[i] = ts[r];
            ts[r] = tmp;
        }
    }
}

public class StealyDingusRoom : MonoBehaviour
{
    public List<GameObject> pointcubes;

    public List<GameObject> lootList;
    public List<GameObject> junkList;

    public List<GameObject> enemyAgents;

    //public List<GameObject> boulderSpawners;

    public GameObject playerSpawnArea;

    public float spawnDir = 0f;

    public bool theOne = false;

    public List<GameObject> lootSpawnAreas;
    public List<GameObject> GuaranteedLootSpawnAreas;

    public int noOfLootSpawns = 2;

    public List<GameObject> lootPrefabs;
    public List<GameObject> junkPrefabs;

    //private Ball ball;

    private void Start()
    {
        pointcubes = new List<GameObject>();
        //boulderSpawners = new List<GameObject>();

        foreach (Transform transform in this.transform)
        {
            if (transform.CompareTag("goal"))
            {
                transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z); 
                pointcubes.Add(transform.gameObject);
            }
            // else if (transform.CompareTag("spawner"))
            // {
            //     boulderSpawners.Add(transform.gameObject);
            // }
            // else if (transform.CompareTag("ball"))
            // {
            //     ball = transform.gameObject.GetComponent<Ball>();
            // }
        }

        //SpawnLoot();
        // var templootlist = this.transform.GetComponentsInChildren<Loot>();
        // lootList = new List<GameObject>();
        // foreach(var loot in templootlist){
        //     lootList.Add(loot.gameObject);
        // }
    }

    public void SpawnLoot(){
        var loopdur = lootSpawnAreas.Count;
        //var tempLootSpawnAreas = lootSpawnAreas;
        FuckYou.Shuffle(lootSpawnAreas);

        lootList = new List<GameObject>();
        junkList = new List<GameObject>();

        // for(int i = 0; i < noOfLootSpawns; i++){
        //     var index = Random.Range(0,tempLootSpawnAreas.Count);

        //     Bounds bounds = tempLootSpawnAreas[index].GetComponent<Collider>().bounds;
        //     float offsetX = Random.Range(-bounds.extents.x, bounds.extents.x);
        //     float offsetY = Random.Range(-bounds.extents.y, bounds.extents.y);
        //     var loot = Instantiate(lootPrefabs[Random.Range(0,lootPrefabs.Count)],tempLootSpawnAreas[index].transform.position + new Vector3(offsetX, offsetY, 0), tempLootSpawnAreas[index].transform.rotation, this.transform);
        //     lootList.Add(loot);
        //     tempLootSpawnAreas.RemoveAt(index);
        // }

        // Debug.Log(tempLootSpawnAreas.Count);
        // foreach(var area in tempLootSpawnAreas)
        // {
        //     Bounds bounds = area.GetComponent<Collider>().bounds;
        //     float offsetX = Random.Range(-bounds.extents.x, bounds.extents.x);
        //     float offsetY = Random.Range(-bounds.extents.y, bounds.extents.y);
        //     var loot = Instantiate(junkPrefabs[Random.Range(0,junkPrefabs.Count)],area.transform.position + new Vector3(offsetX, offsetY, 0), area.transform.rotation, this.transform);
        //     junkList.Add(loot);
        // }

        for(int i = 0; i < noOfLootSpawns; i++){
            //var index = Random.Range(0,lootSpawnAreas.Count);

            Bounds bounds = lootSpawnAreas[i].GetComponent<Collider>().bounds;
            float offsetX = Random.Range(-bounds.extents.x, bounds.extents.x);
            float offsetY = Random.Range(-bounds.extents.y, bounds.extents.y);
            var loot = Instantiate(lootPrefabs[Random.Range(0,lootPrefabs.Count)],lootSpawnAreas[i].transform.position + new Vector3(offsetX, offsetY, 0), Random.rotation, this.transform);
            lootList.Add(loot);
            //lootSpawnAreas.RemoveAt(index);
        }

        for(int i = noOfLootSpawns; i < lootSpawnAreas.Count; i++){
            //var index = Random.Range(0,lootSpawnAreas.Count);

            Bounds bounds = lootSpawnAreas[i].GetComponent<Collider>().bounds;
            float offsetX = Random.Range(-bounds.extents.x, bounds.extents.x);
            float offsetY = Random.Range(-bounds.extents.y, bounds.extents.y);
            var loot = Instantiate(junkPrefabs[Random.Range(0,junkPrefabs.Count)],lootSpawnAreas[i].transform.position + new Vector3(offsetX, offsetY, 0), Random.rotation, this.transform);
            junkList.Add(loot);
        }

        //Debug.Log(lootSpawnAreas.Count);
        // foreach(var area in tempLootSpawnAreas)
        // {
        //     Bounds bounds = area.GetComponent<Collider>().bounds;
        //     float offsetX = Random.Range(-bounds.extents.x, bounds.extents.x);
        //     float offsetY = Random.Range(-bounds.extents.y, bounds.extents.y);
        //     var loot = Instantiate(junkPrefabs[Random.Range(0,junkPrefabs.Count)],area.transform.position + new Vector3(offsetX, offsetY, 0), area.transform.rotation, this.transform);
        //     junkList.Add(loot);
        // }
        
        foreach(var area in GuaranteedLootSpawnAreas)
        {
            Bounds bounds = area.GetComponent<Collider>().bounds;
            float offsetX = Random.Range(-bounds.extents.x, bounds.extents.x);
            float offsetY = Random.Range(-bounds.extents.y, bounds.extents.y);
            var loot = Instantiate(lootPrefabs[Random.Range(0,lootPrefabs.Count)],area.transform.position + new Vector3(offsetX, offsetY, 0), Random.rotation, this.transform);
            lootList.Add(loot);
        }
    }

    public void ActivateTheONE(bool theOne) {
        foreach (var agent in enemyAgents)
        {
        }
    }

    public void RestartRoom()
    {
        foreach (var cube in pointcubes)
        {
            cube.SetActive(true);
        }

        // if(ball != null){
        //     ball.ReInitPosition();
        // }

        // foreach (var spawner in boulderSpawners)
        // {
        //     spawner.GetComponent<BoulderSpawner>().ResetSpawner();
        // }
        foreach(var loot in lootList){
            Destroy(loot);
        }
        foreach(var junk in junkList){
            Destroy(junk);
        }

        lootList = new List<GameObject>();
        junkList = new List<GameObject>();

        SpawnLoot();
    }

    public bool isComplete()
    {
        //var fuckyou = this.transform.GetComponentsInChildren<Loot>().Count();
        //Debug.Log("not complete, fuck you");
        return this.transform.GetComponentsInChildren<Loot>().Count() <= 1;

        //return this.transform.GetComponentsInChildren<Loot>().Count() <= 1;
        //return junkList.Count <= 1; //the final item hasn't deleted itself yet so it will be 1 left not 0
    }

    public void DestroyRoom()
    {
        // foreach (var spawner in boulderSpawners)
        // {
        //     spawner.GetComponent<BoulderSpawner>().ResetSpawner();
        // }
        Destroy(this.gameObject);
    }
}
