using UnityEngine;
using UnityEngine.UI;

public class IconPanel: MonoBehaviour
{
    public Image recipeImage;

    public void SetRecipeIcon(Sprite icon)
    {
        recipeImage.sprite = icon;
        recipeImage.enabled = icon != null;
    }
}
