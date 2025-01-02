using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace RapidIcon_1_7_1
{
	static class Utils
	{
		public static Bounds GetObjectBounds(IconSet iconSet)
		{
			//---Get object---//
			GameObject go = (GameObject)iconSet.assetObject;

			//---Store the prefabs position before zero-ing it---//
			Vector3 prefabPos = go.transform.position;
			go.transform.position = Vector3.zero;

			//---Create a bounds object and encapsulate the bounds of the object mesh---//
			MeshRenderer mr = go.GetComponent<MeshRenderer>();
			Bounds bounds = new Bounds(Vector3.zero, 0.000001f * Vector3.one);
			if (mr != null)
				bounds.Encapsulate(mr.bounds);
			else
			{
				SkinnedMeshRenderer smr = go.GetComponent<SkinnedMeshRenderer>();
				if (smr != null)
					bounds.Encapsulate(smr.bounds);
			}

			//---Encapsulate the bounds of the object's children objects as well---//
			Utils.EncapsulateChildBounds(go.transform, ref bounds);

			//---Reset the prefab postion to the stored value---//
			go.transform.position = prefabPos;

			return bounds;
		}

		public static Vector3 GetObjectAutoOffset(Icon icon, Bounds bounds)
		{
			//Apply the offset to the icon object position
			return -bounds.center;
		}

		public static float GetCameraAuto(Icon icon, Bounds bounds)
		{
			//---Scale camera size and position so that the object fits in the render---//
			Matrix4x4 trs = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 45, 0), 1.05f * Vector3.one);
			Vector3 corner = new Vector3(bounds.extents.x, bounds.extents.y, bounds.extents.z);
			corner = trs * corner;

			Vector2 refB2 = new Vector2(0.74f, 0.53f);
			Vector2 b2 = refB2 * corner.magnitude;

			return b2.magnitude;
		}

		public static void ExportIcon(Icon icon, bool inBatchExport, IconEditor iconEditor)
		{
			//---Create export folder if it doesn't already exist---//
			if (!Directory.Exists(icon.iconSettings.exportFolderPath))
				Directory.CreateDirectory(icon.iconSettings.exportFolderPath);

			//---Get the full export file name---//
			string fileName = icon.iconSettings.exportFolderPath + icon.iconSettings.exportPrefix + icon.iconSettings.exportName + icon.iconSettings.exportSuffix + ".png";

			//---If it exists already, check if user want's to replace it---//
			if (System.IO.File.Exists(fileName) && !iconEditor.replaceAll)
			{
				int result = 1;

				if (inBatchExport)
					result = EditorUtility.DisplayDialogComplex("Replace File?", fileName + " already exists, do you want to replace it?", "Replace", "Skip", "Replace All");
				else
					result = EditorUtility.DisplayDialog("Replace File?", fileName + " already exists, do you want to replace it?", "Replace", "Cancel") ? 0 : 1;

				if (result == 1)
					return;
				else if (result == 2)
				{
					iconEditor.replaceAll = true;
				}
			}

			//---Delete any existing file---//
			if (AssetDatabase.IsValidFolder(icon.iconSettings.exportFolderPath))
				AssetDatabase.DeleteAsset(fileName);

			//---Render the icon---//
			Texture2D exportRender = Utils.RenderIcon(icon, icon.iconSettings.exportResolution.x, icon.iconSettings.exportResolution.y);

			//---Encode to png and save file---//
			byte[] bytes = exportRender.EncodeToPNG();
			File.WriteAllBytes(fileName, bytes);
		}

		public static void FinishExportIconSet(List<IconSet> iconSets)
		{
			//---Loop through all icons in list and finish export---//
			foreach (IconSet iconSet in iconSets)
				FinishExportIconSet(iconSet);
		}
		
		public static void FinishExportIcon(Icon icon)
		{
			//---Refresh asset database and get full filename---//
			AssetDatabase.Refresh();
			string fileName = icon.iconSettings.exportFolderPath + icon.iconSettings.exportPrefix + icon.iconSettings.exportName + icon.iconSettings.exportSuffix + ".png";

			if (AssetDatabase.IsValidFolder(icon.iconSettings.exportFolderPath.Substring(0, icon.iconSettings.exportFolderPath.Length - 1)))
			{
				//---Set the importer settings---//
				TextureImporter textureImporter = (TextureImporter)AssetImporter.GetAtPath(fileName);
				textureImporter.alphaIsTransparency = true;
				textureImporter.npotScale = TextureImporterNPOTScale.None;
				textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
				textureImporter.SaveAndReimport();
				textureImporter.filterMode = icon.iconSettings.filterMode;
			}
			
			//---Refresh asset database---//
			AssetDatabase.Refresh();
		}

		public static void FinishExportIconSet(IconSet iconSet)
		{
			foreach (Icon icon in iconSet.icons)
			{
				//---Refresh asset database and get full filename---//
				AssetDatabase.Refresh();
				string fileName = icon.iconSettings.exportFolderPath + icon.iconSettings.exportPrefix + icon.iconSettings.exportName + icon.iconSettings.exportSuffix + ".png";

				if (AssetDatabase.IsValidFolder(icon.iconSettings.exportFolderPath.Substring(0, icon.iconSettings.exportFolderPath.Length - 1)))
				{
					//---Set the importer settings---//
					TextureImporter textureImporter = (TextureImporter)AssetImporter.GetAtPath(fileName);
					textureImporter.alphaIsTransparency = true;
					textureImporter.npotScale = TextureImporterNPOTScale.None;
					textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
					textureImporter.SaveAndReimport();
					textureImporter.filterMode = icon.iconSettings.filterMode;
				}
			}
			//---Refresh asset database---//
			AssetDatabase.Refresh();
		}

		public static void UpdateIcon(Icon icon, IconEditor iconEditor)
		{
			//---Get the render resolution---//
			iconEditor.renderResolution = Utils.MutiplyVector2IntByFloat(iconEditor.currentIcon.iconSettings.exportResolution, iconEditor.resMultiplyers[iconEditor.resMultiplyerIndex]);

			//---Update the icon render---//
			icon.UpdateIcon(iconEditor.renderResolution, new Vector2Int(128, (int)(((float)iconEditor.renderResolution.y / (float)iconEditor.renderResolution.x) * 128)));
		}

		public static void ApplyToAllSelectedIcons(int tab, IconEditor iconEditor)
		{
			//---Display confirmation window---//
			int result = EditorUtility.DisplayDialogComplex("Apply to All Selected Icons", "Would you like to apply only " + iconEditor.tabNames[tab] + " settings, or all settings?", iconEditor.tabNames[tab] + " Settings Only", "Cancel", "All Settings");

			if (result == 1) //cancel
				return;
			else
			{
				//---Record object for undo---//
				foreach (IconSet iconSet in iconEditor.assetGrid.selectedIconSets)
				{
					iconSet.GetCurrentIcon().SaveMatInfo();
					Undo.RegisterCompleteObjectUndo(iconSet.GetCurrentIcon(), "Apply to all icons");
				}

				//---Loop through selected icons---//
				int index = 1;
				foreach (IconSet iconSet in iconEditor.assetGrid.selectedIconSets)
				{
					//---If not the current icon---//
					if (iconSet.GetCurrentIcon() != iconEditor.currentIcon)
					{
						//---Display progress bar---//
						EditorUtility.DisplayProgressBar("Updating Icons (" + index + "/" + (iconEditor.assetGrid.selectedIconSets.Count - 1) + ")", iconSet.assetPath, ((float)index++ / (iconEditor.assetGrid.selectedIconSets.Count - 1)));

						Vector2Int oldExportRes = iconSet.GetCurrentIcon().iconSettings.exportResolution;
						//---Copy icon settings from current icon to this icon---//
						CopyIconSettings(iconEditor.currentIcon, iconSet.GetCurrentIcon(), result == 2 ? -1 : tab);

						//--Save the icon---//
						iconSet.GetCurrentIcon().SaveMatInfo();
						iconSet.saveData = true;

						//---Update icon if all settings copied, or if any tab other than hierarchy/export copied (or export resolution changed)---//
						if (result == 2
							|| (tab != 1 && tab != 6)
							|| (tab == 6 && iconSet.GetCurrentIcon().iconSettings.exportResolution != oldExportRes))
						{
							Utils.UpdateIcon(iconSet.GetCurrentIcon(), iconEditor);
						}
					}
				}

				//---Clear the progress bar---//
				EditorUtility.ClearProgressBar();
			}
		}

		public static void CopyIconSettings(Icon from, Icon to, int tab)
		{
			if (from == to)
				return;

			//---Copy object settings---//
			if (tab == 0 || tab == -1)
			{
				to.iconSettings.objectPosition = from.iconSettings.objectPosition;
				to.iconSettings.objectRotation = from.iconSettings.objectRotation;
				to.iconSettings.objectScale = from.iconSettings.objectScale;
				if (from.iconSettings.autoPosition)
				{
					to.iconSettings.objectPosition = Utils.GetObjectAutoOffset(to, Utils.GetObjectBounds(to.parentIconSet));
				}
				to.iconSettings.autoPosition = from.iconSettings.autoPosition;
			}

			//---Copy camera settings---//
			if (tab == 2 || tab == -1)
			{
				to.iconSettings.cameraPosition = from.iconSettings.cameraPosition;
				to.iconSettings.cameraOrtho = from.iconSettings.cameraOrtho;
				to.iconSettings.cameraFov = from.iconSettings.cameraFov;
				to.iconSettings.cameraSize = from.iconSettings.cameraSize;
				to.iconSettings.camerasScaleFactor = from.iconSettings.camerasScaleFactor;
				to.iconSettings.perspLastScale = from.iconSettings.perspLastScale;
				to.iconSettings.cameraTarget = from.iconSettings.cameraTarget;
				if (from.iconSettings.autoScale)
				{
					to.iconSettings.cameraSize = Utils.GetCameraAuto(to, Utils.GetObjectBounds(to.parentIconSet));
				}
				to.iconSettings.autoScale = from.iconSettings.autoScale;
			}

			//--Copy lighting settings---//
			if (tab == 3 || tab == -1)
			{
				to.iconSettings.ambientLightColour = from.iconSettings.ambientLightColour;
				to.iconSettings.lightColour = from.iconSettings.lightColour;
				to.iconSettings.lightDir = from.iconSettings.lightDir;
				to.iconSettings.lightIntensity = from.iconSettings.lightIntensity;
			}

			//---Copy animation settings---//
			if (tab == 4 || tab == -1)
			{
				to.iconSettings.animationClip = from.iconSettings.animationClip;
				to.iconSettings.animationOffset = from.iconSettings.animationOffset;
			}

			//---Copy post-processing settings---//
			if (tab == 5 || tab == -1)
			{
				foreach (Material mat in to.iconSettings.postProcessingMaterials)
					Editor.DestroyImmediate(mat);

				to.iconSettings.postProcessingMaterials.Clear();

				foreach (Material mat in from.iconSettings.postProcessingMaterials)
				{
					Material newMat = new Material(mat);
					to.iconSettings.postProcessingMaterials.Add(newMat);
					to.iconSettings.materialDisplayNames.Add(newMat, from.iconSettings.materialDisplayNames[mat]);
					to.iconSettings.materialToggles.Add(newMat, from.iconSettings.materialToggles[mat]);
				}

				to.iconSettings.fixEdgesMode = from.iconSettings.fixEdgesMode;
				to.iconSettings.filterMode = from.iconSettings.filterMode;
			}

			//---Copy export settings---//
			if (tab == 6 || tab == -1)
			{
				to.iconSettings.exportResolution = from.iconSettings.exportResolution;
				to.iconSettings.exportFolderPath = from.iconSettings.exportFolderPath;
				to.iconSettings.exportPrefix = from.iconSettings.exportPrefix;
				to.iconSettings.exportSuffix = from.iconSettings.exportSuffix;
			}

			//---Save the icon---//
			to.parentIconSet.saveData = true;
		}

		public static Texture2D CreateColourTexture(int width, int height, Color c)
		{
			//---Create new texture---//
			Texture2D tex = new Texture2D(width, height);

			//---Set pixel colours---//
			Color[] pixels = Enumerable.Repeat(c, width * height).ToArray();
			tex.SetPixels(pixels);

			//---Apply changes and set filter mode---//
			tex.Apply();
			tex.filterMode = FilterMode.Point;

			return tex;
		}

		public static Texture2D RenderIcon(Icon icon, int width = 128, int height = 128)
		{
			//---Fix width/height---//
			width = Mathf.Clamp(width, 8, 2048);
			height = Mathf.Clamp(height, 8, 2048);

			//---Create stage and scene---//
			var scene = EditorSceneManager.NewPreviewScene();
			if (scene == null)
			{
				Debug.LogError("Error creating RapidIcon preview scene");
				return Utils.CreateColourTexture(width, height, Color.clear);
			}

			var stage = ScriptableObject.CreateInstance<RapidIconStage>();
			if (stage == null)
			{
				Debug.LogError("Error creating RapidIcon stage");
				return Utils.CreateColourTexture(width, height, Color.clear);
			}

			stage.SetScene(scene);

			//---Go to stage---//
			StageUtility.GoToStage(stage, true);

			//---Setup scene---//
			stage.SetupScene(icon);

			//and render icon---//
			Texture2D render = stage.RenderIcon(width, height);

			//---Fix alpha edges---//
			if (icon.iconSettings.fixEdgesMode == IconSettings.FixEdgesModes.Regular || icon.iconSettings.fixEdgesMode == IconSettings.FixEdgesModes.WithDepthTexture)
				render = Utils.FixAlphaEdges(render, icon.iconSettings.fixEdgesMode == IconSettings.FixEdgesModes.WithDepthTexture);

			//---Known bug---//
			if (icon.iconSettings.materialToggles == null)
			{
				icon.LoadMatInfo();
				Debug.LogError("[RapidIcon] Undo Error: This is a known bug, if you \"Apply to All Selected Icons\" and then try to undo after changing your icon selection, the tool will not be able to undo the changes.");
			}

			//---Apply post-processing shaders---//
			Texture2D img = CreateColourTexture(width, height, Color.clear);

			foreach (Material m in icon.iconSettings.postProcessingMaterials)
			{
				if (icon.iconSettings.materialToggles != null)
				{
					if (icon.iconSettings.materialToggles[m])
					{
						var rtd = new RenderTextureDescriptor(img.width, img.height) { depthBufferBits = 24, msaaSamples = 8, useMipMap = false, sRGB = true };
						var rt = new RenderTexture(rtd);

						if (m == null)
							continue;

						if (m.shader.name == "RapidIcon/ObjectRender")
							m.SetTexture("_Render", render);

						Graphics.Blit(img, rt, m);

						RenderTexture.active = rt;
						img = new Texture2D(img.width, img.height);
						img.ReadPixels(new Rect(0, 0, img.width, img.height), 0, 0);
						img.Apply();
						RenderTexture.active = null;
						rt.Release();
					}
				}
			}

			//---Apply filter mode---//
			img.filterMode = icon.iconSettings.filterMode;

			//---Cleanup stage and scene---//
			StageUtility.GoToMainStage();
			EditorSceneManager.ClosePreviewScene(scene);
			ScriptableObject.DestroyImmediate(stage);

			return img;
		}

		public static void CheckCurrentIconRender(IconEditor iconEditor)
		{
			//---If the current icon doesn't have a full render, then render one---//
			if (!iconEditor.currentIcon.fullRender)
			{
				iconEditor.renderResolution = Utils.MutiplyVector2IntByFloat(iconEditor.currentIcon.iconSettings.exportResolution, iconEditor.resMultiplyers[iconEditor.resMultiplyerIndex]);
				iconEditor.currentIcon.fullRender = Utils.RenderIcon(iconEditor.currentIcon, iconEditor.renderResolution.x, iconEditor.renderResolution.y);
				iconEditor.currentIcon.fullRender.hideFlags = HideFlags.DontSave;
			}
		}

		public static Texture2D FixAlphaEdges(Texture2D tex, bool useDepthTex)
		{
			//---Create render texture---//
			var rtd = new RenderTextureDescriptor(tex.width, tex.height) { depthBufferBits = 24, msaaSamples = 8, useMipMap = false, sRGB = true };
			var rt = new RenderTexture(rtd);

			//---Blit texture to render texture using ImgShader---//
			Material mat = new Material(Shader.Find("RapidIcon/ImgShader"));
			mat.SetInt("_UseDepthTexture", useDepthTex ? 1 : 0);
			Graphics.Blit(tex, rt, mat);

			//---Copy the render texture to Texture2D---//
			RenderTexture.active = rt;
			Texture2D baked = new Texture2D(tex.width, tex.height);
			baked.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
			baked.Apply();

			//---Cleanup---//
			RenderTexture.active = null;
			rt.Release();

			return baked;
		}

		public static void SaveIconSetData(IconSetData iconSetData)
		{
			//---Perpare each icon for saving---//
			foreach (IconSet iconSet in iconSetData.iconSets)
			{
				iconSet.PrepareForSaveData();
			}

			//---Save the data---//
			string data = JsonUtility.ToJson(iconSetData);
			EditorPrefs.SetString(PlayerSettings.productName + "RapidIconData", data);
		}

		public static IconSetData LoadIconSetData()
		{
			//---Load the icon data---//
			string data = EditorPrefs.GetString(PlayerSettings.productName + "RapidIconData");
			IconSetData iconData = JsonUtility.FromJson<IconSetData>(data);

			//---Complete the load data for each icon---//
			if (iconData != null)
			{
				//---Handle version control---//
				VersionControl.CheckUpdate(iconData.iconSets);
				bool loadFixEdgesModeFromSave = !(VersionControl.GetStoredVersion() < new VersionControl.Version("1.6.2")); //Keep at 1.6.2, do not update to latest version

				foreach (IconSet iconSet in iconData.iconSets)
				{
					iconSet.CompleteLoadData(loadFixEdgesModeFromSave);
				}
			}

			//---Update current stored version---//
			VersionControl.UpdateStoredVersion();

			return iconData;
		}

		//---Extension method to convert Vector2 to Vector2Int---//
		public static Vector2Int ToVector2Int(this Vector2 v)
		{
			return new Vector2Int((int)v.x, (int)v.y);
		}

		public static void EncapsulateChildBounds(Transform t, ref Bounds bounds)
		{
			//---For each child object, encapsulate its bounds---//
			MeshRenderer mr;
			for (int i = 0; i < t.childCount; i++)
			{
				mr = t.GetChild(i).GetComponent<MeshRenderer>();
				if (mr != null)
					bounds.Encapsulate(mr.bounds);
				else
				{
					SkinnedMeshRenderer smr = t.GetChild(i).GetComponent<SkinnedMeshRenderer>();
					if (smr != null)
						bounds.Encapsulate(smr.bounds);
				}

				EncapsulateChildBounds(t.GetChild(i), ref bounds);
			}
		}

		[MenuItem("Tools/RapidIcon Utilities/Delete All Saved Data")]
		static void DeleteEditorPrefs()
		{
			if (EditorUtility.DisplayDialog("Confirm", "Are you sure you want to delete all RapidIcon data? This will delete all of your icon settings and cannot be undone", "Delete", "Cancel"))
			{
				EditorPrefs.DeleteKey(PlayerSettings.productName + "RapidIconOpenedFolders");
				EditorPrefs.DeleteKey(PlayerSettings.productName + "RapidIconSelectedFolders");

				EditorPrefs.DeleteKey(PlayerSettings.productName + "RapidIconSepPosLeft");

				EditorPrefs.DeleteKey(PlayerSettings.productName + "RapidIconSelectedAssets");
				EditorPrefs.DeleteKey(PlayerSettings.productName + "RapidIconAssetGridScroll");

				EditorPrefs.DeleteKey(PlayerSettings.productName + "RapidIconSepPosRight");

				EditorPrefs.DeleteKey(PlayerSettings.productName + "RapidIconSepPosPreview");
				EditorPrefs.DeleteKey(PlayerSettings.productName + "RapidIconPreviewResIdx");
				EditorPrefs.DeleteKey(PlayerSettings.productName + "RapidIconPreviewZoomIdx");
				EditorPrefs.DeleteKey(PlayerSettings.productName + "RapidIconEditorTab");
				EditorPrefs.DeleteKey(PlayerSettings.productName + "RapidIconData");

				EditorPrefs.DeleteKey(PlayerSettings.productName + "RapidIconIconsRefreshed");

				EditorPrefs.DeleteKey(PlayerSettings.productName + "RapidIconFilterIdx");

				EditorPrefs.DeleteKey(PlayerSettings.productName + "RapidIconVersion");
			}
		}

		[MenuItem("Tools/RapidIcon Utilities/Don't Save On Close")]
		static void DontSaveOnExit()
		{
			RapidIconWindow.dontSaveOnExit = true;
		}

		public static Vector2Int MutiplyVector2IntByFloat(Vector2Int vec, float f)
		{
			Vector2Int res = vec;
			res.x = (int)(res.x * f);
			res.y = (int)(res.y * f);

			return res;
		}
	}
}