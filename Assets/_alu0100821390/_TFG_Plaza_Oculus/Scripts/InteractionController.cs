using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionController : MonoBehaviour {

	[SerializeField] private PlayInteraction m_PlayInteraction;		// Reference to the PlayInteraction script



	// Called when the script instance is being loaded
	void Awake () {
		m_PlayInteraction.enabled = false;
	}


	// Called when an object enters the collider
	void OnTriggerEnter (Collider other)
	{
		// If the user enters the collider, enables the PlayInteraction script
		if (other.tag == "Player")
		{
			m_PlayInteraction.enabled = true;
		}
	}


	// Called when an object exits the collider
	void OnTriggerExit (Collider other)
	{
		// If the user exits the collider, disables the PlayInteraction script
		if (other.tag == "Player")
		{
			m_PlayInteraction.enabled = false;
		}
	}
}
