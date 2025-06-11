//Put this script on your blue cube.

using System.Collections;
using UnityEngine;
using Unity.MLAgents;
//using Unity.Barracuda;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgentsExamples;
using System.Collections.Generic;
//using UnityEngine.UIElements;
using UnityEngine.UI;
using Microsoft.Win32;
using TMPro;
using UnityEngine.PlayerLoop;
using System.Media;

public class FiveNightsAtFreddingusAgent : Agent
{
    public bool THEOne = false;
    public bool levelEnded = false;
    private bool enemyLocationsHidden = false;
    public GameObject killingSign;

    Rigidbody m_AgentRb;

    public TMPro.TextMeshPro countDownDisplay;
    public TMPro.TextMeshPro timeTrainedDisplay;
    public TMPro.TextMeshPro successDisplay;
    public TMPro.TextMeshPro levelEndText;

    public GameObject timetrainpanel;
    public GameObject timeWinPanel;

    EnvironmentParameters m_ResetParams;

    private Vector3 spawnArea;


    // private int activeCamera = 0;
    // private bool cameraActive = false;
    // private float cameraWaitTime = 1;
    // private float cameraWaitTimeCounter = 0f;
     public int officeFacingDirection = 0;
     private int officeCurrentlyMovingDirection = 0;
     //private float rotateSpeed = 1f;
     private float rotateAmount = 12f;

     private float officeDirectionChangeTime = 0.2f;
     private float officeDirectionChangeCounter = 0f;
    // private bool leftLightOn = false;
     //private float leftLightWaitTime = 0.5f;
     private float leftLightWaitCounter = 0f;
    // private bool rightRightOn = false;
     //private float rightLightWaitTime = 0.5f;
     private float rightLightWaitCounter = 0f;


    public GameObject dangus;
    public GameObject dongus;
    public GameObject dungus;
    public GameObject kev;

    public GameObject dingusView;

    public float dangusTime = 3.02f; // use freddy
    public float dangusInc = 0;
    public float dangusCountdownMax = 1000f;
    private bool dangusMoving = false;
    public float dangusActualMovementInc = 0;
    public float dongusTime = 4.97f; // use Bonnie
    public float dongusInc = 0;
    public float dungusTime = 4.98f; // use Chica
    public float dungusInc = 0;
    public float kevTime = 5.01f; // use Foxy
    //when you open the cam foxy enters a lock state with a min and max duration
    public float kevlockStateMinTime = 0.83f;
    public float kevlockStateMaxTime = 16.67f;
    private float kevLockInc = 0;
    private float kevLockTime = 99999f;
    public float kevSprintTime = 25f;
    public float kevSprintInc = 0f;
    //when foxy is entering attack (phase 4) when you check the left hall or around 25 or so seconds have passed
    //2A will show him running
    private bool kevLocked = false;
    public float kevInc = 0;
    private int kevPhase = 1;
    public float kevPowerDrainAmount = 1;
    //first time he takes 1%, then increases by 6% each time;
    public List<GameObject> kevPhaseLocations;

    public List<GameObject> roomsList;
    public List<GameObject> camsList;
    public List<GameObject> camButtons;
    public GameObject canvas;
    public GameObject deadPanel;
    public GameObject livePanel;


    public bool camsOpen = false; 

    public GameObject securityCam;
    public int activeCamIndex = 0;
    private int clickedCamIndex = -1;
    private int clickedDoorIndex = -1;
    private float camLockTime = 0.5f;
    private float camLockinc = 0f;


    //lets have spawn locations for each character in each room, use the location and rotation of those points to ensure correct positioning
    //get power drain stats for each day / action from online, 
    private int day = 1;
    public float currentPower = 100;
    private bool beingMurdered = false;

    //public GameObject powerLeftText;
    public TMPro.TMP_Text powerLeftText;
    public TMPro.TMP_Text timeText;
    public TMPro.TMP_Text nightText;
    public TMPro.TMP_Text usageText;

    //public GameObject westDoorGO;
    public LightScript westLight;
    public DoorScript westDoor;
    //public GameObject eastDoorGO;
    public LightScript eastLight;
    public DoorScript eastDoor;

    //In Five Nights at Freddy's, not every hour has the same duration. 12 AM lasts exactly 90 seconds (1:30 min) while 1 AM to 6 AM each last 89 seconds (1:29 min). So a full night duration is 08:55. 
    public float currentTime = 0f;
    private float maxTime;
    private float hourDuration = 86f;

    private float camPulldownKillTime = 30f;
    private float camPulldownKillinc = 0f;
    private float dangusKillTime = 1f;
    private float dangusKillinc = 0f;
    private float applianceUseBaseEnergyCost = 9;
    //private float applianceUseBaseEnergyCost = 9;

    private bool powerOut = false;
    //private float powerOutinc = 0;
    //private float powerOutTime = 99999f;

    private float powerOutShowUpinc = 0;
    private float powerOutSingInc = 0;
    private float PowerOutScareInc = 0;
    private float PowerOutScareTime = 2f;
    private float powerOutShowUpTime = 99999999;
    private float powerOutSingTime = 999999999;
    private bool startedMusic = false;

    public GameObject spookinCharacter;
    private float jumpscareInc = 0;
    private float jumpscareTime = 1.5f;
    public GameObject jumpscareStartSpot;
    public GameObject jumpscareEndSpot;

    private bool justWaitingforTheEnd = false;
    private bool spottedLeftLightEnemy = false;
    private bool spottedRightLightEnemy = false;
    private float spottedLeftLightEnemyInc = 0f;
    private float spottedRightLightEnemyInc = 0f;

    private int DangusLastSpottedIndex = 0;
    private float DangusSpottedOldness = 500f;
    private int DongusLastSpottedIndex = 0;
    private float DongusSpottedOldness = 500f;
    private int DungusLastSpottedIndex = 0;
    private float DungusSpottedOldness = 500f;
    private int KevLastSpottedPhase = 1;
    private float KevSpottedOldness = 500f;

    private bool UppedDifficultyOne = false;
    private bool UppedDifficultyTwo = false;
    private bool UppedDifficultyThree = false;
    private float learningHelper = 25f; 
    private float heardFootstepsCounter = 25f;
    private bool dangusAtTheDoor = false;

    private bool dangusInKillstate = false;
    private bool dongusInKillstate = false;
    private bool dungusInKillstate = false;

    private float lastTimeOpenedCamera = 7f;
    private float lastTimeWatchedDangus = 0f;

    private float timeStayedInCamera = 0f;

    private float kevMusicPlayInc = 0;

    public Image Dangus1A;
    public Image Dangus1B;
    public Image Dangus7;
    public Image Dangus6;
    public Image Dangus4A;
    public Image Dangus4B;
    public Image Dungus1A;
    public Image Dungus1B;
    public Image Dungus7;
    public Image Dungus6;
    public Image Dungus4A;
    public Image Dungus4B;
    public Image DungusEastDoor;
    public Image Dongus1A;
    public Image Dongus1B;
    public Image Dongus5;
    public Image Dongus2A;
    public Image Dongus2B;
    public Image Dongus3;
    public Image DongusWestDoor;
    public TMPro.TMP_Text KevText;
    public List<Image> AllEnemyTronicIcons;

    public GameObject thelevelmodels;

    public AudioSource AmbientSound;
    public AudioSource FootStepsSound;
    public AudioSource ToreadorSound;
    public AudioSource SpottedSound;
    public AudioSource DoorCloseSound;
    public AudioSource KevBangSound;
    public AudioSource DieSound;
    public AudioSource PowerOutSound;
    public AudioSource DryFartSound;
    public AudioSource SurviveSound;
    public AudioSource DangusLaugh;

    public AudioSource CameraFlipSound;

    public Light mainLight1;
    public Light mainLight2;

    private bool heardFootstepsCheckedWest = true;
    private bool heardFootstepsCheckedEast = true;

    private bool silenced = false; 

    private bool freddyPrimeForKill = false;

    
    public void ShowIconLocations() {
        if(!THEOne){return;}
        foreach (var img in AllEnemyTronicIcons)
        {
            img.enabled = false;
        }
        if(enemyLocationsHidden){
            KevText.text = "?";
            return;
        }

        var DangLoc = dangus.GetComponent<EnemyTronic>().currentLocationIndex;
        var DungLoc = dungus.GetComponent<EnemyTronic>().currentLocationIndex;
        var DongLoc = dongus.GetComponent<EnemyTronic>().currentLocationIndex;

        if(DangLoc == 0){Dangus1A.enabled = true;}
        if(DangLoc == 1){Dangus1B.enabled = true;}
        if(DangLoc == 10){Dangus7.enabled = true;}
        if(DangLoc == 9){Dangus6.enabled = true;}
        if(DangLoc == 6){Dangus4A.enabled = true;}
        if(DangLoc == 7){Dangus4B.enabled = true;}

        if(DungLoc == 0){Dungus1A.enabled = true;}
        if(DungLoc == 1){Dungus1B.enabled = true;}
        if(DungLoc == 10){Dungus7.enabled = true;}
        if(DungLoc == 9){Dungus6.enabled = true;}
        if(DungLoc == 6){Dungus4A.enabled = true;}
        if(DungLoc == 7){Dungus4B.enabled = true;}
        if(DungLoc == 12){DungusEastDoor.enabled = true;}
 
        if(DongLoc == 0){Dongus1A.enabled = true;}
        if(DongLoc == 1){Dongus1B.enabled = true;}
        if(DongLoc == 8){Dongus5.enabled = true;}
        if(DongLoc == 3){Dongus2A.enabled = true;}
        if(DongLoc == 4){Dongus2B.enabled = true;}
        if(DongLoc == 5){Dongus3.enabled = true;}
        if(DongLoc == 11){DongusWestDoor.enabled = true;}

        KevText.text = kevPhase.ToString();
    }

    public override void Initialize()
    {
        m_ResetParams = Academy.Instance.EnvironmentParameters;
        spawnArea = this.transform.position;

        maxTime = hourDuration * 6;

        Dangus1A.enabled = false;

        if(!THEOne){
            Destroy(thelevelmodels);
        }
    }

    void ApplyDayPowerPenalty(){
        //allegedly the tech rules numbers but these don't check out based on how I interpreted them
        // if(day > 1 && day <= 5)
        // {
        //     currentPower -= Time.deltaTime / (8 - day);
        // }
        // if(day >= 5){
        //     currentPower -= Time.deltaTime / 3;
        // }

        float dayDegrade = day <= 5 ? day : 5;
        currentPower -= Time.deltaTime * (dayDegrade * 1.1f / hourDuration);
    }



    void Start(){
        if(!THEOne){
            Destroy(dingusView);
            Destroy(securityCam);
            Destroy(canvas);
        }

        canvas.SetActive(false);
        //westDoor = westDoorGO.GetComponent<DoorScript>();
        //eastDoor = eastDoorGO.GetComponent<DoorScript>();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //var agentPos = m_AgentRb.position - ground.transform.position;
        sensor.AddObservation(camsOpen);   
        sensor.AddObservation(powerOut);  
        sensor.AddObservation(beingMurdered);   
        sensor.AddObservation(justWaitingforTheEnd); // can this be fused with above? actually maybe we want those seperate
        sensor.AddObservation(camLockinc/camLockTime);   
        sensor.AddObservation(activeCamIndex);   
        sensor.AddObservation(currentPower/100f);   
        sensor.AddObservation(currentTime/hourDuration/6f); 
        sensor.AddObservation(officeCurrentlyMovingDirection);     //possibly unneeded
        sensor.AddObservation(officeDirectionChangeCounter/officeDirectionChangeTime);    //possibly unneeded
        sensor.AddObservation(officeFacingDirection);  
        sensor.AddObservation(eastDoor.open); 
        sensor.AddObservation(westDoor.open); 
        sensor.AddObservation(eastLight.on); 
        sensor.AddObservation(westLight.on); 
        sensor.AddObservation((int)((getPowerPenaltyMultiplier() - 1)/3f)); 
        sensor.AddObservation(dangusAtTheDoor); 

        sensor.AddObservation(spottedLeftLightEnemy);
        sensor.AddObservation(spottedRightLightEnemy);
        //TODO maybe remove these?
        sensor.AddObservation(spottedLeftLightEnemyInc/10f);
        sensor.AddObservation(spottedRightLightEnemyInc/10f);

        sensor.AddObservation(DangusLastSpottedIndex);
        sensor.AddObservation(DangusSpottedOldness >= 20f ? 20f : DangusSpottedOldness/20f); //possibly uneeded with memory
        //possibly unneeded? we just need to check if they're either side really
        sensor.AddObservation(DongusLastSpottedIndex);
        sensor.AddObservation(DongusSpottedOldness >= 20f ? 20 : DongusSpottedOldness); //possibly uneeded with memory
        sensor.AddObservation(DungusLastSpottedIndex);
        sensor.AddObservation(DungusSpottedOldness >= 20f ? 20 : DungusSpottedOldness); //possibly uneeded with memory
        //maybe cheat with kev? just know the phase?
        sensor.AddObservation(KevLastSpottedPhase);
        sensor.AddObservation(KevSpottedOldness >= 25f ? 25f/25f : KevSpottedOldness/25f); //possibly uneeded with memory
        sensor.AddObservation(kevInc/kevTime);

        //footsteps play when a enemytronic goes to or from the office
        sensor.AddObservation(heardFootstepsCounter);
        sensor.AddObservation(dangusInKillstate);
        sensor.AddObservation(dungusInKillstate);
        sensor.AddObservation(dongusInKillstate);
        sensor.AddObservation(lastTimeOpenedCamera);
        sensor.AddObservation(lastTimeWatchedDangus);
        sensor.AddObservation(timeStayedInCamera);

        sensor.AddObservation(heardFootstepsCheckedWest);
        sensor.AddObservation(heardFootstepsCheckedEast);
    }

    public void MoveAgent(ActionSegment<int> act)
    {
        if(beingMurdered){
            return;
        }
        if(levelEnded){
            return;
        }

        var rotateDirAction = act[1];
        //var jumpAction = act[2];
        if(rotateDirAction > 0 && !camsOpen && CanPerformAction()){
            rotateAgent(rotateDirAction);
        }

        var camAction = act[2];
        //THAR BE CHANGES HERE
        var camOn = act[3];
        //program in the various camera actions here
        if(camLockinc >= camLockTime)
        {
            camLockinc = camLockTime;
            if(clickedCamIndex >= 0 && CanPerformAction()){
                camAction = clickedCamIndex + 2;
                clickedCamIndex = -1;
            }
            if(camOn == 1 && CanPerformAction()){
                ToggleCam();
                camLockinc = 0;
            }
            else if(camAction > 1 && CanPerformAction()){
                ChangeCam(camAction - 2);
                camLockinc = 0;
            }
        }
        //THAR BE CHANGES HERE

        if(powerOut){
            return;
        }
        var doorAction = act[0];

        if(clickedDoorIndex >= 0 && CanPerformAction()){
            doorAction = 1;
            clickedDoorIndex = -1;
        }
        if(doorAction == 1 && !camsOpen && CanPerformAction()){
            if(officeFacingDirection == -1 && !westDoor.IsDoorMoving())
            {
                if(dongus.GetComponent<EnemyTronic>().currentLocationIndex == 13){
                    PlayDoorBlockedSound();
                    return;
                }
                //punish the wrong door action when you've spotted the enemy
                if(westDoor.open && spottedLeftLightEnemy){
                    AddReward(0.4f);
                }
                else if(!westDoor.open && spottedLeftLightEnemy){
                    AddReward(-0.61f);
                }
                else if(westDoor.open && (dongus.GetComponent<EnemyTronic>().currentLocationIndex != 11) && (kevPhase != 4 || kevPhase != 5)){
                    AddReward(-0.6f);
                }
                else if(!westDoor.open && (dongus.GetComponent<EnemyTronic>().currentLocationIndex != 11) && kevPhase != 4 || kevPhase != 5){
                    AddReward(0.3f);
                }
                PlayDoorCloseNoise();
                westDoor.ToggleDoor();
            }
            else if(officeFacingDirection == 1 && !eastDoor.IsDoorMoving())
            {
                if(dungus.GetComponent<EnemyTronic>().currentLocationIndex == 13){
                    PlayDoorBlockedSound();
                    return;
                }
                //punish the wrong door action when you've spotted the enemy
                if(eastDoor.open && spottedRightLightEnemy){
                    AddReward(0.5f);
                }
                else if(!eastDoor.open && spottedRightLightEnemy){
                    AddReward(-0.61f);
                }
                else if(eastDoor.open && (dungus.GetComponent<EnemyTronic>().currentLocationIndex != 12) && dangus.GetComponent<EnemyTronic>().currentLocationIndex != 7){
                    AddReward(-0.2f);
                }
                else if(!eastDoor.open && (dungus.GetComponent<EnemyTronic>().currentLocationIndex != 12) && dangus.GetComponent<EnemyTronic>().currentLocationIndex != 7){
                    AddReward(0.01f);
                }
                PlayDoorCloseNoise();
                eastDoor.ToggleDoor();
            }
        }
        
        if(doorAction == 2 && !camsOpen && CanPerformAction())
        {
            if(officeFacingDirection == -1 && !westLight.on)
            {
                westLight.ToggleLight();
                var dongusLoc = dongus.GetComponent<EnemyTronic>().currentLocationIndex;

                if(!heardFootstepsCheckedWest)
                {
                    heardFootstepsCheckedWest = true;
                    AddReward(0.2f);
                }

                if(dongusLoc == 11){
                    DongusLastSpottedIndex = 11;
                    DongusSpottedOldness = 0f;
                }

                if(!spottedLeftLightEnemy && dongusLoc == 11){
                    AddReward(0.2f);
                    if(heardFootstepsCounter <= 4f){AddReward(0.215f);}
                    spottedLeftLightEnemy = true;
                    spottedLeftLightEnemyInc = 0;
                    PlaySpottedSound();
                    
                }
                else if(spottedLeftLightEnemy && dongusLoc != 11){
                    spottedLeftLightEnemy = false;
                    AddReward(0.2f);
                    if(heardFootstepsCounter <= 4f){AddReward(0.215f);}
                }
            }
            else if(officeFacingDirection == 1 && !eastLight.on)
            {
                eastLight.ToggleLight();
                var dungusLoc = dungus.GetComponent<EnemyTronic>().currentLocationIndex;

                if(!heardFootstepsCheckedEast)
                {
                    heardFootstepsCheckedEast = true;
                    AddReward(0.5f);
                }

                if(dungusLoc == 12){
                    DungusLastSpottedIndex = 12;
                    DungusSpottedOldness = 0f;
                }

                if(!spottedRightLightEnemy && dungusLoc == 12){
                    AddReward(0.5f);
                    if(heardFootstepsCounter <= 4f){AddReward(0.215f);}
                    spottedRightLightEnemy = true;
                    spottedRightLightEnemyInc = 0;
                    PlaySpottedSound();
                }
                else if(spottedRightLightEnemy && dungusLoc != 12){
                    spottedRightLightEnemy = false;
                    AddReward(0.5f);
                    if(heardFootstepsCounter <= 4f){AddReward(0.215f);}
                }
            }
        }

    }

    public void PlayFootStepsNoise(){
        heardFootstepsCounter = 0f;
        heardFootstepsCheckedEast = false;
        heardFootstepsCheckedWest = false;
        if(THEOne){
            FootStepsSound.Play();
        }
    }

    public void PlayDieNoise(){
        if(THEOne){
            DieSound.Play();
        }
    }

    public void PlaySurviveSound(){
        if(THEOne){
            SurviveSound.Play();
        }
    }

    public void PlayDangusLaugh(){
        if(THEOne){
            int dangusLoc = dangus.GetComponent<EnemyTronic>().currentLocationIndex;
            switch(dangusLoc){
                case 1:
                    DangusLaugh.volume = 0.01f;
                    break;
                case 10:
                    DangusLaugh.volume = 0.02f;
                    break;
                case 9:
                    DangusLaugh.volume = 0.03f;
                    break;
                case 6:
                    DangusLaugh.volume = 0.04f;
                    break;
                case 7:
                    DangusLaugh.volume = 0.05f;
                    break;
                default:
                    DangusLaugh.volume = 0.06f;
                    break;
            }
            DangusLaugh.Play();
        }
    }

    public void PlayCameraFlipSound(){
        if(THEOne && !silenced){
            CameraFlipSound.Play();
        }
    }

    public void PlayAmbientNoise(){
        if(THEOne && !silenced){
            //play the footsteps
            AmbientSound.Play();
            AmbientSound.mute = false;
        }
    }

    public void MuteAmbientNoise(){
        if(THEOne){
            //play the footsteps
            AmbientSound.mute = true;
        }
    }

    public void PlayDoorCloseNoise(){
        if(THEOne && !silenced){
            DoorCloseSound.Play();
        }
    }

    public void PlayToreadorNoise(){
        if(THEOne && !silenced){
            ToreadorSound.Play();
        }
    }
    public void StopToreadorNoise(){
        if(THEOne && !silenced){
            ToreadorSound.Stop();
        }
    }

    public void PlayKevChargingSound(){
        if(THEOne && !silenced){

        }
    }

    public void PlayPowerOutSound(){
        if(THEOne && !silenced){
            PowerOutSound.Play();
        }
    }

    public void PlaySpottedSound(){
        if(THEOne && !silenced){
            SpottedSound.Play();
        }
    }

    public void PlayKevBangingSound(){
        if(THEOne && !silenced){
            KevBangSound.Play();
        }
    }

    public void PlayDoorBlockedSound(){
        if(THEOne && !silenced){
            DryFartSound.Play();
        }
    }

    public bool CanPerformAction(){
        return officeCurrentlyMovingDirection == 0 && westDoor.moveState == 0 && eastDoor.moveState == 0;
    }

    public void ToggleCam(){
        if(camsOpen){
            TurnOffCams();
        }
        else{
            if(powerOut)
            {
                return;
            }
            
            PerformCamOnBehaviours(activeCamIndex);

            ChangeCam(activeCamIndex);
        }

    }

    public void TurnOffCams(){
            camsOpen = false;
            kevLockInc = 0;
            kevLockTime = Random.Range(kevlockStateMinTime,kevlockStateMaxTime);
            kevLocked = true;
            //if(THEOne){Debug.Log("kev locked for" + kevLockTime + "seconds");}

            //this code is awful but I'm tired, check and see if dongus or dungus are in the room and perform the jumpscare if they are when the camera is pulled down
            var checkTronic = dongus.GetComponent<EnemyTronic>();
            if(checkTronic.currentLocationIndex == 13)
            {
                kill(checkTronic);
            }
            else {
                checkTronic = dungus.GetComponent<EnemyTronic>(); 
                if(checkTronic.currentLocationIndex == 13)
                {
                    kill(checkTronic);
                }
            }

            if(!THEOne){
                return;
            }

            //if(THEOne){Debug.Log("I don't get it, this ran after killing me, why aint the camera updating?");}
            securityCam.GetComponent<Camera>().depth = -1;
            UpdateCamButton();

            canvas.SetActive(camsOpen);
    }

    public void EmergencyExitCams(){
        if(THEOne){
            securityCam.GetComponent<Camera>().depth = -1;
            UpdateCamButton();
        }
    }

    public void PerformCamOnBehaviours(int newCamIndex){
        camsOpen = true;
        kevLocked = true;
        var kevlevelmulti = kev.GetComponent<EnemyTronic>().level/10;
        if(lastTimeOpenedCamera >= 0.83f)
        {
            var rewardAmount = lastTimeOpenedCamera >= 5f - kevlevelmulti ? 5f - kevlevelmulti : lastTimeOpenedCamera;
            rewardAmount *= 0.01f;
            var camcheckRewardMulti = 1 + (kev.GetComponent<EnemyTronic>().level/4);
            AddReward(rewardAmount * camcheckRewardMulti);

            if(newCamIndex == dangus.GetComponent<EnemyTronic>().currentLocationIndex){
                AddReward(rewardAmount * camcheckRewardMulti * 5f);
            }
        }
        lastTimeOpenedCamera = 0;
        timeStayedInCamera = 0;
    }

    public void ChangeCam(int camIndex){
        if(beingMurdered)
        {
            return;
        }
        if(!camsOpen){
            PerformCamOnBehaviours(camIndex);
        }
        PlayCameraFlipSound();
        camsOpen = true;
        activeCamIndex = camIndex;

        //if kev is phase 4 and you check that cam it immediately shows him charging as I understand it, putting that in here!
        UpdateIfKevRunning();
        //punish going into cams when you've spotted the enemy and yet left the door open
        if(eastDoor.open && spottedRightLightEnemy){
            AddReward(-0.72f);
        }
        else if(westDoor.open && spottedLeftLightEnemy){
            AddReward(-0.72f);
        }

        if(activeCamIndex != 7 && dangusAtTheDoor && eastDoor.open){
            AddReward(-0.72f);
        }
        // if(activeCamIndex != 7 && DangusLastSpottedIndex == 7 && DangusSpottedOldness <= 3f && eastDoor.open){
        //     AddReward(-0.3f);
        // }

        if(!THEOne){
            return;
        }
        securityCam.transform.position = camsList[camIndex].transform.position;
        securityCam.transform.rotation = camsList[camIndex].transform.rotation;
        securityCam.GetComponent<Camera>().depth = 1;

        UpdateCamButton();
        canvas.SetActive(camsOpen);


    }

    public void UpdateIfKevRunning(){
        if(activeCamIndex == 3 && kevPhase >= 4 && kevSprintInc < 22.9)
        {
            kevPhase = 5;
            kevSprintInc = 22.9f;
            KevLastSpottedPhase = kevPhase;
            KevSpottedOldness = 0;
            PlayKevChargingSound();
        }
    }

    public void UpdateCamButton(){
        if(!THEOne)
        {
            return;
        }
        if(!camsOpen){
           foreach(var cam in camButtons) {
            var button = cam.GetComponent<Image>();
            button.color = Color.grey;
           }
        }
        if(camsOpen){
            for(int i = 0; i<camButtons.Count; i++){
                var button = camButtons[i].GetComponent<Image>();
                if(i == activeCamIndex){
                    button.color = Color.green;
                }
                else{
                    button.color = Color.grey;
                }
            }
        }
    }

    public void rotateAgent(int dir){
        var actualdir = 0;
        if(dir == 1){
            actualdir =-1;
        }
        else if(dir == 2){
            actualdir =1;
        }
        if(officeDirectionChangeCounter >= officeDirectionChangeTime && officeFacingDirection + actualdir >= -1 && officeFacingDirection + actualdir <= 1 && officeCurrentlyMovingDirection == 0){
            officeDirectionChangeCounter = 0f;
            officeCurrentlyMovingDirection = actualdir;
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        MoveAgent(actionBuffers.DiscreteActions);
    }

    public void camClicked(int camIndex){
        clickedCamIndex = camIndex;
    }


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[1] = 2;
        }
        if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[1] = 1;
        }
        if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = 2;
        }

        if (Input.GetKey(KeyCode.BackQuote))
        {
            discreteActionsOut[2] = 1;
        }
        if (Input.GetKey(KeyCode.Alpha1))
        {
            discreteActionsOut[2] = 2;
        }
        if (Input.GetKey(KeyCode.Alpha2))
        {
            discreteActionsOut[2] = 3;
        }
        if (Input.GetKey(KeyCode.Alpha3))
        {
            discreteActionsOut[2] = 4;
        }
        if (Input.GetKey(KeyCode.Alpha4))
        {
            discreteActionsOut[2] = 5;
        }
        if (Input.GetKey(KeyCode.Alpha5))
        {
            discreteActionsOut[2] = 6;
        }
        if (Input.GetKey(KeyCode.Alpha6))
        {
            discreteActionsOut[2] = 7;
        }
        if (Input.GetKey(KeyCode.Alpha7))
        {
            discreteActionsOut[2] = 8;
        }
        if (Input.GetKey(KeyCode.Alpha8))
        {
            discreteActionsOut[2] = 9;
        }
        if (Input.GetKey(KeyCode.Alpha9))
        {
            discreteActionsOut[2] = 10;
        }
        if (Input.GetKey(KeyCode.Alpha0))
        {
            discreteActionsOut[2] = 11;
        }
        if (Input.GetKey(KeyCode.Minus))
        {
            discreteActionsOut[2] = 12;
        }

        if (Input.GetKey(KeyCode.C))
        {
            discreteActionsOut[3] = 1;
        }

    }

    // Detect when the agent hits the death wall
    void OnTriggerStay(Collider col)
    {
        if(levelEnded){
            return;
        }

    }

    IEnumerator LevelFinishCountdown(float time, bool weined)
    {
        yield return new WaitForSeconds(time);
        if(weined && THEOne && day < 7){
            //StartNextDay();
            var parent = this.transform.parent.parent;
            var agents = parent.GetComponentsInChildren<FiveNightsAtFreddingusAgent>();
            foreach (var agent in agents)
            {   
                if(!agent.THEOne)
                {
                    agent.StartNextDay();
                }
                
            }
            StartNextDay();
        }
        else{
            RedoLevel();
        }
    }

    IEnumerator showDeadPanel()
    {
        yield return new WaitForSeconds(2f);
        if(THEOne){
            deadPanel.SetActive(true);
        }
    }

    public void StartNextDay()
    {
        if(day < 7){
            day++;
        }
        EndEpisode();
    }

    public void RedoLevel()
    {
        EndEpisode();
    }

    public override void OnEpisodeBegin()
    {
        levelEndText.text = "";
        levelEnded = false;
        transform.eulerAngles = new Vector3(0,180,0);
        //levelMastaScript.currentRoom.RestartRoom();
        //levelMastaScript.ActivateTheOne(THEOne);
        //countDownTime = maxTime;
        //countDownDisplay.text = "Time: " + ((int)countDownTime).ToString();
        // var randomPosX = Random.Range(-m_SpawnAreaBounds.extents.x * 1f,
        //     m_SpawnAreaBounds.extents.x);
        // var randomPosZ = Random.Range(-m_SpawnAreaBounds.extents.z * 1f,
        //     m_SpawnAreaBounds.extents.z);
        transform.position = spawnArea;

        officeCurrentlyMovingDirection = 0;
        officeDirectionChangeCounter = 0;
        officeFacingDirection = 0;
        activeCamIndex = 0;
        clickedCamIndex = -1;
        currentPower = 100;
        currentTime = 0;
        officeFacingDirection = 0;
        dangusInc = 0;
        dangusMoving = false;
        if(dangusMoving){}
        dangusInc = 0;
        dongusInc = 0;
        dungusInc = 0;
        kevInc = 0;
        MoveToLoc(dangus, 0);
        MoveToLoc(dongus, 0);
        MoveToLoc(dungus, 0);
        MoveToLoc(kev, 2);
        westDoor.ResetDoor();
        eastDoor.ResetDoor();
        westLight.ResetLight();
        eastLight.ResetLight();
        kevLocked = false;
        kevPowerDrainAmount = 1;
        kevSprintInc = 0f;
        kevPhase = 1;
        kevSprintInc = 0;
        camPulldownKillinc = 0;
        dangusKillinc = 0;
        beingMurdered = false;
        powerOut = false;
        powerOutShowUpinc = 0;
        powerOutSingInc = 0;
        PowerOutScareInc = 0;
        powerOutShowUpTime = 99999999;
        powerOutSingTime = 9999999;
        startedMusic = false;

        spookinCharacter = null;
        jumpscareInc = 0;

        justWaitingforTheEnd = false;

        spottedLeftLightEnemy = false;
        spottedRightLightEnemy = false;
        spottedLeftLightEnemyInc = 0f;
        spottedRightLightEnemyInc = 0f;

        DangusLastSpottedIndex = 0;
        DangusSpottedOldness = 25f;
        DongusLastSpottedIndex = 0;
        DongusSpottedOldness = 25;
        DungusLastSpottedIndex = 0;
        DungusSpottedOldness = 25;
        KevLastSpottedPhase = 1;
        KevSpottedOldness = 25;
        
        if(THEOne){ livePanel.SetActive(false); }
        if(THEOne){ deadPanel.SetActive(false); }

        //config the enemytronics to the correct day
        dangusCountdownMax = (1000f - 100*dangus.GetComponent<EnemyTronic>().level)/60;
        dangus.GetComponent<EnemyTronic>().setLevelByDay(day);
        dongus.GetComponent<EnemyTronic>().setLevelByDay(day);
        dungus.GetComponent<EnemyTronic>().setLevelByDay(day);
        kev.GetComponent<EnemyTronic>().setLevelByDay(day);
        UppedDifficultyOne = false;
        UppedDifficultyTwo = false;
        UppedDifficultyThree = false;
        heardFootstepsCounter = 25f;

        dangusAtTheDoor = false;

        dangusInKillstate = false;
        dongusInKillstate = false;
        dungusInKillstate = false;

        lastTimeOpenedCamera = 7f;
        lastTimeWatchedDangus = 0f;

        timeStayedInCamera = 0f;

        //needs some help grasping basics methinks
        learningHelper = Random.Range(1f,6f);

        PlayAmbientNoise();

        heardFootstepsCheckedWest = true;
        heardFootstepsCheckedEast = true;

        if(THEOne){
            mainLight1.enabled = true;
            mainLight2.enabled = true;
        }

        freddyPrimeForKill = false;
    }

    private void FixedUpdate()
    {
        if(levelEnded){
            return;
        }
    }

    public void PlayKevMusic(){
        if(THEOne){
            //play the kev music to show he's in phase 1
        }
    }

    private void Update()
    {
        if(levelEnded || justWaitingforTheEnd){
            return;
        }

        killingSign.SetActive(kevPhase > 3);

        ShowIconLocations();

        if (Input.GetKeyDown("n"))
        {
            var parent = this.transform.parent.parent;
            var agents = parent.GetComponentsInChildren<FiveNightsAtFreddingusAgent>();
            foreach (var agent in agents)
            {   
                if(!agent.THEOne)
                {
                    agent.StartNextDay();
                }
                
            }
            StartNextDay();
        }

        if (Input.GetKeyUp("h"))
        {
            enemyLocationsHidden = !enemyLocationsHidden;
        }
        

        leftLightWaitCounter += Time.deltaTime;
        rightLightWaitCounter += Time.deltaTime;
        officeDirectionChangeCounter += Time.deltaTime;
        if(officeDirectionChangeCounter >= officeDirectionChangeTime){officeDirectionChangeCounter = officeDirectionChangeTime + 0.1f;}

        currentTime += Time.deltaTime;
        if(currentTime < hourDuration){
            timeText.text = "12 AM";
        }
        else{
            timeText.text = (currentTime/hourDuration).ToString("0") + " AM";
        }
        nightText.text = "Night " + day;

        if(beingMurdered && !justWaitingforTheEnd){
            jumpscareInc += Time.deltaTime;
            spookinCharacter.transform.LookAt(this.gameObject.transform);
            var lerpval = jumpscareInc * 2f;
            if(jumpscareInc >= 1f){lerpval = 1f;}
            spookinCharacter.transform.position = Vector3.Lerp(jumpscareStartSpot.transform.position,jumpscareEndSpot.transform.position,lerpval);

            if(jumpscareInc >= jumpscareTime){
                //begin the level end
                justWaitingforTheEnd = true;
                StartCoroutine( LevelFinishCountdown(7f,false) );
                StartCoroutine( showDeadPanel() );
            }

            return;
        }

        if(timeText.text ==  "6 AM" && !beingMurdered){
            //Debug.Log("We doneso");
            //need a huge reward here
            PlaySurviveSound();
            StopToreadorNoise();
            if(THEOne){ livePanel.SetActive(true); }

            justWaitingforTheEnd = true;
            AddReward(7f);
            if(kevPowerDrainAmount == 1){
                AddReward(5f);
            }
            StartCoroutine( LevelFinishCountdown(7f,true) );
        }

        if(officeCurrentlyMovingDirection != 0)
        {
            if(officeDirectionChangeCounter <= officeDirectionChangeTime){
                var rotateSpeed = rotateAmount / officeDirectionChangeTime;
                
                float XRotation = 0;
                float YRotation = transform.rotation.eulerAngles.y+(rotateSpeed * Time.deltaTime * officeCurrentlyMovingDirection);
                float ZRotation = 0;

                transform.eulerAngles = new Vector3(XRotation,YRotation,ZRotation);
            }
            else{
                float XRotation = 0;
                float YRotation = 0+( ((officeFacingDirection + officeCurrentlyMovingDirection) * rotateAmount) + 180f);
                float ZRotation = 0;
                transform.eulerAngles = new Vector3(XRotation,YRotation,ZRotation);

                officeFacingDirection += officeCurrentlyMovingDirection;
                officeCurrentlyMovingDirection = 0;
            }
        }

        if(powerOut){
            EmergencyExitCams();
            powerOutShowUpinc += Time.deltaTime;

            if(startedMusic){
                powerOutSingInc += Time.deltaTime;
            }
            if(powerOutShowUpinc >= powerOutShowUpTime && !startedMusic)
            {
                if(THEOne){Debug.Log("Oh lord he started singing");}
                startedMusic = true;
                PlayToreadorNoise();
                //actually play the music file
            }
            if(powerOutSingInc >= powerOutSingTime)
            {
                //stop music, turn off lights
                StopToreadorNoise();
                if(THEOne){Debug.Log("Oh lord he stopped singing");}
                PowerOutScareInc += Time.deltaTime;
            }
            if(PowerOutScareInc >= PowerOutScareTime){
                PowerOutScareInc = 0;
                if(Random.Range(1,6) == 1){
                    kill(dangus.GetComponent<EnemyTronic>());
                }
            }

        }

        if(powerOut){
            return;
        }

        if(kev.GetComponent<EnemyTronic>().currentLocationIndex == 1 && kevPhase == 1){
            kevMusicPlayInc += Time.deltaTime;
            if(kevMusicPlayInc >= 4f){
                kevMusicPlayInc = 0f;
                if(Random.Range(0,30) == 0){
                    KevLastSpottedPhase = 1;
                    KevSpottedOldness = 0;
                    PlayKevMusic();
                }
            }
        }
        else{
            kevMusicPlayInc = 0f;
        }

        if(!camsOpen)
        {
            lastTimeOpenedCamera += Time.deltaTime;
        }
        else{
            if(timeStayedInCamera >= 2.0f)
            {
                AddReward(-0.05f);
                timeStayedInCamera = 0;
            }
            timeStayedInCamera += Time.deltaTime;
        }

        dangusInc += Time.deltaTime;
        dongusInc += Time.deltaTime;
        dungusInc += Time.deltaTime;
        camLockinc += Time.deltaTime;
        heardFootstepsCounter += Time.deltaTime;

        if(dangusInc >= dangusTime){
            //MoveEnemyTronic("Dangus", ); // need to actually pull the name from the enemytronic I guess in this flow?
            MoveEnemyTronic(dangus);
            dangusInc = 0;
        }
        if(dongusInc >= dongusTime){
            //MoveEnemyTronic("Dangus", ); // need to actually pull the name from the enemytronic I guess in this flow?
            MoveEnemyTronic(dongus);
            dongusInc = 0;
        }
        if(dungusInc >= dungusTime){
            //MoveEnemyTronic("Dangus", ); // need to actually pull the name from the enemytronic I guess in this flow?
            MoveEnemyTronic(dungus);
            dungusInc = 0;
        }
        dangusActualMovementInc += Time.deltaTime;
        if(dangusMoving && dangusActualMovementInc >= dangusCountdownMax && !camsOpen){
            //if(THEOne){Debug.Log("Dangus be schmooving");}
            actualDangusMove();
        }
        //DangusKillCheck
        dangusKillinc+= Time.deltaTime;
        if(dangus.GetComponent<EnemyTronic>().currentLocationIndex == 13 && dangusKillinc >= dangusKillTime)
        {
            dangusKillinc = 0;
            if(!camsOpen && Random.Range(0,4) == 0){
                kill(dangus.GetComponent<EnemyTronic>());
            }
        }
        if(camsOpen){
            camPulldownKillinc += Time.deltaTime;
        }
        if(camsOpen && camPulldownKillinc >= camPulldownKillTime)
        {
            //This code is very yuck, but I'm tired
            var checkTronic = dongus.GetComponent<EnemyTronic>();
            if(checkTronic.currentLocationIndex == 13)
            {
                ToggleCam();
            }
            checkTronic = dungus.GetComponent<EnemyTronic>(); 
            if(checkTronic.currentLocationIndex == 13)
            {
                ToggleCam();
            }
        }

        kevInc += Time.deltaTime;
        kevLockInc += Time.deltaTime;
        kevSprintInc += Time.deltaTime;
        if(kevLockInc >= kevLockTime && !camsOpen){
            kevLocked = false;
        }
        if(!kevLocked && kevInc >= kevTime && kevPhase < 4)
        {
            kevInc = 0;
            if(Random.Range(1,21) <= kev.GetComponent<EnemyTronic>().level)
            {
                if(kevPhase < 4){
                    kevPhase++;
                    kevSprintInc = 0;
                    MoveKev(); 
                }

            }
        }else if(kevLocked && kevInc >= kevTime && kevPhase < 4){
            kevInc = 0;
            //Add a reward based on if Kev Likely would have attacked on average, this should hopefully promote better camera management as kev levels go higher
            if(Random.Range(1,21) <= kev.GetComponent<EnemyTronic>().level)
            {
                if(kevPhase < 4){
                    //this on day six is like 0.2 checks per second, 0.75 chance to go rambo, 0.4 chance of being locked if always locked optimally, 85 seconds in a hour, 6 hours a night = 306 points if the multiplier was 1
                    AddReward(0.003f);
                }
            }
        }

        if(kevSprintInc >= kevSprintTime - 2 && kevPhase >= 4)
        {
            kev.transform.position = Vector3.Lerp(kevPhaseLocations[kevPhaseLocations.Count - 2].transform.position, kevPhaseLocations[kevPhaseLocations.Count - 1].transform.position, (kevSprintInc - 23)/(kevSprintTime - 23));
        }
        if(kevSprintInc >= kevSprintTime && kevPhase >= 4)
        {
            kevSprintInc = 0;
            if(westDoor.open){
                if(camsOpen)
                {
                    ToggleCam();
                }
                AddReward(-0.5f);
                applyKillstatePenalty();
                kill(kev.GetComponent<EnemyTronic>());
            }
            else{
                if(THEOne){Debug.Log("kev banged door and took power");}
                currentPower -= kevPowerDrainAmount;
                if(kevPowerDrainAmount == 1){
                    AddReward(0.25f);
                }
                AddReward(-0.1f);
                kevPowerDrainAmount += 6;
                kevPhase = 1;
                PlayKevBangingSound();
                MoveKev();
            }

        }

        if (Input.GetKeyDown("h"))
        {
            Debug.Log("n key was pressed");
            timeWinPanel.SetActive(!timeWinPanel.activeSelf);
            timetrainpanel.SetActive(!timetrainpanel.activeSelf);
        }

        if(Input.GetKeyDown("q"))
        {
            Debug.Log("q, silence key, was pressed");
            silenced = !silenced;
            if(silenced){
                MuteAmbientNoise();
            }
            else{
                PlayAmbientNoise();
            }

            if(THEOne){
                dingusView.GetComponent<AudioListener>().enabled = !dingusView.GetComponent<AudioListener>().enabled;
            }
        }

        //lets do the power loss here
        ApplyDayPowerPenalty();
        var powerMulti = getPowerPenaltyMultiplier();
        if(currentPower <= 0 && !powerOut)
        {
            PlayPowerOutSound();
            if(THEOne){
                mainLight1.enabled = false;
                mainLight2.enabled = false;
            }

            MuteAmbientNoise();
            if(camsOpen){
                ToggleCam();
            }
            if(THEOne){Debug.Log("Oh lord it's power out");}
            powerOut = true;
            powerOutShowUpinc = 0;
            powerOutSingInc = 0;
            PowerOutScareInc = 0;
            PowerOutScareTime = 2f;

            powerOutShowUpTime = 5 * Random.Range(1,5);
            powerOutSingTime = 5 * Random.Range(1,5);

            //don't punish if odds are they'd win! // actually no let's mostly punish them, ideally they win with energy left
            if(currentTime <= maxTime - 30f){
                AddReward(-0.45f);
            }

            if(!eastDoor.open){
                eastDoor.ToggleDoor();
            }
            if(!westDoor.open){
                westDoor.ToggleDoor();
            }
        }

        //something a bit wonky with the power consumption
        currentPower -= Time.deltaTime * powerMulti*applianceUseBaseEnergyCost/hourDuration*0.85f;
        if(THEOne){
            // var text = powerLeftText.GetComponent<TextMeshPro>();
            // text.text = "Power Left: " + currentPower.ToString("0") + "%";
            powerLeftText.text = "Power Left: " + currentPower.ToString("0") + "%";
            usageText.text = "Usage: " + powerMulti.ToString("0");
        }

        float rewardMulti = 1f;

        if(getPowerPenaltyMultiplier() == 2){
            rewardMulti = 0.3f;
        }

        if(getPowerPenaltyMultiplier() == 3){
            rewardMulti = 0.1f;
        }

        if(getPowerPenaltyMultiplier() == 4){
            rewardMulti = 0.0f;
        }


        AddReward(rewardMulti * Time.deltaTime * 0.05f);

        if(!UppedDifficultyOne && currentTime >= hourDuration * 2f){
            UppedDifficultyOne = true;

            int timeAsInt = (int)(currentTime / hourDuration);
            dangus.GetComponent<EnemyTronic>().PerformDifficultyIncrement(timeAsInt);
            dongus.GetComponent<EnemyTronic>().PerformDifficultyIncrement(timeAsInt);
            dungus.GetComponent<EnemyTronic>().PerformDifficultyIncrement(timeAsInt);
            kev.GetComponent<EnemyTronic>().PerformDifficultyIncrement(timeAsInt);
        }
        if(!UppedDifficultyTwo && currentTime >= hourDuration * 3f){
            UppedDifficultyTwo = true;

            int timeAsInt = (int)(currentTime / hourDuration);
            dangus.GetComponent<EnemyTronic>().PerformDifficultyIncrement(timeAsInt);
            dongus.GetComponent<EnemyTronic>().PerformDifficultyIncrement(timeAsInt);
            dungus.GetComponent<EnemyTronic>().PerformDifficultyIncrement(timeAsInt);
            kev.GetComponent<EnemyTronic>().PerformDifficultyIncrement(timeAsInt);
        }
        if(!UppedDifficultyThree && currentTime >= hourDuration * 4f){
            UppedDifficultyThree = true;

            int timeAsInt = (int)(currentTime / hourDuration);
            dangus.GetComponent<EnemyTronic>().PerformDifficultyIncrement(timeAsInt);
            dongus.GetComponent<EnemyTronic>().PerformDifficultyIncrement(timeAsInt);
            dungus.GetComponent<EnemyTronic>().PerformDifficultyIncrement(timeAsInt);
            kev.GetComponent<EnemyTronic>().PerformDifficultyIncrement(timeAsInt);
        }

        DangusSpottedOldness += Time.deltaTime;
        DongusSpottedOldness += Time.deltaTime;
        DungusSpottedOldness += Time.deltaTime;
        KevSpottedOldness += Time.deltaTime;

        //TODO, these may be irrelevant, it may be smart enough to associate the position it has found individual enemytronics instead of this
        spottedLeftLightEnemyInc += Time.deltaTime;
        spottedRightLightEnemyInc += Time.deltaTime;
        if(spottedLeftLightEnemyInc >= 10f){spottedLeftLightEnemyInc = 10f;}
        if(spottedRightLightEnemyInc >= 10f){spottedRightLightEnemyInc = 10f;}

        if(camsOpen){
            var dangPos = dangus.GetComponent<EnemyTronic>().currentLocationIndex;
            var dongPos = dongus.GetComponent<EnemyTronic>().currentLocationIndex;
            var dungPos = dungus.GetComponent<EnemyTronic>().currentLocationIndex;
            var kevPos = kev.GetComponent<EnemyTronic>().currentLocationIndex;

            if(dangPos == activeCamIndex && dangPos == 7){
                //we want to be careful with this, Dangus is the most important to track on cam but make this too high and Dingus will ignore anything else
                AddReward(0.0015f * Time.deltaTime);
            }
            if(dongPos == activeCamIndex){
                DongusLastSpottedIndex = dongPos;
                DongusSpottedOldness = 0;
            }
            if(dungPos == activeCamIndex){
                DungusLastSpottedIndex = dungPos;
                DungusSpottedOldness = 0;
            }
            if(kevPos == activeCamIndex){
                KevLastSpottedPhase = kevPhase;
                KevSpottedOldness = 0;
            }
        }

        //I'm hoping this will give him the chance to learn how to block these early and contribute to his understanding of the game
        //learningHelper -= Time.deltaTime;
        if(learningHelper <= 0 && day < 7){
            var who = Random.Range(0,2);
            if(who == 0){
                MoveToLoc(dongus,4);
            }
            else{
                MoveToLoc(dungus,7);

            }
            learningHelper = 9999f;
        }

        if(beingMurdered){
            return;
        }

        KillCheck(dangus.GetComponent<EnemyTronic>());
    }

    public void actualDangusMove(){
        var enemyScript = dangus.GetComponent<EnemyTronic>();
        var currentLocIndex = enemyScript.currentLocationIndex;
        var enemyTronic = dangus;

        dangusMoving = false;
        dangusActualMovementInc = 0;

        if(currentLocIndex == 0){
            MoveToLoc(enemyTronic,1);
        }
        else if(currentLocIndex == 1){
            MoveToLoc(enemyTronic,10);
        }
        else if(currentLocIndex == 10){
            MoveToLoc(enemyTronic,9);
        }
        else if(currentLocIndex == 9){
            MoveToLoc(enemyTronic,6);
        }
        else if(currentLocIndex == 6){
            MoveToLoc(enemyTronic,7);
        }

        //because it should play a laugh when he moves we can figure out he has moved another location closer because his path is linear
        PlayDangusLaugh();
        DangusLastSpottedIndex = enemyScript.currentLocationIndex;
        DangusSpottedOldness = 0f;
    }

    public void MoveKev(){
        var kevScript = kev.GetComponent<EnemyTronic>();

        kev.transform.position = kevPhaseLocations[kevPhase-1].transform.position + new Vector3(0f,0.9f,0f);
        kev.transform.rotation = kevPhaseLocations[kevPhase-1].transform.rotation;
    }
    
    public void MoveToLoc(GameObject enemyTronic, int locIndex){

        //need to handle the special case of checking if door is closed if they're in index 11 or 12
        var enemyScript = enemyTronic.GetComponent<EnemyTronic>();

        //Debug.Log(enemyScript.name + " succeeded a move opportunity to room index: " + locIndex);

        var daRoom = roomsList[locIndex];

        var daSpawn = daRoom.transform.Find(enemyScript.name + "Spawn");

        enemyTronic.transform.position = daSpawn.position + new Vector3(0f,0.9f,0f);
        enemyTronic.transform.rotation = daSpawn.transform.rotation;
        enemyScript.currentLocationIndex = locIndex;

        if(enemyScript.name == "Dongus" && enemyScript.currentLocationIndex == 11)
        {
            PlayFootStepsNoise();
        }
        else if(enemyScript.name == "Dungus" && enemyScript.currentLocationIndex == 12){
            PlayFootStepsNoise();
        }

        if(enemyScript.name == "Dangus" && enemyScript.currentLocationIndex == 7){
            dangusAtTheDoor = true;
        }
        else if(enemyScript.name == "Dangus" && enemyScript.currentLocationIndex != 7){
            dangusAtTheDoor = false;
        }
    }

    public bool KillCheck(EnemyTronic enemy){
        var enemyName = enemy.enemyTronicName;
        //this should be safe
        if(powerOut){
            return false;
        }

        if(enemyName == "Dongus" && enemy.currentLocationIndex == 11)
        {
            if(westDoor.open){
                killState(enemy);
                return true;
            }
            heardFootstepsCounter = 0;
            PlayFootStepsNoise();
        }

        if(enemyName == "Dungus" && enemy.currentLocationIndex == 12)
        {
            if(eastDoor.open){
                killState(enemy);
                return true;
            }
            heardFootstepsCounter = 0;
            PlayFootStepsNoise();
        }

        if(enemyName == "Dangus" && enemy.currentLocationIndex == 7 && camsOpen && activeCamIndex != 7 && freddyPrimeForKill){
            if(eastDoor.open){
                killState(enemy);
                return true;
            }
            else{
                freddyPrimeForKill = false;
                MoveToLoc(enemy.gameObject,6);
                PlayDangusLaugh();
                DangusLastSpottedIndex = enemy.currentLocationIndex;
                DangusSpottedOldness = 0f;
                dangusInc = 0;
            }
        }

        //if(THEOne){Debug.Log("kill attempt by " + enemyName + "thwarted");}
        return false;
    } 

    void applyKillstatePenalty(){
        if(!dangusInKillstate && !dongusInKillstate && !dungusInKillstate){
            AddReward(-0.5f);
        }
    }

    public void killState(EnemyTronic enemy){
        if(THEOne){Debug.Log("YUP " + enemy.name + "has entered kill state");}
        if(enemy.name == "Dangus"){
            MoveToLoc(enemy.gameObject,13);
            dangusKillinc = 0;
            applyKillstatePenalty();
            dangusInKillstate = true;
            DangusLastSpottedIndex = 13;
            DangusSpottedOldness = 0;
        }

        if(enemy.name == "Dongus" || enemy.name == "Dungus")
        {
            if(enemy.name == "Dongus"){
                DongusLastSpottedIndex = 13;
                DongusSpottedOldness = 0;
                applyKillstatePenalty();
                dongusInKillstate = true;
            }
            else if(enemy.name == "Dungus"){
                DungusLastSpottedIndex = 13;
                DungusSpottedOldness = 0;
                applyKillstatePenalty();
                dungusInKillstate = true;
            }

            if(dongus.GetComponent<EnemyTronic>().currentLocationIndex != 13 && dungus.GetComponent<EnemyTronic>().currentLocationIndex != 13)
            {
                camPulldownKillinc = 0;
            }
        }
    }

    public void kill(EnemyTronic enemy){
        //technically they don't kill us, they wait to kill us, need to check the tech rules vid to recheck
        //if(THEOne){Debug.Log("YUP " + enemy.name + "killed you");}
        //to prevent overlapping jumpscares and the like, check if the player is already being murdered
        if(!beingMurdered)
        {
            PlayDieNoise();
            if(THEOne){Debug.Log("YUP " + enemy.name + "killed you");}
            beingMurdered = true;
            spookinCharacter = enemy.gameObject;
            //activate the jumpscare, begin the timer to end the episode!
        }
    }

    public void MoveEnemyTronic(GameObject enemyTronic){

        var enemyScript = enemyTronic.GetComponent<EnemyTronic>();

        var enemyName = enemyScript.enemyTronicName;
        var currentLocIndex = enemyScript.currentLocationIndex;

        if(Random.Range(1,21) > enemyScript.level)
        {
            //Debug.Log(enemyName + " failed a move opportunity");
            return;
        }
        //Debug.Log(enemyName + " succeeded a move opportunity");

        if(enemyName == "Dongus"){ //Bonny
            if(currentLocIndex == 0){
                MoveToLoc(enemyTronic,ReturnNumberChoice(new[] {1,8} ));
            }
            else if(currentLocIndex == 1){
                MoveToLoc(enemyTronic,ReturnNumberChoice(new[] {8,3} ));
            }
            else if(currentLocIndex == 3){
                MoveToLoc(enemyTronic,ReturnNumberChoice(new[] {5,4} ));
            }
            else if(currentLocIndex == 4){
                MoveToLoc(enemyTronic,ReturnNumberChoice(new[] {5,11} ));
            }
            else if(currentLocIndex == 5){
                MoveToLoc(enemyTronic,ReturnNumberChoice(new[] {4,11} ));
            }
            else if(currentLocIndex == 8){
                MoveToLoc(enemyTronic,ReturnNumberChoice(new[] {1,3} ));
            }
            else if(currentLocIndex == 11){
                if(KillCheck(enemyScript)){
                    MoveToLoc(enemyTronic,13);
                }
                else{
                    MoveToLoc(enemyTronic,1);
                }
                //special case, this can potentially kill
            }
        }

        if(enemyName == "Dungus"){
            if(currentLocIndex == 0){
                MoveToLoc(enemyTronic,1);
            }
            else if(currentLocIndex == 1){
                MoveToLoc(enemyTronic,ReturnNumberChoice(new[] {10,9} ));
            }
            else if(currentLocIndex == 6){
                MoveToLoc(enemyTronic,ReturnNumberChoice(new[] {1,7} ));
            }
            else if(currentLocIndex == 7){
                MoveToLoc(enemyTronic,ReturnNumberChoice(new[] {12,6} ));
            }
            else if(currentLocIndex == 9){
                MoveToLoc(enemyTronic,ReturnNumberChoice(new[] {10,6} ));
            }
            else if(currentLocIndex == 10){
                MoveToLoc(enemyTronic,ReturnNumberChoice(new[] {9,6} ));
            }
            else if(currentLocIndex == 12){
                if(KillCheck(enemyScript)){
                    MoveToLoc(enemyTronic,13);
                }
                else{
                    MoveToLoc(enemyTronic,6);
                }               
                //special case, this can potentially kill
            }
        }

        if(enemyName == "Dangus"){
            if(dangusMoving){
                return;
            }

            if(currentLocIndex == 7){
                freddyPrimeForKill = true;

                KillCheck(enemyScript);
            }
            else if(!camsOpen){
                //if(THEOne){Debug.Log("Dangus schmoove count started");}
                dangusActualMovementInc = 0;
                dangusMoving = true;
            }
            else{
                dangusInc = 0;
            }

            // if(currentLocIndex == 7){
            //     freddyPrimeForKill = true;

            //     if(KillCheck(enemyScript)){
            //         MoveToLoc(enemyTronic,13);
            //     }
            //     else if(!eastDoor.open && camsOpen && activeCamIndex != 7){
            //         MoveToLoc(enemyTronic,6);
            //         PlayDangusLaugh();
            //         DangusLastSpottedIndex = enemyScript.currentLocationIndex;
            //         DangusSpottedOldness = 0f;
            //         dangusInc = 0;
            //     }
            //     else{
            //         //didn't succeed move check but the other mechanics weren't satisfies, do nothing
            //         dangusInc = 0;
            //     }

            //     //special case, this can potentially kill, Freddy will enter the room when his move action comes and the door is open and the camera is not 
                
            //     //Freddy needs special logic for moving into the office and for not moving whilst looked at, do I really want to add that second part?

            //     // Moving on, Freddy lets out a single series of laughs and makes running 
            //     // footsteps every time he moves to a different room, and these sounds get 
            //     // louder as he gets closer. You can use the number of laughs to pinpoint 
            //     // Freddy’s location. For example, if Freddy has left the show stage, 
            //     // he will have laughed only once. Once Freddy has reached the East Hall Corner, 
            //     // he will have let out five laughs. If he laughs six times, then he got into the office, and you’re dead after a few seconds.
            // }
            // else if(!camsOpen){
            //     //if(THEOne){Debug.Log("Dangus schmoove count started");}
            //     dangusActualMovementInc = 0;
            //     dangusMoving = true;
            // }
            // else{
            //     dangusInc = 0;
            // }
        }
    }

    private void FreddyKillCheck(){

    }

    private int ReturnNumberChoice(int[] choices){
        return choices[Random.Range(0,choices.Length)];
    }

    float getPowerPenaltyMultiplier(){
        //every thing on adds one to the multi
        float currentMulti = 1;
        if(!eastDoor.open)
        {
            currentMulti++;
        }
        if(!westDoor.open)
        {
            currentMulti++;
        }
        if(eastLight.on)
        {
            currentMulti++;
        }
        if(westLight.on)
        {
            currentMulti++;
        }
        if(camsOpen)
        {
            currentMulti++;
        }
        if(currentMulti > 4)
        {
            currentMulti = 4;
        }

        return currentMulti;
    }
}
