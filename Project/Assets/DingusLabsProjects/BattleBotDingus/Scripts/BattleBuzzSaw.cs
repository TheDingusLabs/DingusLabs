using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleBuzzSaw : Hazard
{
    private float spinCounter = 99f;
    public float spinTime = 3f;
    private bool attackfailed = false;

    private Vector3 rotationSpeed = new Vector3(0, 600, 0);
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        spinCounter+= Time.deltaTime;
        Spin();
    }

    void Spin()
    {
        if(spinCounter < spinTime)
        {
            this.transform.Rotate(rotationSpeed * Time.deltaTime);
        }   //if he didn't start drilling an enemy at the start then let's just punish, he obviously wasn't trying properly
        else if(spinCounter > 0.6f && attackfailed){
            attackfailed = false;
            owner.GetComponent<BattleBotAgent>().observeAttackFailed();
        }
    }

    public void BeginSpin(float drillDuration){
        spinTime = drillDuration;
        spinCounter = 0;
        attackfailed = true;
    }

    void OnTriggerStay(Collider col)
    {
        var damage = Time.deltaTime*20f;
        if (spinCounter < spinTime && (col.gameObject.CompareTag("agent") || col.gameObject.CompareTag("deadAgent")))
        { 
            if(owner.GetComponent<BattleBotAgent>().isTargetDummy && col.gameObject.GetComponent<BattleBotAgent>().isTargetDummy){
                return;
            }
            var agent = col.gameObject.GetComponent<BattleBotAgent>();
            DoDamage(damage, agent.gameObject);
            attackfailed = false;      
        }
        if(spinCounter < spinTime && col.gameObject.TryGetComponent<Hazard>(out Hazard haz)){
            DoDamage(damage, haz.gameObject);
            if(haz.damageable)
            {
                attackfailed = false;  
            }
        }
    }
}
