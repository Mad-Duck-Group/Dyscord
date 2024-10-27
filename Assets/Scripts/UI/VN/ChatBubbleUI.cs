using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Dyscord.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityRandom = UnityEngine.Random;

namespace Dyscord.UI
{
	public class ChatBubbleUI : MonoBehaviour
	{
		[SerializeField] private TMP_Text speakerName;
		[SerializeField] private TMP_Text text;
		[SerializeField] private Image background;
		[SerializeField] LayoutGroup layoutGroup;
		
		private Sequence sequence;

		public void Setup(Dialogue dialogue)
		{
			speakerName.text = dialogue.speaker;
			layoutGroup.childAlignment = dialogue.characterPosition == CharacterPosition.Left ? TextAnchor.UpperLeft : TextAnchor.UpperRight;
			text.text = dialogue.msg;
		}
		
		public void SetAlpha(float alpha)
		{
			if (sequence.IsActive())
				sequence.Kill();
			sequence = DOTween.Sequence();
			sequence.Append(background.DOFade(alpha, 0.2f));
			sequence.Join(speakerName.DOFade(alpha, 0.2f));
			sequence.Join(text.DOFade(alpha, 0.2f));
		}
	}
}