using UnityEngine;
using UnityEngine.XR.WSA;

public class SpatialMapping : MonoBehaviour {

    public static SpatialMapping Instance;

    internal static int PhysicsRaycastMask;

    internal int physicsLayer = 31;

    private SpatialMappingCollider spatialMappingCollider;

    private void Awake()
    {
        // Allows this instance to behave like a singleton
        Instance = this;
    }

    void Start()
    {
        // Initialize and configure the collider
        spatialMappingCollider = gameObject.GetComponent<SpatialMappingCollider>();
        spatialMappingCollider.surfaceParent = this.gameObject;
        spatialMappingCollider.freezeUpdates = false;
        spatialMappingCollider.layer = physicsLayer;

        // define the mask
        PhysicsRaycastMask = 1 << physicsLayer;

        // set the object as active one
        gameObject.SetActive(true);
    }
}
