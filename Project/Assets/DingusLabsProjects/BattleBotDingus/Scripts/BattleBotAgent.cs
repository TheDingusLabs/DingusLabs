//Put this script on your blue cube.

using System.Collections;
using UnityEngine;
using Unity.MLAgents;
//using Unity.Barracuda;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgentsExamples;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using System;
using Random = UnityEngine.Random;
using UnityEngine.UIElements;
using Unity.Mathematics;

//what four should I make?
//shooter? doneish
//sisyphus? pushes boulder? pushes enemies?
//jedi dingus? swings a sword? can deflect attacks?
//charge dingus? doneish
//bomber dingus?

public class BattleBotAgent : Agent
{
    public enum Position
    {
        Charger,
        Shooter,
        Generic
    }
    public GameObject healthBar;
    public GameObject healthBarProgress;
    public GameObject AhCamera;


    public GameObject ground;
    public bool levelEnded = false;

    public Rigidbody m_AgentRb;

    public float maxVelocity;
    public float maxBackVelocity;
    public float jumpForce;
    public float runForce;
    public float jumpCounter = 999;
    public float jumpCooldown = 0.3f;

    public float actionCooldown = 0.3f;
    public float actionCounter = 0.3f;
    // This is a downward force applied when falling to make jumps look
    // less floaty
    public float fallingForce;

    public float turnSpeed = 0.65f;

    // Use to check the coliding objects
    public Collider[] hitGroundColliders = new Collider[3];

    EnvironmentParameters m_ResetParams;

    public float maxHP = 100;
    public float hp = 100;

    public BattleBotEnvController.BattleTeam team;
    public List<Material> colours;
    public Material healthBarRegularColour;
    public Material healthBarDecayColour;
    public MeshRenderer displayCube;

    public bool dead = false;
    public bool gameOver = false;
    private string properTagType;

    private List<float> agentHealths;
    private GameObject teamIdBox;

    //bot specific objects
    public GameObject enemyTypeIdentifier;

    private string enemyTypeIdentifierTag;

    public GameObject facingBox;

    private float timeSinceLastDamagedEnemy = 0f;

    public bool isMelee;
    public float safeRange;
    public bool isTargetDummy = false;

    public float distanceRewardMod = 1;

    public override void Initialize()
    {
        teamIdBox = this.gameObject.transform.Find("teamID").gameObject;
        teamIdBox.tag = this.team.ToString();
        m_AgentRb = GetComponent<Rigidbody>();
        m_ResetParams = Academy.Instance.EnvironmentParameters;
        properTagType = this.gameObject.tag;

        displayCube.material = colours[(int)team];
        //Debug.Log("hey fam just letting you know I did change the material "  + displayCube.material.name);

        var AgentsHealths = new List<int>();
        for (int i = 0; i < 8; i++){ //hardcoded max list side, should fix all this up later
            AgentsHealths.Add(0);    
        }

        enemyTypeIdentifierTag = enemyTypeIdentifier.tag;


        if(healthBar != null){
            AhCamera = GameObject.Find("PlayerCam");//this.transform.parent.GetComponentInParent<Camera>().gameObject;
        }
    }

    public void ObservedTookDamage(float damageAmount){
        var relativeFullHealthDamage = damageAmount / maxHP * 100;
        AddReward(-1*relativeFullHealthDamage/60f);
    }
    public virtual void ObservedDidDamage(float damageAmount){
        AddReward(damageAmount/20f);
        timeSinceLastDamagedEnemy = 0;
        healthBarProgress.GetComponent<MeshRenderer>().material = healthBarRegularColour;
    }

    //we punish for damage already, I'm concerned about the strange lessons it may learn for taking more damage when it takes damage sometime!
    public void ObservedDied(){
        //AddReward(-0.5f);
    }

    public void ObservedWon(){
        AddReward(0.02f);
    }

    public void ObservedAttackingTheDead(float damageAmount){
        //AddReward(damageAmount * 0.1f);
    }

    //now that we punish for not damaging this may be irrelevent
    public void observeTimedOut(){
        //AddReward(-0.5f);
    }

    public void observeAttackFailed(float multiplier = 1){
        //we really don't want them just attacking aimlessly
        //but let's see if this is helping
        AddReward(-1f*multiplier*0.025f);
    }

    public void UpdateAgentHealths(List<float> healths){
        agentHealths = healths;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(hp);
        //sensor.AddObservation(agentHealths);
        sensor.AddObservation(actionCounter > actionCooldown ? actionCounter/actionCooldown : 1f);
        sensor.AddObservation(dead);
        sensor.AddObservation(gameOver);
        sensor.AddObservation((int)team);
        
        sensor.AddObservation(m_AgentRb.linearVelocity);

        var foes = this.transform.parent.gameObject.GetComponent<BattleBotEnvController>().GetFoes(this);
        sensor.AddObservation(this.gameObject.transform.localPosition);
        if(foes.Count > 0){
            sensor.AddObservation(foes[0].gameObject.transform.localPosition);
        }
        else{
            sensor.AddObservation(this.gameObject.transform.localPosition);
        }

        var allies = this.transform.parent.gameObject.GetComponent<BattleBotEnvController>().GetAllies(this);
        if(allies.Count > 0){
                sensor.AddObservation(allies[0].gameObject.transform.localPosition);
            }
            else{
                sensor.AddObservation(this.gameObject.transform.localPosition);
            }
        // if(!isTargetDummy){ // this is awful code just here to prevent retraining the target dummy
        //     var allies = this.transform.parent.gameObject.GetComponent<BattleBotEnvController>().GetAllies(this);
        //     if(allies.Count > 0){
        //         sensor.AddObservation(allies[0].gameObject.transform.localPosition);
        //     }
        //     else{
        //         sensor.AddObservation(this.gameObject.transform.localPosition);
        //     }
        // }
    }

    public virtual void MoveAgent(ActionSegment<int> act)
    {
        if(levelEnded || dead){return;}

        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;
        var dirToGoForwardAction = act[0];
        var rotateDirAction = act[1];
        var ActionAction = act[2];

        if (dirToGoForwardAction == 1)
            dirToGo = 1f * 1f * transform.forward;
        else if (dirToGoForwardAction == 2)
            dirToGo = 1f * -1f * transform.forward;
        if (rotateDirAction == 1)
            rotateDir = transform.up * -1f;
        else if (rotateDirAction == 2)
            rotateDir = transform.up * 1f;
        if (ActionAction == 1 && actionCounter >= actionCooldown)
        {
            ExecuteAction();
        }

        //could use this to nerf forward move speed though I think it would just make things worse....
        float turningMoveSpeedMod = rotateDirAction == 0 ? 1 : 0.8f; 

        transform.Rotate(rotateDir, Time.fixedDeltaTime * 200f * turnSpeed);

        ApplyDrag(dirToGoForwardAction);

        if(!DoRealGroundCheck()){
            m_AgentRb.AddForce(-1f * new Vector3(0,1,0) * 30f, ForceMode.Force);
        }

        m_AgentRb.AddForce(dirToGo * runForce * 50f, ForceMode.Force);

        jumpCounter += Time.fixedDeltaTime;
    }

    public void ApplyDrag(int dirToGoForwardAction){
        //capping reverse speed
        var nonYSpeed = new Vector3(m_AgentRb.linearVelocity.x, 0, m_AgentRb.linearVelocity.z);
        if(dirToGoForwardAction == 2){
            m_AgentRb.linearDamping = nonYSpeed.magnitude / maxBackVelocity;
        }
        else{
            m_AgentRb.linearDamping = nonYSpeed.magnitude / maxVelocity;
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

    protected virtual void OnCollisionStay(Collision col)
    {
        //being in the wall is bad, it slows turn speed etc
        if(col.gameObject.CompareTag("wall")){
            //Debug.Log("touchin walls");
            //AddReward(-1 * Time.deltaTime * 0.005f);
        }
    }

    protected virtual void OnTriggerStay(Collider col)
    {
        if(levelEnded || gameOver || dead){
            return;
        }
        if(col.gameObject.CompareTag("pit"))
        {//the pit is a good way to die
            AddReward(-1 * Time.deltaTime * 0.4f);
        }
        if(col.gameObject.CompareTag("painZone"))
        {//pain zone hurts
            //AddReward(-1 * Time.deltaTime * 0.3f);
            TakeDamage(Time.deltaTime*2, null);
        }
        if(col.gameObject.CompareTag("theDeadZone"))
        {
            AddReward(-1.5f);
            DieInstantly();
        }
    }

    public virtual void DieInstantly(){ //Really wondering if we can track who did damage recently and reward them the kill hp! We could use TakeDamage to take the source unit and if it's any of the pushing types give them a reward?
        if(levelEnded || gameOver || dead){return;}
        var hpLost = hp;
        TakeDamage(hpLost, null);
        //Die();
    }

    public virtual void TakeDamage(float damage, BattleBotAgent sourceAgent){
        if(levelEnded || gameOver || dead){return;}
        // punish hurting the dead?
        else{
            this.hp -= damage;
            //ObservedTookDamage(damage); //we call tyhis in the environment controller
            if(this.hp <= 0){Die();}
            this.transform.parent.gameObject.GetComponent<BattleBotEnvController>().AnAgentWasHurt(this, sourceAgent, damage);
        }
    }

    public void Die(){
        if(levelEnded || dead || gameOver){return;}
        ObservedDied();
        enemyTypeIdentifier.tag = "dead";
        enemyTypeIdentifier.SetActive(false);
        dead = true;
        facingBox.SetActive(false);
        this.gameObject.tag = "deadAgent";
        NotifyAgentDone(DoneReason.DoneCalled);
        //this.gameObject.tag = "dead";
        StartCoroutine(
            GoToShadowRealm(2)
        );
    }

    IEnumerator GoToShadowRealm(float time)
    {
        yield return new WaitForSeconds(time);

        if(!gameOver && !levelEnded){
            //this.gameObject.SetActive(false);
            this.transform.position = new Vector3(0, -1000, 0);
        }
    }

    public void BeginGameEnded(){
        gameOver = true;
    }

    public virtual void TakeEnemyHurtingAction(){
    }

    public override void OnEpisodeBegin()
    {
        actionCounter = actionCooldown*0.5f;
        //this.gameObject.tag = properTagType;
        dead = false;
        levelEnded = false;
        gameOver = false;
        hp = maxHP;
        this.gameObject.tag = "agent";

        //this.transform.rotation = Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up);
        enemyTypeIdentifier.SetActive(true);
        enemyTypeIdentifier.tag = enemyTypeIdentifierTag;
        m_AgentRb.linearVelocity = default(Vector3);

        facingBox.SetActive(true);
        timeSinceLastDamagedEnemy = 0f;

        healthBarProgress.GetComponent<MeshRenderer>().material = healthBarRegularColour;

        teamIdBox.tag = this.team.ToString();
        displayCube.material = colours[(int)team];

        //TODO may want to reward being in the correct range of enemeies
        if(!dead && !gameOver){PerformIncorrectRangeCheck();}
        //closestDistanceToEnemies
    }

    public virtual void ExecuteAction()
    {
        //RewardGoodAim(0.2f);
    }

    public virtual void RewardGoodAim(float multi, bool requireMinimumRange = false){
        var foes = this.transform.parent.gameObject.GetComponent<BattleBotEnvController>().GetFoes(this);
        if(foes.Count > 0){
            bool wellAimed = false;
            foreach(var foe in foes){
                if(foe.dead){continue;}
                Vector3 toEnemy = (foe.gameObject.transform.position - this.gameObject.transform.position).normalized;
                Vector3 agentForward = this.gameObject.transform.forward;

                // Calculate alignment using the dot product
                float alignment = Vector3.Dot(agentForward, toEnemy);

                // Reward the agent for facing the enemy
                if (alignment > 0.8f ) // Reward if within ~36.87° of directly facing the enemy
                {
                    if(!requireMinimumRange || 
                        (requireMinimumRange && ( 
                             isMelee && Vector3.Distance(this.gameObject.transform.position, foe.gameObject.transform.position) < safeRange || 
                            !isMelee && Vector3.Distance(this.gameObject.transform.position, foe.gameObject.transform.position) > safeRange)))
                    {
                        AddReward(multi); // Small reward for alignment
                        wellAimed = true;
                    }
                }
            }
            if(!wellAimed){
                AddReward(-multi);
            }
        }
    }

    public virtual void PerformOverTimeActions(){

    }

    public bool DoRealGroundCheck(){
        RaycastHit hit;
        Physics.Raycast(transform.position + new Vector3(0, -0.45f, 0), -Vector3.up, out hit, 0.1f);

        if (hit.collider != null && hit.collider.CompareTag("walkableSurface") && hit.normal.y > 0.95f)
        {
            return true;
        }

        return false;
    }

    private void Update()
    {
        actionCounter += Time.deltaTime;
        timeSinceLastDamagedEnemy += Time.deltaTime;
        //life is pain, let's punish being alive so they aspire to finish quickly
        AddReward(-1 * Time.deltaTime * 0.01f);


        float forwardVelocity = Vector3.Dot(m_AgentRb.linearVelocity, transform.forward);
        //reward melee agents for moving forwards
        // if(isMelee && m_AgentRb.linearVelocity.magnitude > maxVelocity*0.5f && forwardVelocity > 0){    
        if(isMelee && forwardVelocity > maxVelocity* 0.3f ){    
            // if(isMelee){Debug.Log($"{this.gameObject.name}'s linear velocity magnitude is: {forwardVelocity}");}
            // if(isMelee){Debug.Log($"{this.gameObject.name}'s forward linear velocity magnitude is: {forwardVelocity}");}
            AddReward(Time.deltaTime * 0.01f * forwardVelocity);
        }
        //let's reduce the punishment from decay if they are in the correct range of foes
        float distanceModifier = 1f;
        if(!dead && !gameOver)
        {
            float distance = this.transform.parent.gameObject.GetComponent<BattleBotEnvController>().GetUnitDistanceFromNearestFoe(this);
            if(distance != 99f){
                if(isMelee){
                    float minRange = safeRange / 2f;
                    float maxRange = safeRange * 2f;
                    if(distance < maxRange){
                        //var distanceReward = math.max( (maxRange / distance), minRange) / minRange;
                        var distanceReward = 1f - (math.max(distance,minRange) - minRange) / (maxRange - minRange);
                        //distanceModifier = 0.8f;
                        AddReward(Time.deltaTime * distanceReward * distanceRewardMod * 0.035f);
                        //Debug.Log($"rewarding melee unit: {this.gameObject.name} at distance:{distance} with reward {Time.deltaTime * distanceReward * 0.015f} for being within max range of an enemy!");
                    }
                    
                }
                else if(!isMelee){
                    if(distance > safeRange){
                        //Debug.Log($"rewarding {this.gameObject.name} for being far enough away!");
                        //distanceModifier = 0.5f;
                        AddReward(Time.deltaTime * 0.015f);
                    }
                }
            }
            else{
                //Debug.Log($"somehow {this.gameObject.name} is getting a closest enemy distance of 99f, this is is a bug as it doesn't get distances when dead or the game is over!");
            }
        }
        //let's try punishing them for taking too long to attack enemies
        if(timeSinceLastDamagedEnemy > 6f && !dead && !gameOver && !isTargetDummy){
            var decayAmount = -1f * Time.deltaTime * 0.01f * distanceModifier;
            //AddReward(decayAmount);
            TakeDamage(Mathf.Abs(maxHP * Time.deltaTime * 0.01f * distanceModifier), null);
            healthBarProgress.GetComponent<MeshRenderer>().material = healthBarDecayColour;
        }

        if(!teamIdBox.CompareTag(this.team.ToString()) /*|| displayCube.material != colours[(int)team]*/){
            //Debug.Log($"there was an issue with the tag or colour after round starting but we've resolved it: {this.name} : team: {this.team}, colour: {((int)team).ToString()} vs idboxtag: {teamIdBox.tag}" );
            teamIdBox.tag = this.team.ToString();
            displayCube.material = colours[(int)team];
        }

        healthBar.transform.LookAt(AhCamera.transform);
        healthBarProgress.transform.LookAt(AhCamera.transform);

        var xScale = Mathf.Max(0, hp);
        xScale = xScale / maxHP;
        healthBarProgress.transform.localScale = new Vector3(xScale, healthBarProgress.transform.localScale.y, healthBarProgress.transform.localScale.z);
        
        PerformOverTimeActions();
        if(levelEnded){
            return;
        }
    }

    private void PerformIncorrectRangeCheck(){

    }
}
