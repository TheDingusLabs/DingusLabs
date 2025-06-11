using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct HurtAgent
{
    // Fields
    public GameObject hurtAgent;
    public float TimeHit;

    // Constructor
    public HurtAgent(GameObject _hurtAgent, float _TimeHit)
    {
        hurtAgent = _hurtAgent;
        TimeHit = _TimeHit;
    }

} 

public class HurtBox : Hazard
{
    private Dictionary<GameObject, float> hurtAgents = new Dictionary<GameObject, float>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider col)
    {
        var damage = 35f;
        if (col.gameObject.CompareTag("top") || col.gameObject.CompareTag("bottom"))
        { 
            bool doDamage = false;
            //this code sucks
            var agentGO = col.gameObject.transform.parent.parent.gameObject;
            var agent = agentGO.GetComponent<BattleBotAgent>();
            if(agent != null && hurtAgents.ContainsKey(agentGO)){
            //we onyl allow these hits if mostly upright
            float uprightThreshold = 0.85f;
            float uprightness = Vector3.Dot(transform.up, Vector3.up);
                if(Time.time - hurtAgents[agentGO] > 1.5 && uprightness >= uprightThreshold){
                    doDamage = true;
                    hurtAgents.Remove(agentGO);
                }
            }
            else if(agent != null){
                doDamage = true;
            }

            if(doDamage){
                DoDamage(damage, agent.gameObject);
                owner.GetComponent<BattleBotAgent>().TakeEnemyHurtingAction();
                hurtAgents.Add(agentGO, Time.time);
            }    
            //attackfailed = false;      
        }

        if(gameObject.TryGetComponent<Hazard>(out Hazard haz)){
            DoDamage(damage, haz.gameObject);
        }
    }
}
