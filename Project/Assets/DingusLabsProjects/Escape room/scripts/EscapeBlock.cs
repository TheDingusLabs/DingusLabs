using UnityEngine;

public class EscapeBlock : MonoBehaviour
{
    private EscapeEnvController cont;

    public void Start()
    {
         cont = this.transform.parent.parent.GetComponent<EscapeEnvController>();
    }

    protected virtual void OnTriggerStay(Collider col)
    {
        if(col.gameObject.CompareTag("purpleGoal"))
        {
            Destroy(col.gameObject);
            cont.PlayerPushedBlockToRewardZone();
        }
        if(col.gameObject.CompareTag("theDeadZone"))
        {
            cont.BlockOffTheEdge();
        }
    }

    public void Update()
    {
        if(this.GetComponent<Rigidbody>().linearVelocity.magnitude > 1f){
            cont.BlockPushed();
        }   
    }

}
