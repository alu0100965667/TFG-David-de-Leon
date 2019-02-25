using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using VRStandardAssets.Utils;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayTransitionInteraction : MonoBehaviour {

	[SerializeField] private string m_SceneToLoad;
	[SerializeField] private Image m_FadeImage;                     	// Reference to the image that covers the screen.
	[SerializeField] private Color m_FadeColor;       					// The colour the image fades out to.
	[SerializeField] private float m_FadeDuration = 2.0f;           	// How long it takes to fade in seconds.

	private Color m_FadeOutColor;                                   	// This is a opaque version of the fade colour, it will ensure fading looks normal.

	[SerializeField] private SelectionRadial m_SelectionRadial;         // Optional reference to the SelectionRadial, if non-null the duration of the SelectionRadial will be used instead.
	[SerializeField] private VRInteractiveItem m_InteractiveItem;       // Reference to the VRInteractiveItem to determine when to fill the bar.

	[SerializeField] private AudioSource m_GazeAudio;                   // Reference to the audio source that will play effects when the user looks at it and when it fills.
	[SerializeField] private AudioClip m_OnOverClip;                    // The clip to play when the user looks at the bar.
	[SerializeField] private AudioClip m_OnFilledClip;                  // The clip to play when the bar finishes filling.
	[SerializeField] private TextMeshProUGUI m_Messages;				// Reference to the GUI where the messages are displayed

	[SerializeField]  private PauseMenuController m_MenuOptions;								// Reference to the options menu
	private bool m_MusicEnabled { get { return m_MenuOptions.m_MusicEnabled; } }				// Gets the music option from the menu
	private bool m_VoicesEnabled { get { return m_MenuOptions.m_VoicesEnabled; } }				// Gets the voices option from the menu
	private bool m_SoundEffectsEnabled { get { return m_MenuOptions.m_SoundEffectsEnabled; } }	// Gets the interaction sounds option from the menu
	private bool m_MessagesEnabled { get { return m_MenuOptions.m_MessagesEnabled; } }			// Gets the mesages option from the menu
	private bool m_PauseMenuIsActive { get { return m_MenuOptions.m_PauseMenu.activeSelf; } }	// Gets the pause menu active

	private bool m_GazeOver;                            // Whether the user is pointing at the VRInteractiveItem currently

	[SerializeField] private string m_DefaultText;		// String for the default texts
	[SerializeField] private string m_InteractMessage;	// String for the interact messages



	// Called when the script instance is being loaded
	void Awake ()
	{
		m_FadeOutColor = new Color(m_FadeColor.r, m_FadeColor.g, m_FadeColor.b, 1f);

		m_Messages.text = m_DefaultText;

		m_GazeOver = false;
	}
	

	// Called when the object becomes enabled and active
	void OnEnable ()
	{
		m_InteractiveItem.OnOver += HandleOver;
		m_InteractiveItem.OnOut += HandleOut;
		m_SelectionRadial.OnSelectionComplete += HandleSelectionComplete;
	}


	// Called when the behaviour becomes disabled
	void OnDisable ()
	{
		m_InteractiveItem.OnOver -= HandleOver;
		m_InteractiveItem.OnOut -= HandleOut;
		m_SelectionRadial.OnSelectionComplete -= HandleSelectionComplete;

		HandleOut ();
	}


	// Called when the user points to the object
	private void HandleOver ()
	{
		// If the options menu is not active...
		if (!m_PauseMenuIsActive)
		{

			m_GazeOver = true;
			m_SelectionRadial.Show ();

			// If the messages option is enabled, displays the interact message
			if (m_MessagesEnabled)
			{
				m_Messages.text = m_InteractMessage;
			}

			// If the interaction sounds option is enabled...
			if (m_SoundEffectsEnabled)
			{
				// ... and the clip is not already playing, plays the clip
				if (!m_GazeAudio.isPlaying)
				{
					m_GazeAudio.clip = m_OnOverClip;
					m_GazeAudio.Play ();
				}
			}
		}
	}


	// Called when the user ends pointing to the object
	private void HandleOut ()
	{
		m_GazeOver = false;
		m_SelectionRadial.Hide ();
		// If the messages option is enabled, deletes the message
		if (m_MessagesEnabled)
		{
			m_Messages.text = m_DefaultText;
		}
	}


	// Called when the user completes the interaction selection
	private void HandleSelectionComplete ()
	{
		// If the options menu is not active...
		if (!m_PauseMenuIsActive)
		{
			if (m_GazeOver)
			{
				// If the messages option is enabled, deletes the message
				if (m_MessagesEnabled)
				{
					m_Messages.text = "CARGANDO ESCENA...";
				}

				if (m_SoundEffectsEnabled)
				{
					m_GazeAudio.clip = m_OnFilledClip;
					m_GazeAudio.Play ();
				}

				StartCoroutine (BeginFadeOut (m_FadeColor, m_FadeOutColor, m_FadeDuration));
				//SceneManager.LoadScene (m_SceneToLoad);
			}
		}
	}


	private IEnumerator BeginFadeOut (Color startCol, Color endCol, float duration)
	{
		// Execute this loop once per frame until the timer exceeds the duration.
		float timer = 0f;
		while (timer <= duration)
		{
			// Set the colour based on the normalised time.
			m_FadeImage.color = Color.Lerp(startCol, endCol, timer / duration);

			// Increment the timer by the time between frames and return next frame.
			timer += Time.deltaTime;
			yield return null;
		}

		SceneManager.LoadScene (m_SceneToLoad);
	}


	private IEnumerator LoadAsyncScene ()
	{
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync (m_SceneToLoad);

		while (!asyncLoad.isDone)
		{
			m_Messages.text = "CARGANDO ESCENA...";
			yield return null;
		}
	}
}
