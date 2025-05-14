using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;

public class stove : MonoBehaviour
{
    
    [Header("Stove Settings")]
    public List<Transform> burnerList = new List<Transform>();
    public ParticleSystem steamEffectPrefab;

    void Awake()
    {
        foreach (Transform child in transform)
        {
            if (child.CompareTag("Quemador"))
            {
                burnerList.Add(child);
            }
        }
    }

    public bool AddPanToStove(GameObject pan)
    {
        foreach (Transform burner in burnerList)
        {
            Burner burnerScript = burner.GetComponent<Burner>();
            if (burnerScript != null && !burnerScript.isOccupied)
            {
                Vector3 worldPos = pan.transform.position;
                Quaternion worldRot = pan.transform.rotation;

                pan.transform.SetParent(burner);
                pan.transform.position = worldPos;
                pan.transform.rotation = worldRot;

                Rigidbody rb = pan.GetComponent<Rigidbody>();
                Collider col = pan.GetComponent<Collider>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                    rb.useGravity = false;
                }

                Sequence seq = DOTween.Sequence();
                Vector3 midJump = pan.transform.localPosition + new Vector3(0, 0.2f, 0);

                seq.Append(pan.transform.DOLocalMove(midJump, 0.2f).SetEase(Ease.OutQuad));
                seq.Append(pan.transform.DOLocalMove(Vector3.zero, 0.4f).SetEase(Ease.OutBack));
                seq.Join(pan.transform.DOLocalRotate(Vector3.zero, 0.4f));
                seq.Append(pan.transform.DOLocalMoveY(0.05f, 0.1f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.InOutQuad));

                seq.OnComplete(() =>
                {
                    pan.transform.SetParent(null);
                    if (rb != null)
                    {
                        rb.isKinematic = false;
                        rb.detectCollisions = true;
                        rb.useGravity = true;
                    }
                    if (col != null) col.enabled = true;
                });

                if (steamEffectPrefab != null)
                {
                    seq.AppendCallback(() =>
                    {
                        ParticleSystem steam = Instantiate(steamEffectPrefab, burner.position + Vector3.up * 0.1f, Quaternion.identity);
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
