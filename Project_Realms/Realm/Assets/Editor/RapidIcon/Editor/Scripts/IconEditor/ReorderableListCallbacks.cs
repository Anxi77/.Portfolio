using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace RapidIcon_1_7_1
{
	public class ReorderableListCallbacks
	{
		private IconEditor iconEditor;

		public ReorderableListCallbacks(IconEditor iconEditor)
		{
			this.iconEditor = iconEditor;
		}

		private ReorderableListCallbacks()
		{ }

		public void DrawHeader(Rect rect)
		{
			//---Title label---//
			EditorGUI.LabelField(rect, "Shaders");

			//---Fullscreen check---//
			float sw2 = iconEditor.sepWidth;
			if (iconEditor.fullscreen)
				sw2 += iconEditor.fullWidth;
			rect.width = 100;
			rect.x = sw2 - 154;

			//---Draw Save preset button---//
			if (GUI.Button(rect, "Save Preset"))
			{
				//---Get save path---//
				string savePath = EditorUtility.SaveFilePanel("Save Preset", iconEditor.lastPresetPath == "" ? Application.dataPath : iconEditor.lastPresetPath, "PostProcessingPreset", "rippp");

				//---If savepath is not empty then save preset---//
				if (savePath != "")
				{
					//---Convert data to JSON string---//
					iconEditor.currentIcon.SaveMatInfo();
					string data = JsonUtility.ToJson(iconEditor.currentIcon);
					int pos = data.IndexOf("\"matInfo\":");
					data = "{" + data.Substring(pos);

					//---Save bytes---//
					File.WriteAllBytes(savePath, System.Text.Encoding.ASCII.GetBytes(data));

					//---Store path as last preset path---//
					iconEditor.lastPresetPath = savePath;
				}
			}

			//---Draw Load Preset button---//
			rect.x += 102;
			if (GUI.Button(rect, "Load Preset"))
			{
				//---Get open path---//
				string openPath = EditorUtility.OpenFilePanel("Open Preset", iconEditor.lastPresetPath == "" ? Application.dataPath : iconEditor.lastPresetPath, "rippp");

				//---If open path is not empty then load the preset---//
				if (openPath != "")
				{
					//---Read bytes and get JSON string---//
					Byte[] bytes = File.ReadAllBytes(openPath);
					iconEditor.lastPresetPath = openPath;
					string data = System.Text.Encoding.ASCII.GetString(bytes);

					//---Convert to icon from JSON---//
					Icon tmp = JsonUtility.FromJson<Icon>(data);

					//---Load mat info---//
					iconEditor.currentIconSet.saveData = true;
					iconEditor.currentIcon.iconSettings.matInfo = new List<Icon.MaterialInfo>(tmp.iconSettings.matInfo);
					iconEditor.currentIcon.LoadMatInfo();

					//---Update icon---//
					Utils.UpdateIcon(iconEditor.currentIcon, iconEditor);

					//---Update reorderable list---//
					iconEditor.reorderableList.list = iconEditor.currentIcon.iconSettings.postProcessingMaterials;
					iconEditor.reorderableList.index = 0;

					//---Create new material editor---//
					Editor.DestroyImmediate(iconEditor.materialEditor);
					iconEditor.materialEditor = (MaterialEditor)Editor.CreateEditor((UnityEngine.Object)iconEditor.reorderableList.list[iconEditor.reorderableList.index]);
				}
			}
		}

		public void DrawListItems(Rect rect, int index, bool isActive, bool isFocused)
		{
			//---If index within bounds of the list---//
			if (index >= 0 && index < iconEditor.reorderableList.list.Count)
			{
				//---Check and fix editor---//
				if (iconEditor.materialEditor == null)
				{
					Editor.DestroyImmediate(iconEditor.materialEditor);
					iconEditor.materialEditor = (MaterialEditor)Editor.CreateEditor((UnityEngine.Object)iconEditor.reorderableList.list[iconEditor.reorderableList.index]);
				}

				//---Check and fix materials---//
				if (iconEditor.currentIcon.iconSettings.postProcessingMaterials[index] == null)
				{
					iconEditor.currentIcon.LoadMatInfo();
				}

				//---Check for fullscreen---//
				float sw2 = iconEditor.sepWidth;
				if (iconEditor.fullscreen)
					sw2 += iconEditor.fullWidth;

				//---Get widths of elements in list item - layer toggle, layer name, layer shader---//
				float[] widths = new float[] { 16, (sw2 - 150) / 2, (sw2 + 100) / 2 };

				//---Prevent error, close window if material toggles is null - reopening window should fix---//
				if (iconEditor.currentIcon.iconSettings.materialToggles == null)
				{
					iconEditor.window.Close();
					return;
				}

				//---Draw text field for layer name, this is draw first before the change check as changing the layer name doesn't need to update the icon---//
				GUI.enabled = iconEditor.currentIcon.iconSettings.materialToggles[iconEditor.currentIcon.iconSettings.postProcessingMaterials[index]];
				iconEditor.currentIcon.iconSettings.materialDisplayNames[iconEditor.currentIcon.iconSettings.postProcessingMaterials[index]] = EditorGUI.TextField(new Rect(rect.x + widths[0] + 4, rect.y + 3, widths[1], EditorGUIUtility.singleLineHeight), iconEditor.currentIcon.iconSettings.materialDisplayNames[iconEditor.currentIcon.iconSettings.postProcessingMaterials[index]]);

				//---Draw the layer toggle---//
				GUI.enabled = true;
				EditorGUI.BeginChangeCheck();
				iconEditor.currentIcon.iconSettings.materialToggles[iconEditor.currentIcon.iconSettings.postProcessingMaterials[index]] = EditorGUI.Toggle(new Rect(rect.x, rect.y + 3, widths[0], EditorGUIUtility.singleLineHeight), iconEditor.currentIcon.iconSettings.materialToggles[iconEditor.currentIcon.iconSettings.postProcessingMaterials[index]]);

				//---Draw the shader selection field---//
				GUI.enabled = iconEditor.currentIcon.iconSettings.materialToggles[iconEditor.currentIcon.iconSettings.postProcessingMaterials[index]];
				iconEditor.currentIcon.iconSettings.postProcessingMaterials[index].shader = (Shader)EditorGUI.ObjectField(new Rect(rect.x + widths[0] + widths[1] + 8, rect.y + 3, widths[2], EditorGUIUtility.singleLineHeight), iconEditor.currentIcon.iconSettings.postProcessingMaterials[index].shader, typeof(Shader), true);
				GUI.enabled = true;

				//---If layer toggles/shaders changed then update the icon---//
				if (EditorGUI.EndChangeCheck())
				{
					iconEditor.updateFlag = true;
					Editor.DestroyImmediate(iconEditor.materialEditor);
					iconEditor.materialEditor = (MaterialEditor)Editor.CreateEditor((UnityEngine.Object)iconEditor.reorderableList.list[iconEditor.reorderableList.index]);
				}
			}
		}

		public void SelectShader(ReorderableList l)
		{
			//---If selected shader changes, create a new material editor---//
			Editor.DestroyImmediate(iconEditor.materialEditor);
			iconEditor.materialEditor = (MaterialEditor)Editor.CreateEditor((UnityEngine.Object)iconEditor.reorderableList.list[iconEditor.reorderableList.index]);
		}

		public void AddShader(ReorderableList l)
		{
			//---Record object for undo---//
			Undo.RecordObject(iconEditor.currentIcon, "Add New Shader");

			//---Create new material with default shader---//
			Material m = new Material(Shader.Find("RapidIcon/ObjectRender"));

			//---Add the new material to the list and select it---//
			l.list.Add(m);
			l.index = l.list.Count - 1;

			//---Add the material display name and toggle to the current icon---//
			iconEditor.currentIcon.iconSettings.materialDisplayNames.Add(m, "Shader " + l.list.Count);
			iconEditor.currentIcon.iconSettings.materialToggles.Add(m, true);

			//---Create a new material editor---//
			Editor.DestroyImmediate(iconEditor.materialEditor);
			iconEditor.materialEditor = (MaterialEditor)Editor.CreateEditor((UnityEngine.Object)iconEditor.reorderableList.list[iconEditor.reorderableList.index]);

			//---Update and save the current icon---//
			iconEditor.currentIconSet.saveData = true;
			iconEditor.updateFlag = true;
		}

		public void RemoveShader(ReorderableList l)
		{
			//---Record object for undo---//
			Undo.RecordObject(iconEditor.currentIcon, "Remove Shader");

			//---Remove the material from the list---//
			l.list.Remove(l.list[l.index]);
			l.index = (int)Mathf.Clamp(l.index, 0, l.list.Count - 1);

			//---Create a new material editor, if any materials left in the list---//
			Editor.DestroyImmediate(iconEditor.materialEditor);
			if (l.list.Count > 0)
				iconEditor.materialEditor = (MaterialEditor)Editor.CreateEditor((UnityEngine.Object)iconEditor.reorderableList.list[iconEditor.reorderableList.index]);

			//---Update and save the current icon---//
			iconEditor.currentIconSet.saveData = true;
			iconEditor.updateFlag = true;
		}

		public void ShadersReorded(ReorderableList l)
		{
			//---Update and save the current icon if the list is reordered---//
			iconEditor.currentIconSet.saveData = true;
			iconEditor.updateFlag = true;
		}
	}
}
