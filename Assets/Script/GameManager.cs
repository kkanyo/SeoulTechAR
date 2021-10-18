using System.Collections;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//Unity Design Patterns - Singleton
public class GameManager : MonoBehaviour
{
    #region Singleton
    private static GameManager instance = null;

    public string bdNumSelected;
    public int currentCanvasNum = 0;
    public bool isNavigate = false;

    #endregion

    public Image panel;

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
            // 카메라 및 위치 접근권한
            while (!Permission.HasUserAuthorizedPermission(Permission.FineLocation)
                    && !Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                Permission.RequestUserPermission(Permission.FineLocation);
                Permission.RequestUserPermission(Permission.Camera);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(FadeCoroutine());
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
    }
}
