using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleDrill : Hazard
{
    private float spinCounter = 99f;
    private float spinTime = 3f;
    private bool attackfailed = false;

    private Vector3 rotationSpeed = new Vector3(0, 0, 600);
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
        }
        else if(attackfailed){
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
            var agent = col.gameObject.GetComponent<BattleBotAgent>();
            DoDamage(damage, agent.gameObject);
            attackfailed = false;      
        }
        if(spinCounter < spinTime && col.gameObject.TryGetComponent<Hazard>(out Hazard haz)){
            DoDamage(damage, haz.gameObject);
            if(haz.damageable){
                attackfailed = false;
            }
        }
    }
}
