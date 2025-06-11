using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class BattleLaser : Hazard
{
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerStay(Collider col){

        if ( col.gameObject.CompareTag("agent") || col.gameObject.CompareTag("deadAgent") )
        { 
            var agent = col.gameObject.GetComponent<BattleBotAgent>();
            if(agent.gameObject && owner != agent.gameObject && agent.team != owner.GetComponent<BattleBotAgent>().team){
                owner.GetComponent<BattleBotAgent>().AddReward(Time.deltaTime * 0.5f);
                //UnityEngine.Debug.Log("Rewarding sniper for targetting well");
            }
        } 
    }
}
