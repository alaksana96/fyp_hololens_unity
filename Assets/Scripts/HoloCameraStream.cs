using UnityEngine;
using UnityEngine.UI;

using System;

using HoloLensCameraStream;

namespace RosSharp.RosBridgeClient
{
    class HoloCameraStream : Publisher<Messages.Sensor.CompressedImage>
    {
        byte[] _latestImageBytes;
        HoloLensCameraStream.Resolution _resolution;

        VideoCapture _videoCapture;

        IntPtr _spatialCoordinateSystemPtr;

        public RawImage rawImage;

        public string FrameId = "Camera";
        [Range(0, 100)]
        public int qualityLevel = 50;
        private Messages.Sensor.CompressedImage message;


        override protected void Start()
        {
            _spatialCoordinateSystemPtr = UnityEngine.XR.WSA.WorldManager.GetNativeISpatialCoordinateSystemPtr();

            CameraStreamHelper.Instance.GetVideoCaptureAsync(OnVideoCaptureCreated);

            InitializeMessage();
        }

        private void OnDestroy()
        {
            if (_videoCapture != null)
            {
                _videoCapture.FrameSampleAcquired -= OnFrameSampleAcquired;
                _videoCapture.Dispose();
            }
        }

        #region Video
        void OnVideoCaptureCreated(VideoCapture videoCapture)
        {
            if (videoCapture == null)
            {
                Debug.LogError("Did not find a video capture object. You may not be using the HoloLens.");
                return;
            }

            this._videoCapture = videoCapture;

            //Request the spatial coordinate ptr if you want fetch the camera and set it if you need to 
            CameraStreamHelper.Instance.SetNativeISpatialCoordinateSystemPtr(_spatialCoordinateSystemPtr);

            _resolution = CameraStreamHelper.Instance.GetLowestResolution();
            float frameRate = CameraStreamHelper.Instance.GetHighestFrameRate(_resolution);
            videoCapture.FrameSampleAcquired += OnFrameSampleAcquired;

            //You don't need to set all of these params.
            //I'm just adding them to show you that they exist.
            CameraParameters cameraParams = new CameraParameters();
            cameraParams.cameraResolutionHeight = _resolution.height;
            cameraParams.cameraResolutionWidth = _resolution.width;
            cameraParams.frameRate = Mathf.RoundToInt(frameRate);
            cameraParams.pixelFormat = CapturePixelFormat.BGRA32;
            cameraParams.rotateImage180Degrees = true; //If your image is upside down, remove this line.
            cameraParams.enableHolograms = false;

            UnityEngine.WSA.Application.InvokeOnAppThread(() =>
            {
                var texture = new Texture2D(_resolution.width, _resolution.height, TextureFormat.BGRA32, false);
                rawImage.texture = texture;
            }, false);

            videoCapture.StartVideoModeAsync(cameraParams, OnVideoModeStarted);
        }

        void OnVideoModeStarted(VideoCaptureResult result)
        {
            if (result.success == false)
            {
                Debug.LogWarning("Could not start video mode.");
                return;
            }

            Debug.Log("Video capture started.");
        }

        void OnFrameSampleAcquired(VideoCaptureSample sample)
        {
            //When copying the bytes out of the buffer, you must supply a byte[] that is appropriately sized.
            //You can reuse this byte[] until you need to resize it (for whatever reason).
            if (_latestImageBytes == null || _latestImageBytes.Length < sample.dataLength)
            {
                _latestImageBytes = new byte[sample.dataLength];
            }
            sample.CopyRawImageDataIntoBuffer(_latestImageBytes);

            //If you need to get the cameraToWorld matrix for purposes of compositing you can do it like this
            float[] cameraToWorldMatrix;
            if (sample.TryGetCameraToWorldMatrix(out cameraToWorldMatrix) == false)
            {
                return;
            }

            //If you need to get the projection matrix for purposes of compositing you can do it like this
            float[] projectionMatrix;
            if (sample.TryGetProjectionMatrix(out projectionMatrix) == false)
            {
                return;
            }

            sample.Dispose();

            //This is where we actually use the image data
            UnityEngine.WSA.Application.InvokeOnAppThread(() =>
            {
                var texture = rawImage.texture as Texture2D;
                texture.LoadRawTextureData(_latestImageBytes);
                texture.Apply();

                message.header.Update();
                message.data = texture.EncodeToJPG(qualityLevel);
                Publish(message);

            }, false);
        }
        #endregion

        #region Messaging
        private void InitializeMessage()
        {
            message = new Messages.Sensor.CompressedImage();
            message.header.frame_id = FrameId;
            message.format = "jpeg";
        }
        #endregion
    }
}
