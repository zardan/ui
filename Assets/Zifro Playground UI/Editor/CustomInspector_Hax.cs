﻿namespace PM.Editor
{
	using UnityEditor;

	[CustomEditor(typeof(Hax)), CanEditMultipleObjects]
	public class CustomInspector_Hax : Editor
	{
		public override void OnInspectorGUI()
		{
			EditorGUILayout.HelpBox(
				"\nThis script allows you to hax through the levels!\n\nIf you press SHIFT + F4 it unlocks the next level.\n",
				MessageType.Info, true);
		}
	}
}