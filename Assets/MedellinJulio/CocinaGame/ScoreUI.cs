using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class ScoreUI : MonoBehaviour
{
    public TextMeshProUGUI currentScoreText;
    public TextMeshProUGUI goalScoreText;

    public GameObject WinCanvas;

   
    private void Start()
    {
        if (GameManager.Instance != null)
        {
            goalScoreText.text =  GameManager.Instance.goalScore.ToString();
        }

        WinCanvas.SetActive(false);

    
    }

    private void Update()
    {
        if (GameManager.Instance != null)
        {
            currentScoreText.text =  GameManager.Instance.score.ToString();

            if (GameManager.Instance.score >= GameManager.Instance.goalScore)
            {
               
                ShowWinCanvas();
            }
        }

        
    }

    private void ShowWinCanvas()
    {
        if (WinCanvas != null)
        {
            WinCanvas.SetActive(true);
            Debug.Log("¡Has alcanzado la meta!");
            // Puedes añadir efectos, animaciones, transiciones de nivel, etc.
        }
    }


}
