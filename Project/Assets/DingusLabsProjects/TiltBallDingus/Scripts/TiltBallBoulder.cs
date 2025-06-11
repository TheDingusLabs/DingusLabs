using UnityEngine;

public class TiltBallBoulder : MonoBehaviour
{
    private Vector3 startingPos;
    public bool dead = false;
    public TiltBallEnvController controller;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //this is dumb
        controller = this.transform.parent.parent.parent.GetComponent<TiltBallEnvController>();
        //startingPos = this.transform.position;
    }

    // public void ReturnToStartingPos(){
    //     this.transform.position = spawnPoint.transform.position;
    //     this.gameObject.GetComponent<Rigidbody>().linearVelocity = new Vector3(0,0,0);
    //     dead = false;
    // }

    protected virtual void OnTriggerStay(Collider col)
    {
        if(col.gameObject.CompareTag("theDeadZone") && !dead)
        {
            Die();
        }
        if(col.gameObject.CompareTag("deathWall") && !dead)
        {
            //Debug.Log("we do be hitting deathwalls yo");
            controller.insideDeathWall();
        }
    }

    protected virtual void OnTriggerEnter(Collider col){
        if(col.gameObject.CompareTag("deathWall") && !dead)
        {
            //Debug.Log("we do be hitting deathwalls yo");
            controller.enteredOutofBounds();
        }
    }

    protected virtual void OnCollisionStay(Collision col)
    {
        if(col.gameObject.CompareTag("wall") && !dead)
        {
            controller.hitWall();
            //Debug.Log("hit wall");
        }
    }

    private void Die(){
        dead = true;
    }

    // Update is called once per frame
    void Update()
    {
        this.gameObject.GetComponent<Rigidbody>().AddForce(new Vector3(0,-9.8f*0.5f,0),ForceMode.Acceleration);
        // if(this.gameObject.GetComponent<Rigidbody>().linearVelocity.magnitude > 0.01f )
        // Debug.Log(this.gameObject.GetComponent<Rigidbody>().linearVelocity.magnitude);
    }
}
