using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RapidIcon_1_7_1
{
	[Serializable]
	public class Icon : ScriptableObject
	{
		//---MatProperty Definition---//
		[Serializable]
		public struct MatProperty<T>
		{
			public string name;
			public T value;

			public MatProperty(string n, T v)
			{
				name = n;
				value = v;
			}
		}

		//---MaterialInfo Definition---//
		[Serializable]
		public struct MaterialInfo
		{
			public string shaderName;
			public string displayName;
			public bool toggle;
			public List<MatProperty<int>> intProperties;
			public List<MatProperty<float>> floatProperties;
			public List<MatProperty<float>> rangeProperties;
			public List<MatProperty<Color>> colourProperties;
			public List<MatProperty<Vector4>> vectorProperties;
			public List<MatProperty<string>> textureProperties;

			//---Constructor---//
			public MaterialInfo(string n)
			{
				shaderName = n;
				displayName = n;
				toggle = true;
				intProperties = new List<MatProperty<int>>();
				floatProperties = new List<MatProperty<float>>();
				rangeProperties = new List<MatProperty<float>>();
				colourProperties = new List<MatProperty<Color>>();
				vectorProperties = new List<MatProperty<Vector4>>();
				textureProperties = new List<MatProperty<string>>();
			}
		}

		//---Variable Definitions---//
		//Icon Set
		[NonSerialized] public IconSet parentIconSet;

		//Icon Settings
		public IconSettings iconSettings;

		//Renders
		public Texture2D previewRender;
		public Texture2D fullRender;

		public void Init(Shader objRenderShader, string rapidIconRootFolder, GameObject rootObject)
		{
			iconSettings = new IconSettings(objRenderShader, rapidIconRootFolder, rootObject);
		}

		public void UpdateIcon(Vector2Int fullRenderSize)
		{
			fullRender = Utils.RenderIcon(this, fullRenderSize.x, fullRenderSize.y);
		}

		public void UpdateIcon(Vector2Int fullRenderSize, Vector2Int previewSize)
		{
			previewRender = Utils.RenderIcon(this, previewSize.x, previewSize.y);
			fullRender = Utils.RenderIcon(this, fullRenderSize.x, fullRenderSize.y);
		}

		public void CompleteLoadData(IconSet iconSet, bool loadFixEdgesMode)
		{
			//---Set parent icon set---//
			parentIconSet = iconSet;

			//---Load animation from path---//
			if (iconSettings.animationPath != string.Empty)
			{
				iconSettings.animationClip = (AnimationClip)AssetDatabase.LoadAssetAtPath(iconSettings.animationPath, typeof(AnimationClip));
			}

			//---Load the post-processing material info---//
			LoadMatInfo(loadFixEdgesMode);

			//---Load the sub object enables dictionary---//
			iconSettings.subObjectEnables = new Dictionary<string, bool>();
			int idx = 0;
			foreach (string s in iconSettings.soeStrings)
			{
				iconSettings.subObjectEnables.Add(s, iconSettings.soeBools[idx++]);
			}
		}

		public void PrepareForSaveData()
		{
			//---Get path of animation---//
			iconSettings.animationPath = AssetDatabase.GetAssetPath(iconSettings.animationClip);

			//---Clear the renders and assset object---//
			previewRender = fullRender = null;

			//---Save the post-processing material info---//
			SaveMatInfo();

			//---Clear the post-processing list/dictionaries---//
			iconSettings.postProcessingMaterials.Clear();
			iconSettings.materialDisplayNames.Clear();
			iconSettings.materialToggles.Clear();

			//---Save the sub object enables dictionary---//
			iconSettings.soeStrings = iconSettings.subObjectEnables.Keys.ToList();
			iconSettings.soeBools = iconSettings.subObjectEnables.Values.ToList();
		}

		public void LoadMatInfo(bool loadFixEdgesMode = true)
		{
			//---Initialise post-processing list/dictionaries---//
			iconSettings.postProcessingMaterials = new List<Material>();
			iconSettings.materialDisplayNames = new Dictionary<Material, string>();
			iconSettings.materialToggles = new Dictionary<Material, bool>();

			if (iconSettings.matInfo != null && iconSettings.matInfo.Count > 0)
			{
				//---For each material, load it's properties---//
				foreach (MaterialInfo mi in iconSettings.matInfo)
				{
					//Create new material with the correct shader
					Material m = new Material(Shader.Find(mi.shaderName));

					//Set int properties (2021.1 or newer)
					bool reg = false;
					bool depthTex = false;
					foreach (MatProperty<int> property in mi.intProperties)
					{
						//Custom property for default render shader
						if (mi.shaderName == "RapidIcon/ObjectRender")
						{
							if (property.name == "custom_PreMulAlpha")
							{
								reg = (property.value == 1 ? true : false);
							}


							if (property.name == "custom_UseDepthTexture")
							{
								depthTex = (property.value == 1 ? true : false);
							}
						}

#if UNITY_2021_1_OR_NEWER //Not implemented in older versions of Unity
						m.SetInt(property.name, property.value);
#endif
					}

					//---Handle default render shader---//
					if (loadFixEdgesMode && mi.shaderName == "RapidIcon/ObjectRender")
					{
						if (reg && depthTex)
							iconSettings.fixEdgesMode = IconSettings.FixEdgesModes.WithDepthTexture;
						else if (reg)
							iconSettings.fixEdgesMode = IconSettings.FixEdgesModes.Regular;
						else
							iconSettings.fixEdgesMode = IconSettings.FixEdgesModes.None;
					}

					//Set float properties
					foreach (MatProperty<float> property in mi.floatProperties)
						m.SetFloat(property.name, property.value);

					//Set range properties
					foreach (MatProperty<float> property in mi.rangeProperties)
						m.SetFloat(property.name, property.value);

					//Set colour properties
					foreach (MatProperty<Color> property in mi.colourProperties)
						m.SetColor(property.name, property.value);

					//Set vector properties
					foreach (MatProperty<Vector4> property in mi.vectorProperties)
						m.SetVector(property.name, property.value);

					//Set texture properties
					foreach (MatProperty<string> property in mi.textureProperties)
					{
						//Load the texture from GUID, if not null
						if (property.name != "null")
						{
							m.SetTexture(property.name, (Texture2D)AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(property.value)));
						}
					}

					//Add the material to the post-processing stack
					iconSettings.postProcessingMaterials.Add(m);
					iconSettings.materialDisplayNames.Add(m, mi.displayName);
					iconSettings.materialToggles.Add(m, mi.toggle);
				}
			}
		}

		public void SaveMatInfo()
		{
			//---Clear the matInfo and then store the info for each material in the post-processing stack---//
			iconSettings.matInfo.Clear();
			foreach (Material mat in iconSettings.postProcessingMaterials)
			{
				if (mat == null)
					continue;

				//---Create the new material info---//
				MaterialInfo mi = new MaterialInfo(mat.shader.name);

				//---Store all of the material's properties---//
				int propCount = mat.shader.GetPropertyCount();
				for (int i = 0; i < propCount; i++)
				{
					//---Get properties name and type---//
					string propName = mat.shader.GetPropertyName(i);
					UnityEngine.Rendering.ShaderPropertyType propType = mat.shader.GetPropertyType(i);

					switch (propType)
					{
						//---Store int properties (2021.1 or newer)---///
#if UNITY_2021_1_OR_NEWER //Not implemented in older versions of Unity
						case UnityEngine.Rendering.ShaderPropertyType.Int:
							mi.intProperties.Add(new MatProperty<int>(propName, mat.GetInt(propName)));
							break;
#endif
						//---Store float properties---//
						case UnityEngine.Rendering.ShaderPropertyType.Float:
							mi.floatProperties.Add(new MatProperty<float>(propName, mat.GetFloat(propName)));
							break;

						//---Store range properties---//
						case UnityEngine.Rendering.ShaderPropertyType.Range:
							mi.rangeProperties.Add(new MatProperty<float>(propName, mat.GetFloat(propName)));
							break;

						//---Store colour properties---//
						case UnityEngine.Rendering.ShaderPropertyType.Color:
							mi.colourProperties.Add(new MatProperty<Color>(propName, mat.GetColor(propName)));
							break;

						//---Store vector properties---//
						case UnityEngine.Rendering.ShaderPropertyType.Vector:
							mi.vectorProperties.Add(new MatProperty<Vector4>(propName, mat.GetVector(propName)));
							break;

						//---Store texture properties---//
						case UnityEngine.Rendering.ShaderPropertyType.Texture:
							Texture t = mat.GetTexture(propName);
							if (t != null)
								//Store GUID of texture
								mi.textureProperties.Add(new MatProperty<string>(propName, AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(t))));
							else
								mi.textureProperties.Add(new MatProperty<string>(propName, "null"));
							break;
					}

				}

				//---Add custom property for default render shader---//
				if (mat.shader.name == "RapidIcon/ObjectRender")
				{
					bool enable = (iconSettings.fixEdgesMode == IconSettings.FixEdgesModes.Regular) || (iconSettings.fixEdgesMode == IconSettings.FixEdgesModes.WithDepthTexture);
					mi.intProperties.Add(new MatProperty<int>("custom_PreMulAlpha", enable ? 1 : 0));
					mi.intProperties.Add(new MatProperty<int>("custom_UseDepthTexture", (iconSettings.fixEdgesMode == IconSettings.FixEdgesModes.WithDepthTexture) ? 1 : 0));
				}

				//---Store the name and toggle state, then add the matInfo to the list of all matInfos to be save---//
				mi.displayName = iconSettings.materialDisplayNames[mat];
				mi.toggle = iconSettings.materialToggles[mat];
				iconSettings.matInfo.Add(mi);
			}
		}
	}
}