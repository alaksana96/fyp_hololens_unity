using RosSharp.RosBridgeClient.Messages.HoloFyp;
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    [RequireComponent(typeof(RosConnector))]
    public class DetectionAndDirectionSubscriber : Subscriber<Messages.HoloFyp.DetectionAndDirection>
    {

        public DetectionAndDirection receivedMessage;

        protected override void Start()
        {
            base.Start();
        }

        protected override void ReceiveMessage(DetectionAndDirection message)
        {
            //SceneOrganiser.Instance.cursor.GetComponent<Renderer>().material.color = Color.red;
            receivedMessage = message;
        }
    }
}
