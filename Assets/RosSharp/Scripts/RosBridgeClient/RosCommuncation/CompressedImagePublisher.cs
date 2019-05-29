using UnityEngine;
using UnityEngine.UI;

namespace RosSharp.RosBridgeClient
{
    public class CompressedImagePublisher : Publisher<Messages.Sensor.CompressedImage>
    {
        public RawImage rawImage;

        public string FrameId = "Camera";
        [Range(0, 100)]
        public int qualityLevel = 50;

        private Messages.Sensor.CompressedImage message;

        public void SetResolution(int width, int height)
        {
            var texture = new Texture2D(width, height, TextureFormat.BGRA32, false);
            rawImage.texture = texture;
        }

        public void SetBytes(byte[] image)
        {
            var texture = rawImage.texture as Texture2D;
            texture.LoadRawTextureData(image); //TODO: Should be able to do this: texture.LoadRawTextureData(pointerToImage, 1280 * 720 * 4);
            texture.Apply();

            message.header.Update();
            message.data = texture.EncodeToJPG(qualityLevel);
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
