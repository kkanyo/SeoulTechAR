using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android;

public enum gpsModes {
    Unity3D,
    NativeToolkit,
    Off };

public class LocativeGPS : MonoBehaviour
{
    public static LocativeGPS Instance { set; get; }

   
    [Header("User")]
    public float BodyHeight=1.7f;

    [Header("Actual Location")]
    [Tooltip("Read Only latitude in runtime")]
    //[HideInInspector]
    public  double latitude;
    [Tooltip("Read Only longitude in runtime")]
    //[HideInInspector]
    public  double longitude;
    [Tooltip("Read Only altitude in runtime")]
    //[HideInInspector]
    public  double altitude;

    
    [Header("GPS Mode")]
    [Tooltip("Smartphone only")]
    public gpsModes gpsMode;

    [Header("Editor Mode simulator")]
    public double EdLatitude;
    public double EdLongitude;
    public double EdAltitude;
    [Range(0, 20)]
    public float MouseSensibility=5f;


    private GameObject cameraContainer;


    // Gyro
    private Gyroscope gyro;
    private Quaternion rotation;

    // Camera
    private WebCamTexture cam;
    private RawImage background;
    private AspectRatioFitter fit;

    // general
    private bool arReady = false;
    private bool GPS = false;
    private Text DebugGPS;
    private Text DebugConsole;
    private Vector2 currentRotation;


    private void Start()
    {
        Instance = this;

        // load debugUI
        DebugGPS        = transform.Find("LAR_BackgroundCamera").Find("UI_Background").Find("DebugGPS").gameObject.GetComponent<Text>();
        DebugConsole    = transform.Find("LAR_BackgroundCamera").Find("UI_Background").Find("DebugConsole").gameObject.GetComponent<Text>();
        background      = transform.Find("LAR_BackgroundCamera").Find("UI_Background").Find("Background").gameObject.GetComponent<RawImage>();
        fit             = transform.Find("LAR_BackgroundCamera").Find("UI_Background").Find("Background").gameObject.GetComponent<AspectRatioFitter>();


        //GPS NATIVE TOOLKIT
        if (!Application.isEditor)
        {
            // Position Camera
            cameraContainer = new GameObject("Camera Container");
            cameraContainer.transform.position = transform.position;
            transform.SetParent(cameraContainer.transform);
            cameraContainer.transform.rotation = Quaternion.Euler(90f, -90F, 0); //(90f, 0, 0);


            // check if we support Gyro
            if (!SystemInfo.supportsGyroscope)
            {
                Debug.Log("no Gyro");

            }

            //스마트폰의 카메라 정보
            WebCamDevice[] devices = WebCamTexture.devices;
            int selectedCameraIndex = -1;

            //후면 카메라 찾기
            for (int i = 0; i < devices.Length; i++)
            {
                if (!devices[i].isFrontFacing)
                {
                    selectedCameraIndex = i;
                    break;
                }
            }

            //카메라 연동 시작
            if (selectedCameraIndex >= 0)
            {
                cam = new WebCamTexture(WebCamTexture.devices[selectedCameraIndex].name);
                
                cam.Play();

                background.texture = cam;
                if (devices[selectedCameraIndex].isAutoFocusPointSupported)
                    cam.autoFocusPoint = null;
                else
                    cam.autoFocusPoint = new Vector2(0, 0);
            }


            //자이로센서
            gyro = Input.gyro;
            gyro.enabled = true;
            rotation = new Quaternion(0, 0, 1, 0);

            arReady = true;


            //GPS
            //-------------------------------------
            // NATIVE TOOLKIT
            if (gpsMode == gpsModes.NativeToolkit)
            {
                // user wants NativeToolkit
                // check if this class exists in runtime 
                // before he use it, to prevent error
                bool classNativeToolkitExists = (null != Type.GetType("NativeToolkit"));
                if (classNativeToolkitExists)
                {
                    // START NativeToolkit GPS Interface
                    DebugConsole.text = "Starting NativeToolkit " + NativeToolkit.StartLocation();
                    GPS = true;
                }
                else
                {
                    //if not installed change to Unity3D GPS
                    DebugConsole.text = "Warning: NativeToolkit not present in Unity3D!\n using Unit3D GPS interface (less acurate)" + NativeToolkit.StartLocation();
                    gpsMode = gpsModes.Unity3D;

                }


            }
            else if (gpsMode == gpsModes.Unity3D)
            {
                // START Unity3D  GPS Interface
                DebugConsole.text = "Starting GPS Unity3D IEnterface";
                StartCoroutine(StartLocationService());
            }
        }

        // SET USER HIGHT
        transform.position = new Vector3(0, BodyHeight, 0);
    }


    private void Update()
    {
        if (!Application.isEditor)
        {
            //
            if (GameManager.Instance.currentCanvasNum == 0)
            {
                if (!cam.isPlaying)
                    cam.Play();
            }
            else
            {
                if (cam.isPlaying)
                    cam.Stop();
            }

            if (GPS)
            {
                if (gpsMode==gpsModes.NativeToolkit)
                {
                    latitude = NativeToolkit.GetLatitude();   
                    longitude = NativeToolkit.GetLongitude(); 
                    altitude = BodyHeight; 
                }
                if (gpsMode == gpsModes.Unity3D)
                {
                    latitude = (double) Input.location.lastData.latitude;
                    longitude = (double) Input.location.lastData.longitude;
                    altitude = BodyHeight;                      
                }


                if (arReady)
                {
                    //update Camera
                    float ratio = (float)cam.width / (float)cam.height;
                    fit.aspectRatio = ratio;
                    float scaleY = cam.videoVerticallyMirrored ? -1.0f : 1.0f;
                    background.rectTransform.localScale = new Vector3(1f, scaleY, 1f);
                    int orient = -cam.videoRotationAngle;
                    background.rectTransform.localEulerAngles = new Vector3(0, 0, orient);

                    //Update Gyro
                    transform.localRotation = gyro.attitude * rotation;
                }
            }
        }


        // EDITOR
        //---------------------------
        if (Application.isEditor && Camera.main.name == "LarCamera")
        {
            // no need to show video on rawimage background so
            background.enabled = false;
            fit.enabled = false;

            // COORDINATES
            latitude    = EdLatitude;
            longitude   = EdLongitude;
            altitude    = EdAltitude;

            // Simulator mouse look
            if (Input.GetMouseButton(1))
            {
                    currentRotation.x += Input.GetAxis("Mouse X") * MouseSensibility;
                    currentRotation.y -= Input.GetAxis("Mouse Y") * MouseSensibility;
                    currentRotation.x = Mathf.Repeat(currentRotation.x, 360);
                    currentRotation.y = Mathf.Clamp(currentRotation.y, -80, 80);
                    Camera.main.transform.rotation = Quaternion.Euler(currentRotation.y, currentRotation.x, 0);
            }


            // keyboard
            double slat= 0.000001;
            double slon = 0.000001;

            Vector3 eulerRotation = transform.rotation.eulerAngles;
            double dlat = slat* Math.Cos(Math.PI*(eulerRotation.y / 180.0));
            double dlon = slon* Math.Sin(Math.PI*(eulerRotation.y / 180.0));
            dlat = Input.GetKey(KeyCode.LeftShift) ? 5 * dlat : dlat;
            dlon = Input.GetKey(KeyCode.LeftShift) ? 5 * dlon : dlon;

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                EdLatitude -= dlat;
                EdLongitude -= dlon;
            }

            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                EdLatitude += dlat;
                EdLongitude += dlon;
            }

            dlat = slat * Math.Cos(Math.PI * ((eulerRotation.y+90) / 180.0));
            dlon = slon * Math.Sin(Math.PI * ((eulerRotation.y+90) / 180.0));
            dlat = Input.GetKey(KeyCode.LeftShift) ? 5 * dlat : dlat;
            dlon = Input.GetKey(KeyCode.LeftShift) ? 5 * dlon : dlon;

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.DownArrow))
            {
                EdLatitude -= dlat;
                EdLongitude -= dlon;
            }
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.UpArrow))
            {
                EdLatitude += dlat;
                EdLongitude += dlon;
            }
        }
    }


    //GPS UNITY3D INPUT.LOCATION (FLOAT) evitar usar
    private IEnumerator StartLocationService()
    {
        if (!Input.location.isEnabledByUser)
        {
            DebugConsole.text = "Please enabled GPS";
            yield break;
        }
        Input.location.Start(0.1f,0.1f);

        int maxWait = 20;
        while(Input.location.status==LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }
        if (maxWait <= 0)
        {
            DebugConsole.text="GPS is taking too long";
            yield break;
        }
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            DebugConsole.text = "GPS status failed";
            yield break;
        }
        latitude = (double)Input.location.lastData.latitude;
        longitude = (double)Input.location.lastData.longitude;
        GPS = true;
        DebugConsole.text = "Unity3D GPS interface OK!";
        yield break;
    }
}
