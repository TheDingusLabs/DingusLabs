using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForkliftLevelmasta : MonoBehaviour
{
    public ForkliftRoom currentRoom;
    public List<GameObject> rooms;
    public int currentRoomNo = 0;
    public GameObject spawnArea;
    public TMPro.TextMeshPro levelText;

    public GameObject playerSpawnArea;
    // Start is called before the first frame update
    void Start()
    {
        var newRoom = Instantiate(rooms[currentRoomNo], spawnArea.transform.position + new Vector3(0f, 0f, 0), spawnArea.transform.rotation, this.transform.parent);
        currentRoom = newRoom.GetComponent<ForkliftRoom>();

        playerSpawnArea.transform.position = currentRoom.playerSpawnArea.transform.position;
    }

    public void StartNextLevel(){
        if(currentRoomNo < rooms.Count - 1){
            currentRoom.DestroyRoom();
            currentRoomNo++;
            var newRoom = Instantiate(rooms[currentRoomNo], spawnArea.transform.position + new Vector3(0f, 0f, 0), spawnArea.transform.rotation, this.transform.parent);
            currentRoom = newRoom.GetComponent<ForkliftRoom>();
        }
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
