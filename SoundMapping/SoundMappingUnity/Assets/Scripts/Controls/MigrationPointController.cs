using System;
using System.Collections.Generic;
using UnityEngine;

public class MigrationPointController : MonoBehaviour
{
    public Camera mainCamera; // Assign your main camera in the Inspector
    public LayerMask groundLayer; // Layer mask for the ground
    public LayerMask droneLayer; // Layer mask for the drones
    public float spawnHeight = 10f; // Height at which drones operate
    public float radius = 1f; // Radius of the migration point

    public Vector2 migrationPoint = new Vector2(0, 0);

    private int lastSelectedChild = 0;
    public static GameObject selectedDrone = null;

    public Material normalMaterial;
    public Material selectedMaterial;

    public Vector3 deltaMigration = new Vector3(0, 0, 0); 
    public static Vector3 alignementVector = new Vector3(0, 0, 0);
    public static Vector3 alignementVectorNonZero = new Vector3(0, 0, 0);

    public static float maxSpreadness = 5f;
    public static float minSpreadness = 1f;

    bool firstTime = true;

    public bool control_movement
    {
        get{
            return LevelConfiguration._control_movement;
        }
    }
    public bool control_spreadness
    {
        get{
            return LevelConfiguration._control_spreadness;
        }
    }
    public bool control_embodiement
    {
        get{
            return LevelConfiguration._control_embodiement;
        }
    }

    public bool _control_desembodiement
    {
        get{
            return LevelConfiguration._control_desembodiement;
        }
    }
    public bool control_selection
    {
        get
        {
            return LevelConfiguration._control_selection;
        }
    }
    public bool control_rotation
    {
        get
        {
            return LevelConfiguration._control_rotation;
        }
    }


    void Update()
    {
        UpdateMigrationPoint();
        SelectionUpdate();  
        SpreadnessUpdate();
    }

    void SelectionUpdate()
    { 
        if(Input.GetKeyDown("joystick button " + 3))
        {
            
            swarmModel.dummyForcesApplied = !swarmModel.dummyForcesApplied;
        }       

        if((Input.GetKeyDown("joystick button " + 5) || Input.GetKeyDown("joystick button " + 4)) && control_selection) //selection
        {
            if(selectedDrone == null)
            {
                if(swarmModel.swarmHolder.transform.childCount > 0)
                {
                    selectedDrone = swarmModel.swarmHolder.transform.GetChild(0).gameObject;

                }
            }
            else
            {
                if(CameraMovement.embodiedDrone != null)
                {
                    Dictionary<GameObject, float> scores = new Dictionary<GameObject, float>();

                    foreach(Transform drone in swarmModel.swarmHolder.transform)
                    {
                        if(drone.gameObject == CameraMovement.embodiedDrone)
                        {
                            continue;
                        }

                        Vector3 diff = drone.position - CameraMovement.embodiedDrone.transform.position;
                        float score = Vector3.Dot(diff, CameraMovement.embodiedDrone.transform.forward);

                        if(score > 0.5)
                        {
                            score /= diff.magnitude;
                            scores.Add(drone.gameObject, score);
                        }

                    }

                    //select the highest score
                    if(scores.Count > 0)
                    {
                        //sort the dictionary
                        List<KeyValuePair<GameObject, float>> sortedScores = new List<KeyValuePair<GameObject, float>>(scores);
                        sortedScores.Sort((x, y) => y.Value.CompareTo(x.Value));

                        //select the highest score
                        selectedDrone = sortedScores[0].Key;
                    }
                }
                else
                {
                    int increment = Input.GetKeyDown("joystick button " + 5) ? 1 : -1;
                    //change material
                    lastSelectedChild = (lastSelectedChild + increment) % swarmModel.swarmHolder.transform.childCount;
                    if(lastSelectedChild < 0)
                    {
                        lastSelectedChild = swarmModel.swarmHolder.transform.childCount - 1;
                    }

                    selectedDrone = swarmModel.swarmHolder.transform.GetChild(lastSelectedChild).gameObject;
                }
            }

            this.GetComponent<HapticsTest>().VibrateController(0.3f, 0.3f, 0.2f); // selection vibration
        }

        // button 0
        if(Input.GetKeyDown("joystick button " + 0) && control_embodiement) //embodiement
        {
            if(CameraMovement.state == "animation")
            {
                return;
            }
            if(CameraMovement.embodiedDrone != null)
            {
                if(selectedDrone != CameraMovement.embodiedDrone)//drone 2 drone
                {
                    CameraMovement.nextEmbodiedDrone = selectedDrone; // set next selected drone diff to null to trigger animation to the other drone
                }
            }
            else if(selectedDrone != null)
            {
                CameraMovement.setEmbodiedDrone(selectedDrone);
            }
        }

        if(Input.GetKeyDown("joystick button " + 1) && _control_desembodiement) //desembodie
        {
            if(CameraMovement.embodiedDrone != null)
            {
                CameraMovement.embodiedDrone.GetComponent<Camera>().enabled = false;                
                CameraMovement.desembodiedDrone(CameraMovement.embodiedDrone); 
            }
        }
    }

    void SpreadnessUpdate()
    {
        float spreadness = Input.GetAxis("LR");
        if(spreadness != 0 && control_spreadness)
        {
            swarmModel.desiredSeparation+= spreadness * Time.deltaTime * 1.3f;
            swarmModel.desiredSeparation = Mathf.Clamp(swarmModel.desiredSeparation, minSpreadness, maxSpreadness);
        }
    }

    void UpdateMigrationPoint()
    {
        if(!control_movement)
        {
            return;
        }


        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        float heightControl = Input.GetAxis("JoystickRightVertical");
        Transform body = null;
        Vector3 right = new Vector3(0, 0, 0);
        Vector3 forward = new Vector3(0, 0, 0);
        Vector3 up = new Vector3(0, 0, 0);

        Vector3 final = new Vector3(0, 0, 0);

        if(CameraMovement.embodiedDrone == null)
        {
            body = CameraMovement.cam.transform;
            right = body.right;
            forward = body.up;
            up = -body.forward;

        }else{
            body = CameraMovement.embodiedDrone.transform;
            right = body.right;
            forward = body.forward;
            up = body.up;
        }

        CameraMovement.forward = forward;
        CameraMovement.right = right;
        CameraMovement.up = up;

        if(horizontal == 0 && vertical == 0 && heightControl == 0)
        {
            if(firstTime)
            {
                migrationPoint = new Vector2(body.position.x, body.position.z);
                firstTime = false;
            }
            //migrationPoint = new Vector2(body.position.x, body.position.z);
            deltaMigration = new Vector3(0, 0, 0);
        }else{
            firstTime = true;
            Vector3 centerOfSwarm = body.position;
            final = vertical * forward + horizontal * right + heightControl * up;
            final.Normalize();

            float newR = Mathf.Sqrt(horizontal * horizontal + vertical * vertical + heightControl * heightControl);
            Vector3 finalAlignement = final * newR * radius;

            final = final * radius;


            migrationPoint = new Vector2(centerOfSwarm.x + final.x, centerOfSwarm.z + final.z);
            deltaMigration = new Vector3(finalAlignement.x, finalAlignement.y, finalAlignement.z);
        }
        if( deltaMigration.magnitude > 0.1f)
        {
            alignementVectorNonZero = deltaMigration;
        }
        
        alignementVector = deltaMigration;

        Debug.DrawRay(body.position, alignementVector, Color.red, 0.01f);
    }

}