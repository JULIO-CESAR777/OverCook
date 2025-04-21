using UnityEngine;

public class TakeIngredient : MonoBehaviour
{
    public void BeingGrab(GameObject handPosition){
        gameObject.transform.position = handPosition.transform.position;
        gameObject.transform.SetParent(handPosition.transform);
    }
}
