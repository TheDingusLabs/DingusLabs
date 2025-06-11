using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ForkliftRoom : MonoBehaviour
{
    public List<GameObject> enemyAgents;

    public List<GameObject> boulderSpawners;

    public GameObject playerSpawnArea;

    public List<GameObject> lootList;

    public GameObject goal;

    public float spawnDir = 0f;

    public bool theOne = false;

    public List<GameObject> lootMovers;


    private void Start()
    {
        lootList = new List<GameObject>();
        boulderSpawners = new List<GameObject>();

        foreach (Transform transform in this.transform)
        {
            if (transform.CompareTag("loot")){
                //transform.GetComponent<ForkliftLoot>();
                lootList.Add(transform.gameObject);
            }
            else if (transform.CompareTag("spawner"))
            {
                boulderSpawners.Add(transform.gameObject);
            }
            else if (transform.CompareTag("spawner"))
            {
                boulderSpawners.Add(transform.gameObject);
            }
            else if (transform.CompareTag("ex")){
                lootMovers.Add(transform.gameObject);
            }
        }

        moveLoot();
    }

    public void moveLoot(){
        foreach(var mover in lootMovers){
            mover.GetComponent<ForkliftLootMover>().movedLoot = false;
        }
    }

    public void ActivateTheONE(bool theOne) {
        foreach (var agent in enemyAgents)
        {
        }
    }

    public void RestartRoom()
    {
        foreach (var loot in lootList)
        {
            loot.GetComponent<ForkliftLoot>().ReInitPosition(); 
        }
        foreach (var spawner in boulderSpawners)
        {
            spawner.GetComponent<BoulderSpawner>().ResetSpawner();
        }

        moveLoot();
    }

    public bool isComplete()
    {
        foreach(var loot in lootList){
            if(!loot.GetComponent<ForkliftLoot>().inGoal)
            {return false;}
        }
        return true;
    }

    public void DestroyRoom()
    {
        foreach (var spawner in boulderSpawners)
        {
            spawner.GetComponent<BoulderSpawner>().ResetSpawner();
        }
        Destroy(this.gameObject);
    }
}
