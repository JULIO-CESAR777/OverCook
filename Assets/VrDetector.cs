using UnityEngine;
using UnityEngine.XR.Management;

public class VrDetector : MonoBehaviour
{
    void Start()
    {
        var xrManager = XRGeneralSettings.Instance.Manager;

        if (xrManager.isInitializationComplete && xrManager.activeLoader != null)
        {
            Debug.Log("XR is initialized and active.");
            // Lógica VR
        }
        else
        {
            Debug.Log("XR is not active.");
            // Lógica no-VR
        }
    }
}
