using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Management;

public class VrDetector : MonoBehaviour
{

    [SerializeField] public GameObject Canvas;
    [SerializeField] public GameObject Camera;

    [SerializeField] public GameObject CanvasVR;
    [SerializeField] public GameObject Player;

    public bool flag;


    void Start()
    {
        var xrManager = XRGeneralSettings.Instance.Manager;

        if (xrManager.isInitializationComplete && xrManager.activeLoader != null)
        {
            // Lógica VR
            Canvas.SetActive(false);
            Camera.SetActive(false);

            CanvasVR.SetActive(true);
            Player.SetActive(true);

            flag = false;


        }
        else
        {
            // Lógica no-VR
            Canvas.SetActive(true);
            Camera.SetActive(true);

            CanvasVR.SetActive(false);
            Player.SetActive(false);

            flag = true;

        }
    }

    public void StartGame(){
        if (flag)
        {
            SceneManager.LoadScene("Cooking");
        }
        else {
            SceneManager.LoadScene("CookingVR");
        }
    }

    public void QuitGame(){
        Application.Quit();
    }


}
