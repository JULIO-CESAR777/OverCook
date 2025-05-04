using UnityEngine;
using System.Collections.Generic;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Recetas disponibles en el juego")]
    public List<RecipeSO> allRecipes;
    public int score = 0;

   

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    public List<RecipeSO> GetAllRecipes()
    {
        return allRecipes;
    }


    //JULIOOOOOO

    public void AddPoints(int points)
    {
        score += points;
        Debug.Log($"Puntos actuales: {score}");
    }
}
