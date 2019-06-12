using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using System;
using System.Collections;
using System.Collections.Generic;

using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.Messages.Sensor;
using RosSharp.RosBridgeClient.Messages.HoloFyp;

using Assets.Scripts;

public class LookingForward : MonoBehaviour {

    public OdometrySubscriber subscriberOdom;

    public static LookingForward Instance;

    internal GameObject cursor;

    private Vector3 initialArtaRotation;
    private float offset;

    private void Awake()
    {
        Instance = this;
        Invoke("CalibrateArtaRotation", 5);
    }


    public void CalibrateArtaRotation()
    {
        initialArtaRotation = subscriberOdom.PublishedTransform.rotation.eulerAngles;

        if(initialArtaRotation.y >= 180)
        {
            offset = 360 - initialArtaRotation.y;
        }
        else
        {
            offset = -initialArtaRotation.y;
        }
    }

    // Update is called once per frame
    void Update () {
        // Compare Hololens rotations with Arta rotations

        Vector3 artaRotation = subscriberOdom.PublishedTransform.rotation.eulerAngles;
        Vector3 holoRotation = Camera.main.transform.rotation.eulerAngles;

        // Tolerance of hololens looking left and right
        float tolerance = 25f;

        float artaRotY = artaRotation.y + offset;
        float holoRotY = holoRotation.y;

        float upper = artaRotY + tolerance;
        float lower = artaRotY - tolerance;

        float holoRotYWrap = holoRotY;

        //Debug.Log($"artaStart {initialArtaRotation} --- arta {artaRotY} --- holo {holoRotY}");

        // Account for wrap around
        if(upper > 360f)
        {
            //Upper wraparound has occured, check holo on right side
            if(holoRotY < 180)
            {
                holoRotYWrap = holoRotY + 360;
            }
        }

        if(lower < 0f)
        {
            //Lower wraparound has occured
            if(holoRotY > 180)
            {
                holoRotYWrap = holoRotY - 360; // We get a negative value we can compare
            }
        }
        
        if((holoRotYWrap <= upper) && 
           (holoRotYWrap >= lower))
        {
            //Debug.Log("Forward");
            cursor.GetComponent<Renderer>().material.color = Color.green;
        }
        else
        {
            //Debug.Log("Away");
            cursor.GetComponent<Renderer>().material.color = Color.red;
        }
    }
}
