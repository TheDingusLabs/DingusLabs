using UnityEngine;

public class TiltBallGoal : MonoBehaviour
{
    TiltBallEnvController cont;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //TODO this code is so bad, gotta be a better way of doing this
        cont = this.transform.parent.parent.parent.GetComponent<TiltBallEnvController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other){
        if(other.CompareTag("ball")){
            cont.hitGoal();
            this.gameObject.SetActive(false);
        }
    }
}
