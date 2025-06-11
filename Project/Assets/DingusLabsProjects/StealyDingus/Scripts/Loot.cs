using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loot : MonoBehaviour
{
    public float value = 0;

    public Vector3 startingPos;

    public StealyDingusAgent player;

    private StealyDingusRoom room;

    private GameObject roomGO;

    private float distanceToClosestGoal = 20f;
    private float goalDistTimer = 0f;

    void Start() 
    {
        startingPos = this.transform.position;
        roomGO = this.transform.parent.gameObject;
        room = roomGO.GetComponent<StealyDingusRoom>();

        //Debug.Log(this.transform.parent.parent.name);
        player = this.transform.parent.parent.Find("Agent").GetComponent<StealyDingusAgent>();
        // foreach (Transform transform in this.transform.parent.parent.Find("Agent"))
        // {
        //     if (transform.CompareTag("agent"))
        //     {
        //         player = transform.gameObject.GetComponent<PhysicsPlaygroundAgent>();
        //     }
        // }
    }

    void OnTriggerStay(Collider col)
    {

        if (col.gameObject.CompareTag("goal"))
        {

            //col.gameObject.SetActive(false);
            distanceToClosestGoal = getClosestGoalDistance();
            player.BallHitGoal();
            Destroy(this.gameObject);
        }

        // if (col.gameObject.CompareTag("deathWall"))
        // {
        //     player.BallDied();
        // }
        // if (col.gameObject.CompareTag("theDeadZone"))
        // { 
        //     player.BallDied();
        // }
    }

    private float getClosestGoalDistance()
    {
        float distance = 20f;
        if(room.pointcubes.Count  >= 1)
        {
            foreach(var goal in room.pointcubes)
            {
                float goalDist = Vector3.Distance(goal.transform.position, transform.position);
                if (goalDist < distance)
                {
                    distance = goalDist;
                }
            }
        }

        return distance;
    }

    private void Update()
    {
        if(room.pointcubes.Count < 1){
            return;
        }

        goalDistTimer += Time.deltaTime;
        if (goalDistTimer > 0.2f)
        {
            var calcGoalDist = getClosestGoalDistance();

            if (calcGoalDist < distanceToClosestGoal - 1f)
            {
                distanceToClosestGoal = calcGoalDist;
                player.BallCloserToGoal();
            }
        }
    }

    public void ReInitPosition(){
        if(startingPos == null){
            return;
        }
        this.transform.position = startingPos;
        this.gameObject.GetComponent<Rigidbody>().linearVelocity = new Vector3(0f,0f,0f);
        this.gameObject.GetComponent<Rigidbody>().angularVelocity = new Vector3(0f,0f,0f);
    }
}
