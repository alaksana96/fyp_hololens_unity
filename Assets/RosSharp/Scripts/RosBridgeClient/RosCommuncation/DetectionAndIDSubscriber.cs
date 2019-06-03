using RosSharp.RosBridgeClient.Messages.HoloFyp;
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    [RequireComponent(typeof(RosConnector))]
    public class DetectionAndIDSubscriber : Subscriber<Messages.HoloFyp.DetectionAndID>
    {

        public DetectionAndID receivedMessage;

        protected override void Start()
        {
            base.Start();
        }

        protected override void ReceiveMessage(DetectionAndID message)
        {
            receivedMessage = message;
        }
    }
}
