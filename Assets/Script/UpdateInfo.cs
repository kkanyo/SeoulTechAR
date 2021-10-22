using Mono.Data.Sqlite;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UpdateInfo : MonoBehaviour
{
    private Image infoImage;
    private Text[] infoTextList;

    // Start is called before the first frame update
    private void Start()
    {
        infoImage = transform.Find("BuildingImage").GetComponent<Image>();
        infoTextList = new Text[] {
            transform.Find("BuildingNumText").GetComponent<Text>(),
            transform.Find("BuildingNameText").GetComponent<Text>(),
            transform.Find("Scroll View").Find("Viewport").Find("BuildingContent").Find("BuildingContentText").GetComponent<Text>()
        };
        
        StartCoroutine(ApplyBuildingInfo());
    }

    //갱신된 건물 정보 적용 Coroutine
    private IEnumerator ApplyBuildingInfo()
    {
        var wait = new WaitForEndOfFrame();

        while (true)
        {
            //현재 건물 정보와 선택된 건물의 정보가 다를 경우
            if (GameManager.Instance.bdNumSelected != infoTextList[0].text)
            {
                UpdateBuildingInfo();
            }
            yield return wait;
        }
    }

    //데이터베이스에서 건물 정보 획득 후 갱신
    private void UpdateBuildingInfo()
    {
        DatabaseManager databaseManager = GameManager.Instance.databaseManager;

        string targetNum = GameManager.Instance.bdNumSelected;
        string targetName = "";
        string targetContent = "";

        //Building name and overview data
        SqliteDataReader tempSql = databaseManager.SelectWhere("Buildings", null,
            new string[] {"BuildingName", "BuildingOverview"},
            new string[] {"BuildingNumber"},
            new string[] {"="},
            new string[] {targetNum},
            null, null);

        targetName = tempSql[0].ToString();
        targetContent += (tempSql[1] + "\n\n");

        //College Data
        tempSql = databaseManager.SelectWhere("College", null,
            new string[] {"CollegeName", "CollegeOffice", "CollegeOfficeTell"},
            new string[] {"BuildingNumber"},
            new string[] {"="},
            new string[] {targetNum},
            null, null );
        
        if (!tempSql.IsDBNull(0)) {
            targetContent += tempSql[0]
                + "\n사무실: " + tempSql[1]
                + "호\nTel. " + tempSql[2] + "\n\n";
        }

        //Facilities Data
        tempSql = databaseManager.SelectWhere("Facilities", null,
            new string[] {"FacilityName", "Location", "FacilityOverview"},
            new string[] {"BuildingNumber"},
            new string[] {"="},
            new string[] {targetNum},
            null, null );

        if (!tempSql.IsDBNull(0)) {
            targetContent += "편의시설 정보: \n";
            while (tempSql.Read())
            {
                targetContent += tempSql[0] + " (" + tempSql[1] + ")\n" + tempSql[2] + "\n\n";
            }
        }
        
        //Major Data
        tempSql = databaseManager.SelectWhere("Major", null,
            new string[] {"MajorName", "MajorOffice", "MajorOfficeTel"},
            new string[] {"BuildingNumber"},
            new string[] {"="},
            new string[] {targetNum},
            null, null );

        if (!tempSql.IsDBNull(0)) {
            targetContent += "학과 정보: \n";
            while (tempSql.Read())
            {
                targetContent += tempSql[0]
                    + "\n사무실: " + tempSql[1]
                    + "호\nTel. " + tempSql[2] + "\n\n";
            }
        }

        //Professor Data
        tempSql = databaseManager.SelectWhere("Professor", null,
            new string[] {"ProfessorName", "MajorName", "Lab"},
            new string[] {"BuildingNumber"},
            new string[] {"="},
            new string[] {targetNum},
            "ProfessorName", "ASC" );

        if (!tempSql.IsDBNull(0)) {
            targetContent += "교수 정보: \n";
            while (tempSql.Read())
            {
                targetContent += tempSql[0]
                    + " (" + tempSql[1]
                    + ") " + tempSql[2] + "호\n\n";
            }
        }

        //건물 정보 화면의 데이터들을 변경
        infoImage.sprite = 
            Resources.Load<Sprite>("Image/BuildingPictures/" + targetNum);
        infoTextList[0].text = targetNum;
        infoTextList[1].text = targetName;
        infoTextList[2].text = targetContent;

        tempSql.Close();
    }
}