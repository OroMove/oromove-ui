// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

namespace Mediapipe.Unity
{
  public class WebCamSource : ImageSource
  {
    private readonly int _preferableDefaultWidth = 1280;

    private const string _TAG = nameof(WebCamSource);

    private readonly ResolutionStruct[] _defaultAvailableResolutions;

    public WebCamSource(int preferableDefaultWidth, ResolutionStruct[] defaultAvailableResolutions)
    {
      _preferableDefaultWidth = preferableDefaultWidth;
      _defaultAvailableResolutions = defaultAvailableResolutions;
    }

    private static readonly object _PermissionLock = new object();
    private static bool _IsPermitted = false;

    private WebCamTexture _webCamTexture;
    private WebCamTexture webCamTexture
    {
      get => _webCamTexture;
      set
      {
        if (_webCamTexture != null)
        {
          _webCamTexture.Stop();
        }
        _webCamTexture = value;
      }
    }

    public override int textureWidth => !isPrepared ? 0 : webCamTexture.width;
    public override int textureHeight => !isPrepared ? 0 : webCamTexture.height;

    public override bool isVerticallyFlipped => isPrepared && webCamTexture.videoVerticallyMirrored;
    public override bool isFrontFacing => isPrepared && (webCamDevice is WebCamDevice valueOfWebCamDevice) && valueOfWebCamDevice.isFrontFacing;
    public override RotationAngle rotation => !isPrepared ? RotationAngle.Rotation0 : (RotationAngle)webCamTexture.videoRotationAngle;

    private WebCamDevice? _webCamDevice;
    private WebCamDevice? webCamDevice
    {
      get => _webCamDevice;
      set
      {
        if (_webCamDevice is WebCamDevice valueOfWebCamDevice)
        {
          if (value is WebCamDevice valueOfValue && valueOfValue.name == valueOfWebCamDevice.name)
          {
            // not changed
            return;
          }
        }
        else if (value == null)
        {
          // not changed
          return;
        }
        _webCamDevice = value;
        resolution = GetDefaultResolution();
      }
    }
    public override string sourceName => (webCamDevice is WebCamDevice valueOfWebCamDevice) ? valueOfWebCamDevice.name : null;

    private WebCamDevice[] _availableSources;
    private WebCamDevice[] availableSources
    {
      get
      {
        if (_availableSources == null)
        {
          _availableSources = WebCamTexture.devices;
        }

        return _availableSources;
      }
      set => _availableSources = value;
    }

    public override string[] sourceCandidateNames => availableSources?.Select(device => device.name).ToArray();

#pragma warning disable IDE0025
    public override ResolutionStruct[] availableResolutions
    {
      get
      {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        if (webCamDevice is WebCamDevice valueOfWebCamDevice) {
          return valueOfWebCamDevice.availableResolutions.Select(resolution => new ResolutionStruct(resolution)).ToArray();
        }
#endif
        return webCamDevice == null ? null : _defaultAvailableResolutions;
      }
    }
#pragma warning restore IDE0025

    public override bool isPrepared => webCamTexture != null;
    public override bool isPlaying => webCamTexture != null && webCamTexture.isPlaying;

        private IEnumerator Initialize()
        {
            yield return RequestCameraPermission(); // Step 1: Ask for permission

            if (!_IsPermitted)
            {
                Debug.LogWarning("Camera permission denied.");
                yield break; // Exit if permission is not granted
            }

            availableSources = WebCamTexture.devices; // Get available cameras

            if (availableSources != null && availableSources.Length > 0)
            {
                // Step 2: Select the Front Camera by Default
                var frontCamera = availableSources.FirstOrDefault(device => device.isFrontFacing);
                webCamDevice = frontCamera.name != null ? frontCamera : availableSources[0];

            }
        }


        private IEnumerator RequestCameraPermission()
        {
            lock (_PermissionLock)
            {
                if (_IsPermitted) yield break; // Skip if already permitted

#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
            yield return new WaitForSeconds(1.0f); // Wait for user response
        }
#elif UNITY_IOS
        if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        }
#endif

#if UNITY_ANDROID
        _IsPermitted = Permission.HasUserAuthorizedPermission(Permission.Camera);
#elif UNITY_IOS
        _IsPermitted = Application.HasUserAuthorization(UserAuthorization.WebCam);
#endif

                if (!_IsPermitted)
                {
                    Debug.LogWarning("User denied camera access.");
                }
            }
        }


        public override void SelectSource(int sourceId)
    {
      if (sourceId < 0 || sourceId >= availableSources.Length)
      {
        throw new ArgumentException($"Invalid source ID: {sourceId}");
      }

      webCamDevice = availableSources[sourceId];
    }

        public override IEnumerator Play()
        {
            yield return Initialize(); // Ensure permission and camera setup
            if (!_IsPermitted)
            {
                throw new InvalidOperationException("Camera permission not granted.");
            }

            InitializeWebCamTexture(); // Start webcam
            webCamTexture.Play();
            yield return WaitForWebCamTexture();
        }


        public override IEnumerator Resume()
    {
      if (!isPrepared)
      {
        throw new InvalidOperationException("WebCamTexture is not prepared yet");
      }
      if (!webCamTexture.isPlaying)
      {
        webCamTexture.Play();
      }
      yield return WaitForWebCamTexture();
    }

    public override void Pause()
    {
      if (isPlaying)
      {
        webCamTexture.Pause();
      }
    }

    public override void Stop()
    {
      if (webCamTexture != null)
      {
        webCamTexture.Stop();
      }
      webCamTexture = null;
    }

    public override Texture GetCurrentTexture() => webCamTexture;

    private ResolutionStruct GetDefaultResolution()
    {
      var resolutions = availableResolutions;
      return resolutions == null || resolutions.Length == 0 ? new ResolutionStruct() : resolutions.OrderBy(resolution => resolution, new ResolutionStructComparer(_preferableDefaultWidth)).First();
    }

    private void InitializeWebCamTexture()
    {
      Stop();
      if (webCamDevice is WebCamDevice valueOfWebCamDevice)
      {
        webCamTexture = new WebCamTexture(valueOfWebCamDevice.name, resolution.width, resolution.height, (int)resolution.frameRate);
        return;
      }
      throw new InvalidOperationException("Cannot initialize WebCamTexture because WebCamDevice is not selected");
    }

    private IEnumerator WaitForWebCamTexture()
    {
      const int timeoutFrame = 2000;
      var count = 0;
      Debug.Log("Waiting for WebCamTexture to start");
      yield return new WaitUntil(() => count++ > timeoutFrame || webCamTexture.width > 16);

      if (webCamTexture.width <= 16)
      {
        throw new TimeoutException("Failed to start WebCam");
      }
    }

    private class ResolutionStructComparer : IComparer<ResolutionStruct>
    {
      private readonly int _preferableDefaultWidth;

      public ResolutionStructComparer(int preferableDefaultWidth)
      {
        _preferableDefaultWidth = preferableDefaultWidth;
      }

      public int Compare(ResolutionStruct a, ResolutionStruct b)
      {
        var aDiff = Mathf.Abs(a.width - _preferableDefaultWidth);
        var bDiff = Mathf.Abs(b.width - _preferableDefaultWidth);
        if (aDiff != bDiff)
        {
          return aDiff - bDiff;
        }
        if (a.height != b.height)
        {
          // prefer smaller height
          return a.height - b.height;
        }
        // prefer smaller frame rate
        return (int)(a.frameRate - b.frameRate);
      }
    }
  }
}
