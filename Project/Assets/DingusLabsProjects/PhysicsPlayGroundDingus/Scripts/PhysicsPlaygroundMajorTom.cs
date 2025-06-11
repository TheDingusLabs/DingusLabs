using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsPlaygroundMajorTom : MonoBehaviour
{
    List<PhysicsPlaygroundAgent> agents;
    void Start()
    {
        agents = new List<PhysicsPlaygroundAgent>();
        agents.AddRange(GetComponentsInChildren<PhysicsPlaygroundAgent>());
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
            //Debug.Log("n key was pressed");
            NextLevel();
        }
    }
}
