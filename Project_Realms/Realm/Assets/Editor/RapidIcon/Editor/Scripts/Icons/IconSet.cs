using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace RapidIcon_1_7_1
{
	[Serializable]
	public class IconSet
	{
		public List<Icon> icons;
		[SerializeField] List<string> serialisedIconSettings;
		public int iconIndex;

		//Asset
		public string assetPath;
		public string assetName;
		public string assetShortenedName;
		public string folderPath;
		public UnityEngine.Object assetObject;
		public string assetGUID;
		public string[] GUIDs; //No longer used, but kept for backwards compatibility of old save data

		//Misc
		public Texture2D selectionTexture;
		public bool selected;
		public bool saveData;
		public bool deleted;
		public int assetGridIconIndex;

		public IconSet(AssetGrid assetGrid, ObjectPathPair objectPathPair)
		{
			icons = new List<Icon>();
			iconIndex = 0;

			AddDefaultIcon(assetGrid, objectPathPair);
		}

		public Icon GetCurrentIcon()
		{
			return icons[iconIndex];
		}

		public Icon AddUninitialisedIcon()
		{
			Icon icon = ScriptableObject.CreateInstance<Icon>();
			icon.iconSettings = new IconSettings();

			icon.parentIconSet = this;
			icons.Add(icon);

			return icon;
		}

		public Icon AddDefaultIcon(AssetGrid assetGrid, ObjectPathPair objectPathPair)
		{
			Icon icon = ScriptableObject.CreateInstance<Icon>();
			icon.Init(Shader.Find("RapidIcon/ObjectRender"), assetGrid.rapidIconRootFolder, (GameObject)objectPathPair.UnityEngine_object);

			//---Add the new icon to the icon set---//
			icon.parentIconSet = this;
			icons.Add(icon);

			//---Set the asset object and path of the icon---//
			assetObject = objectPathPair.UnityEngine_object;
			assetPath = objectPathPair.path;

			//---Get the asset as a GameObject, zero the postion if it's very small in magnitude---//
			GameObject go = (GameObject)assetObject;
			if (icon.iconSettings.objectPosition.magnitude < 0.0001f)
				icon.iconSettings.objectPosition = Vector3.zero;

			//---Zero the GameObject euler angles if they're very small in magnitude---//
			icon.iconSettings.objectRotation = go.transform.eulerAngles;
			if (icon.iconSettings.objectRotation.magnitude < 0.0001f)
				icon.iconSettings.objectRotation = Vector3.zero;

			//---Set the object scale variable---//
			icon.iconSettings.objectScale = go.transform.localScale;

			//---Get default object position to centre it in the icon---//
			Bounds bounds = Utils.GetObjectBounds(this);
			icon.iconSettings.objectPosition = Utils.GetObjectAutoOffset(icon, bounds);

			//---Get default camera settings to fit the object in the icon render---//
			float camAuto = Utils.GetCameraAuto(icon, bounds);
			icon.iconSettings.cameraSize = camAuto;
			icon.iconSettings.cameraPosition = Vector3.one * camAuto;

			//---Render the preview---//
			icon.previewRender = Utils.RenderIcon(icon, assetGrid.previewSize, (int)(((float)icon.iconSettings.exportResolution.y / (float)icon.iconSettings.exportResolution.x) * assetGrid.previewSize));
			icon.previewRender.hideFlags = HideFlags.DontSave;

			//---Set asset name and shortened asset name---//
			string[] split;
			split = assetPath.Split('/');
			assetName = split[split.Length - 1];
			if (assetName.Length > 19)
				assetShortenedName = assetName.Substring(0, 16) + "...";
			else
				assetShortenedName = assetName;

			//---Set folder path---//
			split = assetPath.Split('/');
			folderPath = "";
			for (int i = 0; i < split.Length - 1; i++)
				folderPath += split[i] + (i < split.Length - 2 ? "/" : "");

			//---Set export name---//
			icon.iconSettings.exportName = assetName;
			int extensionPos = icon.iconSettings.exportName.LastIndexOf('.');
			icon.iconSettings.exportName = icon.iconSettings.exportName.Substring(0, extensionPos);

			//---Set selection texture---//
			selectionTexture = assetGrid.assetSelectionTextures[1];

			//---Load animations---//
			LoadDefaultAnimationClip(icon);

			return icon;
		}

		public void LoadDefaultAnimationClip(Icon icon)
		{
			//---Load the animation included in the imported assset (if there is one)---//
			AnimationClip clip = (AnimationClip)AssetDatabase.LoadAssetAtPath(assetPath, typeof(AnimationClip));
			icon.iconSettings.animationClip = clip;

			//---If no clip loaded, then try and get one from the Animator component (if there is one)---//
			if (icon.iconSettings.animationClip == null)
			{
				//---Try to get animator component from prefab---//
				GameObject gameObject = null;
				Animator animator = null;
				try
				{
					gameObject = PrefabUtility.LoadPrefabContents(assetPath);
					animator = gameObject.GetComponent<Animator>();
				}
				catch
				{ /*Not a prefab - don't need to do anything just catch the error*/ }

				//---If prefab has Animator component, then try to get first animation clip---//
				if (animator != null)
				{
                    UnityEditor.Animations.AnimatorController animatorController = animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
					if (animatorController != null && animatorController.animationClips != null && animatorController.animationClips.Length > 0)
						icon.iconSettings.animationClip = animatorController.animationClips[0];
				}

				//---Unload the prefab---//
				if (gameObject != null)
					PrefabUtility.UnloadPrefabContents(gameObject);
			}
		}

		public void CompleteLoadData(bool loadFixEdgesModeFromSave)
		{
			//---Load asset from GUID---//
			if (assetGUID != string.Empty)
			{
				assetObject = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(assetGUID));
			}

			icons = new List<Icon>();

			//---Complete load data of icons---//
			foreach (string iconSettings in serialisedIconSettings)
			{
				Icon icon = AddUninitialisedIcon();
				JsonUtility.FromJsonOverwrite(iconSettings, icon.iconSettings);

				icon.CompleteLoadData(this, loadFixEdgesModeFromSave);
				icon.LoadMatInfo();
			}
		}

		public void PrepareForSaveData()
		{
			//---Get GUID of asset---//
			assetGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(assetObject));
			assetObject = null;

			//---Prepare icons for save data---//
			serialisedIconSettings = new List<string>();
			foreach (Icon icon in icons)
			{
				icon.PrepareForSaveData();
				serialisedIconSettings.Add(JsonUtility.ToJson(icon.iconSettings));
			}
		}
	}
}