using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using Mono.Data.Sqlite;
using UnityEngine;
using VRStandardAssets.Utils;
using TMPro;


public class PlayInteraction : MonoBehaviour {

	private bool dbOpening;							//
	private string dbPath;							//
	private string dbStrConnection;					//
	private string dbFileName;						//
	[SerializeField] private string dbTableName;	// Database parameters
													//
	private IDbConnection dbConnection;				//
	private IDbCommand dbCommand;					//
	private IDataReader dbReader;					//


	[SerializeField] private SelectionRadial m_SelectionRadial;         // Optional reference to the SelectionRadial, if non-null the duration of the SelectionRadial will be used instead.
	[SerializeField] private VRInteractiveItem m_InteractiveItem;       // Reference to the VRInteractiveItem to determine when to fill the bar.

	[SerializeField] private AudioSource m_DescriptionAudio;			// Reference to the audio source that will play descriptions
	[SerializeField] private AudioSource m_GazeAudio;                   // Reference to the audio source that will play effects when the user looks at it and when it fills.
	[SerializeField] private AudioClip m_OnOverClip;                    // The clip to play when the user looks at the bar.
	[SerializeField] private AudioClip m_OnFilledClip;                  // The clip to play when the bar finishes filling.
	[SerializeField] private AudioClip m_Description;					// The clip to play when the user interacts
	[SerializeField] private TextMeshProUGUI m_Subtitles;				// Reference to the GUI where the subtitles are displayed
	[SerializeField] private TextMeshProUGUI m_Messages;				// Reference to the GUI where the messages are displayed
	[SerializeField] private OVRPlayerController m_Player;				// Reference to the player controller
	[HideInInspector] public bool m_NoPanel;							// Enables or disables the panel controller option
	[HideInInspector] public PanelController m_PanelController;			// Reference to the panel controller

	[SerializeField]  private PauseMenuController m_MenuOptions;								// Reference to the options menu
	private bool m_MusicEnabled { get { return m_MenuOptions.m_MusicEnabled; } }				// Gets the music option from the menu
	private bool m_VoicesEnabled { get { return m_MenuOptions.m_VoicesEnabled; } }				// Gets the voices option from the menu
	private bool m_SoundEffectsEnabled { get { return m_MenuOptions.m_SoundEffectsEnabled; } }	// Gets the interaction sounds option from the menu
	private bool m_MessagesEnabled { get { return m_MenuOptions.m_MessagesEnabled; } }			// Gets the mesages option from the menu
	private bool m_SubtitlesEnabled { get { return m_MenuOptions.m_SubtitlesEnabled; } }		// Gets the subtitles option from the menu
	private bool m_PauseMenuIsActive { get { return m_MenuOptions.m_PauseMenu.activeSelf; } }	// Gets the pause menu active

	private bool m_SubtitlesPlaying;					// Whether the subtitles are playing or not
	private bool m_GazeOver;                            // Whether the user is pointing at the VRInteractiveItem currently

	private IEnumerator m_CoroutineDescription;			// Coroutine to manage the description play
	private IEnumerator m_CoroutinePanel;				// Coroutine to manage the panel play
	private IEnumerator m_CoroutineSubtitles;			// Coroutine to manage the subtitles play
	private IEnumerator m_CoroutineDBOpen;				// Coroutine to manage the opening of the database

	[SerializeField] private string m_DefaultText;		// String for the default texts
	[SerializeField] private string m_InteractMessage;	// String for the interact messages
	[SerializeField] private string m_CancelMessage;	// String for the cancel messages



	// Called when the script instance is being loaded
	void Awake ()
	{
		dbFileName = "Historical_Descriptions.db";

		m_Subtitles.text = m_DefaultText;
		m_SubtitlesPlaying = false;
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

			// If the description is not playing...
			if (!m_DescriptionAudio.isPlaying && !m_SubtitlesPlaying && !(m_GazeAudio.isPlaying && m_GazeAudio.clip.name == m_OnFilledClip.name))
			{
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
			// If the description is playing and the messages option is enabled, displays the cancel message
			else if (m_MessagesEnabled)
			{
				m_Messages.text = m_CancelMessage;
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
					m_Messages.text = m_DefaultText;
				}
				// If the voices option is enabled, manages the interaction with audio on
				if (m_VoicesEnabled)
				{
					HandleSlectionCompleteAudioOn ();
				}
				// If the voices option is enabled, manages the interaction with audio off
				else
				{
					HandleSlectionCompleteAudioOff ();
				}
			}
		}
	}


	// Called when the user completes the interaction selection with audio on
	public void HandleSlectionCompleteAudioOn ()
	{
		m_SelectionRadial.Hide ();

		// If the description is not already playing...
		if (!m_GazeAudio.isPlaying && !m_DescriptionAudio.isPlaying) {
			m_Player.enabled = false;	// Blocks the displacement of the user
			m_MenuOptions.m_Active = false;	// Disables the options menu so it can not be displayed

			// Starts the coroutine that will play the description
			m_CoroutineDescription = PlayDescription ();
			StartCoroutine (m_CoroutineDescription);
		}
		// If the description is already playing...
		else
		{
			StopCoroutine (m_CoroutineDescription);	// Stops the coroutine that is playing the description

			// If the description clip is playing, stops it
			if (m_DescriptionAudio.isPlaying)
			{
				m_DescriptionAudio.Stop ();
			}
				
			m_Player.enabled = true;	// Enables the displacement of the user
			m_MenuOptions.m_Active = true;	// Enables the options menu so it can be displayed again

			// If the subtitles were playing, stops and disables them
			if (m_SubtitlesEnabled)
			{
				// Stops the coroutine that is playing the subtitles, resets and disbles the subtitles text
				StopCoroutine (m_CoroutineSubtitles);
				m_Subtitles.text = m_DefaultText;
				m_SubtitlesPlaying = false;

				// Closes the database
				dbReader.Close ();
				dbReader = null;
				DB_Close ();
			}
				
			if (!m_NoPanel)
			{
				StopCoroutine (m_CoroutinePanel);	// Stops the coroutine that is playing the panel images transition
				m_PanelController.ResetPanel ();		// Resets the panel to the original status
			}
		}
	}


	// Called when the user completes the interaction selection with audio off
	private void HandleSlectionCompleteAudioOff ()
	{
		m_SelectionRadial.Hide ();

		// If the description is not already playing...
		if (!m_SubtitlesPlaying)
		{
			m_Player.enabled = false;	// Blocks the displacement of the user
			m_MenuOptions.m_Active = false;	// Disables the options menu so it can not be displayed

			// Starts the coroutine that plays the subtitles
			m_CoroutineSubtitles = PlaySubtitles ();
			StartCoroutine (m_CoroutineSubtitles);

			if (!m_NoPanel)
			{
				// Starts the coroutine that activates the panels image transition
				m_CoroutinePanel = ActivatePanel ();
				StartCoroutine (m_CoroutinePanel);
			}
		}
		// If the description is already playing...
		else
		{
			// Stops the coroutine that is playing the subtitles, resets and disbles the subtitles text
			StopCoroutine (m_CoroutineSubtitles);
			m_Subtitles.text = m_DefaultText;
			m_SubtitlesPlaying = false;

			m_Player.enabled = true;	// Enables the displacement of the user
			m_MenuOptions.m_Active = true;	// Enables the options menu so it can be displayed again

			if (!m_NoPanel)
			{
				StopCoroutine (m_CoroutinePanel);	// Stops the coroutine that is playing the panel images transition
				m_PanelController.ResetPanel ();		// Resets the panel to the original status
			}

			// Closes the database
			dbReader.Close ();
			dbReader = null;
			DB_Close ();
		}
	}


	// Coroutine that plays the description
	private IEnumerator PlayDescription ()
	{
		// If the interaction sounds option is enabled...
		if (m_SoundEffectsEnabled)
		{
			// Sets the gaze audio clip to the on filled clip, plays it, and waits until it finishes
			m_GazeAudio.clip = m_OnFilledClip;
			m_GazeAudio.Play ();
			while (m_GazeAudio.isPlaying) {
				yield return null;
			}
		}

		// Sets the description audio clip to the description clip and plays it
		m_DescriptionAudio.clip = m_Description;
		m_DescriptionAudio.Play();

		if (!m_NoPanel)
		{
			// Starts the coroutine that activates the panels image transition
			m_CoroutinePanel = ActivatePanel ();
			StartCoroutine (m_CoroutinePanel);
		}

		// If the subtitles option is enabled, starts the coroutine that plays the subtitles
		if (m_SubtitlesEnabled)
		{
			m_CoroutineSubtitles = PlaySubtitles ();
			StartCoroutine (m_CoroutineSubtitles);
		}

		// Waits until the description and the subtitles finish playing
		while (m_DescriptionAudio.isPlaying || m_SubtitlesPlaying)
		{
			yield return null;
		}

		// If the messages option is enabled...
		if (m_MessagesEnabled)
		{
			// If the user is pointing to the object, displays the interact message again
			if (m_GazeOver)
			{
				m_Messages.text = m_InteractMessage;
			}
			// If the user is not pointing to the object, resets the message text
			else
			{
				m_Messages.text = m_DefaultText;
			}
		}

		m_Player.enabled = true;	// Enables the displacement of the user
		m_MenuOptions.m_Active = true;	// Enables the options menu so it can be displayed again

		yield break;
	}


	// Coroutine that activates the panel images transition
	private IEnumerator ActivatePanel ()
	{
		float startFunctionTime = Time.time;
		int n_Photos = m_PanelController.m_nPhotos;	// Number of photos associated to the panel
		float elapsedTime = Time.time - startFunctionTime;	// Elapsed time since the panel was enabled

		// For each photo associated to the panel...
		for (int i = 0; i < n_Photos; i++)
		{
			float waitTime = m_PanelController.m_Photos[i].m_SecondWhenDisplay - elapsedTime;	// Time the photo has to wait to be displayed on the panel
			// When the time has finished, updates the photo
			yield return new WaitForSeconds (waitTime);
			m_PanelController.UpdatePhoto ();

			// Updates the elapsed time
			elapsedTime += waitTime;
		}

		// Waits until the description and the subtitles finish playing
		while (m_DescriptionAudio.isPlaying || m_SubtitlesPlaying)
		{
			yield return null;
		}

		// Resets the panel to its original status
		m_PanelController.ResetPanel ();

		yield break;
	}


	// Coroutine that plays the subtitles
	private IEnumerator PlaySubtitles ()
	{
		float startFunctionTime = Time.time;
		m_SubtitlesPlaying = true;

		// Opens the database and waits until it is open
		m_CoroutineDBOpen = DB_Open ();
		StartCoroutine (m_CoroutineDBOpen);
		while (dbOpening)
		{
			yield return null;
		}

		dbCommand = dbConnection.CreateCommand ();	// Creates the database connection

		// Creates and executes the query
		string sqlQuery = string.Format("SELECT text, seconds FROM {0};", dbTableName);
		dbCommand.CommandText = sqlQuery;
		dbReader = dbCommand.ExecuteReader ();

		float elapsedTime = Time.time - startFunctionTime;	// Elapsed time since the subtitles were enabled

		// For each row returned by the query...
		while (dbReader.Read ())
		{
			float waitTime = dbReader.GetFloat (1) - elapsedTime; // Time the text has to wait to be displayed on the subtitles

			yield return new WaitForSeconds (waitTime);

			// When the time has finished, updates the text on the subtitles
			m_Subtitles.text = dbReader.GetString (0);

			// Updates the elapsed time
			elapsedTime += waitTime;
		}

		// Waits until the description clip finishes
		while (m_DescriptionAudio.isPlaying)
		{
			yield return null;
		}

		// Closes the database
		dbReader.Close ();
		dbReader = null;
		DB_Close ();

		// Deletes the text of the subtitles and disables them
		m_Subtitles.text = m_DefaultText;
		m_SubtitlesPlaying = false;

		// If the voices option is disabled, enables itself the player displacement and the options menu to be displayed
		if (!m_VoicesEnabled)
		{
			m_Player.enabled = true;
			m_MenuOptions.m_Active = true;
		}

		yield break;
	}


	// Opens the database
	private IEnumerator DB_Open ()
	{
		dbOpening = true;

		// Create the connection
		// Checking which platform is being used
		// If it is PC...
		if (Application.platform == RuntimePlatform.WindowsEditor)
		{
			dbPath = Application.dataPath + "/StreamingAssets/" + dbFileName;
		}
		// If it is Android...
		else if (Application.platform == RuntimePlatform.Android)
		{
			dbPath = Application.persistentDataPath + "/" + dbFileName;
			// Checking if the file is stored in persistent data
			if (!File.Exists (dbPath))
			{
				WWW loadDB = new WWW ("jar:file://" + Application.dataPath + "!/assets/" + dbFileName);
				File.WriteAllBytes (dbPath, loadDB.bytes);
			}
		}

		// Create and open the connection
		dbStrConnection = "URI=file:" + dbPath;
		dbConnection = new SqliteConnection (dbStrConnection);

		// Opens the database in a new thread to do not block the main thread
		Thread _thread = new Thread (dbConnection.Open);
		_thread.Start ();
		while (_thread.IsAlive)
		{
			yield return null;
		}

		dbOpening = false;

		yield break;
	}


	// Closes the database
	void DB_Close ()
	{
		// Close connection
		dbCommand.Dispose ();
		dbCommand = null;
		dbConnection.Close ();
		dbConnection = null;
	}
}
