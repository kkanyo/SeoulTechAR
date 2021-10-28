/*
 * Mário Vairinhos 2020 
 * DECA LAR - LOCATIVE AUGMENTED REALITY
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocativeTarget : MonoBehaviour
{

    [Header("Google Maps Coordinates")]
    public double latitude;
    public double longitude;
    public double altitude;

    private double  raioTerra = 6372797.560856f;


    private void Start()
    {

    }


    private void Update()
    {
        Vector3 coord = calculaPosCoordCam();
        transform.position = coord;
    }

    
    // 사용자의 위치를 기준으로 상대적인 좌표 계산
    private Vector3 calculaPosCoordCam()
    {
        double dlat = latitude - LocativeGPS.Instance.latitude;
            dlat = getCoordEmMetrosDeRaio(raioTerra, dlat);

        double dlon = longitude - LocativeGPS.Instance.longitude;

            double raioLat = Mathf.Cos((float)latitude) * raioTerra;
            dlon = getCoordEmMetrosDeRaio(raioLat, dlon);


        double dalt = altitude - 0.0f;
        return new Vector3((float)dlat, (float)dalt, -(float)dlon);
    }


    // 위도경도 좌표를 xy 좌표로 변환
    private double getCoordEmMetrosDeRaio(double raio, double angulo)
    {
        double metros = (raio / 180) * Mathf.PI;
        metros *= angulo;
        return metros;
    }


}
