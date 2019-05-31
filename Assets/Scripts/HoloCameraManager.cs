using UnityEngine;
using UnityEngine.UI;

using System;
using System.Collections;
using System.Collections.Generic;

using HoloLensCameraStream;

using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.Messages.Sensor;

namespace Assets.Scripts
{
    public class HoloCameraManager : MonoBehaviour
    {
        byte[] _latestImageBytes;
        [HideInInspector]
        public HoloLensCameraStream.Resolution _resolution;

        VideoCapture _videoCapture;

        IntPtr _spatialCoordinateSystemPtr;

        HoloCameraManager Instance;

        public CompressedImagePublisher publisherCameraStream;

        [HideInInspector]
        public Matrix4x4 camera2WorldMatrix;
        [HideInInspector]
        public Matrix4x4 projectionMatrix;

        private bool stopVideo;
        private UnityEngine.XR.WSA.Input.GestureRecognizer _gestureRecognizer;


        private void OnPostRender()
        {
            if (!stopVideo)
            {
                publisherCameraStream.PublishMessage();
            }
        }

        private void Awake()
        {
            Instance = this;

            // Create and set the gesture recognizer
            _gestureRecognizer = new UnityEngine.XR.WSA.Input.GestureRecognizer();
            _gestureRecognizer.TappedEvent += (source, tapCount, headRay) => { Debug.Log("Tapped"); StartCoroutine(StopVideoMode()); };
            _gestureRecognizer.SetRecognizableGestures(UnityEngine.XR.WSA.Input.GestureSettings.Tap);
            _gestureRecognizer.StartCapturingGestures();
        }

        public void Start()
        {
            _spatialCoordinateSystemPtr = UnityEngine.XR.WSA.WorldManager.GetNativeISpatialCoordinateSystemPtr();

            CameraStreamHelper.Instance.GetVideoCaptureAsync(OnVideoCaptureCreated);
        }

        private IEnumerator StopVideoMode()
        {
            yield return new WaitForSeconds(0.65f);
            stopVideo = !stopVideo;

            if (!stopVideo)
                OnVideoCaptureCreated(_videoCapture);
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

            publisherCameraStream.SetResolution(_resolution.width, _resolution.height);
            publisherCameraStream.InitializeMessage();

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

            float[] holoCameraToWorldMatrix;
            float[] holoProjectionMatrix;

            //If you need to get the cameraToWorld matrix for purposes of compositing you can do it like this
            if (sample.TryGetCameraToWorldMatrix(out holoCameraToWorldMatrix) == false)
            {
                return;
            }

            //If you need to get the projection matrix for purposes of compositing you can do it like this
            if (sample.TryGetProjectionMatrix(out holoProjectionMatrix) == false)
            {
                return;
            }

            sample.Dispose();

            camera2WorldMatrix = LocatableCameraUtils.ConvertFloatArrayToMatrix4x4(holoCameraToWorldMatrix);
            projectionMatrix = LocatableCameraUtils.ConvertFloatArrayToMatrix4x4(holoProjectionMatrix);

            //This is where we actually use the image data
            UnityEngine.WSA.Application.InvokeOnAppThread(() =>
            {
                publisherCameraStream.SetBytes(_latestImageBytes);
            }, false);

            if(stopVideo)
            {
                _videoCapture.StopVideoModeAsync(onVideoModeStopped);
            }
        }

        private void onVideoModeStopped(VideoCaptureResult result)
        {
            Debug.Log("Video Mode Stopped");
        }
    }
}
