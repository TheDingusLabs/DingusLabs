using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleKungFuHand : Hazard
{
    // Start is called before the first frame update
    void Start()
    {
        float radius = 0.7f;         // Radius of the sphere
        float maxDistance = 0f;   // Maximum distance the sphere will travel
        Vector3 direction = Vector3.forward; // Direction to cast the sphere
        Vector3 origin = transform.position;

        bool didHitplayer = false;

        RaycastHit[] hits = Physics.SphereCastAll(origin, radius, direction, maxDistance);
        // Iterate through all the hit objects
        foreach (RaycastHit hit in hits)
        {
            var damage = 20;
            var hitobj = hit.collider.gameObject;
            var hitPlayer = hitobj.GetComponent<BattleBotAgent>();

            if(hitPlayer != null && hitPlayer.gameObject != owner)
            {
                if(owner.GetComponent<BattleBotAgent>().isTargetDummy && hitPlayer.isTargetDummy)
                {
                    
                }
                else{
                    DoDamage(damage, hitPlayer.gameObject);
                }
                
                didHitplayer = true;
            }
            var hitrb = hitobj.GetComponent<Rigidbody>();
            if(hitrb != null && hitPlayer != owner){
                var dirvector = (hitrb.transform.position - this.gameObject.transform.position).normalized;
                hitrb.AddForce(dirvector*500, ForceMode.Acceleration);
            }

            if(hitobj.gameObject.TryGetComponent<Hazard>(out Hazard haz)){
                DoDamage(damage, haz.gameObject);
                if(haz.damageable){
                    didHitplayer = true;
                }
            }
            //Debug.Log("Hit object: " + hit.collider.name);
        }

        if(!didHitplayer){
            owner.GetComponent<BattleBotAgent>().observeAttackFailed(1f);
        }

        Destroy(this.gameObject, 0.08f);
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position = this.transform.position + transform.forward * Time.deltaTime * 12f; 
    }
}
