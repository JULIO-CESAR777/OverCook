using UnityEngine;

public class Estufa : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        Sarten sarten = other.GetComponent<Sarten>();
        if (sarten != null)
        {
            sarten.onStove = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Sarten sarten = other.GetComponent<Sarten>();
        if (sarten != null)
        {
            sarten.onStove = false;
        }
    }

}
