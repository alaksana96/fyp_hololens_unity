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

    public HoloCameraManager holoCameraManager;

    public GameObject prefabRedArrow;
    public GameObject prefabGreenArrow;

    private void Awake()
    {
        this.transform.position = new Vector3(0f, 0f, 0f); 
    }

    private void DestroyGameObjects(string tag)
    {
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag(tag);
        Debug.Log($"{tag} count: {gameObjects.Length}");
        foreach(GameObject target in gameObjects)
        {
            GameObject.Destroy(target, 0.15f);
        }
    }

    private void OnPostRender()
    {
        DestroyGameObjects("Green Arrow");
        DestroyGameObjects("Red Arrow");
    }

    private void Update()
    {
        if(subscriberDetectionAndDirection.receivedMessage.detections.Length > 0)
        {
            BoundingBoxDirection bbd = subscriberDetectionAndDirection.receivedMessage.detections[0];

            float bbHeight = bbd.boundingBox.ymax - bbd.boundingBox.ymin;

            Vector2 bbCentre = new Vector2((bbd.boundingBox.xmin + bbd.boundingBox.xmax) / 2,
                                           bbd.boundingBox.ymin + (0.25f * bbHeight));

            Vector3 bbCentreWorld = LocatableCameraUtils.PixelCoordToWorldCoord(
                holoCameraManager.camera2WorldMatrix,
                holoCameraManager.projectionMatrix,
                holoCameraManager._resolution,
                bbCentre
                );

            GameObject Arrow;

            if(bbd.directionTowardsCamera)
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
