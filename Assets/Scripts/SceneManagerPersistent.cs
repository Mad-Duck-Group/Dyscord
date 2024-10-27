using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using NaughtyAttributes;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif


public enum SceneTypes
{
    MainMenu,
    HQ,
    GamePlay
}


[System.Serializable]
public class SceneField
{
    [SerializeField]
    private Object m_SceneAsset;

    [SerializeField]
    private string m_SceneName = "";
    public string SceneName
    {
        get { return m_SceneName; }
    }

    // makes it work with the existing Unity methods (LoadLevel/LoadScene)
    public static implicit operator string( SceneField sceneField )
    {
        return sceneField.SceneName;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(SceneField))]
public class SceneFieldPropertyDrawer : PropertyDrawer 
{
    public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
    {
        EditorGUI.BeginProperty(_position, GUIContent.none, _property);
        SerializedProperty sceneAsset = _property.FindPropertyRelative("m_SceneAsset");
        SerializedProperty sceneName = _property.FindPropertyRelative("m_SceneName");
        _position = EditorGUI.PrefixLabel(_position, GUIUtility.GetControlID(FocusType.Passive), _label);
        if (sceneAsset != null)
        {
            sceneAsset.objectReferenceValue = EditorGUI.ObjectField(_position, sceneAsset.objectReferenceValue, typeof(SceneAsset), false); 

            if( sceneAsset.objectReferenceValue != null )
            {
                sceneName.stringValue = (sceneAsset.objectReferenceValue as SceneAsset).name;
            }
        }
        EditorGUI.EndProperty( );
    }
}
#endif
public class SceneManagerPersistent : PersistentMonoSingleton<SceneManagerPersistent>
{
    [Header("Scenes")]
    [SerializeField] private SceneField mainMenu;
    [SerializeField] private SceneField hq;
    [SerializeField] private SceneField gamePlay;
    [SerializeField] private SceneField loadingScene;

    [Header("Fade")] 
    [SerializeField] private Image background;
    [SerializeField] private float fadeOutTime = 1.5f;
    [SerializeField] private Ease fadeOutEase = Ease.OutQuint;
    [SerializeField] private Ease fadeInEase = Ease.InQuint;
    
    public delegate void FinishFadeOut();
    public static event FinishFadeOut OnFinishFadeOut;
    
    private Tween _fadeTween;

    private AsyncOperation _asyncOperation;
    public string NextScene { get; private set; }
    public LoadSceneMode LoadSceneMode { get; private set; }

    public void LoadNextScene(SceneTypes sceneType, LoadSceneMode loadSceneMode, bool useLoadingScene)
    {
        if ((_asyncOperation != null && !_asyncOperation.isDone) || _fadeTween.IsActive()) return;
        string sceneName;
        switch (sceneType)
        {
            case SceneTypes.MainMenu:
                sceneName = mainMenu;
                break;
            case SceneTypes.HQ:
                sceneName = hq;
                break;
            case SceneTypes.GamePlay:
                sceneName = gamePlay;
                break;
            default:
                sceneName = mainMenu;
                break;
        }
        NextScene = sceneName;
        LoadSceneMode = loadSceneMode;
        _fadeTween = background.DOFade(1f, fadeOutTime).SetEase(fadeOutEase).SetUpdate(true).OnComplete(() =>
        {
            OnFinishFadeOut?.Invoke();
            if (useLoadingScene)
            {
                background.color = new Color(0, 0, 0, 0);
                SceneManager.LoadScene(loadingScene);
            }
            else
            {
                StartCoroutine(LoadSceneAsync());
            }
        });
    }

    private IEnumerator LoadSceneAsync()
    {
        Scene thisScene = SceneManager.GetActiveScene();
        SceneManager.activeSceneChanged += UnloadScene;
        _asyncOperation = SceneManager.LoadSceneAsync(NextScene, LoadSceneMode);
        _asyncOperation.allowSceneActivation = false;
        while (_asyncOperation.progress < 0.9f)
        {
            yield return null;
        }
        _asyncOperation.allowSceneActivation = true;
        SceneManager.sceneLoaded += (scene, mode) => SceneManager.SetActiveScene(scene);
        yield return null;
    }

    private void UnloadScene(Scene lastScene, Scene current)
    {
        Debug.Log("Unloading " + lastScene.name);
        if (LoadSceneMode == LoadSceneMode.Additive)
        {
            SceneManager.UnloadSceneAsync(lastScene);
        }
        _fadeTween = background.DOFade(0f, fadeOutTime).SetEase(fadeInEase).SetUpdate(true);
        SceneManager.activeSceneChanged -= UnloadScene;
    }
    
}
