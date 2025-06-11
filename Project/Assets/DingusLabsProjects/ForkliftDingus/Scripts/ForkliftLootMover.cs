using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForkliftLootMover : MonoBehaviour
{
    public bool movedLoot = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerStay(Collider col){
        if (!movedLoot && col.gameObject.CompareTag("loot"))
        {
            Bounds bounds = this.transform.GetComponent<Collider>().bounds;
            float offsetX = Random.Range(-bounds.extents.x, bounds.extents.x);
            float offsetZ = Random.Range(-bounds.extents.z, bounds.extents.z);
            // Debug.Log(offsetX);
            // Debug.Log(offsetZ);
            col.transform.position = this.transform.position +  new Vector3(offsetX, 1f, offsetZ);
            movedLoot = true;

        }
    }

}
