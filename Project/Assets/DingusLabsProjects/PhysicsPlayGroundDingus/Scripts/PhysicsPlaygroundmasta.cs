using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsPlaygroundmasta : MonoBehaviour
{
    public PhysicsPlaygroundRoom currentRoom;
    public List<GameObject> rooms;
    public int currentRoomNo = 0;
    public GameObject spawnArea;
    public TMPro.TextMeshPro levelText;

    public GameObject ball;

    public GameObject playerSpawnArea;
    // Start is called before the first frame update
    void Start()
    {
        var newRoom = Instantiate(rooms[currentRoomNo], spawnArea.transform.position + new Vector3(0f, 0f, 0), spawnArea.transform.rotation, this.transform.parent);
        currentRoom = newRoom.GetComponent<PhysicsPlaygroundRoom>();

        playerSpawnArea.transform.position = currentRoom.playerSpawnArea.transform.position;
    }

    public void StartNextLevel(){
        currentRoom.DestroyRoom();
        currentRoomNo++;
        var newRoom = Instantiate(rooms[currentRoomNo], spawnArea.transform.position + new Vector3(0f, 0f, 0), spawnArea.transform.rotation, this.transform.parent);
        currentRoom = newRoom.GetComponent<PhysicsPlaygroundRoom>();
    }

    public void ActivateTheOne(bool theOne){
        if(theOne){
            currentRoom.ActivateTheONE(theOne);
        }
    }

    private void Update()
    {
        levelText.text = $"Room: {(currentRoomNo + 1).ToString()}";
    }
}
