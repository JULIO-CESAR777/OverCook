using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class stove : MonoBehaviour
{
    public List<Transform> quemadores = new List<Transform>();
    public ParticleSystem steamEffectPrefab; // asigna esto en el inspector



    void Awake()
    {
        foreach(Transform child in transform){
            if(child.CompareTag("Quemador")){
                quemadores.Add(child);
            }
        }
    }

    public bool addPanToStove(GameObject pan){

        foreach (Transform quemador in quemadores)
        {
            if (quemador.childCount == 0)
            {
                // Guardar posición global antes de hacer parent
                Vector3 worldPos = pan.transform.position;
                Quaternion worldRot = pan.transform.rotation;

                pan.transform.SetParent(quemador);
                pan.transform.position = worldPos;
                pan.transform.rotation = worldRot;

                Rigidbody rb = pan.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                    rb.useGravity = false;
                }

                // Animación: salto y rebote al centro
                Sequence seq = DOTween.Sequence();
                Vector3 midJump = pan.transform.localPosition + new Vector3(0, 0.2f, 0);

                seq.Append(pan.transform.DOLocalMove(midJump, 0.2f).SetEase(Ease.OutQuad)); // brinca
                seq.Append(pan.transform.DOLocalMove(Vector3.zero, 0.4f).SetEase(Ease.OutBack)); // cae al centro
                seq.Join(pan.transform.DOLocalRotate(Vector3.zero, 0.4f)); // rota

                // Bounce sutil
                seq.Append(pan.transform.DOLocalMoveY(0.05f, 0.1f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.InOutQuad));

                // Efecto de partícula
                if (steamEffectPrefab != null)
                {
                    seq.AppendCallback(() =>
                    {
                        ParticleSystem steam = Instantiate(steamEffectPrefab, quemador.position + Vector3.up * 0.1f, Quaternion.identity);
                        steam.Play();
                        Destroy(steam.gameObject, steam.main.duration + 1f);
                    });
                }

                return true;
            }
        }
        Debug.Log("No hay quemadores libres en este horno.");
        return false;
    }


}
