@mainpage Free Sound Manager

## Free Sound Manager Documentation

---

## About Free Sound Manager

Free Sound Manager adds additional audio functionality to unity. These are: 
- Fade In
- Fade Out
- Random Pitch
- Delayed Loop Playback
- Random Audio Selection
Using these combination of features allow you take your game audio to another layer. Without having to re-record or by more sound assets.
The easiest method is to select the random pitch. This tweaks the audio to be higher or lower pitch. This slight change makes it sound like audio file. 

Free Sound Manager was designed from the ground up to quickly and easily fit into any project as well as to be used by anyone at any skill level.
This is achieved by the use of UnityEvents (A method to call code via GUI), this is the same system the base UI button use for on click. This is also done by the Free Sound Manager only altering the Provided audio source. This means that Free Sound Manager is less likely to have conflicts with custom code and other asset store scripts. Even if there are conflicts can quickly be resolved. If you need help with any conflicts let us know via any of our social medias discord server or via the email all which can be found on our unity asset store publisher page.

---

## Using Free Sound Manger

   Using Free Sound Manager is very simple, 
   </br>First add the script to any game object you want. We recommend adding it to the same object as the audio source, However this is not required.
   </br>Once added you should see the setup screen, Click the SetupScript button.
   </br>Next you need to select the wanted features you want to have, By default a singular Audio source option is selected.
   </br>Free Sound Manager needs to have Singular Audio File or Multiple Audio File selected. This is forced.
   
   The other features can be selected and combined in any fashion you want. These features are:
   
   - Delayed Sound Loop Control
   - Random Pitch
   - Fade In
   - Fade Out
   
   To get the basic functionality working expand the Singular audio file section of the GUI. Or the Multiple Audio File section
   Within these sections you need to provide the audio source that Free Sound Manager will control. For multiple you can add as many as you want but a minimum of one is required Add the audio source componenet to the "Audio To Control" or the "Multiple Audio" Inputs
   
   Free Sound Manager will now play the provided audio once play is called. You can check this via explaniding the Controller GUI Section, and clicking on either the Randomised Play Button or Play Button.
   
   Free Sound Manager can now be setup to fade audio in / out, Randomise Pitch and Delay Audio Loop Playback.
   </br>

   #### Setting Section, contains one setting
   - "When To Start" - This instructs Free Sound Manager if and when to start playback automaticly
     - "Dont Start Automatically" - The code will not start playback automatically
     - "Start" - This will start playback when the script received the Start function call.
     - "Awake" - This will start playback when the script received the Awake function call.
     - "On Enable" - This will start playback when the script received the On Enable function call.
     - "On Disable" - This will start playback when the script received the on Disable function call.
       </br>

   #### Delayed Sound Loop Control Feature has two settings,
   - "Minimum Amount Of Delay In Seconds" - This is the minimum amount of time between looped playback 
   - "Maximum Amount Of Delay In Seconds" - This is the maximum amount of time between looped playback
   </br>
   
   #### Random Pitch Setting has two settings,
   - "Random Pitch Range Start" - This can be any value between -3 to 3. We dont recommend going lower than 0.03 since you struggle to hear any sound below this value
   - "Random Pitch Range End" - This values can be any value between -3 to 3 but needs to be larger than the value set for "Random Pitch Start" setting. Again we dont recommend going below 0.03 since you will struggle to hear any sound below this value
   </br>

   #### Fade In Feature has Two Settings, 
   - "Fade in Ends at X% of the audio clips duration" - This is how long through the fade in takes. One this point has been reached the volume is fixed at the current level.
   - "Speed Of Fade In" - This is the speed on how fast the volume goes from 0 -> 1. If the value is high or low enough enough the fade in can be completed before the previous given fade in or not be fully complete. So make sure to try a few values out to get the fade in just right 

   #### Fade Out Feature has Two Settings,
   - "Fade Out starts at X% of the audio clips duration" - This is when in the audio playback fade out should start. The length of fadeout and how gradual it is tied to the "Speed of fade out value". This values is just when the logic behind the fade in should start.
   - "Speed Of Fade Out" - This controls the speed of fadeout. Too low and the fade out wont be completed before the end of the audio value, Too large and the fade out effect wont be noticeable. 
   </br>

---

## Technical details

### Requirements
- Free Sound Manager is compatible with unity Versions:
   - 2021.3 and later
- Required Taylor Made Code Core V2.1.0+

### Known limitations
- Free Sound Manager Version 1.0.0 includes the following issues :
   - The GUI for the sound volume in the control panel will not update if the volume is altered directly in the Audio Source component - To Fix just select a new game object so the inspector changes and then select the object that has the Free Sound Manager attached this will update the Gui to match

### Package Content
   | File Directory                                                            | Description                                                                                                                                 |
   |---------------------------------------------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------|
   | Art                                                                       | Directory for art asset to be stored in                                                                                                     |
   | Art/Sprite                                                                | Directory for the Sprite Assets to be store in                                                                                              |
   | Art/Sprites/TMC_FREE_AUDIO_MANAGER.png                                    | The Products Icon that is used in the script in the top left                                                                                |
   | Art/Sounds/0.74 Secconds.ogg                                              | One of the audio clips used for the demo scenes                                                                                             |
   | Art/Sounds/1.38 Secconds.ogg                                              | One of the audio clips used for the demo scenes                                                                                             |
   | Art/Sounds/1.74 Secconds.ogg                                              | One of the audio clips used for the demo scenes                                                                                             |
   | Demo/Free Sound Manager Demo Scene - Delayed Loop & Random Pitch.unity    | Unity Scene to demonstrate the use of the Free Sound Manager Script. Specifically Delayed Loop and Random Pitch Feature                   |
   | Demo/Free Sound Manager Demo Scene - Multi Source & Fade In and Out.unity | Unity Scene to demonstrate the use of the Free Sound Manager Script. Specifically Multi Source and Fade In / Fade Out Features            |
   | Documentation/Documentation.pdf                                           | Contains information about the Asset (About the Script, How to use the script, Requirements, Known limitations and technical documentation) |
   | Documentation/Documentation.md                                            | Contains information about the Asset (About the Script, How to use the script, Requirements, Known limitations and technical documentation) |
   | Editor                                                                    | Folder that contains all the Editor Only Code within                                                                                        |
   | Editor/TaylorMadeCode.Free Sound Manager.Editor.asmdef                    | The assembly definition file for the editor scripts used for this product. These are used to improve user build time                       |
   | Editor/TMC_Sound_Manager_Editor.Editor.cs                                 | This is the actual Custom editor code used for this asset.                                                                                  |
   | Scripts                                                                   | The scripts folder contains the .cs files that handes the functionality of the asset.                                                       |
   | Scripts/TaylorMadeCode.Free Sound Manager.asmdef                          | The assembly definition file for the non editor code.                                                                                       |
   | Scripts/TMC_Sound_Manager.cs                                              | File that contains all the code for the countdown                                                                                           |

### Class
Class is called TMC_Sound_Manager and inherits from TMC_Monobehaviour 
(TMC_Monobehaviour supports the when to start and custom GUI)

- #### Functions

   | Scope   | Return Value  | Name                         | Description                                                                                                                                                           |
   |---------|---------------|------------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------|
   | public  | void          | StartFunction                | Code needed to start the script (This function is used for the automatic start on Awake, start, enable and disable)                                                   |
   | private | void          | Update                       | Update loop called by unity                                                                                                                                           |
   | public  | void          | PlayWithNewRandomizedEffects | When this function is called the selected randomized options will be re-done. These are the "Delayed Sound Loop", "Random Pitch" and "Multiple Audio File" features   |
   | public  | void          | Play                         | This function will just start playback without any changes from the previous playback. Perfect for when the audio is paused and it needs to be picked back up!        |
   | public  | void          | Pause                        | This function pauses audio Playback                                                                                                                                   |
   | public  | void          | Stop                         | This function causes the audio playback to be stopped. This will cause the audio track to be picked back up from the start of the audio clip when play is next called |
   | private | AudioSource   | GetCorrectAudioSource        | This function is used to make the code easier to read and just returns the correct audioSource component depending on if the Multiple Audio Files are selected        |

- #### Variables

   | Scope                | namespace                         | Type                     | Name                    | Description                                                                                                                                                 |
   |----------------------|-----------------------------------|--------------------------|-------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------|
   | public               | TaylorMadeCode.FreeAudioManager | AudioSource              | m_Audio                 | The AudioSource that is used when the feature "Multiple Audio File" is not selected                                                                         |
   | public               | TaylorMadeCode.FreeAudioManager | List<AudioSource>        | m_MultipleAudio         | The multiple audioSources that is used when the feature "Multiple Audio File" is selected, from this list the audio source to use is randomly selected      |
   | public               | TaylorMadeCode.FreeAudioManager | float                    | mf_Volume               | float that stores the current volume for of the AudioFileSelected GUI                                                                                       |
   | public               | TaylorMadeCode.FreeAudioManager | float                    | mf_LoopTimeRangeStart   | The lowest value of the loop playback time delay                                                                                                            |
   | public               | TaylorMadeCode.FreeAudioManager | float                    | mf_LoopTimeRangeEnd     | The highest value of the loop playback time delay                                                                                                           |
   | public               | TaylorMadeCode.FreeAudioManager | float                    | mf_PitchRangeStart      | The lowest value of the Pitch can can be randomly selected                                                                                                  |
   | public               | TaylorMadeCode.FreeAudioManager | float                    | mf_PitchRangeEnd        | The Highest value of the Pitch can can be randomly selected                                                                                                 |
   | public               | TaylorMadeCode.FreeAudioManager | float                    | mf_FadeOutStart         | The time that the audio starts to fade out by in a percentage. So a fade out for a 1 minute clip occurring at 75% will start at 0:45 in the audio recording |
   | public               | TaylorMadeCode.FreeAudioManager | float                    | mf_SpeedOfFadeOut       | The speed of the fade out. This is a linear progression starting at the mf_FadeOutStart Percentage                                                          |
   | public               | TaylorMadeCode.FreeAudioManager | float                    | mf_FadeInEnd            | The time that the audio is fully faded in by in a percentage. So a fade int for a 1 minute clip occurring at 25% will be fully faded in by 0:15             |
   | public               | TaylorMadeCode.FreeAudioManager | float                    | mf_SpeedOfFadeIn        | The speed of the fade in. This is a linear progression starting at 0.                                                                                       |
   | private              | TaylorMadeCode.FreeAudioManager | bool                     | mb_FirstFadeOutTrigger  | Boolien to represent whether the fade out should occur                                                                                                      |
   | private              | TaylorMadeCode.FreeAudioManager | bool                     | mb_FirstFadeInTrigger   | The gameObject that the background is attached to                                                                                                           |
   | private              | TaylorMadeCode.FreeAudioManager | float                    | mf_InitialVolume        | The inital volume that the AudioSource was set to before it was randomized.                                                                                 |
   | private              | TaylorMadeCode.FreeAudioManager | int                      | mi_AudioClipToPlay      | The selected AudioSource to use for the "Multiple Audio File" feature                                                                                       |
   | public (inherited)   | TaylorMadeCode.Core               | bool                     | mb_HasBeenSetup         | Tracks if the UI has been setup for not. This will automatically be toggled by the custom set GUI if present otherwise it needs to be manually set.         |
   | public (inherited)   | TaylorMadeCode.Core               | WhenToStart              | me_StartScriptOn        | Readability friendly method to when the script will start.                                                                                                  |
   | public (inherited)   | TaylorMadeCode.Core               | UnityEvent               | m_ScriptStartingEvents  | Unity events allow you to trigger other code/components when Free AI Patrol Starts.                                                                       |
   | public (inherited)   | TaylorMadeCode.Core               | UnityEvent               | m_ScriptDuringEvents    | Unity events allow you to trigger other code/components when the patrolling ai reaches a User Placed node.                                                  |
   | public (inherited)   | TaylorMadeCode.Core               | UnityEvent               | m_ScriptEndingEvents    | Unity events allow you to trigger other code/components when Free AI Patrol Stops.                                                                        |
   | public (inherited)   | TaylorMadeCode.Core               | Dictionary<string, bool> | m_UIChoices             | Dictionary that is used to access the settings set in the custom TMC UI System.                                                                             |
   | public (inherited)   | TaylorMadeCode.Core               | string                   | ms_UIJson               | A string that gets Json saved to it to allow the Custom UI state to be saved. This is used in editor and play mode and once in a final executable file.     |

---

#### Document Revision History
   | Date       | Version | Reason                                                                                                                                                                                                                                                                                                                                                       |
   |------------|---------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
   | 22/11/2023 | V1.0.0  | Initial Creation                                                                                                                                                                                                                                                                                                                                             |
   | 11/08/2024 | V1.0.1  | Updated Documentation and Support for all Render Pipelines, Multiple small fixes like Improved array OOB handeling, Fixed an issue where the multi source would not randomly select a source when delayed loop was selected. Added setting to allow the user to have the WhenToStart setting to trigger RandomisedPlay insted of the standard play function. |
---