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

public class PlayIntro : MonoBehaviour {

	private bool dbOpening;							//
	private string dbPath;							//
	private string dbStrConnection;					//
	private string dbFileName;						//
	[SerializeField] private string dbTableName;	// Database parameters
													//
	private IDbConnection dbConnection;				//
	private IDbCommand dbCommand;					//
	private IDataReader dbReader;					//

	[SerializeField] private AudioSource m_DescriptionAudio;			// Reference to the audio source that will play descriptions
	[SerializeField] private AudioSource m_GazeAudio;                   // Reference to the audio source that will play effects when the user looks at it and when it fills.
	[SerializeField] private AudioClip m_Description;					// The clip to play when the user interacts
	[SerializeField] private TextMeshProUGUI m_Subtitles;				// Reference to the GUI where the messages are displayed
	[SerializeField] private OVRPlayerController m_Player;				// Reference to the player controller

	[SerializeField]  private PauseMenuController m_MenuOptions;								// Reference to the options menu
	private bool m_MusicEnabled { get { return m_MenuOptions.m_MusicEnabled; } }				// Gets the music option from the menu
	private bool m_VoicesEnabled { get { return m_MenuOptions.m_VoicesEnabled; } }				// Gets the voices option from the menu
	private bool m_SoundEffectsEnabled { get { return m_MenuOptions.m_SoundEffectsEnabled; } }	// Gets the interaction sounds option from the menu
	private bool m_MessagesEnabled { get { return m_MenuOptions.m_MessagesEnabled; } }			// Gets the mesages option from the menu
	private bool m_SubtitlesEnabled { get { return m_MenuOptions.m_SubtitlesEnabled; } }		// Gets the subtitles option from the menu
	private bool m_PauseMenuIsActive { get { return m_MenuOptions.m_PauseMenu.activeSelf; } }	// Gets the pause menu active

	private bool m_SubtitlesPlaying;					// Whether the subtitles are playing or not

	private IEnumerator m_CoroutineDescription;			// Coroutine to manage the description play
	private IEnumerator m_CoroutineSubtitles;			// Coroutine to manage the subtitles play
	private IEnumerator m_CoroutineDBOpen;				// Coroutine to manage the opening of the database

	[SerializeField] private string m_DefaultText;		// String for the default texts



	// Called when the script instance is being loaded
	void Awake ()
	{
		dbFileName = "Historical_Descriptions.db";

		m_Subtitles.text = m_DefaultText;
		m_SubtitlesPlaying = false;
	}


	// Called on the frame when the script is enabled
	void Start ()
	{
		m_Player.enabled = false;	// Blocks the displacement of the user
		m_MenuOptions.m_Active = false;	// Disables the options menu so it can not be displayed

		// Starts the coroutine that will play the description
		m_CoroutineDescription = PlayDescription ();
		StartCoroutine (m_CoroutineDescription);
	}


	// Coroutine that plays the description
	private IEnumerator PlayDescription ()
	{
		if (m_VoicesEnabled)
		{
			// Sets the description audio clip to the description clip and plays it
			m_DescriptionAudio.clip = m_Description;
			m_DescriptionAudio.Play();
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

		m_Player.enabled = true;	// Enables the displacement of the user
		m_MenuOptions.m_Active = true;	// Enables the options menu so it can be displayed again

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
