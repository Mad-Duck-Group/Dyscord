using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Dyscord.ScriptableObjects.VN;
using Dyscord.UI;
using NaughtyAttributes;
using UnityCommunity.UnitySingleton;
using UnityEngine;
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
		[SerializeField] private CanvasGroup vnPanel;
		[SerializeField] private Image characterLeft;
		[SerializeField] private Image characterRight;
		[SerializeField] private ChatBubbleUI chatBubbleUIPrefab;
		[SerializeField] private ScrollRect chatScrollRect;
		[SerializeField] private LayoutGroup contentLayoutGroup;
		[SerializeField] private Button historyButton;
		[SerializeField] private Button skipButton;
		[SerializeField] private VNClickableArea clickableArea;

		private List<Dialogue> dialogues = new List<Dialogue>();
		private List<ChatBubbleUI> chatBubbles = new List<ChatBubbleUI>();
		private int currentDialogueIndex;
		private Image scrollRectImage;
		private Tween panelFadeTween;
		private Tween fadeTween;
		private Tween chatBubbleTween;
		private float originalAlpha;
		private bool showingHistory;

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

		public void NextChatBubble()
		{
			if (panelFadeTween.IsActive()) return;
			if (showingHistory) return;
			if (chatBubbleTween.IsActive()) return;
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
			chatBubbleTween = chatBubble.transform.DOScale(Vector3.one, 0.2f);
			Color gray = new Color(0.5f, 0.5f, 0.5f);
			Color white = Color.white;
			Debug.Log(dialogue.characterPosition);
			Color leftColor = dialogue.characterPosition == CharacterPosition.Left ? white : gray;
			Color rightColor = dialogue.characterPosition == CharacterPosition.Right ? white : gray;
			characterLeft.DOColor(leftColor, 0.2f);
			characterRight.DOColor(rightColor, 0.2f);
			currentDialogueIndex++;
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
			if (string.IsNullOrEmpty(vnPathSO.FilePath)) return;
			string[] lines = File.ReadAllLines(vnPathSO.FilePath);
			dialogues.Clear();
			foreach (var line in lines)
			{
				string[] values = line.Split(',');
				CharacterPosition position = values[2] == "Left" ? CharacterPosition.Left : CharacterPosition.Right;
				Dialogue dialogue = new Dialogue(values[0], values[1], position);
				dialogues.Add(dialogue);
			}
			vnPanel.gameObject.SetActive(true);
			vnPanel.alpha = 0;
			panelFadeTween = vnPanel.DOFade(1f, 0.2f);
			characterLeft.sprite = vnPathSO.CharacterLeft;
			characterRight.sprite = vnPathSO.CharacterRight;
			DOVirtual.DelayedCall(1f, () => StartCoroutine(AddChatBubble(dialogues[currentDialogueIndex])));
		}
		
		public void CloseVN()
		{
			if (panelFadeTween.IsActive()) return;
			foreach (var bubble in chatBubbles)
			{
				Destroy(bubble.gameObject);
			}
			chatBubbles.Clear();
			currentDialogueIndex = 0;
			panelFadeTween = vnPanel.DOFade(0f, 0.2f).OnComplete(() => vnPanel.gameObject.SetActive(false));
		}
	}
}