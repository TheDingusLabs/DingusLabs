//Put this script on your blue cube.

using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgentsExamples;
using System.Collections.Generic;

public class TrashManAgent : Agent
{
    public GameObject spawnArea;
    public GameObject ground;
    public GameObject trashSpawner;
    Bounds m_SpawnAreaBounds;
    Rigidbody m_AgentRb;
    public float maxVelocity;
    public float jumpForce;
    public float runForce;
    public float jumpCounter = 999;
    public float jumpCooldown = 0.3f;
    public int successTime = 90;
    public bool succeeded = false;

    public bool failed = false;
    // This is a downward force applied when falling to make jumps look
    // less floaty
    public float fallingForce;
    // Use to check the coliding objects
    public Collider[] hitGroundColliders = new Collider[3];
    Vector3 m_JumpTargetPos;
    Vector3 m_JumpStartingPos;

    float episodeCounter = 0f;
    float totalTrainingTime = 0f;
    public TMPro.TextMeshPro episodeConterLabel;
    public TMPro.TextMeshPro timeTrainedDisplay;
    public TMPro.TextMeshPro successDisplay;

    EnvironmentParameters m_ResetParams;

    public override void Initialize()
    {
        m_AgentRb = GetComponent<Rigidbody>();
        m_SpawnAreaBounds = spawnArea.GetComponent<Collider>().bounds;

        //todo this was set to false
        spawnArea.SetActive(true);

        m_ResetParams = Academy.Instance.EnvironmentParameters;
    }

    /// <summary>
    /// Does the ground check.
    /// </summary>
    /// <returns><c>true</c>, if the agent is on the ground,
    /// <c>false</c> otherwise.</returns>
    /// <param name="smallCheck"></param>
    public bool DoGroundCheck(bool smallCheck)
    {
        if (!smallCheck)
        {
            hitGroundColliders = new Collider[3];
            var o = gameObject;
            Physics.OverlapBoxNonAlloc(
                o.transform.position + new Vector3(0, -0.05f, 0),
                new Vector3(0.95f / 2f, 0.5f, 0.95f / 2f),
                hitGroundColliders,
                o.transform.rotation);
            var grounded = false;
            foreach (var col in hitGroundColliders)
            {
                if (col != null && col.transform != transform &&
                    (col.CompareTag("walkableSurface") || col.CompareTag("block") || col.CompareTag("squareTrash") || col.CompareTag("cylinderTrash") || col.CompareTag("sphereTrash"))
                    )
                {
                    grounded = true; //then we're grounded
                    break;
                }
            }
            return grounded;
        }
        else
        {
            RaycastHit hit;
            Physics.Raycast(transform.position + new Vector3(0, -0.05f, 0), -Vector3.up, out hit, 0.2f);

            if (hit.collider != null &&
                ( hit.collider.CompareTag("walkableSurface") || hit.collider.CompareTag("block") || hit.collider.CompareTag("squareTrash") || hit.collider.CompareTag("cylinderTrash") || hit.collider.CompareTag("sphereTrash"))
                && hit.normal.y > 0.95f)
            {
                return true;
            }

            return false;
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        var agentPos = m_AgentRb.position - ground.transform.position;
        //sensor.AddObservation(m_AgentRb.velocity / 10f);
        sensor.AddObservation(agentPos / 20f);
        sensor.AddObservation(DoGroundCheck(true) ? 1 : 0);
    }

    public void MoveAgent(ActionSegment<int> act)
    {
        //AddReward(0.00001f);
        var smallGrounded = DoGroundCheck(true);
        var largeGrounded = DoGroundCheck(false);

        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;
        var dirToGoForwardAction = act[0];
        var rotateDirAction = act[1];
        var jumpAction = act[2];

        if (dirToGoForwardAction == 1)
            dirToGo = (largeGrounded ? 1f : 0.6f) * 1f * transform.forward;
        else if (dirToGoForwardAction == 2)
            dirToGo = (largeGrounded ? 1f : 0.6f) * -1f * transform.forward;
        if (rotateDirAction == 1)
            rotateDir = transform.up * -1f;
        else if (rotateDirAction == 2)
            rotateDir = transform.up * 1f;
        if (jumpAction == 1)
        {
            if (smallGrounded && jumpCounter >= jumpCooldown)
            {
                jumpCounter = 0;
                m_AgentRb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            }
        }

        transform.Rotate(rotateDir, Time.fixedDeltaTime * 300f * 0.5f);
        m_AgentRb.linearDamping = (m_AgentRb.linearVelocity.magnitude / maxVelocity);
        if (!largeGrounded)
        {
            m_AgentRb.AddForce(Vector3.down * fallingForce, ForceMode.Acceleration);
        }

        m_AgentRb.AddForce(dirToGo * runForce, ForceMode.VelocityChange);

        jumpCounter += Time.fixedDeltaTime;
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        MoveAgent(actionBuffers.DiscreteActions);
        var ray = new Ray(m_AgentRb.position, Vector3.down);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.tag == "theDeadZone")
            {
                AddReward(-0.05f);
            }
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[1] = 2;
        }
        if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[1] = 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = 2;
        }
        discreteActionsOut[2] = Input.GetKey(KeyCode.Space) ? 1 : 0;
    }

    public void YouDidAGood()
    {
        AddReward(0.6f);
    }

    public void touchedTrash()
    {
        AddReward(0.1f);
    }

    public void touchedTrashIsMoving()
    {
        AddReward(0.01f);
    }

    public void YouFailed()
    {
        failed = false;
        //AddReward(-1f);
        EndEpisode();
    }

    // Detect when the agent hits the death wall
    void OnTriggerStay(Collider col)
    {
        if (col.gameObject.CompareTag("theDeadZone"))
        {
            failed = true;
        }
    }

    public override void OnEpisodeBegin()
    {
        episodeCounter = 0;
        episodeConterLabel.text = "Time: " + ((int)episodeCounter).ToString();
        var randomPosX = Random.Range(-m_SpawnAreaBounds.extents.x * 1f,
            m_SpawnAreaBounds.extents.x);
        var randomPosZ = Random.Range(-m_SpawnAreaBounds.extents.z * 1f,
            m_SpawnAreaBounds.extents.z);
        transform.localPosition = spawnArea.transform.localPosition + new Vector3(randomPosX, 0f, randomPosZ);

        m_AgentRb.linearVelocity = default(Vector3);
        this.transform.rotation = Quaternion.AngleAxis(Random.Range(-180f,180f), Vector3.up);

        trashSpawner.GetComponent<TrashSpawner>().ResetTrashSpawner();
        //trashSpawner.GetComponent<TrashSpawner>().CreateTrash();
    }

    private void FixedUpdate()
    {
        if (!succeeded && episodeCounter > successTime)
        {
            successDisplay.text = $"Succeeded at {(int)totalTrainingTime}";
            succeeded = true;
        }
        episodeCounter += Time.deltaTime;
        totalTrainingTime += Time.deltaTime;
        timeTrainedDisplay.text = "Total Time Trained: " + ((int)totalTrainingTime).ToString() +"s";

        episodeConterLabel.text = "Time: " +((int)episodeCounter).ToString();
        if (failed)
        {
            YouFailed();
        }
    }
}
