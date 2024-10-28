using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microlight.MicroAudio;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.Serialization;
using UnityRandom = UnityEngine.Random;

namespace Dyscord.Managers
{
	public enum BGMTypes
	{
		MainMenu,
		HQ,
		Gameplay,
	}
	
	public enum UISFXTypes
	{
		Cancel,
		HardMouseClick,
		HardMouseHover,
		SoftMouseClick,
		SoftMouseHover,
		Popup,
		Swoosh,
		PressAnyKey,
		Unavailable,
	}
	public class GlobalSoundManager : MonoSingleton<GlobalSoundManager>
	{
		[Header("BGM")]
		[SerializeField] private MicroSoundGroup bgmGroup;
		
		[Header("VN")]
		[SerializeField] private AudioClip bubbleSFX;
		
		[Header("UI")]
		[SerializeField] private AudioClip cancelSFX;
		[SerializeField] private AudioClip hardMouseClickSFX;
		[SerializeField] private AudioClip hardMouseHoverSFX;
		[SerializeField] private AudioClip softMouseClickSFX;
		[SerializeField] private AudioClip softMouseHoverSFX;
		[SerializeField] private AudioClip popupSFX;
		[SerializeField] private AudioClip swooshSFX;
		[SerializeField] private AudioClip pressAnyKeySFX;
		[SerializeField] private AudioClip unavailableSFX;
		
		public void PlayBGM(BGMTypes bgmType, bool crossfade = true)
		{
			if (crossfade && MicroAudio.MusicAudioSource.isPlaying)
			{
				float beforeVolume = MicroAudio.MusicAudioSource.volume;
				SoundFade fade = new SoundFade(MicroAudio.MusicAudioSource, MicroAudio.MusicAudioSource.volume, 0f, 1f);
				fade.OnFadeEnd += (SoundFade fade) =>
				{
					MicroAudio.StopMusic();
					MicroAudio.MusicAudioSource.volume = beforeVolume;
					MicroAudio.PlayOneTrack(bgmGroup.ClipList[(int)bgmType]);
				};
			}
			else
			{
				MicroAudio.PlayOneTrack(bgmGroup.ClipList[(int)bgmType]);
			}
		}
		
		public void StopBGM(bool fade = true)
		{
			if (fade)
			{
				float beforeVolume = MicroAudio.MusicAudioSource.volume;
				SoundFade soundFade = new SoundFade(MicroAudio.MusicAudioSource, MicroAudio.MusicAudioSource.volume, 0f, 0.5f);
				soundFade.OnFadeEnd += (SoundFade fade) =>
				{
					MicroAudio.StopMusic();
					MicroAudio.MusicAudioSource.volume = beforeVolume;
				};
			}
			else
			{
				MicroAudio.StopMusic();
			}
		}
		
		public void PlayBubbleSFX()
		{
			MicroAudio.PlayUISound(bubbleSFX);
		}
		
		public void PlayUISFX(UISFXTypes sfxType)
		{
			switch (sfxType)
			{
				case UISFXTypes.Cancel:
					MicroAudio.PlayUISound(cancelSFX);
					break;
				case UISFXTypes.HardMouseClick:
					MicroAudio.PlayUISound(hardMouseClickSFX);
					break;
				case UISFXTypes.HardMouseHover:
					MicroAudio.PlayUISound(hardMouseHoverSFX);
					break;
				case UISFXTypes.Unavailable:
					MicroAudio.PlayUISound(unavailableSFX);
					break;
				case UISFXTypes.SoftMouseClick:
					MicroAudio.PlayUISound(softMouseClickSFX);
					break;
				case UISFXTypes.SoftMouseHover:
					MicroAudio.PlayUISound(softMouseHoverSFX);
					break;
				case UISFXTypes.Popup:
					MicroAudio.PlayUISound(popupSFX);
					break;
				case UISFXTypes.Swoosh:
					MicroAudio.PlayUISound(swooshSFX);
					break;
				case UISFXTypes.PressAnyKey:
					MicroAudio.PlayUISound(pressAnyKeySFX);
					break;
			}
		}
		
		public void PlayEffectClip(AudioClip clip)
		{
			MicroAudio.PlayEffectSound(clip);
		}
	}
}