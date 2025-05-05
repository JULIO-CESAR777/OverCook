using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Management;

public class VrDetector : MonoBehaviour
{

    [SerializeField] public GameObject Canvas;
    [SerializeField] public GameObject Camera;

    [SerializeField] public GameObject CanvasVR;
    [SerializeField] public GameObject Player;


    void Start()
    {
        var xrManager = XRGeneralSettings.Instance.Manager;

        if (xrManager.isInitializationComplete && xrManager.activeLoader != null)
        {
            Debug.Log("XR is initialized and active.");
            // Lógica VR
            Canvas.SetActive(false);
            Camera.SetActive(false);

            CanvasVR.SetActive(true);
            Player.SetActive(true);
        }
        else
        {
            Debug.Log("XR is not active.");
            // Lógica no-VR
            Canvas.SetActive(true);
            Camera.SetActive(true);

            CanvasVR.SetActive(false);
            Player.SetActive(false);
        }
    }

    public void StartGame(){
        SceneManager.LoadScene("Cooking");
    }

    public void QuitGame(){
        Application.Quit();
    }


}
