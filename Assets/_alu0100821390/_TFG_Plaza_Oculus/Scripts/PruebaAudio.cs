using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PruebaAudio : MonoBehaviour {

	private float [] tiempos;
	private int i;
	private AudioSource audio;
	bool chivato;


	void Start ()
	{
		tiempos = new float[500];
		i = 0;
		audio = GetComponent<AudioSource> ();
		chivato = true;

		Debug.Log (audio.clip.name);
	}
	

	void Update ()
	{
		if (audio.isPlaying) {
			if (Input.GetKeyDown ("space")) {
				tiempos [i] = Time.time;
				i++;
			}
		}
		else if (chivato)
		{
			Mostrar ();
		}
	}


	void Mostrar ()
	{
		string cadena = "Tiempos: ";
		for (int j = 0; j < i-1; j++)
		{
			cadena = cadena + tiempos [j] + ", ";
		}
		cadena = cadena + tiempos [i - 1];

		Debug.Log (cadena);

		chivato = false;
	}
}
