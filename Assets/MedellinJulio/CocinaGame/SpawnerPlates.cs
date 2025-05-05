using Unity.XR.CoreUtils;
using UnityEngine;

public class SpawnerPlates : MonoBehaviour
{
    //[SerializeField] private GameObject ingredient;
    public GameObject plate;
   
    public Transform spawnPoint;

 
    public void SpawnPlate()
    {
        
        GameObject obj = Instantiate(plate, spawnPoint.position, spawnPoint.rotation);
      
             
            
        
    }

}
