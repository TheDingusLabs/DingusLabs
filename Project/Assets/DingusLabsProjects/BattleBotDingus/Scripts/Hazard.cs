using System.Collections;
using System.Collections.Generic;
using System.Security.Permissions;
using UnityEngine;

public class Hazard : MonoBehaviour
{
    public float hp;
    public bool damageable = false;

    public GameObject owner;
    public virtual void TookDamage(float damage, BattleBotAgent damager){
        //hazards can't be hurt by harzards owned by the same agent, that may need to be changed in the future
        if(damageable && damager.gameObject != owner){
            this.transform.parent.gameObject.GetComponent<BattleBotEnvController>().AHazardWasHurt(owner.GetComponent<BattleBotAgent>(), damager, damage);

            hp -= damage;
        }
    }

    public virtual void DoDamage(float damage, GameObject damagee)
    {
        if(damagee.TryGetComponent<BattleBotAgent>(out BattleBotAgent damagedAgent))
        {
            damagedAgent.TakeDamage(damage, owner.GetComponent<BattleBotAgent>());
        }

        if(damagee.TryGetComponent<Hazard>(out Hazard haz))
        {
            haz.TookDamage(damage, owner.GetComponent<BattleBotAgent>());
        }

    }
}
