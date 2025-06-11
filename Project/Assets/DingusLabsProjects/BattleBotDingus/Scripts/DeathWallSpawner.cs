using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using UnityEngine;

public class DeathWallSpawner : Hazard
{
    private float activationTimer = 0f;
    private float activationTime = 0.65f;
    public bool readyForSpawning = false;
    void Start()
    {
        hp = 15;
    }

    // Update is called once per frame
    void Update()
    {
        activationTimer += Time.deltaTime;
        if(!readyForSpawning && activationTimer > activationTime){
            readyForSpawning = true;
            owner.GetComponent<BattleBotAgentDeathWaller>().CheckIfWallIsOkayToSpawn();
        }

        if(hp <= 0){
            Die();
        }
    }

    void OnTriggerStay(Collider col){
        if(col.gameObject.CompareTag("theDeadZone"))
        {
            Die();
        }
    }

    public void Die(){
        readyForSpawning = false;
        Destroy(this.gameObject);
        owner.GetComponent<BattleBotAgentDeathWaller>().SpawnerDied();
    }

    public void Despawn(){

    }

}
