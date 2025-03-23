using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoPlayerController : MonoBehaviour
{
    public GameObject videoPanel;  // Panel to display video
    public VideoPlayer videoPlayer; // Video Player component
    public Button quitButton; // Quit button

    private void Start()
    {
        videoPanel.SetActive(false); // Hide panel initially
        quitButton.onClick.AddListener(StopVideo);
    }

    public void PlayVideo(VideoClip clip)
    {
        videoPanel.SetActive(true); // Show video panel
        videoPlayer.clip = clip;
        videoPlayer.isLooping = true; // Loop the video
        videoPlayer.Play();
    }

    public void StopVideo()
    {
        videoPlayer.Stop();
        videoPanel.SetActive(false); // Hide panel
    }
}
