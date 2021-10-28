using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocativeLabel : MonoBehaviour
{


    [Header("Hotspot data")]
    public string SpotTitle="The perfect Spot";
    public string SpotSubTitle= "In a Bar Under the See";

    [Header("Label Rotation")]
    public bool FaceCamera=true;

    [Header("Visibility")]
    [Range(0.0f, 100.0f)]
    public float MaximumDistance=40;
    [Range(0.0f, 100.0f)]
    public float MinimumDistance=10;
    [Range(0.0f, 5.0f)]
    public float fade = 2;

    private GameObject UI_Canvas;

    void Start()
    {
        UI_Canvas = transform.Find("UI_Canvas").gameObject;
        UI_Canvas.transform.Find("SpotTitle").gameObject.GetComponent<Text>().text = SpotTitle;
        UI_Canvas.transform.Find("SpotSubTitle").gameObject.GetComponent<Text>().text = SpotSubTitle;

        StartCoroutine("UpdateLocativeLabel");
    }

    
    private void Update()
    {
        // 카메라를 향하도록 함
        if (FaceCamera)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
        }
    }


    IEnumerator UpdateLocativeLabel()
    {
        var wait = new WaitForSeconds(0.2f);

        while(true)
        {
            float dist = transform.parent.transform.position.magnitude;

            // 건물이 멀리 있거나 가까이 있는 경우에만 활성화
            if ((dist > (MinimumDistance-fade)) && (dist < (MaximumDistance+fade)))
            {
                UI_Canvas.SetActive(true);
                UI_Canvas.GetComponent<CanvasGroup>().alpha = 1.0f;

                //건물이 최대 표시 거리보다 멀리 있는 경우 fade out
                if (dist> MaximumDistance) 
                {
                    float a = 1.00f * ((MaximumDistance+fade)-dist) / fade;
                    UI_Canvas.GetComponent<CanvasGroup>().alpha = a;
                }
                //건물이 최th 표시 거리보다 가까이 있는 경우 fade out
                if (dist < MinimumDistance)
                {             
                    float a = 1.00f*(dist - (MinimumDistance - fade)) / fade;
                    UI_Canvas.GetComponent<CanvasGroup>().alpha = a;
                }
            }
            else
            {
                UI_Canvas.SetActive(false);
            }
            
            yield return wait;
        }
        
    }
}
