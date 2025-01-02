using System;
using System.Collections.Generic;
using UnityEngine;

namespace RapidIcon_1_7_1
{
	[Serializable]
	public class IconSettings
	{
		//Object Settings
		public Vector3 objectPosition;
		public Vector3 objectRotation;
		public Vector3 objectScale;

		//Hierarchy Settings
		[NonSerialized] public Dictionary<string, bool> subObjectEnables;
		public List<string> soeStrings;
		public List<bool> soeBools;

		//Camera Settings
		public Vector3 cameraPosition;
		public Vector3 cameraTarget;
		public bool autoPosition;
		public bool autoScale;
		public bool cameraOrtho;
		public float cameraFov;
		public float cameraSize;
		public float camerasScaleFactor;
		public float perspLastScale;

		//Lighting Settings
		public Color lightColour;
		public Vector3 lightDir;
		public float lightIntensity;
		public Color ambientLightColour;

		//Animation Settings
		public AnimationClip animationClip;
		public float animationOffset;
		public string animationPath;

		//Post-Processing Settings
		public List<Material> postProcessingMaterials;
		public Dictionary<Material, String> materialDisplayNames;
		public Dictionary<Material, bool> materialToggles;
		public List<Icon.MaterialInfo> matInfo;
		public enum FixEdgesModes { None, Regular, WithDepthTexture };
		public FixEdgesModes fixEdgesMode;
		public FilterMode filterMode;

		//Export Settings
		public string exportFolderPath;
		public string exportName;
		public string exportPrefix;
		public string exportSuffix;
		public Vector2Int exportResolution;

		public IconSettings()
		{
			//Do nothing
		}

		public IconSettings(Shader objRenderShader, string rapidIconRootFolder, GameObject rootObject)
		{
			//---Initialise Icon---//
			//Object Settings
			objectPosition = Vector3.zero;
			objectRotation = Vector3.zero;
			objectScale = Vector3.one;
			autoPosition = true;

			//Hierarchy Settings
			subObjectEnables = new Dictionary<string, bool>();
			SetSubObjectEnables(rootObject, 0, "");

			//Camera Settings
			cameraPosition = new Vector3(1, Mathf.Sqrt(2), 1);
			perspLastScale = 1;
			cameraOrtho = true;
			cameraFov = 60;
			cameraSize = 5;
			camerasScaleFactor = 1;
			cameraTarget = Vector3.zero;
			autoScale = true;

			//Lighting Settings
			ambientLightColour = Color.gray;
			lightColour = Color.white;
			lightDir = new Vector3(50, -30, 0);
			lightIntensity = 1;

			//Post-Processing Settings
			postProcessingMaterials = new List<Material>();
			matInfo = new List<Icon.MaterialInfo>();
			materialDisplayNames = new Dictionary<Material, string>();
			materialToggles = new Dictionary<Material, bool>();
			filterMode = FilterMode.Point;
			fixEdgesMode = FixEdgesModes.Regular;
			Material defaultRender = new Material(objRenderShader);
			postProcessingMaterials.Add(defaultRender);
			materialDisplayNames.Add(defaultRender, "Object Render");
			materialToggles.Add(defaultRender, true);

			//Export Settings
			exportResolution = new Vector2Int(256, 256);
			exportFolderPath = rapidIconRootFolder;
			exportFolderPath += "Exports/";
		}

		void SetSubObjectEnables(GameObject obj, int childIdx, string lastPath)
		{
			string path = lastPath + "/" + childIdx;
			subObjectEnables.Add(path, true);

			for (int i = 0; i < obj.transform.childCount; i++)
			{
				SetSubObjectEnables(obj.transform.GetChild(i).gameObject, i, path);
			}
		}
	}
}