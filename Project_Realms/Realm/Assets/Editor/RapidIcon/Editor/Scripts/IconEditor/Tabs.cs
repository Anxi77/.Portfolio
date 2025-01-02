using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RapidIcon_1_7_1
{
	public static class Tabs
	{
		public static void DrawObjectControls(IconEditor iconEditor)
		{
			//---Begin change check---//
			EditorGUI.BeginChangeCheck();

			//---Draw position field---//
			GUI.enabled = !iconEditor.currentIcon.iconSettings.autoPosition;
			Vector3 tmpObjPos = EditorGUILayout.Vector3Field("Position", iconEditor.currentIcon.iconSettings.objectPosition);
			Rect posR = GUILayoutUtility.GetLastRect();
			posR.x += 50;
			posR.height = 18;
			posR.width = 50;
			GUI.enabled = true;

			//---Draw auto button for position---//
			bool tmpAutoPos = GUI.Toggle(posR, iconEditor.currentIcon.iconSettings.autoPosition, "Auto", GUI.skin.button);

			//---Draw object rotation and scale fields---//
			Vector3 tmpObjRot = EditorGUILayout.Vector3Field("Rotation", iconEditor.currentIcon.iconSettings.objectRotation);
			Vector3 tmpObjScale = EditorGUILayout.Vector3Field("Scale", iconEditor.currentIcon.iconSettings.objectScale);

			//---Draw link scale toggle---//
			Rect r = GUILayoutUtility.GetLastRect();
			r.position += new Vector2(40, 0);
			if (GUI.Button(r, iconEditor.linkScale ? iconEditor.scaleLinkOnImage : iconEditor.scaleLinkOffImage, GUIStyle.none))
			{
				iconEditor.linkScale = !iconEditor.linkScale;
			}

			//---If any fields changed---//
			if (EditorGUI.EndChangeCheck())
			{
				//---Record object for undo---//
				Undo.RecordObject(iconEditor.currentIcon, "Edit Icon Object");

				//---Apply position and rotation---//
				iconEditor.currentIcon.iconSettings.objectPosition = tmpObjPos;
				iconEditor.currentIcon.iconSettings.objectRotation = tmpObjRot;

				if (tmpAutoPos && !iconEditor.currentIcon.iconSettings.autoPosition)
				{
					iconEditor.currentIcon.iconSettings.objectPosition = Utils.GetObjectAutoOffset(iconEditor.currentIcon, Utils.GetObjectBounds(iconEditor.currentIconSet));
				}
				iconEditor.currentIcon.iconSettings.autoPosition = tmpAutoPos;

				//---If link scale disabled, apply scale---//
				if (!iconEditor.linkScale)
				{
					iconEditor.currentIcon.iconSettings.objectScale = tmpObjScale;
				}
				else
				{
					//---If link scale enabled, detect which axis changed and apply to all axes---//
					if (tmpObjScale.x != iconEditor.currentIcon.iconSettings.objectScale.x)
					{
						iconEditor.currentIcon.iconSettings.objectScale = tmpObjScale.x * Vector3.one;
					}
					else if (tmpObjScale.y != iconEditor.currentIcon.iconSettings.objectScale.y)
					{
						iconEditor.currentIcon.iconSettings.objectScale = tmpObjScale.y * Vector3.one;
					}
					else if (tmpObjScale.z != iconEditor.currentIcon.iconSettings.objectScale.z)
					{
						iconEditor.currentIcon.iconSettings.objectScale = tmpObjScale.z * Vector3.one;
					}
				}

				//---Save and update icon---//
				iconEditor.currentIconSet.saveData = true;
				iconEditor.updateFlag = true;
			}
		}

		public static void DrawHierarchyControls(IconEditor iconEditor)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label("Object Hierarchy");

			if (GUILayout.Button("Enable All"))
			{
				List<string> keys = iconEditor.currentIcon.iconSettings.subObjectEnables.Keys.ToList();

				foreach (string s in keys)
					iconEditor.currentIcon.iconSettings.subObjectEnables[s] = true;

				iconEditor.updateFlag = true;
				iconEditor.currentIconSet.saveData = true;
			}

			if (GUILayout.Button("Disable All"))
			{
				List<string> keys = iconEditor.currentIcon.iconSettings.subObjectEnables.Keys.ToList();

				foreach (string s in keys)
					iconEditor.currentIcon.iconSettings.subObjectEnables[s] = false;

				iconEditor.updateFlag = true;
				iconEditor.currentIconSet.saveData = true;
			}

			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			GameObject obj = (GameObject)iconEditor.currentIconSet.assetObject;

			Rect r = GUILayoutUtility.GetRect(new GUIContent(obj.name), GUI.skin.label);
			bool active = iconEditor.currentIcon.iconSettings.subObjectEnables["/0"];

			EditorGUI.BeginChangeCheck();
			bool tmpToggle = GUI.Toggle(r, active, obj.name);

			if (EditorGUI.EndChangeCheck())
			{
				iconEditor.currentIcon.iconSettings.subObjectEnables["/0"] = tmpToggle;

				iconEditor.updateFlag = true;
				iconEditor.currentIconSet.saveData = true;
			}

			DrawChildObjects(iconEditor, obj, 0, 0, "/0", !tmpToggle);
		}

		private static void DrawChildObjects(IconEditor iconEditor, GameObject obj, int depth, int childIdx, string path, bool parentDisabled)
		{
			for (int i = 0; i < obj.transform.childCount; i++)
			{
				GameObject child = obj.transform.GetChild(i).gameObject;
				Rect r = GUILayoutUtility.GetRect(new GUIContent(child.name), GUI.skin.label);
				r.x += (depth + 1) * 16;

				string newPath = path + "/" + i;
				bool active = iconEditor.currentIcon.iconSettings.subObjectEnables[newPath];

				if (parentDisabled)
				{
					active = false;
					GUI.enabled = false;
				}

				EditorGUI.BeginChangeCheck();
				bool tmpToggle = GUI.Toggle(r, active, child.name);

				if (EditorGUI.EndChangeCheck())
				{
					iconEditor.currentIcon.iconSettings.subObjectEnables[newPath] = tmpToggle;

					iconEditor.updateFlag = true;
					iconEditor.currentIconSet.saveData = true;
				}

				GUI.enabled = true;
				DrawChildObjects(iconEditor, child, depth + 1, i, newPath, !tmpToggle);
			}
		}

		public static void DrawCameraControls(IconEditor iconEditor)
		{
			//---Begin change check---//
			EditorGUI.BeginChangeCheck();

			//---Draw camera position field---//
			Vector3 tmpCamPos = EditorGUILayout.Vector3Field("Position", iconEditor.currentIcon.iconSettings.cameraPosition);

			//---Draw camera point of focus field---//
			GUILayout.Space(8);
			float tmpWdith = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 80;
			Vector3 tmpCamTgt = iconEditor.currentIcon.iconSettings.cameraTarget;
			tmpCamTgt = EditorGUILayout.Vector3Field("Point of Focus", iconEditor.currentIcon.iconSettings.cameraTarget);

			//---Draw camera projection mode field---//
			GUILayout.Space(8);
			string[] listOptions = { "Perspective", "Orthographic" };
			EditorGUIUtility.labelWidth = 80;
			int tmpOrtho = EditorGUILayout.Popup("Projection", iconEditor.currentIcon.iconSettings.cameraOrtho ? 1 : 0, listOptions);

			//---Temporary variables for undo---//
			float tmpCamSize = iconEditor.currentIcon.iconSettings.cameraSize;
			float tmpScaleFactor = iconEditor.currentIcon.iconSettings.camerasScaleFactor;
			float tmpCamFov = iconEditor.currentIcon.iconSettings.cameraFov;
			bool tmpCamAuto = iconEditor.currentIcon.iconSettings.autoScale;

			//---If projection mode is ortho---//
			if (tmpOrtho == 1)
			{
				GUILayout.BeginHorizontal();

				GUI.enabled = !iconEditor.currentIcon.iconSettings.autoScale;

				//---Draw camera size field---//
				tmpCamSize = EditorGUILayout.FloatField("Size", iconEditor.currentIcon.iconSettings.cameraSize);

				GUI.enabled = true;
				//---Draw auto button---//
				GUIStyle s = new GUIStyle(GUI.skin.button);
				s.fixedWidth = 50;
				tmpCamAuto = GUILayout.Toggle(iconEditor.currentIcon.iconSettings.autoScale, "Auto", s);

				GUILayout.EndHorizontal();
			}
			else
			{
				//---Draw FOV field---//
				tmpCamFov = EditorGUILayout.FloatField("Field of View", iconEditor.currentIcon.iconSettings.cameraFov);
			}

			//---Draw scale factor field---//
			tmpScaleFactor = EditorGUILayout.FloatField("Scale Factor", iconEditor.currentIcon.iconSettings.camerasScaleFactor);

			//---If perspective projection mode---//


			EditorGUIUtility.labelWidth = tmpWdith;
			//---If any field changed---//
			if (EditorGUI.EndChangeCheck())
			{
				//---Record object for undo---//
				Undo.RecordObject(iconEditor.currentIcon, "Edit Icon Camera");

				//---Apply camera position, point of focus, and projection mode---//
				iconEditor.currentIcon.iconSettings.cameraPosition = tmpCamPos;
				iconEditor.currentIcon.iconSettings.cameraTarget = tmpCamTgt;
				iconEditor.currentIcon.iconSettings.cameraOrtho = tmpOrtho == 1 ? true : false;

				//---Apply size, fov, and scale factor---//
				iconEditor.currentIcon.iconSettings.cameraSize = tmpCamSize;
				iconEditor.currentIcon.iconSettings.cameraFov = tmpCamFov;
				iconEditor.currentIcon.iconSettings.camerasScaleFactor = tmpScaleFactor;

				//---Auto scale---//
				if (tmpCamAuto && !iconEditor.currentIcon.iconSettings.autoScale)
				{
					iconEditor.currentIcon.iconSettings.cameraSize = Utils.GetCameraAuto(iconEditor.currentIcon, Utils.GetObjectBounds(iconEditor.currentIconSet));
				}
				iconEditor.currentIcon.iconSettings.autoScale = tmpCamAuto;

				//---Save and update icon---//
				iconEditor.currentIconSet.saveData = true;
				iconEditor.updateFlag = true;
			}
		}

		private static void ApplyScaleFactor(Icon icon, IconEditor iconEditor)
		{
			//---If not current icon then apply the scale factor---//
			if (icon != iconEditor.currentIcon)
			{
				icon.iconSettings.camerasScaleFactor = iconEditor.currentIcon.iconSettings.camerasScaleFactor;
				icon.iconSettings.perspLastScale = iconEditor.currentIcon.iconSettings.perspLastScale;
				icon.parentIconSet.saveData = true;
				Utils.UpdateIcon(icon, iconEditor);
			}
		}

		public static void DrawLightingControls(IconEditor iconEditor)
		{
			//---Begin change check---//
			EditorGUI.BeginChangeCheck();

			//---Draw ambient light field---///
			Color tmpAmbLight = EditorGUILayout.ColorField("Ambient Light Colour", iconEditor.currentIcon.iconSettings.ambientLightColour);

			//---Draw diretional light fields---//
			EditorGUILayout.Space(4);
			EditorGUILayout.LabelField("Directional Light");
			Color tmpLightColour = EditorGUILayout.ColorField("Colour", iconEditor.currentIcon.iconSettings.lightColour);
			Vector3 tmpLightDir = EditorGUILayout.Vector3Field("Rotation", iconEditor.currentIcon.iconSettings.lightDir);
			float tmpLightIntensity = EditorGUILayout.FloatField("Intensity", iconEditor.currentIcon.iconSettings.lightIntensity);

			//---If any fields have changed---//
			if (EditorGUI.EndChangeCheck())
			{
				//---Record object for undo---//
				Undo.RecordObject(iconEditor.currentIcon, "Edit Icon Lighting");

				//---Apply settings---//
				iconEditor.currentIcon.iconSettings.ambientLightColour = tmpAmbLight;
				iconEditor.currentIcon.iconSettings.lightColour = tmpLightColour;
				iconEditor.currentIcon.iconSettings.lightDir = tmpLightDir;
				iconEditor.currentIcon.iconSettings.lightIntensity = tmpLightIntensity;

				//---Update and save icon---//
				iconEditor.currentIconSet.saveData = true;
				iconEditor.updateFlag = true;
			}
		}

		public static void DrawAnimationControls(IconEditor iconEditor)
		{
			//---Begin change check---//
			EditorGUI.BeginChangeCheck();

			//---Draw animation clip field---//
			AnimationClip c = (AnimationClip)EditorGUILayout.ObjectField("Animation Clip", iconEditor.currentIcon.iconSettings.animationClip, typeof(AnimationClip), false);

			//---Draw animation offset field---//
			Rect r = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
			float offset = EditorGUI.Slider(r, "Animation Offset", iconEditor.currentIcon.iconSettings.animationOffset, 0, 1);

			//---If any fields have changed---//
			if (EditorGUI.EndChangeCheck())
			{
				//---Record object for undo---//
				Undo.RecordObject(iconEditor.currentIcon, "Edit Icon Animations");

				//---Apply settings---//
				iconEditor.currentIcon.iconSettings.animationClip = c;
				iconEditor.currentIcon.iconSettings.animationOffset = offset;

				//---Update and save icon---//
				iconEditor.updateFlag = true;
				iconEditor.currentIconSet.saveData = true;
			}
		}

		public static void DrawPostProcessingControls(IconEditor iconEditor)
		{
			//---Begin change check---//
			EditorGUI.BeginChangeCheck();

			//---Draw filter mode field---//
			FilterMode tmpFilterMode = (FilterMode)EditorGUILayout.EnumPopup("Filter Mode", iconEditor.currentIcon.iconSettings.filterMode);

			//---If filter mode changed---//
			if (EditorGUI.EndChangeCheck())
			{
				//---Record object for undo---//

				//---Apply sttings, update icon---//
				Undo.RecordObject(iconEditor.currentIcon, "Change Filter Mode");
				iconEditor.currentIcon.iconSettings.filterMode = tmpFilterMode;
				iconEditor.updateFlag = true;
			}

			//---Draw reorderable list---//
			iconEditor.reorderableList.list = iconEditor.currentIcon.iconSettings.postProcessingMaterials;
			iconEditor.reorderableList.index = (int)Mathf.Clamp(iconEditor.reorderableList.index, 0, iconEditor.reorderableList.list.Count - 1);
			GUILayout.Space(2);
			iconEditor.reorderableList.DoLayoutList();

			//---Begin change check---//
			EditorGUI.BeginChangeCheck();
			if (iconEditor.reorderableList.list.Count > 0)
			{
				//---Draw shader settings label---//
				string shaderName = iconEditor.currentIcon.iconSettings.postProcessingMaterials[iconEditor.reorderableList.index].shader.name;
				GUILayout.Label("Shader Settings (" + shaderName + ")");
				GUILayout.BeginVertical(GUI.skin.box);

				//---If not the default render shader---//
				if (shaderName != "RapidIcon/ObjectRender")
				{
					//---Get the material properties---//
					UnityEngine.Object[] obj = new UnityEngine.Object[1];
					obj[0] = (UnityEngine.Object)iconEditor.reorderableList.list[iconEditor.reorderableList.index];
					MaterialProperty[] props = MaterialEditor.GetMaterialProperties(obj);

					//---If the shader doesn't have a custom GUI---//
					if (iconEditor.materialEditor.customShaderGUI == null)
					{
						//---Draw the properties fields---//
						foreach (MaterialProperty prop in props)
						{
							if (iconEditor.materialEditor == null)
							{
								Debug.LogWarning("null editor!!!");
								continue;
							}

							if (prop.name != "_MainTex" && prop.flags != MaterialProperty.PropFlags.HideInInspector)
							{
								iconEditor.materialEditor.ShaderProperty(prop, prop.displayName);
							}
						}
					}
					else
					//---If the shader does have a custom GUI---//
					{

						//---Get list of all properties---//
						List<MaterialProperty> list = new List<MaterialProperty>(props);

						//---Remove the property from list if it is _MainTex---//
						foreach (MaterialProperty prop in list)
						{
							if (prop.name == "_MainTex")
							{
								list.Remove(prop);
								break;
							}
						}

						//---Draw the custom GUI---//
						props = list.ToArray();
						iconEditor.materialEditor.customShaderGUI.OnGUI(iconEditor.materialEditor, props);
					}

				}
				//---If the default render shader---//
				else
				{
					//---Begine change check---//
					EditorGUI.BeginChangeCheck();

					//---Draw fix alpha edges toggle---//
					IconSettings.FixEdgesModes tmpFixEdges = (IconSettings.FixEdgesModes)EditorGUILayout.EnumPopup("Edge Fix Mode", iconEditor.currentIcon.iconSettings.fixEdgesMode);

					//---If toggle changed---//
					if (EditorGUI.EndChangeCheck())
					{
						//---Record object for undo and apply---//
						Undo.RecordObject(iconEditor.currentIcon, "Toggle Icon Premultiplied Alpha");
						iconEditor.currentIcon.iconSettings.fixEdgesMode = tmpFixEdges;

						//---Update and save icon---//
						iconEditor.currentIconSet.saveData = true;
						iconEditor.updateFlag = true;
					}
				}
				GUILayout.EndVertical();
			}

			//---If any fields changed---//
			if (EditorGUI.EndChangeCheck())
			{
				//---Save material info and update icon---//
				iconEditor.currentIcon.SaveMatInfo();
				iconEditor.updateFlag = true;
			}
		}

		public static void DrawExportControls(IconEditor iconEditor)
		{
			//---Begin change check---//
			EditorGUI.BeginChangeCheck();

			//---Draw export resolution field, limit to 8x8 - 2048x2048---//
			Vector2Int tmpExportRes = EditorGUILayout.Vector2IntField("Export Resolution", iconEditor.currentIcon.iconSettings.exportResolution);
			tmpExportRes.x = (int)Mathf.Clamp(tmpExportRes.x, 8, 2048);
			tmpExportRes.y = (int)Mathf.Clamp(tmpExportRes.y, 8, 2048);

			//---If resolution fields change---//
			if (EditorGUI.EndChangeCheck())
			{
				//---Record object for undo---//
				Undo.RecordObject(iconEditor.currentIcon, "Edit Icon Export Resolution");

				//---Apply settings---//
				iconEditor.currentIcon.iconSettings.exportResolution = tmpExportRes;

				//---Save and update icon---//
				iconEditor.currentIconSet.saveData = true;
				iconEditor.updateFlag = true;
			}

			//---Begin new change check---//
			EditorGUI.BeginChangeCheck();

			GUILayout.Space(2);
			GUILayout.BeginHorizontal();
			//---Add forward slash to end of export path if missing---//
			if (iconEditor.currentIcon.iconSettings.exportFolderPath[iconEditor.currentIcon.iconSettings.exportFolderPath.Length - 1] != '/')
				iconEditor.currentIcon.iconSettings.exportFolderPath += "/";

			//---Draw export folder field---//
			float tmpWdith = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 80;
			iconEditor.currentIcon.iconSettings.exportFolderPath = EditorGUILayout.TextField("Export Folder", iconEditor.currentIcon.iconSettings.exportFolderPath);

			//---If export path is blank, change it to default Export folder---//
			string tmpFolder = iconEditor.currentIcon.iconSettings.exportFolderPath;
			if (tmpFolder == "")
			{
				string[] split = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("RapidIconWindow")[0]).Split('/');
				string rapidIconRootFolder = "";
				for (int i = 0; i < split.Length - 3; i++)
					rapidIconRootFolder += split[i] + "/";

				iconEditor.currentIcon.iconSettings.exportFolderPath = rapidIconRootFolder + "Exports/";
			}

			//---Draw button to open file explorer to select export folder---//
			EditorGUIUtility.labelWidth = tmpWdith;
			if (GUILayout.Button("Browse", GUILayout.Width(100)))
			{
				string folder = EditorUtility.OpenFolderPanel("Export Folder", iconEditor.currentIcon.iconSettings.exportFolderPath, "");
				if (folder != "")
				{
					string dataPath = Application.dataPath;
					dataPath = dataPath.Substring(0, dataPath.LastIndexOf('/') + 1);
					folder = folder.Replace(dataPath, "");
					iconEditor.currentIcon.iconSettings.exportFolderPath = folder;
				}
			}
			GUILayout.EndHorizontal();

			//---Draw export path prefix/suffix fields---//
			tmpWdith = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 80;
			string exportPrefix = EditorGUILayout.TextField("Export Prefix", iconEditor.currentIcon.iconSettings.exportPrefix);
			string exportSuffix = EditorGUILayout.TextField("Export Suffix", iconEditor.currentIcon.iconSettings.exportSuffix);

			//---If any fields changes---//
			if (EditorGUI.EndChangeCheck())
			{
				//---Record object for undo---//
				Undo.RecordObject(iconEditor.currentIcon, "Edit Icon Export Path");

				//---Apply settings---//
				iconEditor.currentIcon.iconSettings.exportResolution = tmpExportRes;
				iconEditor.currentIcon.iconSettings.exportPrefix = exportPrefix;
				iconEditor.currentIcon.iconSettings.exportSuffix = exportSuffix;
				iconEditor.currentIconSet.saveData = true;
			}

			//---Add button apply to all selected icons---//
			if (GUILayout.Button("Copy to Other Icons"))
				CopyWindow.Init(iconEditor);

			//---Draw a separator---//
			GUILayout.Space(12);
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			Rect r = GUILayoutUtility.GetRect(iconEditor.sepWidth, 1);
			if (iconEditor.separatorTex == null)
			{
				if (EditorGUIUtility.isProSkin)
					iconEditor.separatorTex = Utils.CreateColourTexture(2, 2, new Color32(31, 31, 31, 255));
				else
					iconEditor.separatorTex = Utils.CreateColourTexture(2, 2, new Color32(153, 153, 153, 255));
			}
			GUI.DrawTexture(r, iconEditor.separatorTex);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.Space(12);

			//---Begin change check---//
			EditorGUI.BeginChangeCheck();

			//---Draw export name field---//
			string exportName = EditorGUILayout.TextField("Export Name", iconEditor.currentIcon.iconSettings.exportName);

			//---Draw label showing full name with prefix/suffix---//
			EditorGUIUtility.labelWidth = tmpWdith;
			GUILayout.Label("Full Export Name: " + iconEditor.currentIcon.iconSettings.exportPrefix + iconEditor.currentIcon.iconSettings.exportName + iconEditor.currentIcon.iconSettings.exportSuffix + ".png");

			//---If fields changesd, and export name is valid---//
			if (EditorGUI.EndChangeCheck())
			{
				if (exportName.Length > 0)
				{
					//---Record object for undo---//
					Undo.RecordObject(iconEditor.currentIcon, "Edit Icon Export Name");

					//---Apply and save---//
					iconEditor.currentIcon.iconSettings.exportName = exportName;
					iconEditor.currentIconSet.saveData = true;
				}
			}

			//---Draw button to export icon---//
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Export Icon", GUILayout.Width(160)))
			{
				int result = EditorUtility.DisplayDialogComplex("Export Icon", "Export All Variants?", "All Variants", "Cancel", "Currently Selected Variant");

				//---Export icon---//
				if (result != 1)
				{
					if (result == 2)
					{
						Utils.ExportIcon(iconEditor.currentIcon, false, iconEditor);
						Utils.FinishExportIcon(iconEditor.currentIcon);
					}
					else
					{
						//---Start asset editing---//
						AssetDatabase.StartAssetEditing();

						foreach (Icon icon in iconEditor.currentIconSet.icons)
						{
							Utils.ExportIcon(icon, iconEditor.currentIconSet.icons.Count > 1 ? true : false, iconEditor);
						}

						AssetDatabase.StopAssetEditing();
						Utils.FinishExportIconSet(iconEditor.currentIconSet);
					}

					//---Reset replaceAll---//
					iconEditor.replaceAll = false;
				}
			}

			//---Draw button to export all selected icons---//
			if (GUILayout.Button("Export Selected Icons", GUILayout.Width(160)))
			{
				int result = EditorUtility.DisplayDialogComplex("Export Icon", "Export All Variants?", "All Variants", "Cancel", "Currently Selected Variant");

				if (result != 1)
				{
					//---Loop through all selected icons---//
					int index = 1;
					List<string> exportPaths = new List<string>();
					foreach (IconSet iconSet in iconEditor.assetGrid.selectedIconSets)
					{
						if (result == 2)
						{
							Icon icon = iconSet.GetCurrentIcon();

							//--Show progress bar---//
							EditorUtility.DisplayProgressBar("Exporting Icons (" + index + "/" + iconEditor.assetGrid.selectedIconSets.Count + ")", iconSet.assetPath, ((float)index++ / iconEditor.assetGrid.selectedIconSets.Count));

							//---Export icon---//
							Utils.ExportIcon(icon, iconEditor.assetGrid.selectedIconSets.Count > 1 ? true : false, iconEditor);

							//---Stop asset editing and finish export---//
							Utils.FinishExportIcon(icon);
						}
						else
						{
							//--Show progress bar---//
							EditorUtility.DisplayProgressBar("Exporting Icons (" + index + "/" + iconEditor.assetGrid.selectedIconSets.Count + ")", iconSet.assetPath, ((float)index++ / iconEditor.assetGrid.selectedIconSets.Count));

							//---Start asset editing---//
							AssetDatabase.StartAssetEditing();

							foreach (Icon icon in iconSet.icons)
							{
								//---Export icon---//
								Utils.ExportIcon(icon, iconEditor.assetGrid.selectedIconSets.Count > 1 || iconSet.icons.Count > 1 ? true : false, iconEditor);
							}

							//---Stop asset editing and finish export---//
							AssetDatabase.StopAssetEditing();
							Utils.FinishExportIconSet(iconSet);
						}
					}

					//---Clear progress bar---//
					EditorUtility.ClearProgressBar();

					//---Reset replaceAll---//
					iconEditor.replaceAll = false;
				}
			}
			GUILayout.EndHorizontal();
		}
	}
}