using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using UnityRandom = UnityEngine.Random;

namespace Dyscord.ScriptableObjects.VN
{
	[CreateAssetMenu(menuName = "VN/VNPathSO", fileName = "VNPathSO")]
	public class VNPathSO : ScriptableObject
	{
		[SerializeField] private string filePath;
		[SerializeField][ShowAssetPreview(128, 128)] private Sprite characterLeft;
		[SerializeField][ShowAssetPreview(128, 128)] private Sprite characterRight;
		
		public string FilePath => filePath;
		public Sprite CharacterLeft => characterLeft;
		public Sprite CharacterRight => characterRight;
		
		[Button("Select File")]
		private void SelectFile()
		{
			filePath = UnityEditor.EditorUtility.OpenFilePanel("Select Dialogue File", "", "csv");
		}
	}
}