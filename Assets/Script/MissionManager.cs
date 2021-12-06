using Mono.Data.Sqlite;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

//미션 수행 관련 기능 및 길 찾기 기능
public class MissionManager : MonoBehaviour
{
    //[미션과 관련된 변수]
    #region forMission
    private bool isMissionCheck;    //미션을 확인했는지 체크
    private bool[] isMissionClear;  //각 미션이 완료됐는지 기록
    private string userMajorOffice;
    private string[] gradeList;     //사용자 등급 리스트
    private int levelCount;
    private Text gradeText;
    private TMPro.TMP_InputField myMajor;
    private GameObject buildingMsn3;
    private Text buildingNameMsn3;
    private Image[] missionList;
    public GameObject buildingList;
    public GameObject completePopUp;
    #endregion

    //[길 찾기 기능과 관련된 변수]
    #region forNavigate
    private GameObject naviTarget;
    private Vector3 targetPosition;
    public GameObject cancelNaviBtn;
    public GameObject navigator;
    #endregion

    private void Start()
    {
        //변수들을 초기화
        Transform temp = transform.Find("Scroll View").Find("Viewport").Find("MissionList");

        gradeText = transform.Find("GradeText").Find("Text").GetComponent<Text>();
        myMajor = transform.Find("MajorInput").GetComponent<TMPro.TMP_InputField>();
        buildingMsn3 = temp.GetChild(2).Find("00").gameObject;
        buildingNameMsn3 = buildingMsn3.transform.Find("NameText").GetComponent<Text>();
        missionList = new Image[] {
            temp.GetChild(0).GetComponent<Image>(),
            temp.GetChild(1).GetComponent<Image>(),
            temp.GetChild(2).GetComponent<Image>(),
            temp.GetChild(3).GetComponent<Image>(),
            temp.GetChild(4).GetComponent<Image>(),
            temp.GetChild(5).GetComponent<Image>()
        };

        isMissionCheck = false;
        isMissionClear = new bool[] {false, false, false, false, false, false};
        gradeList = new string[] { "Lv1. 갓난내기", "Lv2. 새내기", "Lv3. 밥값내기", "Lv4. 헌내기" };
        gradeText.text = gradeList[0];
        levelCount = 0;
        buildingMsn3.GetComponent<Button>().enabled = false;
    
        cancelNaviBtn.SetActive(false);
        navigator.SetActive(false);
    }
    
    //<<길 찾기 코루틴>>
    IEnumerator NavigateCoroutine()
    {
        var wait = new WaitForEndOfFrame();

        string targetNum = GameManager.Instance.bdNumSelected;

        float dist;

        while (GameManager.Instance.isNavigate)
        {
            //목적지까지와의 거리 계산
            dist = buildingList.transform.Find(targetNum)
                    .Find(targetNum + "_Target").position.magnitude;

            targetPosition = new Vector3(naviTarget.transform.position.x, 
                                            navigator.transform.position.y, 
                                            naviTarget.transform.position.z);

            navigator.transform.LookAt(targetPosition);

            //목적지가 미션 건물이고 목적지와 가까워지면 미션 성공 처리
            if (dist < 30)
            {
                if (isMissionCheck)
                {
                    MissionClear(targetNum);
                }
                
                EndNavigate();
            }

            yield return wait;
        }
    }
    
    //<<사용자의 학과 설정>>
    public void SetMyMajor()
    {
        //데이터 베이스에 접근
        DatabaseManager databaseManager = GameManager.Instance.databaseManager;

        SqliteDataReader tempSql = databaseManager.SelectWhere("Major", null,
            new string[] { "BuildingNumber" },
            new string[] { "MajorName" },
            new string[] { " LIKE " },
            new string[] { "%" + myMajor.text + "%" },
            null, null );

        userMajorOffice = tempSql[0].ToString();

        int rowCount = 0;

        //검색한 데이터의 수를 계산
        while (tempSql.Read())
        {
            rowCount++;
        }
        
        //입력된 학과명으로 검색된 학과가 1개가 아닌 경우 예외 처리
        if (rowCount != 1)
        {
            userMajorOffice = null;

            //토스트 메세지 출력
            UiController.DisplayAndroidToastMessage("잘못된 입력입니다.");
        }
        else
        {
            tempSql = databaseManager.SelectWhere("Buildings", null,
                new string[] { "BuildingName" },
                new string[] { "BuildingNumber" },
                new string[] { "=" },
                new string[] { userMajorOffice },
                null, null );

            //미션3의 데이터 변경
            buildingMsn3.name = userMajorOffice;
            buildingMsn3.GetComponent<Button>().enabled = true;
            buildingNameMsn3.text = tempSql[0].ToString();
            
        }

        tempSql.Close();
    }

    //<<완료된 미션 판단>>
    private void MissionClear(string targetNum)
    {
        //미션에 해당하는 목적지에 도착하면 해당 미션 완료 작업
        switch (targetNum)
        {
            case "1":
                UpdateMissionUI(0);
                break;
            case "7":
            case "37":
                UpdateMissionUI(1);
                break;
            case "52":
                UpdateMissionUI(1);
                UpdateMissionUI(3);
                break;
            case "34":
            case "35":
                UpdateMissionUI(4);
                break;
            case "11":
                UpdateMissionUI(5);
                break;
            default:
                if (targetNum == userMajorOffice)
                {
                    UpdateMissionUI(2);
                }
                break;
        }
    }

    //<<완료된 미션의 UI 변경>>
    private void UpdateMissionUI(int index)
    {
        //완료되지 않은 미션의 UI를 완료 처리
        if (!isMissionClear[index])
        {
            //Debug.Log("성공");
            completePopUp.SetActive(true);

            //미션 2개가 성공할 때마다 등급 상승
            gradeText.text = gradeList[(++levelCount)/2];

            missionList[index].color = Color.grey;

            missionList[index].transform.Find("DoneText").GetComponent<TMPro.TMP_Text>().text = "완료";

            //완료된 미션 버튼 비활성화
            for (int i = 3; i < missionList[index].transform.childCount; i++)
            {
                missionList[index].transform.GetChild(i).GetComponent<Button>().enabled = false;
            }
        
        isMissionClear[index] = true;
        }
    }

    //<<사용자가 미션 리스트를 확인했는지 체크>>
    public void MissionCheck()
    {
        isMissionCheck = true;
    }

    //<<길 찾기 시작 - 길 찾기 버튼 클릭>>
    public void StartNavigate()
    {
        string targetNum = GameManager.Instance.bdNumSelected;

        //길 찾기 목적지 설정
        naviTarget = buildingList.transform.Find(targetNum).Find(targetNum + "_Target").gameObject;

        //UI 활성화
        cancelNaviBtn.SetActive(true);
        navigator.SetActive(true);

        GameManager.Instance.isNavigate = true;

        //길 찾기 코루틴 시작
        StartCoroutine(NavigateCoroutine());

        //Debug.Log("안내를 시작합니다.");
        UiController.DisplayAndroidToastMessage("안내를 시작합니다.");
    }

    //<<길 찾기 종료 - 안내 종료 버튼 클릭>>
    public void EndNavigate()
    {
        //UI 비활성화
        cancelNaviBtn.SetActive(false);
        navigator.SetActive(false);

        GameManager.Instance.isNavigate = false;

        //길 찾기 코루틴 종료
        StopCoroutine(NavigateCoroutine());

        //Debug.Log("안내를 종료합니다.");
        UiController.DisplayAndroidToastMessage("안내를 종료합니다.");
    }
}
