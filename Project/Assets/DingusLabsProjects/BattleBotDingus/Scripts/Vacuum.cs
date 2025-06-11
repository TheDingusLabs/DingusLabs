using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vacuum : Hazard
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
            col.gameObject.transform.position = Vector3.MoveTowards(col.gameObject.transform.position, owner.transform.position, 2 * Time.deltaTime);
            //Debug.Log("in vacuum, should be sucking");
            var agent = col.gameObject.GetComponent<BattleBotAgent>();
            DoDamage(damage, agent.gameObject);    
        }

        if(gameObject.TryGetComponent<Hazard>(out Hazard haz)){
            DoDamage(damage, haz.gameObject);  
        }
    }
}
