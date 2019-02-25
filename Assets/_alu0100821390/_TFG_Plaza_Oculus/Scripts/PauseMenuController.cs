using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRStandardAssets.Utils;
using TMPro;

public class PauseMenuController : MonoBehaviour {

	[HideInInspector] public bool m_Active;										// Represents wether the pause menu controller is active or not

	[HideInInspector] public bool m_MusicEnabled { get; private set; }			// Represents wether the music option is enabled or not
	[HideInInspector] public bool m_VoicesEnabled { get; private set; }			// Represents wether the voices option is enabled or not
	[HideInInspector] public bool m_SoundEffectsEnabled { get; private set; }	// Represents wether the sound effects option is enabled or not
	[HideInInspector] public bool m_MessagesEnabled { get; private set; }		// Represents wether the messages option is enabled or not
	[HideInInspector] public bool m_SubtitlesEnabled { get; private set; }		// Represents wether the subtitles option is enabled or not

	[HideInInspector] public Color m_EnabledColor { get; private set; }			// Represents the enabled button color
	[HideInInspector] public Color m_DisabledColor { get; private set; }		// Represents the disbaled button color
	[HideInInspector] public float m_AroundAlpha { get; private set; }			// Represents the level of transparency around the pause menu

	[SerializeField] private Color m_DefaultEnabledColor;						// Reference to the default enabled button color
	[SerializeField] private Color m_DefaultDisabledColor;						// Reference to the default disabled button color
	[SerializeField] private float m_DefaultAroundAlpha;						// Reference to the default level of transparency around the pause menu

	[SerializeField] private VRCameraFade m_CameraFade;							// Reference to the camera fade script
	public GameObject m_PauseMenu;												// Reference to the pause menu GameObject

	[SerializeField] private Image m_MusicOptionButtonImage;					// Reference to the music option button image
	[SerializeField] private Image m_VoicesOptionButtonImage;					// Reference to the voices option button image
	[SerializeField] private Image m_SoundEffectsOptionButtonImage;				// Reference to the sound effects button image
	[SerializeField] private Image m_MessagesOptionButtonImage;					// Reference to the messages option button image
	[SerializeField] private Image m_SubtitlesOptionButtonImage;				// Reference to the subtitles option button image

	[SerializeField] private TextMeshProUGUI m_MusicOptionButtonText;			// Reference to the music option button text
	[SerializeField] private TextMeshProUGUI m_VoicesOptionButtonText;			// Reference to the voices option button text
	[SerializeField] private TextMeshProUGUI m_SoundEffectsOptionButtonText;	// Reference to the sound effects button text
	[SerializeField] private TextMeshProUGUI m_MessagesOptionButtonText;		// Reference to the messages option button text
	[SerializeField] private TextMeshProUGUI m_SubtitlesOptionButtonText;		// Reference to the subtittles option button text

	[SerializeField] private AudioSource m_AmbientMusic;						// Reference to the ambient music audio source



	// Called when the script instance is being loaded
	void Awake () {
		m_Active = true;

		m_MusicEnabled = true;
		m_VoicesEnabled = true;
		m_SoundEffectsEnabled = true;
		m_MessagesEnabled = true;
		m_SubtitlesEnabled = true;

		m_EnabledColor = m_DefaultEnabledColor;
		m_DisabledColor = m_DefaultDisabledColor;
		m_AroundAlpha = m_DefaultAroundAlpha;

		m_PauseMenu.SetActive (false);
	}
	

	// Called every frame
	void Update () {
		// If the panel controller is active and the user presses the "back" button, enables/disables the pause menu
		if (m_Active && OVRInput.Get (OVRInput.Button.Back))
		{
			m_CameraFade.FadePause ();
			m_PauseMenu.SetActive (!m_PauseMenu.activeSelf);
		}
	}


	// Swipes the music option button value
	public void SwipeMusic (float alpha)
	{
		m_MusicEnabled = !m_MusicEnabled;

		// If the music option has been activated, plays the ambient music and sets the button to ON
		if (m_MusicEnabled)
		{
			m_AmbientMusic.Play ();
			m_MusicOptionButtonText.text = "ON";
			m_MusicOptionButtonImage.color = new Color (m_EnabledColor.r , m_EnabledColor.g, m_EnabledColor.b, alpha);
		}
		// If th emusic option has been deactivated, stops the ambient music and sets the button to OFF
		else
		{
			m_AmbientMusic.Stop ();
			m_MusicOptionButtonText.text = "OFF";
			m_MusicOptionButtonImage.color = new Color (m_DisabledColor.r , m_DisabledColor.g, m_DisabledColor.b, alpha);
		}
	}


	// Swipes the voices option button value
	public void SwipeVoices (float alpha)
	{
		m_VoicesEnabled = !m_VoicesEnabled;

		// If the voices option has been activated, sets the button to ON
		if (m_VoicesEnabled)
		{
			m_VoicesOptionButtonText.text = "ON";
			m_VoicesOptionButtonImage.color = new Color (m_EnabledColor.r , m_EnabledColor.g, m_EnabledColor.b, alpha);
		}
		// If the voices option has been deactivated, sets the button to OFF and checks the subtitles option value
		else
		{
			m_VoicesOptionButtonText.text = "OFF";
			m_VoicesOptionButtonImage.color = new Color (m_DisabledColor.r , m_DisabledColor.g, m_DisabledColor.b, alpha);

			// If the subtitles option is also deactivated, swipes it
			if (!m_SubtitlesEnabled)
			{
				SwipeSubtitles (1f);
			}
		}
	}


	// 
	public void SwipeInteractionSounds (float alpha)
	{
		m_SoundEffectsEnabled = !m_SoundEffectsEnabled;

		// If the interaction sounds option has been activated, sets the button to ON
		if (m_SoundEffectsEnabled)
		{
			m_SoundEffectsOptionButtonText.text = "ON";
			m_SoundEffectsOptionButtonImage.color = new Color (m_EnabledColor.r , m_EnabledColor.g, m_EnabledColor.b, alpha);
		}
		// If the interaction sounds option has been deactivated, sets the button to OFF
		else
		{
			m_SoundEffectsOptionButtonText.text = "OFF";
			m_SoundEffectsOptionButtonImage.color = new Color (m_DisabledColor.r , m_DisabledColor.g, m_DisabledColor.b, alpha);
		}
	}


	// Swipes the messages option button value
	public void SwipeMessages (float alpha)
	{
		m_MessagesEnabled = !m_MessagesEnabled;

		// If the messages option has been activated, sets the button to ON
		if (m_MessagesEnabled)
		{
			m_MessagesOptionButtonText.text = "ON";
			m_MessagesOptionButtonImage.color = new Color (m_EnabledColor.r , m_EnabledColor.g, m_EnabledColor.b, alpha);
		}
		// If the messages option has been deactivated, sets the button to OFF
		else
		{
			m_MessagesOptionButtonText.text = "OFF";
			m_MessagesOptionButtonImage.color = new Color (m_DisabledColor.r , m_DisabledColor.g, m_DisabledColor.b, alpha);
		}
	}


	// Swipes the subtitles option button value
	public void SwipeSubtitles (float alpha)
	{
		m_SubtitlesEnabled = !m_SubtitlesEnabled;

		// If the subtitles option has been activated, sets the button to ON
		if (m_SubtitlesEnabled)
		{
			m_SubtitlesOptionButtonText.text = "ON";
			m_SubtitlesOptionButtonImage.color = new Color (m_EnabledColor.r , m_EnabledColor.g, m_EnabledColor.b, alpha);
		}
		// If the subtitles option has been deactivated, sets the button to OFF and checks the voices option value
		else
		{
			m_SubtitlesOptionButtonText.text = "OFF";
			m_SubtitlesOptionButtonImage.color = new Color (m_DisabledColor.r , m_DisabledColor.g, m_DisabledColor.b, alpha);

			// If the voices option is also deactivated, swipes it
			if (!m_VoicesEnabled)
			{
				SwipeVoices (1f);
			}
		}
	}



}
