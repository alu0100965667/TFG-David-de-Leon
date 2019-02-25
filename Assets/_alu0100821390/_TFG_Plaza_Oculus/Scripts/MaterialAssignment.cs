using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialAssignment : MonoBehaviour {

	// Class that represents a object tag and the material to assign to each object with that tag
	[System.Serializable]
	public class MaterialObject : System.Object
	{
		public string m_Object;
		public Material m_Material;
	}

	public MaterialObject[] m_MaterialObject;		// Array reference to the tags and each respective materials to assign to the objects



	// Called when the script instance is being loaded
	void Awake ()
	{
		InitializeMaterials (this.gameObject);
	}


	// Assign to each object a material corresponding to its tag, if there
	void InitializeMaterials (GameObject part)
	{
		// For each child of the object...
		for (int i = 0; i < part.transform.childCount; i++)
		{
			GameObject m_Child = part.transform.GetChild (i).gameObject;

			// If the child does not have children and it has a Renderer component,
			// checks the materials array looking for any material to assign by the tag
			if ((m_Child.transform.childCount == 0) && (m_Child.GetComponent<Renderer> () != null))
			{
				for (int j = 0; j < m_MaterialObject.Length; j++)
				{
					if (m_Child.tag == m_MaterialObject [j].m_Object)
					{
						m_Child.GetComponent<Renderer> ().material = m_MaterialObject [j].m_Material;
						break;
					}
				}
			}
			// If the child has children, invokes the function recursively with the current child
			else
			{
				InitializeMaterials (m_Child);
			}
		}
	}
}
