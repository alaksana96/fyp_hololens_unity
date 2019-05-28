using RosSharp.RosBridgeClient.Messages.HoloFyp;
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    [RequireComponent(typeof(RosConnector))]
    public class DetectionAndDirectionSubscriber : Subscriber<Messages.HoloFyp.DetectionAndDirection>
    {
        protected override void Start()
        {
            base.Start();
        }

        protected override void ReceiveMessage(DetectionAndDirection message)
        {
            Debug.Log("Received Something");
        }
    }
}
