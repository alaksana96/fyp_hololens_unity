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

    public GameObject prefabRedArrow;
    public GameObject prefabGreenArrow;
    public GameObject prefabLabelID;

    private void Awake()
    {
        this.transform.position = new Vector3(0f, 0f, 0f); 
    }

    /// <summary>
    /// Creates an arrow hologram for every detection received from people_tracker.py
    /// </summary>
    private void Update()
    {
        if (subscriberDetectionAndDirection.receivedMessage != null) // Check if yact/people_direction node is active
        {
            if (subscriberDetectionAndDirection.receivedMessage.detections.Length > 0)
            {
                foreach (BoundingBoxDirection bbd in subscriberDetectionAndDirection.receivedMessage.detections)
                {
                    float bbHeight = bbd.boundingBox.ymax - bbd.boundingBox.ymin;

                    Vector2 bbCentre = new Vector2((bbd.boundingBox.xmin + bbd.boundingBox.xmax) / 2,
                                                   bbd.boundingBox.ymin + (0.3f * bbHeight));

                    // Get World Coordinates of the detection in the image
                    Vector3 bbCentreWorld = LocatableCameraUtils.PixelCoordToWorldCoord(
                        holoCameraManager.camera2WorldMatrix,
                        holoCameraManager.projectionMatrix,
                        holoCameraManager._resolution,
                        bbCentre
                        );

                    // Create Direction Arrow Hologram for this detection
                    GameObject Arrow;

                    if (bbd.directionTowardsCamera)
                    {
                        Arrow = Instantiate(prefabRedArrow, bbCentreWorld, Quaternion.identity);
                        Arrow.tag = "Red Arrow";
                    }
                    else
                    {
                        Arrow = Instantiate(prefabGreenArrow, bbCentreWorld, Quaternion.identity);
                        Arrow.tag = "Green Arrow";
                    }

                    Vector3 headPosition = Camera.main.transform.position;
                    RaycastHit objHitInfo;
                    Vector3 objDirection = Arrow.transform.position;

                    Vector3 gazeDirection = Camera.main.transform.forward;

                    if (Physics.Raycast(headPosition, objDirection, out objHitInfo, 30.0f, SpatialMapping.PhysicsRaycastMask))
                    {
                        Arrow.transform.position = objHitInfo.point;
                        Arrow.transform.rotation = Quaternion.LookRotation(gazeDirection);
                    }
                }
            }
        }

        if (subscriberDetectionAndID.receivedMessage != null) // Check if yact/people_tracker node is active
        {
            if (subscriberDetectionAndID.receivedMessage.detections.Length > 0)
            {
                foreach (BoundingBoxID bbid in subscriberDetectionAndID.receivedMessage.detections)
                {
                    float bbHeight = bbid.boundingBox.ymax - bbid.boundingBox.ymin;

                    Vector2 bbCentre = new Vector2((bbid.boundingBox.xmin + bbid.boundingBox.xmax) / 2,
                                                   bbid.boundingBox.ymin + (0.4f * bbHeight));

                    // Get World Coordinates of the detection in the image
                    Vector3 bbCentreWorld = LocatableCameraUtils.PixelCoordToWorldCoord(
                        holoCameraManager.camera2WorldMatrix,
                        holoCameraManager.projectionMatrix,
                        holoCameraManager._resolution,
                        bbCentre
                        );

                    // Create Direction Label Hologram for this detection
                    GameObject LabelID = Instantiate(prefabLabelID, bbCentreWorld, Quaternion.identity);
                    LabelID.tag = "ID";

                    LabelID.GetComponent<TextMesh>().text = bbid.id.ToString();


                    Vector3 headPosition = Camera.main.transform.position;
                    RaycastHit objHitInfo;
                    Vector3 objDirection = LabelID.transform.position;

                    Vector3 gazeDirection = Camera.main.transform.forward;

                    if (Physics.Raycast(headPosition, objDirection, out objHitInfo, 30.0f, SpatialMapping.PhysicsRaycastMask))
                    {
                        LabelID.transform.position = objHitInfo.point;
                        LabelID.transform.rotation = Quaternion.LookRotation(gazeDirection);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Destroys the arrow game objects instantiated for each detection
    /// NOTE: Make sure the tag has been added to the Unity project!
    ///       This prevents the Tag not recognized error (Debug if not working)
    /// </summary>
    private void DestroyGameObjects(string tag, float delay)
    {
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag(tag);

        //Debug.Log($"{tag} count: {gameObjects.Length}");

        foreach (GameObject target in gameObjects)
        {
            GameObject.Destroy(target, delay);
        }
    }

    private void OnPostRender()
    {
        DestroyGameObjects("Green Arrow", 0.2f);
        DestroyGameObjects("Red Arrow", 0.2f);
        DestroyGameObjects("ID", 0.1f);
    }
}
