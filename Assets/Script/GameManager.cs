using System.Collections;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    #region Singleton
    private static GameManager instance = null;

    public DatabaseManager databaseManager;
    public string bdNumSelected;
    public int currentCanvasNum = 0;
    public bool isNavigate = false;

    #endregion

    public Image panel;
    private bool checkPermission = false;

    private void Awake()
    {
        if (null == instance)
        {
            instance = this;
            
            //Scene 전환 시 파괴되지 않음
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            //Scene 전환 후 이미 GameManager가 존재한다면 자신을 삭제
            Destroy(this.gameObject);
        }

        //사용자에게 접근 권한 허용 요청
        if (!Application.isEditor)
        {
            StartCoroutine(PermissionCheckCoroutine());
        }
        else
        {
            StartCoroutine(FadeCoroutine());
        }
        
        databaseManager = new DatabaseManager("BuildingInfo.db");
    }

    //Property
    public static GameManager Instance
    {
        get
        {
            if (null == instance)
            {
                return null;
            }
            return instance;
        }
    }

    //접근 권한 허용 체크 Coroutine
    IEnumerator PermissionCheckCoroutine()
    {
        var waitFrame = new WaitForEndOfFrame();
        var waitSec = new WaitForSeconds(0.2f);
        var waitFocus = new WaitUntil(() => Application.isFocused == true);
        
        yield return waitFrame;
        
        //카메라 권한 체크
        if (Permission.HasUserAuthorizedPermission(Permission.Camera) == false)
        {   
            //권한 요청
            Permission.RequestUserPermission(Permission.Camera);

            //0.2초 딜레이 후 Focus 체크
            yield return waitSec;
            yield return waitFocus;
        }
        
        if (Permission.HasUserAuthorizedPermission(Permission.FineLocation) == false)
        {
            Permission.RequestUserPermission(Permission.FineLocation);

            yield return waitSec;
            yield return waitFocus;
        }

        if (SceneManager.GetActiveScene().name == "Start")
        {
            StartCoroutine(FadeCoroutine());
            
        }
    }
        
    //Fade Effect Coroutine
    IEnumerator FadeCoroutine()
    {
        var wait = new WaitForSeconds(0.01f);

        float fadeCount = 0;
        
        while (fadeCount < 1.0f)
        {
            fadeCount += 0.005f;
            panel.color = new Color(0, 0, 0, fadeCount);
            yield return wait;
        }

        MoveScene("Main");

        StopCoroutine(FadeCoroutine());
        yield break;
    }

    //Scene 이동
    private void MoveScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
