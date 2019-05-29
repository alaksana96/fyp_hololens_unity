using UnityEngine;
using UnityEngine.UI;

using System;

using HoloLensCameraStream;

using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.Messages.Sensor;

namespace Assets.Scripts
{
    public class HololensManager : MonoBehaviour
    {
        byte[] _latestImageBytes;
        HoloLensCameraStream.Resolution _resolution;

        VideoCapture _videoCapture;

        IntPtr _spatialCoordinateSystemPtr;

        public CompressedImagePublisher publisherCameraStream;
        public DetectionAndDirectionSubscriber subscriberDetectionAndDirection;

        public void Start()
        {
            _spatialCoordinateSystemPtr = UnityEngine.XR.WSA.WorldManager.GetNativeISpatialCoordinateSystemPtr();

            CameraStreamHelper.Instance.GetVideoCaptureAsync(OnVideoCaptureCreated);
        }

        private void OnDestroy()
        {
            if (_videoCapture != null)
            {
                _videoCapture.FrameSampleAcquired -= OnFrameSampleAcquired;
                _videoCapture.Dispose();
            }
        }

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

            UnityEngine.WSA.Application.InvokeOnAppThread(() => {
                publisherCameraStream.SetResolution(_resolution.width, _resolution.height);
                publisherCameraStream.InitializeMessage();
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
                publisherCameraStream.SetBytes(_latestImageBytes);
            }, false);
        }
    }
}
