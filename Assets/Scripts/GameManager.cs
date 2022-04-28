using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameObject faderObject;

    public Image faderImg;
    private float fadeSpeed = .02f;

    private Color fadeTransparency = new Color(0, 0, 0, .04f);
    private string currScene;

    public bool gameOver;
    public bool gamePause;

    private AsyncOperation async;
    // Start is called before the first frame update
    void Awake()
    {
        gameOver = false;
        gamePause = false;

        UnPauseGame();

        if (instance == null)
        {
            DontDestroyOnLoad(gameObject);

            instance = GetComponent<GameManager>();
            SceneManager.sceneLoaded += OnLevelFinishedLoading;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        currScene = scene.name;
        instance.StartCoroutine(FadeIn(instance.faderObject, instance.faderImg));
    }

    public IEnumerator FadeIn(GameObject faderObject, Image fader)
    {
        faderObject.SetActive(true);
        while (fader.color.a > 0)
        {
            fader.color -= fadeTransparency;
            yield return new WaitForSeconds(fadeSpeed);
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
        gamePause = true;
    }

    public void UnPauseGame()
    {
        Time.timeScale = 1;
        gamePause = false;
    }

    public void LoadScene(string sceneName)
    {
        instance.StartCoroutine(Load(sceneName));
        instance.StartCoroutine(FadeOut(instance.faderObject, instance.faderImg));
    }

    public void ReloadScene()
    {
        Debug.Log("Clicked");
        LoadScene(SceneManager.GetActiveScene().name);
        ActivateScene();
    }
    public IEnumerator FadeOut(GameObject faderObject, Image fader)
    {
        faderObject.SetActive(true);
        while (fader.color.a < 1)
        {
            fader.color += fadeTransparency;
            yield return new WaitForSeconds(fadeSpeed);
        }
        
        instance.StartCoroutine(FadeIn(instance.faderObject, instance.faderImg));

        //Activate the scene when the fade ends
    }
    public void ActivateScene()
    {
        async.allowSceneActivation = true;
    }

    // Get the current scene name
    public string CurrentSceneName
    {
        get { return currScene; }
    }

    IEnumerator Load(string sceneName)
    {
        async = SceneManager.LoadSceneAsync(sceneName);
        async.allowSceneActivation = false;
        yield return async;
    }

    public void ExitGame()
    {
        // If we are running in a standalone build of the game
    #if UNITY_STANDALONE
        // Quit the application
        Application.Quit();
    #endif

        // If we are running in the editor
    #if UNITY_EDITOR
        // Stop playing the scene
        UnityEditor.EditorApplication.isPlaying = false;
    #endif
    }

}
