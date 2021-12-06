using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

//<<UI 관련 기능>>
public class UiController : MonoBehaviour
{
    const int NUM_OF_CANVAS = 6;

    private UpdateInfo updateInfo;
    
    //이전 화면을 저장하기 쉬한 스택
    private int[] stackCanvas = new int[NUM_OF_CANVAS];
    private int stackIndex;

    private Ray tempRay;

    //0: AR, 1: Info, 2: Map, 3: Mission, 4: Search 5: LarBg
    public Canvas[] canvasList;
    public CanvasGroup[] canvasGroupList;
    //0: UI, 1: Minimap, 2: Lar
    public Camera[] cameraList;
    public GameObject buildings;
    private int clickCount;


    private void Start()
    {
        updateInfo = new UpdateInfo();

        canvasList[0].enabled = true;
        canvasGroupList[0].alpha = 1;

        for (int i = 1; i < NUM_OF_CANVAS - 1; i++)
        {
            canvasList[i].enabled = false;
            canvasGroupList[i].alpha = 0;
        }

        stackIndex = 0;
        stackCanvas[stackIndex] = 0;

        clickCount = 0;
    }
    
    private void Update()
    {
        //Raycast를 이용하여 오브젝트와 충돌 감지
        if (Input.GetMouseButtonDown(0))
        {   
            //클릭다운된 좌표에 Ray 저장
            tempRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        }
        if (Input.GetMouseButtonUp(0))
        {
            RaycastHit downHit, upHit;

            if (Physics.Raycast(tempRay, out downHit, 200))
            {
                //클릭업된 Ray와 충돌된 오브젝트 RaycastHit 정보
                tempRay = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(tempRay, out upHit, 200))
                {
                    //마우스가 클릭되고 떼어진 좌표의 오브젝트가 같은 경우에만 실행
                    if (upHit.collider.gameObject.name == downHit.collider.gameObject.name)
                    {
                        int currentCanvasNum = GameManager.Instance.currentCanvasNum;
                        //AR화면에서는 ARTag만 Map화면에서는 MapTag의 오브젝트만 충돌
                        if ((upHit.collider.gameObject.tag == "ARTag" && currentCanvasNum == 0)
                            || (upHit.collider.gameObject.tag == "MapTag" && currentCanvasNum == 2))
                        {
                            GameManager.Instance.bdNumSelected = upHit.collider.gameObject.name.Split('_')[0];
                            //Info화면으로 전환
                            ConvertCanvas(1);
                        }
                    }
                    
                }
            }

            //For UGUI
            GameObject tempObj;

            if ( (tempObj = EventSystem.current.currentSelectedGameObject) && tempObj.tag == "BuildingTag" )
            {
                GameManager.Instance.bdNumSelected = tempObj.name;   
            }
        }

        //뒤로가기 버튼 1초 안에 2번 연속 누를 시 어플 종료
        if (Application.platform == RuntimePlatform.Android)
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                clickCount++;
                DisplayAndroidToastMessage("한 번 더 누를 시 어플이 종료됩니다.");

                if (!IsInvoking("InitDoubleClick"))
                    Invoke("InitDoubleClick", 1.0f);
            }
            if (clickCount == 2)
            {
                CancelInvoke("InitDoubleClick");
                Application.Quit();
            }
        }
    }

    //<<더블클릭 초기화>>
    private void InitDoubleClick()
    {
        clickCount = 0;
    }

    //<<화면 전환 기능>>
    //Canvas 활설화/비활성화를 통해 화면을 전환
    //stackCanvas에 거쳐온 화면의 순서를 저장하면서 이전 화면으로 전환할 수 있도록 함
    public void ConvertCanvas(int canvasNum)
    {
        int beforeStack, currentStack;
        
        //뒤로 가기 버튼 클릭 시
        if (canvasNum < 0)
        {
            //현재 화면을 currentStack에 임시 저장, 직전 화면을 beforeStack에 저장
            //최상위 스택을 하나 삭제
            //현재 화면을 비활성화 하고, 직전 화면을 활성화
            currentStack = stackCanvas[stackIndex--];
            beforeStack = stackCanvas[stackIndex];

            canvasList[currentStack].enabled = false;
            canvasGroupList[currentStack].alpha = 0;

            canvasList[beforeStack].enabled = true;
            canvasGroupList[beforeStack].alpha = 1;

            GameManager.Instance.currentCanvasNum = beforeStack;
        }
        else
        {
            //현재 화면을 beforeStack에 임시 저장하고 비활성화
            //이동할 화면을 스택에 추가
            beforeStack = stackCanvas[stackIndex];
            stackCanvas[++stackIndex] = canvasNum;

            canvasList[beforeStack].enabled = false;
            canvasGroupList[beforeStack].alpha = 0;

            canvasList[canvasNum].enabled = true;
            canvasGroupList[canvasNum].alpha = 1;

            GameManager.Instance.currentCanvasNum = canvasNum;
        }


        //------카메라 제어-------
        //AR 화면인 경우
        if (stackCanvas[stackIndex] == 0)
        {
            //캔버스 스택 초기화
            stackIndex = 0;

            buildings.SetActive(true);

            cameraList[0].gameObject.SetActive(false);
            cameraList[1].gameObject.SetActive(true);
            cameraList[2].gameObject.SetActive(true);
        }
        else
        {
            cameraList[0].gameObject.SetActive(true);
            cameraList[2].gameObject.SetActive(false);

            //AR 화면이 아니고, 2D 지도 화면인 경우
            if (stackCanvas[stackIndex] == 2)
            {
                buildings.SetActive(true);
                cameraList[1].gameObject.SetActive(true);
            }
            else
            {
                buildings.SetActive(false);
                cameraList[1].gameObject.SetActive(false);
            }
        }
    }

    //<<팝업창 닫기>>
    public void ClosePopUp(GameObject popUp)
    {
        popUp.SetActive(false);
    }

    //<<토스트 메시지 출력>>
    public static void DisplayAndroidToastMessage(string message)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            if (unityActivity != null)
            {
                AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
                unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity, message, 0);
                    toastObject.Call("show");
                }));
            }
        }
    }
}
