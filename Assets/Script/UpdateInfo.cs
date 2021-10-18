using Mono.Data.Sqlite;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UpdateInfo : MonoBehaviour
{
    private string databaseFileName = "BuildingInfo.db";
    private DatabaseManager databaseManager;
    
    public Image infoBuildingImage;
    public Text infoNumText;
    public Text infoNameText;
    public Text infoContentText;

    // Start is called before the first frame update
    void Start()
    {
        databaseManager = new DatabaseManager(databaseFileName);

        StartCoroutine(ApplyBuildingInfo());
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator ApplyBuildingInfo()
    {
        var wait = new WaitForSeconds(0.05f);

        while (true)
        {
            if (GameManager.Instance.currentCanvasNum == 1)
                UpdateBuildingInfo();

            yield return wait;
        }
    }

    public void UpdateBuildingInfo()
    {
        Debug.Log(GameManager.Instance.currentCanvasNum);
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

        //Update text and image of InfoCanvas
        infoBuildingImage.sprite = 
            Resources.Load<Sprite>("Image/BuildingPictures/" + targetNum);
        infoNameText.text = targetName;
        infoNumText.text = targetNum;
        infoContentText.text = targetContent;

        tempSql.Close();
    }
}