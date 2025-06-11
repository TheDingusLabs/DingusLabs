using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class BattleBall : Hazard
{
    public GameObject spikes;

    private float spikesCounter = 0;
    private float spikeActiveAt = 0.8f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        spikesCounter += Time.deltaTime;

        if(spikesCounter > spikeActiveAt)
        {
            spikes.SetActive(true);
        }


        if(spikesCounter > 8)
        {
            owner.GetComponent<BattleBotAgent>().observeAttackFailed(0.4f);
            Destroy(this.gameObject);
        }
    }

    void OnCollisionStay(Collision col){
        var damage = 20;
        if (spikesCounter > spikeActiveAt && (col.gameObject.CompareTag("agent") || col.gameObject.CompareTag("deadAgent")) )
        { 
            var agent = col.gameObject.GetComponent<BattleBotAgent>();
            if(agent.gameObject && owner != agent.gameObject){
                DoDamage(damage, agent.gameObject);
                Destroy(this.gameObject);        
            }
        } else if(spikesCounter > spikeActiveAt && col.gameObject.TryGetComponent<Hazard>(out Hazard haz)){
                DoDamage(damage, haz.gameObject);
            }
    }
}
