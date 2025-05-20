using DG.Tweening;
using UnityEngine;

public class Burner : MonoBehaviour
{
    public bool isOccupied = false;
    public GameObject currentPan;
    public float cookingTime = 5f;
    private Tween cookingTween;

    private void OnTriggerEnter(Collider other)
    {
        if (!isOccupied && other.CompareTag("Pan"))
        {
            currentPan = other.gameObject;
            isOccupied = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        Sarten pan = other.GetComponent<Sarten>();
        if (pan.ingredientOnPan == null) return;

        if (cookingTween == null || !cookingTween.IsActive())
        {
            StartCookingAnimation(currentPan.transform);
        }

        pan.progress = pan.progress + Time.deltaTime;
        if (pan.progress >= cookingTime) {
            StopCookingAnimation();
            pan.CompleteCook(pan);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == currentPan)
        {
            currentPan = null;
            isOccupied = false;
            StopCookingAnimation();
        }
    }

    private void StartCookingAnimation(Transform panTransform)
    {
        StopCookingAnimation(); // Por seguridad

        Debug.Log("Esta cocinando");

        cookingTween = panTransform
            .DOLocalRotate(new Vector3(0f, 0f, 5f), 0.4f)
            .SetRelative()
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void StopCookingAnimation()
    {
        if (cookingTween != null && cookingTween.IsActive())
        {
            cookingTween.Kill();
        }
    }

}
