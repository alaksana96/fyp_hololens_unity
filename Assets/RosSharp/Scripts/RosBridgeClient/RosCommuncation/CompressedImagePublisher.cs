using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace RosSharp.RosBridgeClient
{
    public class CompressedImagePublisher : Publisher<Messages.Sensor.CompressedImage>
    {
        public string FrameId = "Camera";
        [Range(0, 100)]
        public int qualityLevel = 1;

        private Messages.Sensor.CompressedImage message;
        private Texture2D texture2D;

        public void SetResolution(int width, int height)
        {
            texture2D = new Texture2D(width, height, TextureFormat.BGRA32, false);
        }

        public void SetBytes(byte[] image)
        {
            texture2D.LoadRawTextureData(image); //TODO: Should be able to do this: texture.LoadRawTextureData(pointerToImage, 1280 * 720 * 4);
        }

        public void PublishMessage()
        {
            message.header.Update();
            message.data = ImageConversion.EncodeToJPG(texture2D, qualityLevel);
            Publish(message);
        }

        public void InitializeMessage()
        {
            message = new Messages.Sensor.CompressedImage();
            message.header.frame_id = FrameId;
            message.format = "jpeg";
        }
    }
}
