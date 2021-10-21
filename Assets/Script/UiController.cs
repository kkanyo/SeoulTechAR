using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UiController : MonoBehaviour
{
    const int NUM_OF_CANVAS = 6;

    private UpdateInfo updateInfo;
    
    //이전 화면을 저장하기 쉬한 스택
    private int[] stackCanvas = new int[NUM_OF_CANVAS];
    private int stackIndex;
    private Ray tempRay;
    private Vector2 nowPos, prePos, movePosDiff;

    //0: AR, 1: Info, 2: Map, 3: Mission, 4: Search 5: LarBg
    public Canvas[] canvasList;
    public CanvasGroup[] canvasGroupList;
    //0: UI, 1: Minimap, 2: Lar
    public Camera[] cameraList;
    public GameObject buildings;

    // Start is called before the first frame update
    private void Start()
    {
        updateInfo = new UpdateInfo();

        //초기 실행 시 화면 설정
        canvasList[0].enabled = true;
        canvasGroupList[0].alpha = 1;

        for (int i = 1; i < NUM_OF_CANVAS - 1; i++)
        {
            canvasList[i].enabled = false;
            canvasGroupList[i].alpha = 0;
        }

        stackIndex = 0;
        stackCanvas[stackIndex] = 0;
    }
    
    // Update is called once per frame
    private void Update()
    {
        //Raycast를 이용하여 오브젝트와 충돌 감지
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log(EventSystem.current.currentSelectedGameObject);
            tempRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        }
        if (Input.GetMouseButtonUp(0))
        {
            RaycastHit downHit, upHit;

            Physics.Raycast(tempRay, out downHit);

            tempRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            //클릭하고 벗어나거나 클릭된 상태로 들어왔을 때 실행되는 것을 방지
            if (Physics.Raycast(tempRay, out upHit) && upHit.Equals(downHit))
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

            //UGUI에서 작동
            GameObject tempObj;

            if ( (tempObj = EventSystem.current.currentSelectedGameObject) && tempObj.tag == "BuildingTag" )
            {
                GameManager.Instance.bdNumSelected = tempObj.name;   
            }
        }
        
        if (Application.platform == RuntimePlatform.Android)
        {
            if (Input.GetKey(KeyCode.Escape))
                ConvertCanvas(-1);
        }
    }


    public void ConvertCanvas(int canvasNum)
    {
        int beforeStack, currentStack;
        
        //뒤로 가기 버튼 클릭 시
        if (canvasNum < 0)
        {
            currentStack = stackCanvas[stackIndex--];
            beforeStack = stackCanvas[stackIndex];

            canvasList[currentStack].enabled = false;
            canvasGroupList[currentStack].alpha = 0;

            canvasList[beforeStack].enabled = true;
            canvasGroupList[beforeStack].alpha = 1;

            GameManager.Instance.currentCanvasNum = beforeStack;
        }
        //화면 이동 버튼 클릭 시
        else
        {
            beforeStack = stackCanvas[stackIndex];
            stackCanvas[++stackIndex] = canvasNum;

            canvasList[beforeStack].enabled = false;
            canvasGroupList[beforeStack].alpha = 0;

            canvasList[canvasNum].enabled = true;
            canvasGroupList[canvasNum].alpha = 1;

            GameManager.Instance.currentCanvasNum = canvasNum;
        }


        //카메라 제어
        if (stackCanvas[stackIndex] == 0)
        {
            //AR 화면으로 이동 시 캔버스 스택 초기화
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


    //팝업 창 종료
    public void ClosePopUp(GameObject popUp)
    {
        popUp.SetActive(false);
    }


    //Android Native
    //토스트 메시지 출력
    public void DisplayAndroidToastMessage(string message)
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
