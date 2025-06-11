using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleBoulder : Hazard
{
    private Dictionary<GameObject, float> hurtAgents = new Dictionary<GameObject, float>();
    public float timeMostRecentlyTouched = 10f;
    // Start is called before the first frame update
    void Start()
    {

    }

    void OnTriggerStay(Collider col)
    {
        if(col.gameObject.CompareTag("theDeadZone")){
            owner.GetComponent<BattleBotAgentSisyphus>().BoulderFellDownTheWell();
        }

        var damage = Math.Min(Math.Max(this.GetComponent<Rigidbody>().linearVelocity.magnitude*8f,5f),35f); 
        if (timeMostRecentlyTouched < 1f && col.gameObject.CompareTag("agent"))
        { 
            var agent = col.gameObject.GetComponent<BattleBotAgent>();
            if(agent.gameObject != owner.gameObject)
            {
                if(!hurtAgents.ContainsKey(agent.gameObject)){
                    DoDamage(damage, agent.gameObject);
                    hurtAgents.Add(agent.gameObject, Time.time);
                }
                else if(Time.time - hurtAgents[agent.gameObject] > 2f){
                    DoDamage(damage, agent.gameObject);
                    //additional reward, he needs lots of encouragement to move but then there's a need to reward smacking enemies more
                    if(owner.GetComponent<BattleBotAgent>().team == agent.GetComponent<BattleBotAgent>().team){
                        owner.GetComponent<BattleBotAgent>().AddReward(-1 * damage/20f);
                    }
                    else{
                        //Debug.Log("Sisyphus hit a dude and got a reward");
                        owner.GetComponent<BattleBotAgent>().AddReward(damage/5f);
                    }
                    
                    
                    hurtAgents.Remove(agent.gameObject);
                    hurtAgents.Add(agent.gameObject, Time.time);
                }   
            } 


        }

        if(col.gameObject.TryGetComponent<Hazard>(out Hazard haz)){
            DoDamage(damage, haz.gameObject);
        }
    }

    void Update()
    {
        timeMostRecentlyTouched += Time.deltaTime;
        if(timeMostRecentlyTouched < 0.5f){
            owner.GetComponent<BattleBotAgent>().AddReward(this.GetComponent<Rigidbody>().linearVelocity.magnitude * Time.deltaTime * 0.05f);
        }
    }
}
