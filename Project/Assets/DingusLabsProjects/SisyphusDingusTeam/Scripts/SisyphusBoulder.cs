using UnityEngine;

public class SisyphusBoulder : MonoBehaviour
{
    SisyphusEnvController cont;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cont = this.transform.parent.GetComponent<SisyphusEnvController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other){
        if(other.CompareTag("theDeadZone")){
            cont.TheBoulderHasFallen();
        }
    }
}
