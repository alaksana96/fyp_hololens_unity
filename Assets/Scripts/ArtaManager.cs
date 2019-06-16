using UnityEngine;
using UnityEngine.UI;

using System;
using System.Collections;
using System.Collections.Generic;

using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.Messages.Sensor;
using RosSharp.RosBridgeClient.Messages.HoloFyp;


using Assets.Scripts;
public class ArtaManager : MonoBehaviour {

    private static ArtaManager _instance;

    private OdometrySubscriber odomSubscriber;

    [HideInInspector]
    public TwistSubscriber cmd_velSubscriber;
    [HideInInspector]
    public TwistSubscriber main_js_cmd_velSubscriber;
    [HideInInspector]
    public TwistPublisher holo_joyPublisher;

    public Vector3 linearVelocityJoy;
    public Vector3 angularVelocityJoy;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
         
        odomSubscriber = this.GetComponent<OdometrySubscriber>();

        cmd_velSubscriber = this.GetComponents<TwistSubscriber>()[0];
        main_js_cmd_velSubscriber = this.GetComponents<TwistSubscriber>()[1];
        holo_joyPublisher = this.GetComponent<TwistPublisher>();

    }
}
