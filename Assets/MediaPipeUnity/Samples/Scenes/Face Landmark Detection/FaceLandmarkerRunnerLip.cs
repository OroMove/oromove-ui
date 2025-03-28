
// Copyright (c) 2023 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections;
using Mediapipe.Tasks.Vision.FaceLandmarker;
using UnityEngine;
using UnityEngine.Rendering;
using System;

namespace Mediapipe.Unity.Sample.FaceLandmarkDetection
{
    public class FaceLandmarkerRunnerLip : VisionTaskApiRunner<FaceLandmarker>
    {
        [SerializeField] private FaceLandmarkerResultAnnotationController _faceLandmarkerResultAnnotationController;

        private Experimental.TextureFramePool _textureFramePool;

        public readonly FaceLandmarkDetectionConfig config = new FaceLandmarkDetectionConfig();

        public override void Stop()
        {
            base.Stop();
            _textureFramePool?.Dispose();
            _textureFramePool = null;
        }

        protected override IEnumerator Run()
        {
            Debug.Log($"Delegate = {config.Delegate}");
            Debug.Log($"Running Mode = {config.RunningMode}");
            Debug.Log($"NumFaces = {config.NumFaces}");
            Debug.Log($"MinFaceDetectionConfidence = {config.MinFaceDetectionConfidence}");
            Debug.Log($"MinFacePresenceConfidence = {config.MinFacePresenceConfidence}");
            Debug.Log($"MinTrackingConfidence = {config.MinTrackingConfidence}");
            Debug.Log($"OutputFaceBlendshapes = {config.OutputFaceBlendshapes}");
            Debug.Log($"OutputFacialTransformationMatrixes = {config.OutputFacialTransformationMatrixes}");

            yield return AssetLoader.PrepareAssetAsync(config.ModelPath);

            var options = config.GetFaceLandmarkerOptions(config.RunningMode == Tasks.Vision.Core.RunningMode.LIVE_STREAM ? OnFaceLandmarkDetectionOutput : null);
            taskApi = FaceLandmarker.CreateFromOptions(options, GpuManager.GpuResources);
            var imageSource = ImageSourceProvider.ImageSource;

            yield return imageSource.Play();

            if (!imageSource.isPrepared)
            {
                Debug.LogError("Failed to start ImageSource, exiting...");
                yield break;
            }

            // Use RGBA32 as the input format.
            // TODO: When using GpuBuffer, MediaPipe assumes that the input format is BGRA, so maybe the following code needs to be fixed.
            _textureFramePool = new Experimental.TextureFramePool(imageSource.textureWidth, imageSource.textureHeight, TextureFormat.RGBA32, 10);

            // NOTE: The screen will be resized later, keeping the aspect ratio.
            screen.Initialize(imageSource);

            SetupAnnotationController(_faceLandmarkerResultAnnotationController, imageSource);

            var transformationOptions = imageSource.GetTransformationOptions();
            var flipHorizontally = transformationOptions.flipHorizontally;
            var flipVertically = transformationOptions.flipVertically;
            var imageProcessingOptions = new Tasks.Vision.Core.ImageProcessingOptions(rotationDegrees: (int)transformationOptions.rotationAngle);

            AsyncGPUReadbackRequest req = default;
            var waitUntilReqDone = new WaitUntil(() => req.done);
            var result = FaceLandmarkerResult.Alloc(options.numFaces);

            // NOTE: we can share the GL context of the render thread with MediaPipe (for now, only on Android)
            var canUseGpuImage = options.baseOptions.delegateCase == Tasks.Core.BaseOptions.Delegate.GPU &&
              SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3 &&
              GpuManager.GpuResources != null;
            using var glContext = canUseGpuImage ? GpuManager.GetGlContext() : null;

            while (true)
            {
                if (isPaused)
                {
                    yield return new WaitWhile(() => isPaused);
                }

                if (!_textureFramePool.TryGetTextureFrame(out var textureFrame))
                {
                    yield return new WaitForEndOfFrame();
                    continue;
                }

                // Build the input Image
                Image image;
                if (canUseGpuImage)
                {
                    yield return new WaitForEndOfFrame();
                    textureFrame.ReadTextureOnGPU(imageSource.GetCurrentTexture(), flipHorizontally, flipVertically);
                    image = textureFrame.BuildGpuImage(glContext);
                }
                else
                {
                    req = textureFrame.ReadTextureAsync(imageSource.GetCurrentTexture(), flipHorizontally, flipVertically);
                    yield return waitUntilReqDone;

                    if (req.hasError)
                    {
                        Debug.LogError($"Failed to read texture from the image source, exiting...");
                        break;
                    }
                    image = textureFrame.BuildCPUImage();
                    textureFrame.Release();
                }

                switch (taskApi.runningMode)
                {
                    case Tasks.Vision.Core.RunningMode.IMAGE:
                        if (taskApi.TryDetect(image, imageProcessingOptions, ref result))
                        {
                            _faceLandmarkerResultAnnotationController.DrawNow(result);
                        }
                        else
                        {
                            _faceLandmarkerResultAnnotationController.DrawNow(default);
                        }
                        break;
                    case Tasks.Vision.Core.RunningMode.VIDEO:
                        if (taskApi.TryDetectForVideo(image, GetCurrentTimestampMillisec(), imageProcessingOptions, ref result))
                        {
                            _faceLandmarkerResultAnnotationController.DrawNow(result);
                        }
                        else
                        {
                            _faceLandmarkerResultAnnotationController.DrawNow(default);
                        }
                        break;
                    case Tasks.Vision.Core.RunningMode.LIVE_STREAM:
                        taskApi.DetectAsync(image, GetCurrentTimestampMillisec(), imageProcessingOptions);
                        break;
                }
            }
        }

        private void OnFaceLandmarkDetectionOutput(FaceLandmarkerResult result, Image image, long timestamp)
        {
            ProcessFaceLandmarks(result);
            _faceLandmarkerResultAnnotationController.DrawLater(result);
        }

        public static event Action<string> OnLipPositionChanged; // Event for lip position

        private void ProcessFaceLandmarks(FaceLandmarkerResult result)
        {
            if (result.faceLandmarks == null || result.faceLandmarks.Count == 0)
            {
                Debug.Log("No face detected.");
                return;
            }

            var faceLandmarks = result.faceLandmarks[0].landmarks;

            if (faceLandmarks == null || faceLandmarks.Count < 170)
            {
                Debug.Log("Not enough landmarks detected.");
                return;
            }

            float fixedReferenceX = faceLandmarks[1].x;
            int[] lipLandmarkIndices = { 0, 13, 14, 17, 78, 191, 80, 81, 82, 311, 308, 402, 317, 324, 291, 61 };

            int leftCount = 0, rightCount = 0;
            foreach (int index in lipLandmarkIndices)
            {
                if (faceLandmarks[index].x < fixedReferenceX)
                {
                    leftCount++;
                }
                else
                {
                    rightCount++;
                }
            }

            int tolerance = 2;
            string lipPosition = "CENTER"; // Default

            if (Math.Abs(leftCount - rightCount) > tolerance)
            {
                if (leftCount > rightCount)
                {
                    lipPosition = "LEFT";
                }
                else
                {
                    lipPosition = "RIGHT";
                }
            }

            Debug.Log($"Lip Position: {lipPosition}");
            OnLipPositionChanged?.Invoke(lipPosition); // Notify Player.cs
        }
    }
}