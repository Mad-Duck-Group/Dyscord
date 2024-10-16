using System;

namespace TaylorMadeCode.FreeAudioManager
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    using TaylorMadeCode.Core;
    
    [AddComponentMenu("Taylor Made Code/Audio/Free Sound Manager")]
    public class TMC_Sound_Manager : TMC_MonoBehaviour
    {
        /// <summary>
        /// The primary AudioSource that will be managed by the sound manager.
        /// </summary>
        public AudioSource m_Audio;

        /// <summary>
        /// A list of AudioSources, used if multiple audio sources are to be managed.
        /// </summary>
        public List<AudioSource> m_MultipleAudio = new List<AudioSource>();

        /// <summary>
        /// The desired volume level for the AudioSource(s).
        /// </summary>
        public float mf_WantedVolume = 1;
        
        /// <summary>
        /// The minimum time range for random delays when looping sounds.
        /// </summary>
        public float mf_LoopTimeRangeStart = 0;

        /// <summary>
        /// The maximum time range for random delays when looping sounds.
        /// </summary>
        public float mf_LoopTimeRangeEnd = 2.0f;

        /// <summary>
        /// The minimum pitch range for pitch modulation.
        /// </summary>
        public float mf_PitchRangeStart = 1.0f;

        /// <summary>
        /// The maximum pitch range for pitch modulation.
        /// </summary>
        public float mf_PitchRangeEnd = 2.0f;

        /// <summary>
        /// The percentage of the clip's length at which fade out should start.
        /// </summary>
        public float mf_FadeOutStart = 75f;

        /// <summary>
        /// The speed at which the audio fades out.
        /// </summary>
        public float mf_SpeedOfFadeOut = 5.0f;

        /// <summary>
        /// Internal flag indicating if the fade out has been triggered.
        /// </summary>
        private bool mb_FirstFadeOutTrigger = false;

        /// <summary>
        /// The percentage of the clip's length at which fade in should end.
        /// </summary>
        public float mf_FadeInEnd = 25f;

        /// <summary>
        /// The speed at which the audio fades in.
        /// </summary>
        public float mf_SpeedOfFadeIn = 5.0f;

        /// <summary>
        /// Internal flag indicating if the fade in has been triggered.
        /// </summary>
        private bool mb_FirstFadeInTrigger = false;
        
        /// <summary>
        /// Index of the audio clip to play from the list of multiple audio sources.
        /// </summary>
        private int mi_AudioClipToPlay = -1;

        /// <summary>
        /// Reference to the currently running coroutine for managing fades.
        /// </summary>
        private Coroutine m_currentlyRunningCoroutine = null;

        /// <summary>
        /// Internal flag indicating if the audio has finished playing.
        /// </summary>
        private bool mb_AudioFinished = false;

        /// <summary>
        /// Internal flag indicating if audio playback is desired.
        /// </summary>
        private bool mb_AudioPlaybackWanted = false;

        /// <summary>
        /// Represents whether to use randomised play for determining when to start. Set GUI or Code, Just needs to be before the respected call selected in WhenToStart
        /// </summary>
        public bool mb_UseRandomisedPlayForWhenToStart = false;
        
        /// <summary>
        /// Starting Function that is called for the When To Start Option.
        /// </summary>
        public override void StartFunction()
        {
            if (mb_UseRandomisedPlayForWhenToStart)
                PlayWithNewRandomizedEffects();
            else
                Play();
        }
        
        /// <summary>
        /// Initializes the sound manager and validates configuration. Checks if the correct audio source is provided.
        /// Logs an error if configurations are invalid.
        /// </summary>
        private new void Start()
        {
            if (TMC.GurantieeGetDictonary(m_UIChoices, "Singular Audio File_Title_ScriptButtonSelected"))
            {
                if (GetCorrectAudioSource() == null)
                {
                    enabled = false;
                    Debug.LogError("Singular Audio Source is selected but the audio source has not been provided. Please provide Free Sound Manager a script");
                    return;
                }
            }
            
            //If the Multi Input is selected, Then and only then start the random range checks
            if (TMC.GurantieeGetDictonary(m_UIChoices, "Multiple Audio File_Title_ScriptButtonSelected"))
            {
                //If there is no Audio Clip to play, then we need to randomly select one. This is only when nothing has previously been selected.
                if (mi_AudioClipToPlay == -1)
                    mi_AudioClipToPlay = Random.Range(0, m_MultipleAudio.Count);
                
                //If we then can get a null then flag that there is an issue with the no Audio Clips added.
                if (GetCorrectAudioSource() == null)
                {
                    enabled = false;
                    Debug.LogError("No audio clip added to " + this.gameObject.name + " Audio Source component. Please add one to correct functionality");
                    return;
                }
            }

            // If both Unity AudioSource and Taylor Made Code script are set to loop, warn the user to disable one to prevent unexpected behavior.
            if (GetCorrectAudioSource().loop && TMC.GurantieeGetDictonary(m_UIChoices, "Delayed Sound Loop Control_Title_ScriptButtonSelected", false) == true)
                Debug.LogWarning("Warning, Taylor Made Code Free Sound Manager and Unity's AudioSource are both trying to loop audio. Please disable looping on one of the scripts to ensure no unexpected behavior", this);
            
            // In case the volume has been changed in the audio source, save it before the start of the manager.
            mf_WantedVolume = GetCorrectAudioSource().volume;
            
            base.Start();
        }
        
        /// <summary>
        /// Updates the sound manager each frame. Handles random pitch alteration, fade in/out, and loop controls.
        /// </summary>
        private void Update()
        {
            if (!GetCorrectAudioSource().isPlaying && mb_AudioPlaybackWanted && TMC.GurantieeGetDictonary(m_UIChoices, "Delayed Sound Loop Control_Title_ScriptButtonSelected", false))
            {
                mb_FirstFadeInTrigger = true;
                mb_FirstFadeOutTrigger = true;

                if (TMC.GurantieeGetDictonary(m_UIChoices, "Multiple Audio File_Title_ScriptButtonSelected", false))
                    mi_AudioClipToPlay = Random.Range(0, m_MultipleAudio.Count);
                
                if (TMC.GurantieeGetDictonary(m_UIChoices, "Delayed Sound Loop Control_Title_ScriptButtonSelected", false))
                    GetCorrectAudioSource().PlayDelayed(Random.Range(mf_LoopTimeRangeStart, mf_LoopTimeRangeEnd));
                else
                    GetCorrectAudioSource().Play();
                
                // When looping a new element, invoke the during events.
                m_ScriptDuringEvents.Invoke();
                
                if (TMC.GurantieeGetDictonary(m_UIChoices, "Random Pitch_Title_ScriptButtonSelected", false))
                    GetCorrectAudioSource().pitch = Random.Range(mf_PitchRangeStart, mf_PitchRangeEnd);
                else
                    GetCorrectAudioSource().pitch = 1.0f;
            }
            // When the audio has stopped playing and the loop feature isn't on, invoke ending events.
            else if(GetCorrectAudioSource().isPlaying == false && mb_AudioFinished == false)
            {
                mb_AudioFinished = true;
                m_ScriptEndingEvents.Invoke();
            }

            // Handle fade in effect.
            if (TMC.GurantieeGetDictonary(m_UIChoices, "Fade In_Title_ScriptButtonSelected", false) &&
                GetCorrectAudioSource().time <= GetCorrectAudioSource().clip.length /  (mf_FadeInEnd/100) &&
                mb_FirstFadeInTrigger == true)
            {
                if (m_currentlyRunningCoroutine != null)
                    StopCoroutine(m_currentlyRunningCoroutine);
                
                // Determine the target volume based on whether fade out is also active.
                float lf_TargetVolume = (TMC.GurantieeGetDictonary(m_UIChoices, "Fade Out_Title_ScriptButtonSelected", false)) ?  mf_WantedVolume : GetCorrectAudioSource().volume;
                m_currentlyRunningCoroutine = StartCoroutine(FadeAudioSource.StartFade(GetCorrectAudioSource(), (GetCorrectAudioSource().clip.length / (mf_FadeInEnd/100)), 0.01f, lf_TargetVolume, mf_SpeedOfFadeIn)); 
                mb_FirstFadeInTrigger = false;
            }

            // Handle fade out effect.
            if (TMC.GurantieeGetDictonary(m_UIChoices, "Fade Out_Title_ScriptButtonSelected", false) &&
                GetCorrectAudioSource().time >= GetCorrectAudioSource().clip.length * (mf_FadeOutStart/100) &&
                mb_FirstFadeOutTrigger == true)
            {
                if (m_currentlyRunningCoroutine != null)
                    StopCoroutine(m_currentlyRunningCoroutine);
                
                // Start the fade out on the current audio level.
                m_currentlyRunningCoroutine = StartCoroutine(FadeAudioSource.StartFade(GetCorrectAudioSource(), (GetCorrectAudioSource().clip.length * (mf_FadeOutStart/100)), GetCorrectAudioSource().volume, 0.01f, mf_SpeedOfFadeOut));
                mb_FirstFadeOutTrigger = false;
            }
        }

        /// <summary>
        /// Starts audio playback with randomized effects such as pitch and delay.
        /// This method also triggers the necessary events and controls.
        /// </summary>
        /// <example>
        /// Example usage:
        /// <code>
        /// TMC_Sound_Manager soundManager = gameObject.GetComponent&lt;TMC_Sound_Manager&gt;();
        /// soundManager.PlayWithNewRandomizedEffects();
        /// </code>
        /// </example>
        public void PlayWithNewRandomizedEffects()
        {
            mb_AudioPlaybackWanted = true;
            mb_AudioFinished = false;
            
            // Turn on the triggers for fade in and out.
            mb_FirstFadeInTrigger = true;
            mb_FirstFadeOutTrigger = true;

            // Check for multiple audio files.
            if (TMC.GurantieeGetDictonary(m_UIChoices, "Multiple Audio File_Title_ScriptButtonSelected", false))
                mi_AudioClipToPlay = Random.Range(0, m_MultipleAudio.Count);

            // Play audio with delay if the option is selected.
            if (TMC.GurantieeGetDictonary(m_UIChoices, "Delayed Sound Loop Control_Title_ScriptButtonSelected", false))
                GetCorrectAudioSource().PlayDelayed(Random.Range(mf_LoopTimeRangeStart, mf_LoopTimeRangeEnd));
            else
                GetCorrectAudioSource().Play();

            // Randomize pitch if the option is selected.
            if (TMC.GurantieeGetDictonary(m_UIChoices, "Random Pitch_Title_ScriptButtonSelected", false))
                GetCorrectAudioSource().pitch = Random.Range(mf_PitchRangeStart, mf_PitchRangeEnd);
            else
                GetCorrectAudioSource().pitch = 1.0f;

            m_ScriptStartingEvents.Invoke();
        }

        /// <summary>
        /// Directly plays the audio associated with this manager without any additional effects.
        /// </summary>
        /// <example>
        /// Example usage:
        /// <code>
        /// TMC_Sound_Manager soundManager = gameObject.GetComponent&lt;TMC_Sound_Manager&gt;();
        /// soundManager.Play();
        /// </code>
        /// </example>
        public void Play()
        {
            // Check if Play is one of the listeners in m_ScriptStartingEvents
            bool containsPlayMethod = false;
            for (int i = 0; i < m_ScriptStartingEvents.GetPersistentEventCount(); i++)
            {
                if (m_ScriptStartingEvents.GetPersistentTarget(i) == this &&
                    m_ScriptStartingEvents.GetPersistentMethodName(i) == nameof(Play))
                {
                    containsPlayMethod = true;
                    break;
                }
            }

            if (containsPlayMethod)
            {
                Debug.LogWarning("Play method is registered in m_ScriptStartingEvents. Skipping invocation to prevent infinite loop.");
                return;
            }

            mb_AudioPlaybackWanted = true;
            mb_AudioFinished = false;

            GetCorrectAudioSource().Play();
            m_ScriptStartingEvents.Invoke();
        }


        /// <summary>
        /// Pauses the current audio playback.
        /// </summary>
        /// <example>
        /// Example usage:
        /// <code>
        /// TMC_Sound_Manager soundManager = gameObject.GetComponent&lt;TMC_Sound_Manager&gt;();
        /// soundManager.Pause();
        /// </code>
        /// </example>
        public void Pause()
        {
            mb_AudioPlaybackWanted = false;
            GetCorrectAudioSource().Pause();
            m_ScriptDuringEvents.Invoke();
        }

        /// <summary>
        /// Stops the current audio playback immediately.
        /// </summary>
        /// <example>
        /// Example usage:
        /// <code>
        /// TMC_Sound_Manager soundManager = gameObject.GetComponent&lt;TMC_Sound_Manager&gt;();
        /// soundManager.Stop();
        /// </code>
        /// </example>
        public void Stop()
        {
            mb_AudioPlaybackWanted = false;
            mb_AudioFinished = false;
            GetCorrectAudioSource().Stop();
            m_ScriptEndingEvents.Invoke();
        }

        /// <summary>
        /// Retrieves the correct audio source, either singular or multiple.
        /// </summary>
        /// <returns>The correct AudioSource to play.</returns>
        private AudioSource GetCorrectAudioSource()
        {
            //Cheap and easy way to check if Multi source is being used
            if (mi_AudioClipToPlay != -1)
            {
                //Check that its in bounds of the array, else return null
                //Error checks are not carried out here as GetCorrectAudioSource is a basic function to improve other codes readability. 
                if (mi_AudioClipToPlay < m_MultipleAudio.Count)
                    return m_MultipleAudio[mi_AudioClipToPlay];
                else
                    return null;
            }
            return m_Audio;
        }

        /// <summary>
        /// Sets the volume for the audio source(s) and synchronizes with the desired volume.
        /// Recommended to be used when Fade In and Fade Out is active.
        /// </summary>
        /// <param name="af_WantedVolume">The desired volume level.</param>
        /// <example>
        /// Example usage:
        /// <code>
        /// TMC_Sound_Manager soundManager = gameObject.GetComponent&lt;TMC_Sound_Manager&gt;();
        /// soundManager.SetVolume(0.5f);
        /// </code>
        /// </example>
        public void SetVolume(float af_WantedVolume)
        {
            // Save the new volume as the wanted volume.
            mf_WantedVolume = af_WantedVolume;
            
            // Setting volume for multiple audio sources.
            foreach (AudioSource l_audio in m_MultipleAudio)
                l_audio.volume = af_WantedVolume;
            
            // Setting volume for singular audio source.
            m_Audio.volume = af_WantedVolume;
        }
    }

    /// <summary>
    /// A static class that handles fade in and out effects for AudioSources.
    /// </summary>
    internal static class FadeAudioSource
    {
        /// <summary>
        /// Lerps the volume of the audio source to provide a fade in/out effect.
        /// </summary>
        /// <param name="a_AudioSource">AudioSource to fade.</param>
        /// <param name="af_Duration">Duration of the fade.</param>
        /// <param name="af_StartVolume">Starting volume of the fade.</param>
        /// <param name="af_TargetVolume">Target volume of the fade.</param>
        /// <param name="af_Speed">Speed at which to fade.</param>
        /// <returns>IEnumerator for coroutine management.</returns>
        /// <example>
        /// Example usage:
        /// <code>
        /// StartCoroutine(FadeAudioSource.StartFade(audioSource, 1.0f, 0.0f, 1.0f, 0.5f));
        /// </code>
        /// </example>
        public static IEnumerator StartFade(AudioSource a_AudioSource, float af_Duration, float af_StartVolume, float af_TargetVolume, float af_Speed)
        {
            float lf_CurrentTime = 0;
            a_AudioSource.volume = af_StartVolume;

            while (lf_CurrentTime < af_Duration)
            {
                lf_CurrentTime += af_Speed * Time.deltaTime;
                a_AudioSource.volume = Mathf.Lerp(af_StartVolume, af_TargetVolume, lf_CurrentTime / af_Duration);
                yield return null;
            }
            yield break;
        }
    }
}
