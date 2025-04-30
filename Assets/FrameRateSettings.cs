using UnityEngine;

public class FrameRateSettings : MonoBehaviour
{
    void Start()
    {
        // Limit framerate to cinematic 60fps.
        QualitySettings.vSyncCount = 0; // Set vSyncCount to 0 so that using .targetFrameRate is enabled.
        Application.targetFrameRate = 60;
    }
}
