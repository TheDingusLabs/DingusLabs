using System.Collections;
using System.Collections.Generic;
using Unity.Sentis.Layers;
using UnityEngine;

public class BattleHorn : Hazard
{
    public bool charging = false;
    public Material nonchargingColour;
    public Material chargingColour;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
    }

    public void BeginHorn(){
        charging = true;
        ChangeHornColour();
    }

    public void EndHorn(){
        owner.GetComponent<BattleBotAgentHorner>().HitSomethingWhilstCharging();
        charging = false;
        ChangeHornColour();
    }

    private void ChangeHornColour(){
        var col = charging ? chargingColour : nonchargingColour;

        // Iterate over each child of the GameObject
        foreach (Transform hornholder in transform)
        {
            foreach (Transform child in hornholder)
            {
                Renderer childRenderer = child.GetComponent<Renderer>();
                if (childRenderer != null)
                {
                    // Change the material of the child object
                    childRenderer.material = col;
                }
            }
        }
    }

    void OnTriggerStay(Collider col)
    {
        var ownerForwardVelocity = Vector3.Project(owner.GetComponent<Rigidbody>().linearVelocity, transform.forward);
        var damage = 15f + Mathf.Min(30,ownerForwardVelocity.magnitude * 1.5f);
        if (charging && col.gameObject != owner && (col.gameObject.CompareTag("agent") || col.gameObject.CompareTag("deadAgent")))
        { 
            var agent = col.gameObject.GetComponent<BattleBotAgent>();
            DoDamage(damage, agent.gameObject);
            var dirvector = (agent.transform.position - owner.gameObject.transform.position).normalized;

            agent.GetComponent<Rigidbody>().AddForce(dirvector*500 + ownerForwardVelocity*100, ForceMode.Acceleration);
            EndHorn();
        }
        if(charging && col.gameObject.TryGetComponent<Hazard>(out Hazard haz)){
            DoDamage(damage, haz.gameObject);
            if(haz.damageable){
                EndHorn();
            }
            else if(col.tag == "boulder"){
                var dirvector = (haz.transform.position - owner.gameObject.transform.position).normalized;
                haz.GetComponent<Rigidbody>().AddForce(dirvector*500 + ownerForwardVelocity*50, ForceMode.Acceleration);
                EndHorn();
                //owner.GetComponent<BattleBotAgent>().observeAttackFailed(1);
            }
        }

        if(charging && (col.gameObject.CompareTag("wall") || col.gameObject.CompareTag("walkableSurface") || col.gameObject.CompareTag("pit"))){
            owner.GetComponent<BattleBotAgent>().observeAttackFailed(1);
            EndHorn();
        }
    }
}
