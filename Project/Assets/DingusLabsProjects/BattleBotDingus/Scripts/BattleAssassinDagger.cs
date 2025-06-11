using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleAssassinDagger : Hazard
{
    private Vector3 rotationSpeed = new Vector3(0, 600, 0);
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void BeginSpin(float drillDuration){

    }

    void OnTriggerStay(Collider col)
    {
        if (owner.GetComponent<BattleBotAgentAssassin>().IsCloaked() && (col.gameObject.CompareTag("bottom") || col.gameObject.CompareTag("front") || col.gameObject.CompareTag("back") || col.gameObject.CompareTag("side") || col.gameObject.CompareTag("top"))){
            var tagname = col.gameObject.tag;
            float damage = 25f;
            switch (tagname)
            {
                case "front":
                    damage = 5f;
                    break;
                case "side":
                    damage = 10f;
                    break;
                default:
                    // Code to execute if none of the above cases match
                    break;
            }
            DoDamage(damage + damage*owner.GetComponent<BattleBotAgentAssassin>().assassinMulti,col.gameObject.transform.parent.parent.gameObject);
            owner.GetComponent<BattleBotAgentAssassin>().UnCloak();
        }
        //var damage = Time.deltaTime*20f;
        // if ((col.gameObject.CompareTag("agent") || col.gameObject.CompareTag("deadAgent")))
        // { 
        //     if(owner.GetComponent<BattleBotAgent>().isTargetDummy && col.gameObject.GetComponent<BattleBotAgent>().isTargetDummy){
        //         return;
        //     }
        //     var agent = col.gameObject.GetComponent<BattleBotAgent>();
        //     DoDamage(damage, agent.gameObject);
        //     attackfailed = false;      
        // }
        // if(spinCounter < spinTime && col.gameObject.TryGetComponent<Hazard>(out Hazard haz)){
        //     DoDamage(damage, haz.gameObject);
        //     if(haz.damageable)
        //     {
        //         attackfailed = false;  
        //     }
        // }
    }
}
