using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseGame : MonoBehaviour
{
    [Header("Pause Input")]
    public InputActionReference pauseInteraction;

    [Header("UI")]
    public GameObject pauseMenuUI;

    [Header("Player Control")]
    public PlayerMovement playerMovementScript;

    private bool isPaused = false;

    private void OnEnable()
    {
        pauseInteraction.action.performed += OnPausePressed;
        pauseInteraction.action.Enable();
    }

    private void OnDisable()
    {
        pauseInteraction.action.performed -= OnPausePressed;
        pauseInteraction.action.Disable();
    }

    private void OnPausePressed(InputAction.CallbackContext context)
    {
        TogglePause();
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            // Pausar juego
            Time.timeScale = 0f;

            // Mostrar UI
            if (pauseMenuUI != null)
                pauseMenuUI.SetActive(true);

            // Mostrar cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Desactivar movimiento de cámara
            if (playerMovementScript != null)
                playerMovementScript.enabled = false;
        }
        else
        {
            // Reanudar juego
            Time.timeScale = 1f;

            // Quitar UI
            if (pauseMenuUI != null)
                pauseMenuUI.SetActive(false);

            // Ocultar cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Reactivar movimiento de cámara
            if (playerMovementScript != null)
                playerMovementScript.enabled = true;
        }
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }



}
