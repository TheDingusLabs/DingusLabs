using UnityEngine;

public class EscapeDoor : MonoBehaviour
{
    public bool open {get; set;} = false;
    public bool previouslyOpened {get; set;} = false;

    Renderer rend;
    BoxCollider col;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rend = this.gameObject.GetComponent<Renderer>();
        col = this.gameObject.GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Open(){
        open = true;
        previouslyOpened = true;

        rend.enabled = false;
        col.enabled = false;
    }

    public void Close(){
        if(open){
            open = false;

            rend.enabled = true;
            col.enabled = true;
        }
    }
}
