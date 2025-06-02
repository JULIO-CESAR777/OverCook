using UnityEngine;

public class ReturnKnife : MonoBehaviour
{
    [SerializeField] private GameObject Knife;
    [SerializeField] Transform knifeSpawn;

    public void SpawnKnife() {
        Knife.transform.position = knifeSpawn.position;
    }

}
