using UnityEngine;
using UnityEngine.UI;

using System;
using System.Collections;
using System.Collections.Generic;

using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.Messages.Sensor;
using RosSharp.RosBridgeClient.Messages.HoloFyp;

using Assets.Scripts;

public class HologramManager : MonoBehaviour {

    public DetectionAndDirectionSubscriber subscriberDetectionAndDirection;
    public DetectionAndIDSubscriber subscriberDetectionAndID;

    public HoloCameraManager holoCameraManager;

    public GameObject prefabRedArrowID;
    public GameObject prefabGreenArrowID;

    private Vector3 startPosition;
    private Vector3 currPosition;

    private Quaternion startRotation;
    private Quaternion currRotation;

    
    public struct BoundingBoxDirectionID
    {
        public BoundingBox boundingBox;
        public int id;
        public bool directionTowardsCamera;

        public BoundingBoxDirectionID(BoundingBox bb, int i, bool dir)
        {
            boundingBox = bb;
            id = i;
            directionTowardsCamera = dir;
        }
    }

    private void Awake()
    {
        this.transform.position = new Vector3(0f, 0f, 0f);
    }

    /// <summary>
    /// Creates an arrow hologram for every detection received from people_tracker.py
    /// </summary>
    /// 
    private void Update()
    {
        List<BoundingBoxDirection> bbDirections = new List<BoundingBoxDirection>();
        List<BoundingBoxID> bbIds = new List<BoundingBoxID>();

        List<BoundingBoxDirectionID> bbDirIds = new List<BoundingBoxDirectionID>(); // For matched detection-ID pairs

        if (subscriberDetectionAndDirection.receivedMessage != null)
        {
            bbDirections.AddRange(subscriberDetectionAndDirection.receivedMessage.detections);
        }

        if (subscriberDetectionAndID.receivedMessage != null)
        {
            bbIds.AddRange(subscriberDetectionAndID.receivedMessage.detections);
        }

        //Debug.Log($"--before match bbd: {bbDirections.Count}, bbid:{bbIds.Count}, bbdirid:{bbDirIds.Count}");

        // Need to match IDs and Directions if possible
        for (int i = bbDirections.Count - 1; i >= 0; i--)
        {
            for (int j = bbIds.Count - 1; j >= 0; j--)
            {
                float iou = IntersectionOverUnion(bbDirections[i].boundingBox, bbIds[j].boundingBox);
                if(iou > 0.8f)
                {
                    // Add to matched BB ID-Direction list
                    bbDirIds.Add(new BoundingBoxDirectionID(bbIds[j].boundingBox, bbIds[j].id, bbDirections[i].directionTowardsCamera));
                    
                    // Bounding box matched, remove BBID from list
                    bbIds.RemoveAt(j);

                    break;
                }
            }
            // Bounding box matched, remove BBDir from list
            bbDirections.RemoveAt(i);
        }

        //Debug.Log($"after match bbd: {bbDirections.Count}, bbid:{bbIds.Count}, bbdirid:{bbDirIds.Count}");

        foreach (BoundingBoxDirectionID bbdid in bbDirIds)
        {
            float bbHeight = bbdid.boundingBox.ymax - bbdid.boundingBox.ymin;

            Vector2 bbCentre = new Vector2((bbdid.boundingBox.xmin + bbdid.boundingBox.xmax) / 2,
                                           bbdid.boundingBox.ymin + (0.32f * bbHeight));

            // Get World Coordinates of the detection in the image
            Vector3 bbCentreWorld = LocatableCameraUtils.PixelCoordToWorldCoord(
                holoCameraManager.camera2WorldMatrix,
                holoCameraManager.projectionMatrix,
                holoCameraManager._resolution,
                bbCentre
                );

            GameObject ArrowID;

            if (bbdid.directionTowardsCamera)
            {
                ArrowID = Instantiate(prefabRedArrowID, bbCentreWorld, Quaternion.identity);
                ArrowID.tag = "Red Arrow";
            }
            else
            {
                ArrowID = Instantiate(prefabGreenArrowID, bbCentreWorld, Quaternion.identity);
                ArrowID.tag = "Green Arrow";
            }

            // Set ID of Detection Direction
            var tranformArrowID = ArrowID.transform;
            var textArrowID = tranformArrowID.GetChild(1);
            textArrowID.GetComponent<TextMesh>().text = bbdid.id.ToString();

            Vector3 headPosition = Camera.main.transform.position;
            RaycastHit objHitInfo;
            Vector3 objDirection = ArrowID.transform.position;

            Vector3 gazeDirection = Camera.main.transform.forward;

            if (Physics.Raycast(headPosition, objDirection, out objHitInfo, 30.0f, SpatialMapping.PhysicsRaycastMask))
            {
                ArrowID.transform.position = objHitInfo.point;
                ArrowID.transform.rotation = Quaternion.LookRotation(gazeDirection);
            }
        }
    }

    private float IntersectionOverUnion(BoundingBox a, BoundingBox b)
    {
        int xmin = Math.Max(a.xmin, b.xmin);
        int ymin = Math.Max(a.ymin, b.ymin);
        int xmax = Math.Min(a.xmax, b.xmax);
        int ymax = Math.Min(a.ymax, b.ymax);

        int interArea = Math.Max(0, xmax - xmin + 1) * Math.Max(0, ymax - ymin + 1);

        int aArea = (a.xmax - a.xmin + 1) * (a.ymax - a.ymin + 1);
        int bArea = (b.xmax - b.xmin + 1) * (b.ymax - b.ymin + 1);

        float iou = interArea / (float)(aArea + bArea - interArea);

        return iou;
    }

    /// <summary>
    /// Destroys the arrow game objects instantiated for each detection
    /// NOTE: Make sure the tag has been added to the Unity project!
    ///       This prevents the Tag not recognized error (Debug if not working)
    /// </summary>
    private void DestroyGameObjects(string tag, float delay)
    {
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag(tag);

        foreach (GameObject target in gameObjects)
        {
            GameObject.Destroy(target, delay);
        }
    }

    private void OnPostRender()
    {
        // From testing, 0.1 second delay before destroying holograms allows it to persist through lag.
        DestroyGameObjects("Green Arrow", 0.25f);
        DestroyGameObjects("Red Arrow", 0.25f);
    }
}
