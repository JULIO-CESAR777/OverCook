using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Management;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class VrDetector : MonoBehaviour
{
    [SerializeField] public GameObject Canvas;
    [SerializeField] public GameObject Camera;
    [SerializeField] public GameObject EventSystem;

    [SerializeField] public GameObject CanvasVR;
    [SerializeField] public GameObject Player;

    public bool flag;

    void Start()
    {
        var xrManager = XRGeneralSettings.Instance.Manager;

        var standaloneInput = EventSystem.GetComponent<StandaloneInputModule>();
        var inputSystemModule = EventSystem.GetComponent<InputSystemUIInputModule>();

        if (xrManager.isInitializationComplete && xrManager.activeLoader != null)
        {
            // Modo VR
            Canvas.SetActive(false);
            Camera.SetActive(false);

            CanvasVR.SetActive(true);
            Player.SetActive(true);

            flag = false;
        }
        else
        {
            // Modo PC
            Canvas.SetActive(true);
            Camera.SetActive(true);

            CanvasVR.SetActive(false);
            Player.SetActive(false);

            flag = true;
        }

        Debug.Log("Modo actual: " + (flag ? "PC" : "VR"));
    }

    public void StartGame()
    {
        if (flag)
        {
            SceneManager.LoadScene("Cooking");
        }
        else
        {
            SceneManager.LoadScene("CookingVR2");
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
