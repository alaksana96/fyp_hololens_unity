using UnityEngine;

public class GazeCursor : MonoBehaviour {

    private MeshRenderer meshRenderer;

    void Start()
    {
        // Grab the mesh renderer that is on the same object as this script.
        meshRenderer = gameObject.GetComponent<MeshRenderer>();

        LookingForward.Instance.cursor = gameObject;
        gameObject.GetComponent<Renderer>().material.color = Color.blue;
        // If you wish to change the size of the cursor you can do so here
        gameObject.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
    }

    void Update()
    {
        // Do a raycast into the world based on the user's head position and orientation.
        Vector3 headPosition = Camera.main.transform.position;
        Vector3 gazeDirection = Camera.main.transform.forward;

        RaycastHit gazeHitInfo;
        if (Physics.Raycast(headPosition, gazeDirection, out gazeHitInfo, 30.0f, SpatialMapping.PhysicsRaycastMask))
        {
            // If the raycast hit a hologram, display the cursor mesh.
            meshRenderer.enabled = true;
            // Move the cursor to the point where the raycast hit.
            transform.position = gazeHitInfo.point;
            // Rotate the cursor to hug the surface of the hologram.
            transform.rotation = Quaternion.FromToRotation(Vector3.up, gazeHitInfo.normal);
        }
        else
        {
            // If the raycast did not hit a hologram, hide the cursor mesh.
            meshRenderer.enabled = false;
        }
    }
}
