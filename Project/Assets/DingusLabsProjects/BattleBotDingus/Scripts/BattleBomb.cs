using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleBomb : Hazard
{
    public BattleExplosionIndicator indicator;
    private float timeToLive = 1.55f;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(
            ExplosionCountdown()
        );
    }

    IEnumerator ExplosionCountdown()
    {
        yield return new WaitForSeconds(timeToLive);

        Explode();
    }

    void Explode(){
        float radius = 2.5f;         // Radius of the sphere
        float maxDistance = 0f;   // Maximum distance the sphere will travel
        Vector3 direction = Vector3.forward; // Direction to cast the sphere
        Vector3 origin = transform.position;

        RaycastHit[] hits = Physics.SphereCastAll(origin, radius, direction, maxDistance);
        // Iterate through all the hit objects

        bool didHitplayer = false;

        foreach (RaycastHit hit in hits)
        {
            var damage = 30f;
            
            var hitobj = hit.collider.gameObject;
            var hitPlayer = hitobj.GetComponent<BattleBotAgent>();
            //was originally avoiding self damaging but this is funner
            // if(hitPlayer != null && hit.collider.gameObject != owner)
            // {
            //     hitPlayer.TakeDamage(30, this.owner.GetComponent<BattleBotAgent>());
            // }
            if(hitPlayer != null)
            {
                didHitplayer = true;
                DoDamage(damage, hitPlayer.gameObject);
            }
            var hitrb = hitobj.GetComponent<Rigidbody>();
            if(hitrb != null){
                var dirvector = (hitrb.transform.position - this.gameObject.transform.position).normalized;
                hitrb.AddForce(dirvector*800, ForceMode.Acceleration);
            }

            if(hitobj.gameObject.TryGetComponent<Hazard>(out Hazard haz)){
                DoDamage(damage, haz.gameObject);
                if(haz.damageable){
                    didHitplayer = true;
                }
            }

            //Debug.Log("Hit object: " + hit.collider.name);
        }

        if(didHitplayer == false){
            owner.GetComponent<BattleBotAgent>().observeAttackFailed(0.7f);
        }

        var rad = Instantiate(indicator, transform.position, transform.rotation);
        rad.SetRadius(radius*1.3f);
        Destroy(this.gameObject, 0f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
