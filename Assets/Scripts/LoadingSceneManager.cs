using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class LoadingSceneManager : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private float fadeOutTime = 1.5f;
    [SerializeField] private Ease fadeEase = Ease.InQuint;
    
    private AsyncOperation _asyncOperation;
    private Color _originalColor;
    private Tween _fadeTween;
    private float _timer;
    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1;
        StartCoroutine(LoadScene());
    }

    private IEnumerator LoadScene()
    {
        string nextScene = SceneManagerPersistent.Instance.NextScene;
        LoadSceneMode loadSceneMode = SceneManagerPersistent.Instance.LoadSceneMode;
        Scene thisScene = SceneManager.GetActiveScene();
        _asyncOperation = SceneManager.LoadSceneAsync(nextScene, loadSceneMode);
        
        _asyncOperation.allowSceneActivation = false;
        while (_asyncOperation.progress < 0.9f)
        {
            yield return null;
        }
        _asyncOperation.allowSceneActivation = true;
        //SoundManager.Instance.PlaySoundFX(SoundFXTypes.SceneTransition, out _);
        _fadeTween = background.DOFade(0f, fadeOutTime).SetEase(fadeEase).SetUpdate(true);
        while (_timer < fadeOutTime)
        {
            _timer += Time.unscaledDeltaTime;
            yield return null;
        }
        while (_fadeTween.IsActive())
        {
            yield return null;
        }
        if (loadSceneMode == LoadSceneMode.Additive)
        {
            SceneManager.UnloadSceneAsync(thisScene);
        }
        yield return null;
    }

    [Button("Test Ease")]
    private void TestEase()
    {
        if (_fadeTween.IsActive())
            _fadeTween.Kill();
        if (_originalColor == default)
            _originalColor = background.color;
        background.color = _originalColor;
        //SoundManager.Instance.PlaySoundFX(SoundFXTypes.SceneTransition, out _);
        _fadeTween = background.DOColor(new Color(0, 0, 0, 0), fadeOutTime).SetEase(fadeEase).SetUpdate(true);
    }
}
