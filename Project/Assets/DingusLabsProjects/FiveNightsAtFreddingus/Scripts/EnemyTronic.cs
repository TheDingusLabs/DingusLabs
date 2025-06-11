using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTronic : MonoBehaviour
{
    public string enemyTronicName;
    public int currentLocationIndex = 0;
    public int returnLocationIndex;

    public int level = 0;

    public List<int> moveLocationsList;

    public List<int> dayDifficulties;
    public List<int> timeDifficultyIncrements;

    public void setLevelByDay(int day){
        level = dayDifficulties[day-1];
    }

//bored of complex stuff
    // public void PerformDifficultyIncrement(int time){
    //     for(int i = 0; i <= time - 2; i++)
    //     {
    //         //starts going up at 2, which would be 90f*2, then 3 90f*3, then 4 90f*4
    //         if(time >= 2){
    //             if(time >= 4){
    //                 time = 
    //             }
    //             level += timeDifficultyIncrements[time-2];
    //         }
    //     }
    // }

    public void PerformDifficultyIncrement(int time){
        level += timeDifficultyIncrements[time-2];
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
