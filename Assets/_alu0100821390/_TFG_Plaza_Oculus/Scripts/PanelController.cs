using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelController : MonoBehaviour {

	// Class that represents a photo and the time in seconds when the photo will be displayed
	[System.Serializable]
	public class Photo : System.Object
	{
		public Texture m_Photo;
		public float m_SecondWhenDisplay;
	}

	[SerializeField] private Texture m_MainPhoto;		// Reference to the main photo of the panel
	public Photo[] m_Photos;							// Array reference to the photos to display on the panel
	[HideInInspector] public int m_nPhotos;				// Number of photos to display on the panel

	private int m_PhotoIndex;		// Index of the photo that is displayed at each moment
	private Material m_Mat;			// Material to assign a photo to the panel
	private Renderer m_Rend;		// Renderer component of the panel



	// Called when the script instance is being loaded
	void Awake ()
	{
		m_nPhotos = m_Photos.Length;
		m_PhotoIndex = 0;

		m_Mat = new Material (Shader.Find ("Standard"));
		m_Mat.mainTexture = m_MainPhoto;
	}


	// Called on the frame when the script is enabled
	void Start ()
	{
		m_Rend = transform.GetChild(1).gameObject.GetComponentInChildren<Renderer> ();
		m_Rend.material = m_Mat;
	}


	// Resets the panel to it initial status
	public void ResetPanel ()
	{
		m_nPhotos = m_Photos.Length;
		m_PhotoIndex = 0;

		// Creates the new material with the main photo
		m_Mat = new Material (Shader.Find ("Standard"));
		m_Mat.mainTexture = m_MainPhoto;

		// Renders the photo on the Renderer component of the panel
		m_Rend = transform.GetChild(1).gameObject.GetComponentInChildren<Renderer> ();
		m_Rend.material = m_Mat;
	}


	// Updates the photo of the panel to the next one
	public void UpdatePhoto ()
	{
		// Creates the material with the new photo and renders it
		m_Mat.mainTexture = m_Photos [m_PhotoIndex].m_Photo;
		m_Rend.material = m_Mat;

		// If the index of the photo is out of range, starts on the first photo again
		m_PhotoIndex++;
		if (m_PhotoIndex >= m_nPhotos)
		{
			m_PhotoIndex = 0;
		}
	}
}
