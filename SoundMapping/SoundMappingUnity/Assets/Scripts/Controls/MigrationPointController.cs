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
    private GameObject selectedDrone = null;

    public Material normalMaterial;
    public Material selectedMaterial;

    void Update()
    {
        UpdateMigrationPoint();
        SelectionUpdate();  
    }

    void SelectionUpdate()
    {        
        if(Input.GetKeyDown("joystick button " + 5) || Input.GetKeyDown("joystick button " + 4)) // Assuming up to 20 buttons (adjust if needed)
        {
            if(selectedDrone == null)
            {
                //select the first child
                if(this.GetComponent<swarmModel>().swarmHolder.transform.childCount > 0)
                {
                    selectedDrone = this.GetComponent<swarmModel>().swarmHolder.transform.GetChild(0).gameObject;
                    selectedDrone.GetComponent<Renderer>().material = selectedMaterial;
                }
            }
            else
            {
                int increment = Input.GetKeyDown("joystick button " + 5) ? 1 : -1;
                //change material
                selectedDrone.GetComponent<Renderer>().material = normalMaterial;
                lastSelectedChild = (lastSelectedChild + increment) % this.GetComponent<swarmModel>().swarmHolder.transform.childCount;
                if(lastSelectedChild < 0)
                {
                    lastSelectedChild = this.GetComponent<swarmModel>().swarmHolder.transform.childCount - 1;
                }

                selectedDrone = this.GetComponent<swarmModel>().swarmHolder.transform.GetChild(lastSelectedChild).gameObject;
                selectedDrone.GetComponent<Renderer>().material = selectedMaterial;
            }
        }

        // bvutton 0
         if(Input.GetKeyDown("joystick button " + 0)) // Assuming up to 20 buttons (adjust if needed)
        {
            if(CameraMovement.embodiedDrone != null)
            {
                if(selectedDrone != CameraMovement.embodiedDrone)
                {
                    CameraMovement.nextEmbodiedDrone = selectedDrone;
                }
                else
                {
                    CameraMovement.embodiedDrone.GetComponent<Camera>().enabled = false;                
                    CameraMovement.embodiedDrone = null;  
                }
            }
            else if(selectedDrone != null)
            {
                CameraMovement.embodiedDrone = selectedDrone;
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

        horizontal = horizontal * Time.deltaTime * 5;
        vertical = vertical * Time.deltaTime * 5;  

        if(horizontal == 0 && vertical == 0)
        {
            migrationPoint = new Vector2(body.position.x, body.position.z);
        }else{
            Vector3 centerOfSwarm = body.position;
            Vector3 final = vertical * forward + horizontal * right;
            final.Normalize();
            final = final * radius;
            migrationPoint = new Vector2(centerOfSwarm.x + final.x, centerOfSwarm.z + final.z);
        }
        
        DroneController.migrationPoint = new Vector3(migrationPoint.x, spawnHeight, migrationPoint.y);
        Debug.DrawRay(DroneController.migrationPoint, Vector3.up*10, Color.green, 0.01f);
    }
    void UpdateMigrationEmbodiementMouse()
    {


        if (Input.GetMouseButtonDown(2))
        {
            if(CameraMovement.embodiedDrone != null)
            {
                CameraMovement.embodiedDrone.GetComponent<Camera>().enabled = false;
                CameraMovement.embodiedDrone = null;
            }
             // Get the mouse position in screen coordinates
            Vector3 mousePosition = Input.mousePosition;
            // Create a ray from the camera through the mouse position
            Ray ray = mainCamera.ScreenPointToRay(mousePosition);
            RaycastHit hit;
            //draws a line from the camera to the mouse position
            Debug.DrawRay(ray.origin, ray.direction * 100, Color.green, 2f);

            // Perform the raycast and check if it hits the ground
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, droneLayer))
            {
              //  point.y = spawnHeight; // Ensure the height matches the drones' height
                print("embodied to : " + hit.collider.gameObject.name);
                CameraMovement.embodiedDrone = hit.collider.gameObject;
            }
        }

    }
}
