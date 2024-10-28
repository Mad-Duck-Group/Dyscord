using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityRandom = UnityEngine.Random;

namespace Dyscord.Managers
{
	public class MainMenuManager : MonoSingleton<MainMenuManager>
	{
		[SerializeField] private Image logo;
		[SerializeField] private Image pressAnyKey;
		private Tween logoTween;
		private Tween pressAnyKeyTween;

		private void OnEnable()
		{
			SceneManagerPersistent.OnFinishFadeOut += KillTweens;
		}
		
		private void OnDisable()
		{
			SceneManagerPersistent.OnFinishFadeOut -= KillTweens;
		}
		
		private void KillTweens()
		{
			logoTween.Kill();
			pressAnyKeyTween.Kill();
		}

		private void Start()
		{
			logoTween = logo.transform.DOShakePosition(5f, 1, 30, fadeOut: false).SetLoops(-1, LoopType.Restart);
			pressAnyKeyTween = pressAnyKey.DOFade(0, 0.5f).SetLoops(-1, LoopType.Yoyo);
			GlobalSoundManager.Instance.PlayBGM(BGMTypes.MainMenu);
		}
		
		private void Update()
		{
			if (Input.anyKeyDown)
			{
				GlobalSoundManager.Instance.PlayUISFX(UISFXTypes.PressAnyKey);
				SceneManagerPersistent.Instance.LoadNextScene(SceneTypes.HQ, LoadSceneMode.Additive, false);
			}
		}
	}
}