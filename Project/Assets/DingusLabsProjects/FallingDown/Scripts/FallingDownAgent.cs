//Put this script on your blue cube.

using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgentsExamples;
using System.Collections.Generic;

public class FallingDownAgent : Agent
{
    public GameObject rotatePoint;
    public GameObject spinnySpawner;

    float countDownTime = 0f;
    float totalTrainingTime = 0f;
    public TMPro.TextMeshPro countDownDisplay;
    public TMPro.TextMeshPro timeTrainedDisplay;
    public TMPro.TextMeshPro successDisplay;
    public GameObject Cylinder;

    EnvironmentParameters m_ResetParams;

    public override void Initialize()
    {
        m_ResetParams = Academy.Instance.EnvironmentParameters;
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        //sensor.AddObservation(agentPos / 20f);
    }

    /// <summary>
    /// Changes the color of the ground for a moment.
    /// </summary>
    /// <returns>The Enumerator to be used in a Coroutine.</returns>
    /// <param name="mat">The material to be swapped.</param>
    /// <param name="time">The time the material will remain.</param>
    IEnumerator GoalScoredSwapGroundMaterial(Material mat, float time)
    {
        //m_GroundRenderer.material = mat;
        yield return new WaitForSeconds(time); //wait for 2 sec
        //m_GroundRenderer.material = m_GroundMaterial;
    }

    public void MoveAgent(ActionSegment<int> act)
    {
        AddReward(-0.00015f);

        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var rotateDirAction = act[0];

        if (rotateDirAction == 1)
            rotateDir = Cylinder.transform.up * -1f;
        else if (rotateDirAction == 2)
            rotateDir = Cylinder.transform.up * 1f;

        if (rotateDirAction != 0)
        {
            Cylinder.transform.Rotate(rotateDir, Time.fixedDeltaTime * 300f * 0.6f);
        }

    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        MoveAgent(actionBuffers.DiscreteActions);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[0] = 2;
        }
        if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[0] = 1;
        }
    }

    // Detect when the agent hits the death wall
    void OnTriggerStay(Collider col)
    {
        if (col.gameObject.CompareTag("goal"))
        {
            Destroy(col.gameObject);
            AddReward(1f);
        }
        if (col.gameObject.CompareTag("deathWall"))
        {
            AddReward(-1f);
            EndEpisode();
        }
    }

    public override void OnEpisodeBegin()
    {
        countDownTime = 0;
        countDownDisplay.text = "Time: " + ((int)countDownTime).ToString();
        spinnySpawner.GetComponent<SpinnySpawner>().KillChildren();
    }

    private void FixedUpdate()
    {
        countDownTime += Time.deltaTime;
        totalTrainingTime += Time.deltaTime;
        timeTrainedDisplay.text = "Total Time Trained: " + ((int)totalTrainingTime).ToString() +"s";
        countDownDisplay.text = "Time: " +((int)countDownTime).ToString();
    }
}
