using Mono.Data.Sqlite;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SearchManager : MonoBehaviour
{
    private GameObject buildingList;
    private TMPro.TMP_InputField searchInput;
    private Text searchMode;

    private void Start() {
        buildingList = transform.Find("Scroll View").Find("Viewport").Find("BuildingList").gameObject;
        searchInput = transform.Find("SearchInput").GetComponent<TMPro.TMP_InputField>();
        searchMode = transform.Find("Dropdown").Find("SearchMode").GetComponent<Text>();
    }

    //검색 알고리즘
    public void UpdateList()
    {
        DatabaseManager databaseManager = GameManager.Instance.databaseManager;

        SqliteDataReader tempSql = null;

        switch (searchMode.text)
        {
            case "건물명":
                tempSql = databaseManager.SelectWhere("Buildings", "DISTINCT",
                    new string[] {"BuildingNumber"},
                    new string[] {"BuildingName"},
                    new string[] {" LIKE "},
                    new string[] {"%" + searchInput.text + "%"},
                    "BuildingNumber", "ASC");
                break;
            case "학과명":
                tempSql = databaseManager.SelectWhere("Major", "DISTINCT",
                    new string[] {"BuildingNumber"},
                    new string[] {"MajorName"},
                    new string[] {" LIKE "},
                    new string[] {"%" + searchInput.text + "%"},
                    "BuildingNumber", "ASC" );
                break;
            case "편의시설":
                tempSql = databaseManager.SelectWhere("Facilities", "DISTINCT",
                    new string[] {"BuildingNumber"},
                    new string[] {"FacilityType"},
                    new string[] {" LIKE "},
                    new string[] {"%" + searchInput.text + "%"},
                    "BuildingNumber", "ASC" );
                break;
            case "교수명":
                tempSql = databaseManager.SelectWhere("Professor", "DISTINCT",
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

    //검색어 초기화
    public void InitSearchData()
    {
        searchInput.text = "";
    }
}
