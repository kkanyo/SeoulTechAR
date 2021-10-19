using System.Collections;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    #region Singleton
    private static GameManager instance = null;

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
            
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }

        if (!Application.isEditor)
        {
            StartCoroutine(PermissionCheckCoroutine());
        }
        else
        {
            StartCoroutine(FadeCoroutine());
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

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


    IEnumerator PermissionCheckCoroutine()
    {
        var waitFrame = new WaitForEndOfFrame();
        var waitSec = new WaitForSeconds(0.2f);
        var waitFocus = new WaitUntil(() => Application.isFocused == true);
        
        yield return waitFrame;

        if (Permission.HasUserAuthorizedPermission(Permission.Camera) == false)
        {
            Permission.RequestUserPermission(Permission.Camera);

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
    }

    public void MoveScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        StopCoroutine(FadeCoroutine());
    }
}
