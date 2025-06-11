using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathWall : Hazard
{
    // Update is called once per frame
    void Update()
    {

    }


    void OnTriggerStay(Collider col)
    {
        var damage = Time.deltaTime*30f;
        if (col.gameObject.CompareTag("agent") || col.gameObject.CompareTag("deadAgent"))
        { 
            var agent = col.gameObject.GetComponent<BattleBotAgent>();
            DoDamage(damage, agent.gameObject);    
        }

        if(gameObject.TryGetComponent<Hazard>(out Hazard haz)){
            DoDamage(damage, haz.gameObject);  
        }
    }
}
