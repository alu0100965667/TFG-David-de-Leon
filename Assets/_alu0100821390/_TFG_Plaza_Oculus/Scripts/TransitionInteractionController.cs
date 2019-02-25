using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionInteractionController : MonoBehaviour {

	[SerializeField] private PlayTransitionInteraction m_PlayTransitionInteraction;		// Reference to the PlayTransitionInteraction script


	// Called when the script instance is being loaded
	void Awake ()
	{
		m_PlayTransitionInteraction.enabled = false;
	}


	// Called when an object enters the collider
	void OnTriggerEnter (Collider other)
	{
		// If the user enters the collider, enables the PlayTransitionInteraction script
		if (other.tag == "Player")
		{
			m_PlayTransitionInteraction.enabled = true;
		}
	}


	// Called when an object exits the collider
	void OnTriggerExit (Collider other)
	{
		// If the user exits the collider, disables the PlayTransitionInteraction script
		if (other.tag == "Player")
		{
			m_PlayTransitionInteraction.enabled = false;
		}
	}
}
