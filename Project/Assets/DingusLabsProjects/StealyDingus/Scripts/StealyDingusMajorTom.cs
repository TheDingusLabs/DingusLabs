using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StealyDingusMajorTom : MonoBehaviour
{
    List<StealyDingusAgent> agents;
    void Start()
    {
        agents = new List<StealyDingusAgent>();
        agents.AddRange(GetComponentsInChildren<StealyDingusAgent>());
    }

    public void NextLevel()
    {
        foreach (var agent in agents)
        {
            agent.StartNextLevel();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown("n"))
        {
            Debug.Log("n key was pressed");
            NextLevel();
        }
    }
}
