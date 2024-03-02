using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class fadeButtons : MonoBehaviour, IPointerDownHandler
{
    public float fadeTime = 3f;
    public GameObject pause;


    void Update()
    {
        if (Input.GetMouseButtonDown(0)){ pause.SetActive(true); fadeTime = 3f;}


        if (fadeTime > 0)
        {
          fadeTime -= Time.deltaTime;

        }

        if (fadeTime < 0)
        {
            pause.SetActive(false);

        }

    }
    public void OnPointerDown(PointerEventData eventData)
    {
        pause.SetActive(true); fadeTime = 3f;
    }



}
