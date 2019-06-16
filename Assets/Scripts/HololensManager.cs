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

public class HololensManager : MonoBehaviour {

    public OdometrySubscriber subscriberOdom;

    public static HololensManager Instance;

    internal GameObject cursor;

    private Vector3 initialArtaRotation;
    private float offset;

    [HideInInspector]
    public bool inFrontOfWheelchair;

    private GameObject arta;
    private ArtaManager artaManager;

    private void Awake()
    {
        Instance = this;

        arta = GameObject.FindWithTag("Arta");
        artaManager = arta.GetComponent<ArtaManager>();

        Invoke("CalibrateArtaRotation", 5);
        inFrontOfWheelchair = true;
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

        artaManager.linearVelocityJoy = new Vector3(0, 0, 0);
        artaManager.angularVelocityJoy = new Vector3(0, 0, 0);

        if (artaManager.main_js_cmd_velSubscriber.isMessageReceived)
        {
            artaManager.linearVelocityJoy = artaManager.main_js_cmd_velSubscriber.linearVelocity;
            artaManager.angularVelocityJoy = artaManager.main_js_cmd_velSubscriber.angularVelocity;
            Debug.Log($"{artaManager.linearVelocityJoy} --- {artaManager.angularVelocityJoy}");
        }

        // Look into scene and check if there are any objects detected
        GameObject[] redArrows = GameObject.FindGameObjectsWithTag("Red Arrow");
        GameObject[] greenArrows = GameObject.FindGameObjectsWithTag("Green Arrow");

        inFrontOfWheelchair = LookingForward();

        Vector3 curPos = this.transform.position;

        bool objectInFrontApproaching = false;

        if (inFrontOfWheelchair)
        {
            Debug.Log("################Infront of WHEELCHAIR");

            if (redArrows != null && redArrows.Length > 0)
            {
                // Compare hololens positions with objects
                foreach (GameObject redarrow in redArrows)
                {
                    float distance = Vector3.Distance(curPos, redarrow.transform.position);
                    float angle = Vector3.Angle(curPos, redarrow.transform.position);

                    Debug.Log($"Red Distance: {distance}");


                    if (distance < 3.0f)
                    {
                        Debug.Log($"Distance Collision imminient");

                        if (artaManager.linearVelocityJoy.x > 0)
                        {


                            float x = artaManager.linearVelocityJoy.x / (1 + (float)Math.Pow(Math.E, -2 * distance + 6));

                            artaManager.linearVelocityJoy = new Vector3(x, 0, 0);

                            // Theres an object in front, forget about checking everything else and send new velocity commands
                            objectInFrontApproaching = true;
                            break;
                        }
                        
                    }
                }
            }

            if (greenArrows != null && greenArrows.Length > 0 && !objectInFrontApproaching)
            {
                // Compare hololens positions with objects
                foreach (GameObject greenarrow in greenArrows)
                {
                    float distance = Vector3.Distance(curPos, greenarrow.transform.position);
                    float angle = Vector3.Angle(curPos, greenarrow.transform.position);

                    Debug.Log($"Green Distance: {distance}");

                    if (distance < 3.0f)
                    {
                        if (artaManager.linearVelocityJoy.x > 0)
                        {

                            Debug.Log($"Careful with speed, make sure they arent getting closer");

                            float x = artaManager.linearVelocityJoy.x / (1 + (float)Math.Pow(Math.E, -2 * distance + 3));

                            artaManager.linearVelocityJoy = new Vector3(x, 0, 0);
                        }
                    }
                }
            }

        }
        Debug.Log($"Lin {artaManager.linearVelocityJoy}");
        artaManager.holo_joyPublisher.PublishMessage(artaManager.linearVelocityJoy, artaManager.angularVelocityJoy);
    }


    private Vector3 AlignmentHololensArta()
    {
        arta = GameObject.FindWithTag("Arta");

        Vector3 difference = arta.transform.position - this.transform.position;

        Debug.Log($"ARTA: {difference}");

        return difference;
    }


    private bool LookingForward()
    {
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
        if (upper > 360f)
        {
            //Upper wraparound has occured, check holo on right side
            if (holoRotY < 180)
            {
                holoRotYWrap = holoRotY + 360;
            }
        }

        if (lower < 0f)
        {
            //Lower wraparound has occured
            if (holoRotY > 180)
            {
                holoRotYWrap = holoRotY - 360; // We get a negative value we can compare
            }
        }

        if ((holoRotYWrap <= upper) &&
           (holoRotYWrap >= lower))
        {
            //Debug.Log("Forward");
            cursor.GetComponent<Renderer>().material.color = Color.green;
            return true;
        }
        else
        {
            //Debug.Log("Away");
            cursor.GetComponent<Renderer>().material.color = Color.red;
            return false;
        }
    }
}
