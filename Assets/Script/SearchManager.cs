using Mono.Data.Sqlite;
using UnityEngine;
using UnityEngine.UI;

public class SearchManager : MonoBehaviour
{
    private string mDatabaseFileName = "BuildingInfo.db";
    private DatabaseManager mDatabaseManager;

    public GameObject buildingList;
    public TMPro.TMP_InputField searchInput;
    public Text searchMode;
    
    // Start is called before the first frame update
    void Start()
    {
        mDatabaseManager = new DatabaseManager(mDatabaseFileName);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateList()
    {
        SqliteDataReader tempSql = null;

        switch (searchMode.text)
        {
            case "건물명":
                tempSql = mDatabaseManager.SelectWhere("Buildings", "DISTINCT",
                    new string[] {"BuildingNumber"},
                    new string[] {"BuildingName"},
                    new string[] {" LIKE "},
                    new string[] {"%" + searchInput.text + "%"},
                    "BuildingNumber", "ASC");
                break;
            case "학과명":
                tempSql = mDatabaseManager.SelectWhere("Major", "DISTINCT",
                    new string[] {"BuildingNumber"},
                    new string[] {"MajorName"},
                    new string[] {" LIKE "},
                    new string[] {"%" + searchInput.text + "%"},
                    "BuildingNumber", "ASC" );
                break;
            case "편의시설":
                tempSql = mDatabaseManager.SelectWhere("Facilities", "DISTINCT",
                    new string[] {"BuildingNumber"},
                    new string[] {"FacilityType"},
                    new string[] {" LIKE "},
                    new string[] {"%" + searchInput.text + "%"},
                    "BuildingNumber", "ASC" );
                break;
            case "교수명":
                tempSql = mDatabaseManager.SelectWhere("Professor", "DISTINCT",
                    new string[] {"BuildingNumber"},
                    new string[] {"ProfessorName"},
                    new string[] {" LIKE "},
                    new string[] {"%" + searchInput.text + "%"},
                    "BuildingNumber", "ASC" );
                break;        
        }

        tempSql.Read();
        
        //검색어와 관련된 건물만 리스트 출력
        for (int i = 0; i < buildingList.transform.childCount; i++)
        {
            Transform tempChild = buildingList.transform.GetChild(i);

            tempChild.gameObject.SetActive(false);
            
            if (tempChild.name == tempSql[0].ToString())
            {  
                tempChild.gameObject.SetActive(true);
                
                tempSql.Read();
            }
        }

        //검색창이 비어있을 때, 모든 건물 리스트 출력
        if (searchInput.text == "")
        {
            for (int i = 0; i < buildingList.transform.childCount; i++)
            {
                buildingList.transform.GetChild(i).gameObject.SetActive(true);
            }
        }

        tempSql.Close();
    }
}
