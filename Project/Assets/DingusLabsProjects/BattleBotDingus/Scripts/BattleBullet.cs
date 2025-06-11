using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class BattleBullet : Hazard
{
    public float speed = 8f;
    public float range = 30f;
    public float damage = 35f;
    private bool didDamage = false;
    void Start()
    {
        
    }

    void FixedUpdate()
    {
        this.transform.position += this.transform.forward * speed * 0.15f;
    }

    void Update(){
        if(Vector3.Distance(this.gameObject.transform.position, owner.gameObject.transform.position) > range){
            if(didDamage == false){
                //owner.GetComponent<BattleBotAgent>().observeAttackFailed(1f);
            }
            Destroy(this.gameObject);
        }
    }

    void OnTriggerEnter(Collider col){
        if (col.gameObject.CompareTag("agent") || col.gameObject.CompareTag("deadAgent"))
        { 
            var agent = col.gameObject.GetComponent<BattleBotAgent>();
            if(agent.gameObject && owner != agent.gameObject){
                DoDamage(damage, agent.gameObject);
                didDamage = true;
            }
        } else if(col.gameObject.TryGetComponent<Hazard>(out Hazard haz)){
                DoDamage(damage, haz.gameObject);
                didDamage = true;
            }
        // else if(col.gameObject.CompareTag("wall")){
        //     if(didDamage == false){
        //         owner.GetComponent<BattleBotAgent>().observeAttackFailed(2f);
        //     }

        //     Destroy(this.gameObject);
        // }
    }
}
