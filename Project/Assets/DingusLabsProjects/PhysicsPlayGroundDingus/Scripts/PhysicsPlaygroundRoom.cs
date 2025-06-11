using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PhysicsPlaygroundRoom : MonoBehaviour
{
    public List<GameObject> pointcubes;

    public List<GameObject> enemyAgents;

    public List<GameObject> boulderSpawners;

    public GameObject playerSpawnArea;

    public float spawnDir = 0f;

    public bool theOne = false;

    private Ball ball;
    public List<GameObject> tpPoints;

    private void Start()
    {
        pointcubes = new List<GameObject>();
        boulderSpawners = new List<GameObject>();

        foreach (Transform transform in this.transform)
        {
            if (transform.CompareTag("goal"))
            {
                transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z); 
                pointcubes.Add(transform.gameObject);
            }
            else if (transform.CompareTag("spawner"))
            {
                boulderSpawners.Add(transform.gameObject);
            }
            else if (transform.CompareTag("ball"))
            {
                ball = transform.gameObject.GetComponent<Ball>();
            }
            else if (transform.tag == "tppoint"){
                tpPoints.Add(transform.gameObject);
            }

        }
    }

    public void ActivateTheONE(bool theOne) {
        foreach (var agent in enemyAgents)
        {
        }
    }

    public float GetMaxScore(){
        return pointcubes.Count();
    }

    public float GetCurrentScore(){
        return GetMaxScore() - pointcubes.Count(x => x.activeInHierarchy == true);
    }

    public List<GameObject> GetTPPoints(){
        return tpPoints;
    }

    public Ball GetBall(){
        return ball;
    }


    public void RestartRoom()
    {
        foreach (var cube in pointcubes)
        {
            cube.SetActive(true);
        }

        if(ball != null){
            ball.ReInitPosition();
        }

        foreach (var spawner in boulderSpawners)
        {
            spawner.GetComponent<BoulderSpawner>().ResetSpawner();
        }
    }

    public bool isComplete()
    {
        return pointcubes.Count(x => x.activeInHierarchy == true) == 0;
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
