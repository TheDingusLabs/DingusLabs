using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinnySpawner : MonoBehaviour
{
    private float spawnRate = 1.0f;
    private float spawnTimer = 7f;
    public GameObject gameCube;
    public List<GameObject> SpinnyPrefabs;
    private void FixedUpdate()
    {

    }
    
    private void Update()
    {
        spawnTimer += Time.deltaTime;
        if(spawnTimer >= spawnRate)
        {
            spawnTimer = 0;
            Instantiate(SpinnyPrefabs[Random.Range(0, SpinnyPrefabs.Count - 1)], this.transform);
            var cube = Instantiate(gameCube, this.transform);
            cube.transform.position += new Vector3(0f, -50f, 0f);
        }
    }

    public void KillChildren()
    {
        foreach (var child in this.GetComponentsInChildren<Spinny>())
        {
            Destroy(child.gameObject);
        }
        foreach (var child in this.GetComponentsInChildren<GoalCube>())
        {
            Destroy(child.gameObject);
        }
    }
}
