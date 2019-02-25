using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRStandardAssets.Utils;
using TMPro;

public class OptionButtonController : MonoBehaviour {

	// Represents the button type to know how to treat it
	public enum ButtonType
	{
		Music,
		Voices,
		SoundEffects,
		Messages,
		Subtitles
	}
	[SerializeField] private ButtonType m_ButtonType;					// Represents the button type

	[SerializeField] private SelectionRadial m_SelectionRadial;         // Optional reference to the SelectionRadial, if non-null the duration of the SelectionRadial will be used instead.
	[SerializeField] private VRInteractiveItem m_InteractiveItem;       // Reference to the VRInteractiveItem to determine when to fill the bar.

	[SerializeField] private AudioSource m_GazeAudio;                   // Reference to the audio source that will play effects when the user looks at it and when it fills
	[SerializeField] private AudioClip m_OnOverClip;                    // The clip to play when the user looks at the bar
	[SerializeField] private AudioClip m_OnClickClip;					// The clip to play when the user clicks an option button

	[SerializeField] private PauseMenuController m_MenuOptions;									// Reference to the pause menu controller
	private bool m_SoundEffectsEnabled { get { return m_MenuOptions.m_SoundEffectsEnabled; } }	// Gets the sound effects option from the menu
	private Color m_EnabledColor { get { return m_MenuOptions.m_EnabledColor; } }				// Gets the enabled color from the menu
	private float m_AroundAlpha { get { return m_MenuOptions.m_AroundAlpha; } }					// Gets the around alpha from the menu

	private bool m_GazeOver;                                            // Whether the user is looking at the VRInteractiveItem currently.
	private Image m_ButtonImage;										// Image component of the button



	// Called when the script instance is being loaded
	void Awake ()
	{
		m_ButtonImage = GetComponent<Image> ();
	}


	// Called on the frame when the script is enabled
	void Start ()
	{
		m_GazeOver = false;
		m_ButtonImage.color = m_EnabledColor;
	}


	// Called when the object becomes enabled and active
	void OnEnable ()
	{
		m_InteractiveItem.OnOver += HandleOver;
		m_InteractiveItem.OnOut += HandleOut;
		m_InteractiveItem.OnClick += HandleClick;
	}


	// Called when the behaviour becomes disabled
	void OnDisable ()
	{
		m_InteractiveItem.OnOver -= HandleOver;
		m_InteractiveItem.OnOut -= HandleOut;
		m_InteractiveItem.OnClick -= HandleClick;

		HandleOut ();
	}


	// Called when the user points to the button
	private void HandleOver ()
	{
		m_GazeOver = true;
		m_SelectionRadial.Show ();

		// Sets the button color to the same color, but highlighted
		m_ButtonImage.color = new Color (m_ButtonImage.color.r, m_ButtonImage.color.g, m_ButtonImage.color.b, m_AroundAlpha);

		// If the interaction sounds option is enabled...
		if (m_SoundEffectsEnabled)
		{
			// ... and the clip is not already playing, plays the clip
			if (!m_GazeAudio.isPlaying) {
				m_SelectionRadial.Show ();

				m_GazeAudio.clip = m_OnOverClip;
				m_GazeAudio.Play ();
			}
		}
	}


	// Called when the user ends pointing to the object
	private void HandleOut ()
	{
		m_GazeOver = false;

		// Sets the button color to the non highlighted color
		m_ButtonImage.color = new Color (m_ButtonImage.color.r, m_ButtonImage.color.g, m_ButtonImage.color.b);

		m_SelectionRadial.Hide ();
	}


	// Called when the user clicks on the button
	private void HandleClick ()
	{
		// Depending on the button type, it will enable/disable one or other option
		switch (m_ButtonType)
		{
			case ButtonType.Music:
			{
				m_MenuOptions.SwipeMusic (m_AroundAlpha);
				break;
			}
			case ButtonType.Voices:
			{
				m_MenuOptions.SwipeVoices (m_AroundAlpha);
				break;
			}
			case ButtonType.SoundEffects:
			{
				m_MenuOptions.SwipeInteractionSounds (m_AroundAlpha);
				break;
			}
			case ButtonType.Messages:
			{
				m_MenuOptions.SwipeMessages (m_AroundAlpha);
				break;
			}
			case ButtonType.Subtitles:
			{
				m_MenuOptions.SwipeSubtitles (m_AroundAlpha);
				break;
			}
			default:
			{
				break;
			}
		}

		// If the interaction sounds option is enabled, plays the on click clip
		if (m_SoundEffectsEnabled)
		{
			m_GazeAudio.clip = m_OnClickClip;
			m_GazeAudio.Play ();
		}
		// The above conditional is after the switch to give the user a good sense
		// when he/she enables or disables the interaction sounds option:
		// - If the user enables the option, it will play the sound indicating it is enabled.
		// - If the user disables the option, it will not play the sound indicating it is disabled.
	}
}
