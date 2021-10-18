using Mono.Data.Sqlite;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MissionManager : MonoBehaviour
{
    private string mDatabaseFileName = "BuildingInfo.db";
    private DatabaseManager mDatabaseManager;
    private UiController uiController;
    
    #region forMission
    private bool isMissionCheck;
    private bool[] isMissionClear;  //각 미션이 완료됐는지 기록
    private string userMajorOffice;
    private string[] gradeList;
    private int levelCount;
    public TMPro.TMP_InputField myMajor;
    public GameObject buildingMsn3;
    public Text buildingNameMsn3;
    public Image[] missionList;
    public GameObject buildingList;
    public GameObject completePopUp;
    public Text gradeText;
    #endregion

    #region forNavigate
    private GameObject naviTarget;
    private Vector3 targetPosition;
    public GameObject cancelNaviBtn;
    public GameObject navigator;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        mDatabaseManager = new DatabaseManager(mDatabaseFileName);
        uiController = new UiController();

        isMissionCheck = false;
        isMissionClear = new bool[] {false, false, false, false, false};
        gradeList = new string[] { "Lv1. 갓난내기", "Lv2. 새내기", "Lv3. 밥값내기", "Lv4. 헌내기" };
        gradeText.text = gradeList[0];
        levelCount = 0;

        cancelNaviBtn.SetActive(false);
        navigator.SetActive(false);

        StartCoroutine("NavigateCoroutine");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    IEnumerator NavigateCoroutine()
    {
        var wait = new WaitForSeconds(0.25f);

        string targetNum = GameManager.Instance.bdNumSelected;

        float dist;

        while (GameManager.Instance.isNavigate)
        {
            //목적지까지와의 거리
            dist = buildingList.transform.Find(targetNum)
                    .Find(targetNum + "_Target").position.magnitude;

            targetPosition = new Vector3(naviTarget.transform.position.x, 
                                            navigator.transform.position.y, 
                                            naviTarget.transform.position.z);

            navigator.transform.LookAt(targetPosition);

            //목적지가 미션 건물이고 가까워지면 미션 성공
            if (dist < 30)
            {
                if (isMissionCheck)
                {
                    if (targetNum == "1" && !isMissionClear[0])
                    {
                        MissionClear(targetNum);
                    }
                    else if ((targetNum == "7" || targetNum == "37" || targetNum == "52")
                            && !isMissionClear[1])
                    {
                        MissionClear(targetNum);
                    }
                    else if (targetNum == userMajorOffice && !isMissionClear[2])
                    {
                        MissionClear(targetNum);
                    }
                }
                
                EndNavigate();
            }

            yield return wait;
        }
    }
    
    //사용자의 학과 설정
    public void SetMyMajor()
    {
        SqliteDataReader tempSql = mDatabaseManager.SelectWhere("Major", null,
            new string[] { "BuildingNumber" },
            new string[] { "MajorName" },
            new string[] { " LIKE " },
            new string[] { "%" + myMajor.text + "%" },
            null, null );

        //Save the data in advance
        userMajorOffice = tempSql[0].ToString();

        int rowCount = 0;

        //count number of rows
        while (tempSql.Read())
        {
            rowCount++;
        }
        
        //If multiple search results appear, re-request the search.
        if (rowCount != 1)
        {
            //Delete dump data
            userMajorOffice = null;

            Debug.Log("잘못된 입력입니다.");
            
            //Display toast message
                uiController.DisplayAndroidToastMessage("잘못된 입력입니다.");
        }
        else
        {
            tempSql = mDatabaseManager.SelectWhere("Buildings", null,
                new string[] { "BuildingName" },
                new string[] { "BuildingNumber" },
                new string[] { "=" },
                new string[] { userMajorOffice },
                null, null );

            //update destination data of mission3 
            buildingMsn3.name = userMajorOffice;
            buildingNameMsn3.text = tempSql[0].ToString();
        }

        tempSql.Close();
    }

    public void MissionCheck()
    {
        isMissionCheck = true;
    }

    public void MissionClear(string targetNum)
    {
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

    //If complete a mission, update UI of the mission list
    public void UpdateMissionUI(int index)
    {
        completePopUp.SetActive(true);

        gradeText.text = gradeList[(++levelCount)/2];

        missionList[index].color = Color.grey;

        missionList[index].transform.Find("DoneText").GetComponent<TMPro.TMP_Text>().text = "완료";

        for (int i = 3; i < missionList[index].transform.childCount; i++)
        {
            missionList[index].transform.GetChild(i).GetComponent<Button>().enabled = false;
        }
        
        isMissionClear[index] = true;
    }

    public void StartNavigate()
    {
        string targetNum = GameManager.Instance.bdNumSelected;

        naviTarget = buildingList.transform.Find(targetNum).Find(targetNum + "_Target").gameObject;

        cancelNaviBtn.SetActive(true);
        navigator.SetActive(true);

        GameManager.Instance.isNavigate = true;

        StartCoroutine("NavigateCoroutine");

        Debug.Log("안내를 시작합니다.");
        uiController.DisplayAndroidToastMessage("안내를 시작합니다.");
    }

    public void EndNavigate()
    {
        cancelNaviBtn.SetActive(false);
        navigator.SetActive(false);

        GameManager.Instance.isNavigate = false;

        Debug.Log("안내를 종료합니다.");
        uiController.DisplayAndroidToastMessage("안내를 종료합니다.");
    }
}
