using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dyscord.ScriptableObjects.VN;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityRandom = UnityEngine.Random;

namespace Dyscord.Managers
{
	public enum VNTypes
	{
		HQ,
		Shop,
		SouthStreet
	}
	public class ProgressionManager : PersistentMonoSingleton<ProgressionManager>
	{
		[SerializeField] private VNPathSO hqVN;
		[SerializeField] private VNPathSO shopVN;
		[SerializeField] private VNPathSO southStreetVN;
		[SerializeField] private bool hqVNPlayed;
		[SerializeField] private bool shopVNPlayed;
		[SerializeField] private bool southStreetVNPlayed;
		
		public void PlayVN(VNTypes vnType)
		{
			switch (vnType)
			{
				case VNTypes.HQ when !hqVNPlayed:
					VNManager.Instance.ShowVN(hqVN);
					hqVNPlayed = true;
					break;
				case VNTypes.Shop when !shopVNPlayed:
					VNManager.Instance.ShowVN(shopVN);
					shopVNPlayed = true;
					break;
				case VNTypes.SouthStreet when !southStreetVNPlayed:
					VNManager.Instance.ShowVN(southStreetVN);
					southStreetVNPlayed = true;
					break;
			}
		}
	}
}