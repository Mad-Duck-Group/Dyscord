using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DG.Tweening;
using Dyscord.ScriptableObjects.VN;
using Dyscord.UI;
using NaughtyAttributes;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityRandom = UnityEngine.Random;

namespace Dyscord.Managers
{
	public enum CharacterPosition
	{
		Left,
		Right
	}
	
	public struct Dialogue
	{
		public string speaker;
		public string msg;
		public CharacterPosition characterPosition;
		
		public Dialogue(string speaker, string msg, CharacterPosition characterPosition)
		{
			this.speaker = speaker;
			this.msg = msg;
			this.characterPosition = characterPosition;
		}
	}

	public class VNManager : PersistentMonoSingleton<VNManager>
	{
		[SerializeField] private GraphicRaycaster VNraphicRaycaster;
		[SerializeField] private CanvasGroup vnPanel;
		[SerializeField] private Image characterLeft;
		[SerializeField] private Image characterRight;
		[SerializeField] private Image background;
		[SerializeField] private ChatBubbleUI chatBubbleUIPrefab;
		[SerializeField] private ScrollRect chatScrollRect;
		[SerializeField] private LayoutGroup contentLayoutGroup;
		[SerializeField] private Button historyButton;
		[SerializeField] private Button skipButton;
		[SerializeField] private VNClickableArea clickableArea;

		public delegate void VNFinished(VNPathSO vnPathSO);
		public static event VNFinished OnVNFinished;
		
		private VNPathSO currentVNPath;
		private List<Dialogue> dialogues = new List<Dialogue>();
		private List<ChatBubbleUI> chatBubbles = new List<ChatBubbleUI>();
		private int currentDialogueIndex;
		private Color originalBackgroundColor;
		private Image scrollRectImage;
		private Tween panelFadeTween;
		private Tween fadeTween;
		private Tween chatBubbleTween;
		private Tween delayTween;
		private float originalAlpha;
		private bool showingHistory;
		private bool sceneLoaded;
		private bool playing;
		private bool firstChatAdded;
		public bool Playing => playing;
		
		private List<GraphicRaycaster> graphicRaycasters = new List<GraphicRaycaster>();

		protected override void Awake()
		{
			base.Awake();
			SceneManager.sceneLoaded += (scene, mode) => OnSceneLoaded();
			originalBackgroundColor = background.color;
		}
		private void Start()
		{
			historyButton.onClick.AddListener(() => ToggleHistory(!showingHistory));
			skipButton.onClick.AddListener(CloseVN);
			scrollRectImage = chatScrollRect.GetComponent<Image>();
			chatScrollRect.vertical = false;
			originalAlpha = scrollRectImage.color.a;
			scrollRectImage.color = new Color(1f, 1f, 1f, 0f);
			vnPanel.gameObject.SetActive(false);
		}

		private void OnSceneLoaded()
		{
			sceneLoaded = true;
			if (playing && !firstChatAdded)
			{
				delayTween = DOVirtual.DelayedCall(1f, () => StartCoroutine(AddChatBubble(dialogues[currentDialogueIndex])));
				firstChatAdded = true;
			}
		}

		public void NextChatBubble()
		{
			if (panelFadeTween.IsActive()) return;
			if (showingHistory) return;
			if (chatBubbleTween.IsActive()) return;
			if (!firstChatAdded) return;
			if (currentDialogueIndex >= dialogues.Count)
			{
				CloseVN();
				return;
			}
			StartCoroutine(AddChatBubble(dialogues[currentDialogueIndex]));
		}

		public IEnumerator AddChatBubble(Dialogue dialogue)
		{
			ChatBubbleUI chatBubble = Instantiate(chatBubbleUIPrefab, chatScrollRect.content);
			contentLayoutGroup.childAlignment = dialogue.characterPosition == CharacterPosition.Left
				? TextAnchor.UpperRight
				: TextAnchor.UpperLeft;
			chatBubble.Setup(dialogue);
			chatBubbles.Add(chatBubble);
			chatBubble.transform.localScale = Vector3.zero;
			LayoutRebuilder.ForceRebuildLayoutImmediate(chatBubble.transform as RectTransform);
			yield return new WaitForEndOfFrame();
			LayoutRebuilder.ForceRebuildLayoutImmediate(chatBubble.transform as RectTransform);
			chatBubbleTween = chatBubble.transform.DOScale(Vector3.one, 0.2f);
			Color gray = new Color(0.5f, 0.5f, 0.5f);
			Color white = Color.white;
			Debug.Log(dialogue.characterPosition);
			Color leftColor = dialogue.characterPosition == CharacterPosition.Left ? white : gray;
			Color rightColor = dialogue.characterPosition == CharacterPosition.Right ? white : gray;
			characterLeft.DOColor(leftColor, 0.2f);
			characterRight.DOColor(rightColor, 0.2f);
			currentDialogueIndex++;
			GlobalSoundManager.Instance.PlayBubbleSFX();
			FadeBubble();
		}

		private void FadeBubble()
		{
			for (int i = chatBubbles.Count - 1; i >= 0; i--)
			{
				if (i == chatBubbles.Count - 2)
				{
					chatBubbles[i].SetAlpha(0.5f);
				}
				if (i < chatBubbles.Count - 2)
				{
					chatBubbles[i].SetAlpha(0f);
				}
			}
		}

		private void ToggleHistory(bool active)
		{
			if (panelFadeTween.IsActive()) return;
			showingHistory = active;
			if (!active)
			{
				chatScrollRect.verticalNormalizedPosition = 1;
				chatScrollRect.vertical = false;
				clickableArea.gameObject.SetActive(true);
				if (fadeTween.IsActive())
					fadeTween.Kill();
				fadeTween = scrollRectImage.DOFade(0f, 0.2f);
				FadeBubble();
			}
			else
			{
				chatScrollRect.vertical = true;
				clickableArea.gameObject.SetActive(false);
				if (fadeTween.IsActive())
					fadeTween.Kill();
				fadeTween = scrollRectImage.DOFade(originalAlpha, 0.2f);
				foreach (var bubble in chatBubbles)
				{
					bubble.SetAlpha(1f);
				}
			}
		}
		
		public void ShowVN(VNPathSO vnPathSO)
		{
			if (panelFadeTween.IsActive()) return;
			OnFailToLoadDialogueFile += () => Debug.LogError("Failed to load dialogue file");
			OnSuccessToLoadDialogueFile += OnSuccess;
			StartCoroutine(LoadDialogueFile(vnPathSO));
		}

		private void OnSuccess()
		{
			vnPanel.gameObject.SetActive(true);
			vnPanel.alpha = 0;
			panelFadeTween = vnPanel.DOFade(1f, 0.2f);
			characterLeft.sprite = currentVNPath.CharacterLeft;
			characterRight.sprite = currentVNPath.CharacterRight;
			if (currentVNPath.Background)
			{
				background.sprite = currentVNPath.Background;
				background.color = Color.white;
			}
			playing = true;
			if (sceneLoaded && !firstChatAdded)
			{
				delayTween = DOVirtual.DelayedCall(1f, () => StartCoroutine(AddChatBubble(dialogues[currentDialogueIndex])));
				firstChatAdded = true;
			}
			//Find all graphics raycaster and disable them except the VN graphic raycaster
			graphicRaycasters = FindObjectsOfType<GraphicRaycaster>().ToList();
			foreach (var graphicRaycaster in graphicRaycasters)
			{
				graphicRaycaster.enabled = graphicRaycaster == VNraphicRaycaster;
			}
		}

		private delegate void FailToLoadDialogueFile();
		private static event FailToLoadDialogueFile OnFailToLoadDialogueFile;
		
		private delegate void SuccessToLoadDialogueFile();
		private static event SuccessToLoadDialogueFile OnSuccessToLoadDialogueFile;
		private IEnumerator LoadDialogueFile(VNPathSO vnPathSO)
		{
			if (vnPathSO.TextAsset == null)
			{
				OnFailToLoadDialogueFile?.Invoke();
				yield break;
			}
			string[] lines = vnPathSO.TextAsset.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
			dialogues.Clear();
			if (lines.Length == 0)
			{
				OnFailToLoadDialogueFile?.Invoke();
				yield break;
			}
			foreach (var line in lines)
			{
				string pattern = ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)";
				string[] values = Regex.Split(line, pattern);
				values = values.Select(x => x.Trim('"')).ToArray();
				CharacterPosition position = values[2] == "Left" ? CharacterPosition.Left : CharacterPosition.Right;
				Dialogue dialogue = new Dialogue(values[0], values[1], position);
				dialogues.Add(dialogue);
			}
			currentVNPath = vnPathSO;
			OnSuccessToLoadDialogueFile?.Invoke();
		}

		public void CloseVN()
		{
			if (panelFadeTween.IsActive()) return;
			if (delayTween.IsActive())
				delayTween.Kill();
			foreach (var bubble in chatBubbles)
			{
				Destroy(bubble.gameObject);
			}
			Color gray = new Color(0.5f, 0.5f, 0.5f);
			characterLeft.DOColor(gray, 0.2f);
			characterRight.DOColor(gray, 0.2f);
			chatBubbles.Clear();
			currentDialogueIndex = 0;
			firstChatAdded = false;
			playing = false;
			panelFadeTween = vnPanel.DOFade(0f, 0.2f).OnComplete(() => vnPanel.gameObject.SetActive(false));
			foreach (var graphicRaycaster in graphicRaycasters)
			{
				if (graphicRaycaster)
					graphicRaycaster.enabled = true;
			}
			background.sprite = null;
			background.color = originalBackgroundColor;
			graphicRaycasters.Clear();
			OnVNFinished?.Invoke(currentVNPath);
		}
	}
}