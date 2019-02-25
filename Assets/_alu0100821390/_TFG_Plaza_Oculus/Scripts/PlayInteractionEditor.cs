#if UNITY_EDITOR

using UnityEngine;
using System;
using UnityEditor;


[CustomEditor(typeof(PlayInteraction))]
public class PlayInteractionEditor : Editor {

	public override void OnInspectorGUI ()
	{
		DrawDefaultInspector ();

		var playInteraction = target as PlayInteraction;

		playInteraction.m_NoPanel = GUILayout.Toggle (playInteraction.m_NoPanel, "No Panel");

		/*using (new EditorGUI.DisabledScope (playInteraction.m_NoPanel))
		{
			playInteraction.m_PanelController = EditorGUILayout.ObjectField (playInteraction.m_PanelController, typeof(PanelController), false) as PanelController;
		}*/

		using (var group = new EditorGUILayout.FadeGroupScope (Convert.ToSingle (playInteraction.m_NoPanel)))
		{
			if (group.visible == false)
			{
				EditorGUI.indentLevel++;
				playInteraction.m_PanelController = EditorGUILayout.ObjectField (playInteraction.m_PanelController, typeof(PanelController)) as PanelController;
				EditorGUI.indentLevel--;
			}
		}

		EditorUtility.SetDirty (playInteraction);
	}
}

#endif
