using System;
using UnityEngine;

public class MigrationPointController : MonoBehaviour
{
    public Camera mainCamera; // Assign your main camera in the Inspector
    public LayerMask groundLayer; // Layer mask for the ground
    public LayerMask droneLayer; // Layer mask for the drones
    public float spawnHeight = 10f; // Height at which drones operate
    public float radius = 0.5f; // Radius of the migration point

    public Vector2 migrationPoint = new Vector2(0, 0);

    private int lastSelectedChild = 0;
    public static GameObject selectedDrone = null;

    public Material normalMaterial;
    public Material selectedMaterial;

    public Vector3 deltaMigration = new Vector3(0, 0, 0); 
    public static Vector3 alignementVector = new Vector3(0, 0, 0);

    bool firstTime = true;

    void Update()
    {
        UpdateMigrationPoint();
        SelectionUpdate();  
        SpreadnessUpdate();
    }

    void SelectionUpdate()
    {        
        if(Input.GetKeyDown("joystick button " + 5) || Input.GetKeyDown("joystick button " + 4)) 
        {
            if(selectedDrone == null)
            {
                if(swarmModel.swarmHolder.transform.childCount > 0)
                {
                    selectedDrone = swarmModel.swarmHolder.transform.GetChild(0).gameObject;
                    selectedDrone.GetComponent<Renderer>().material = selectedMaterial;
                }
            }
            else
            {
                int increment = Input.GetKeyDown("joystick button " + 5) ? 1 : -1;
                //change material
                selectedDrone.GetComponent<Renderer>().material = normalMaterial;
                lastSelectedChild = (lastSelectedChild + increment) % swarmModel.swarmHolder.transform.childCount;
                if(lastSelectedChild < 0)
                {
                    lastSelectedChild = swarmModel.swarmHolder.transform.childCount - 1;
                }

                selectedDrone = swarmModel.swarmHolder.transform.GetChild(lastSelectedChild).gameObject;
                selectedDrone.GetComponent<Renderer>().material = selectedMaterial;
            }
        }

        // bvutton 0
         if(Input.GetKeyDown("joystick button " + 0))
        {
            if(CameraMovement.embodiedDrone != null)
            {
                if(selectedDrone != CameraMovement.embodiedDrone)
                {
                    CameraMovement.nextEmbodiedDrone = selectedDrone; // set next selected drone diff to null to trigger animation to the other drone
                }
                else
                {
                    CameraMovement.embodiedDrone.GetComponent<Camera>().enabled = false;                
                    CameraMovement.desembodiedDrone(); 
                }
            }
            else if(selectedDrone != null)
            {
                CameraMovement.setEmbodiedDrone(selectedDrone);
            }
        }
    }

    void SpreadnessUpdate()
    {
        float spreadness = Input.GetAxis("LR");
        if(spreadness != 0)
        {
            swarmModel.desiredSeparation+= spreadness * Time.deltaTime * 1.3f;
            if(swarmModel.desiredSeparation < 1.5)
            {
                swarmModel.desiredSeparation = 1.5f;
            } 
            if(swarmModel.desiredSeparation > 10)
            {
                swarmModel.desiredSeparation = 10;
            }
        }
    }

    void UpdateMigrationPoint()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Transform body = null;
        Vector3 right = new Vector3(0, 0, 0);
        Vector3 forward = new Vector3(0, 0, 0);

        Vector3 final = new Vector3(0, 0, 0);

        if(CameraMovement.embodiedDrone == null)
        {
            body = CameraMovement.cam.transform;
            right = body.right;
            forward = body.up;
        }else{
            body = CameraMovement.embodiedDrone.transform;
            right = body.right;
            forward = body.forward;
        }

        if((horizontal == 0 && vertical == 0))
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
            final = vertical * forward + horizontal * right;
            final.Normalize();

            float newR = Mathf.Sqrt(horizontal * horizontal + vertical * vertical);
            Vector3 finalAlignement = final * newR * radius;

            final = final * radius;


            migrationPoint = new Vector2(centerOfSwarm.x + final.x, centerOfSwarm.z + final.z);
            deltaMigration = new Vector3(finalAlignement.x, 0, finalAlignement.z);
        }

        alignementVector = deltaMigration;

        Debug.DrawRay(body.position, alignementVector, Color.red, 0.01f);
    }
}