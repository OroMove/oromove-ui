using System.Collections;
using Mediapipe.Tasks.Vision.FaceLandmarker;
using UnityEngine;
using UnityEngine.Rendering;

namespace Mediapipe.Unity.Sample.FaceLandmarkDetection
{
    public class FaceLandmarkerRunner : VisionTaskApiRunner<FaceLandmarker>
    {
        [SerializeField] private FaceLandmarkerResultAnnotationController _faceLandmarkerResultAnnotationController;
        [SerializeField] private CarController carController; // Reference to CarController

        private Experimental.TextureFramePool _textureFramePool;
        public readonly FaceLandmarkDetectionConfig config = new FaceLandmarkDetectionConfig();

        public float MouthOpeningDistance { get; private set; }

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

            _textureFramePool = new Experimental.TextureFramePool(imageSource.textureWidth, imageSource.textureHeight, TextureFormat.RGBA32, 10);
            screen.Initialize(imageSource);
            SetupAnnotationController(_faceLandmarkerResultAnnotationController, imageSource);

            var transformationOptions = imageSource.GetTransformationOptions();
            var flipHorizontally = transformationOptions.flipHorizontally;
            var flipVertically = transformationOptions.flipVertically;
            var imageProcessingOptions = new Tasks.Vision.Core.ImageProcessingOptions(rotationDegrees: (int)transformationOptions.rotationAngle);

            AsyncGPUReadbackRequest req = default;
            var waitUntilReqDone = new WaitUntil(() => req.done);
            var result = FaceLandmarkerResult.Alloc(options.numFaces);

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
                    case Tasks.Vision.Core.RunningMode.VIDEO:
                        if (taskApi.TryDetect(image, imageProcessingOptions, ref result))
                        {
                            ProcessFaceLandmarks(result);
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

        private void ProcessFaceLandmarks(FaceLandmarkerResult result)
        {
            if (result.faceLandmarks == null || result.faceLandmarks.Count == 0)
            {
                Debug.Log("No face detected.");
                return;
            }

            var faceLandmarks = result.faceLandmarks[0].landmarks;

            if (faceLandmarks == null || faceLandmarks.Count < 15)
            {
                Debug.Log("Not enough landmarks detected.");
                return;
            }

            var topLip = faceLandmarks[13];
            var bottomLip = faceLandmarks[14];

            float mouthOpeningDistance = Vector2.Distance(
                new Vector2(topLip.x, topLip.y),
                new Vector2(bottomLip.x, bottomLip.y)
            );

            float closedMouthThreshold = 0.01f;

            if (mouthOpeningDistance < closedMouthThreshold)
            {
                mouthOpeningDistance = 0f;
                Debug.Log("Mouth Closed");
            }
            else
            {
                Debug.Log($"Mouth Opening Distance: {mouthOpeningDistance}");
            }

            MouthOpeningDistance = mouthOpeningDistance;

            if (carController != null)
            {
                carController.SetMouthOpeningDistance(mouthOpeningDistance);
            }
        }
    }
}
