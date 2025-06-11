using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForkliftLoot : MonoBehaviour
{

    public Vector3 startingPos;

    public ForkliftAgent player;

    private ForkliftRoom room;

    private GameObject roomGO;

    private float distanceToClosestGoal = 999f;
    private float closestDistanceToPlayer = 999f;
    public bool inGoal = false;
    private float greatestLootHeight;

    private bool neverBeenTouched = true;

    private float touchyTimer = 0f;

    void Start() 
    {
        startingPos = this.transform.position;
        greatestLootHeight = startingPos.y;
        roomGO = this.transform.parent.gameObject;
        room = roomGO.GetComponent<ForkliftRoom>();
        player = this.transform.parent.parent.Find("Agent").GetComponent<ForkliftAgent>();
    }

    void OnTriggerStay(Collider col)
    {

        if (col.gameObject.CompareTag("goal"))
        {
            if(!inGoal){
                distanceToClosestGoal = getClosestGoalDistance();
                player.LootHitGoal(); // remember to give reward
                Debug.Log("you did it, stored properly");
                inGoal = true;
            }
        }

        if (col.gameObject.CompareTag("theDeadZone"))
        {
            player.LootIsDead(); // remember to give reward
        }
    }

    void OnCollisionStay(Collision col)
    {
        if (col.gameObject.CompareTag("agent"))
        {
            if(neverBeenTouched)
            {
                neverBeenTouched = false;
                //Debug.Log("first touch");
                player.FirstTouch(); // remember to give reward
            }
            else if(inGoal && touchyTimer >= 5f){
                touchyTimer = 0;
                player.LeaveItAlone();
                //Debug.Log("you are being punished for harassing");
            }

        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.gameObject.CompareTag("goal"))
        {
            if(inGoal){
                player.LootLeftGoal(); // remember to give reward
                Debug.Log("you are being punished for pushing a thing off the goal");
                inGoal = false;
            }      
        }
    }

    private float getClosestGoalDistance()
    {
        float distance = 999f;
        if(!room.isComplete()){

                float goalDist = Vector3.Distance(room.goal.transform.position, transform.position);
                if (goalDist < distance)
                {
                    distance = goalDist;
                }
            
        }

        return distance;
    }

    private float getPlayerDistance()
    {
        float distance = 999f;
        if(!room.isComplete()){

                float goalDist = Vector3.Distance(player.transform.position, transform.position);
                if (goalDist < distance)
                {
                    distance = goalDist;
                }
            
        }

        return distance;
    }

    private void Update()
    {
        if(room.isComplete()){
            return;
        }

        if(inGoal){
            touchyTimer += Time.deltaTime;
            //may need to change this
            transform.tag = "Untagged";
            return;
        }


        if (!inGoal)
        {
            var calcGoalDist = getClosestGoalDistance();
            transform.tag = "loot";

            if (calcGoalDist < distanceToClosestGoal - 0.5f)
            {
                distanceToClosestGoal = calcGoalDist;
                player.LootCloserToGoal(this.transform.position);
            }
        }

        if (!inGoal)
        {
            var calcGoalDist = getPlayerDistance();

            if (calcGoalDist < closestDistanceToPlayer - 0.5f)
            {
                closestDistanceToPlayer = calcGoalDist;
                player.PlayerCloserToLoot();
            }
        }

        if(transform.position.y > greatestLootHeight){
            var inc = transform.position.y - greatestLootHeight;
            greatestLootHeight = transform.position.y;
            player.GreatestLootHeightIncreased(inc);
        }
    }

    public void ReInitPosition(){
        if(startingPos == null){
            return;
        }
        inGoal = false;
        this.transform.position = startingPos;
        distanceToClosestGoal = 999f;
        closestDistanceToPlayer = 999f;
        greatestLootHeight = startingPos.y;
        this.transform.rotation = Random.rotation; 
        this.gameObject.GetComponent<Rigidbody>().linearVelocity = new Vector3(0f,0f,0f);
        this.gameObject.GetComponent<Rigidbody>().angularVelocity = new Vector3(0f,0f,0f);
        neverBeenTouched = true;
        touchyTimer = 0f;
    }
}
