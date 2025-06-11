using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StealyDingusMasta : MonoBehaviour
{
    public StealyDingusRoom currentRoom;
    public List<GameObject> rooms;
    public int currentRoomNo = 0;
    public GameObject spawnArea;
    public TMPro.TextMeshPro levelText;
    public List<bool> completedRooms;

    //public GameObject ball;

    public GameObject playerSpawnArea;
    // Start is called before the first frame update


    void Start()
    {
        completedRooms = new List<bool>();
        for(int i = 0; i < rooms.Count; i++){
            completedRooms.Add(false);
        }
        
        // var newRoom = Instantiate(rooms[currentRoomNo], spawnArea.transform.position + new Vector3(0f, 0f, 0), spawnArea.transform.rotation, this.transform.parent);
        // currentRoom = newRoom.GetComponent<StealyDingusRoom>();

        CreateRoom();

        playerSpawnArea.transform.position = currentRoom.playerSpawnArea.transform.position;
    }

    public void StartNextLevel(){
        currentRoom.DestroyRoom();
        CreateRoom();
    }

    public void CreateRoom(){
        //needs further updating
        //currentRoomNo++;
        var previousRoomNo = currentRoomNo;
        currentRoomNo = Random.Range(0, rooms.Count);
        if(completedRooms[currentRoomNo]){
            currentRoomNo = Random.Range(0, rooms.Count);
        }
        if(completedRooms[currentRoomNo]){
            currentRoomNo = Random.Range(0, rooms.Count);
        }
        if(completedRooms[currentRoomNo]){
            currentRoomNo = Random.Range(0, rooms.Count);
        }


        //be careful with this code, dangerously manual
        var completedRoomCount = 0;
        for(int i = 0; i < rooms.Count; i ++){
            if(completedRooms[i]){
                completedRoomCount++;
            }
        }

        if(completedRoomCount == rooms.Count - 1 && !completedRooms[rooms.Count-1] && previousRoomNo != rooms.Count-1){
            currentRoomNo = rooms.Count-1;
        }

        var newRoom = Instantiate(rooms[currentRoomNo], spawnArea.transform.position + new Vector3(0f, 0f, 0), spawnArea.transform.rotation, this.transform.parent);
        currentRoom = newRoom.GetComponent<StealyDingusRoom>();
    }

    public void MarkRoomCompleted(int roomNo){
        completedRooms[roomNo] = true;
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
