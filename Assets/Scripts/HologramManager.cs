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

    public static HologramManager Instance;

    private GameObject mySphere;

    private void Awake()
    {
        Instance = this;

        mySphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        mySphere.transform.position = new Vector3(0, 0, 1);
        mySphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
    }


    private void Update()
    {
        if(subscriberDetectionAndDirection.receivedMessage.detections.Length > 0)
        {
            Debug.Log("Message Received");

            BoundingBoxDirection bbd = subscriberDetectionAndDirection.receivedMessage.detections[0];

            Vector2 bbCentre = new Vector2((bbd.boundingBox.xmin + bbd.boundingBox.xmax) / 2,
                                           (bbd.boundingBox.ymin + bbd.boundingBox.ymax) / 2);

            Debug.Log($"centreX: {bbCentre.x} and centreY: {bbCentre.y}");

            Vector3 bbCentreWorld = LocatableCameraUtils.PixelCoordToWorldCoord(
                holoCameraManager.camera2WorldMatrix,
                holoCameraManager.projectionMatrix,
                holoCameraManager._resolution,
                bbCentre
                );

            Debug.Log($"X: {bbCentreWorld.x} Y: {bbCentreWorld.y} Z: {bbCentreWorld.z}");

            mySphere.transform.position = bbCentreWorld;

            Vector3 headPosition = Camera.main.transform.position;
            RaycastHit objHitInfo;
            Vector3 objDirection = mySphere.transform.position;
            if(Physics.Raycast(headPosition, objDirection, out objHitInfo, 30.0f, SpatialMapping.PhysicsRaycastMask))
            {
                mySphere.transform.position = objHitInfo.point;
            }
        }


    }



}
