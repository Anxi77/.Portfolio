using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
//using UnityEngine.Rendering.Universal; //URP only

namespace RapidIcon_1_7_1
{
	public class RapidIconStage : PreviewSceneStage
	{
		//---INTERNAL---//
		Camera cam;
		Color ambientLightColour;
		AmbientMode ambientMode;
		bool fogEnabled;

		public void SetScene(UnityEngine.SceneManagement.Scene scene_in)
		{
			this.scene = scene_in;
		}

		public void SetupScene(Icon icon)
		{
			//---Create scene objects---//
			GameObject obj = GameObject.Instantiate((GameObject)icon.parentIconSet.assetObject);
			GameObject camGO = new GameObject("camera");
			GameObject lightGO = new GameObject("light");

			//---Place the objects in the scene---//
			StageUtility.PlaceGameObjectInCurrentStage(camGO);
			StageUtility.PlaceGameObjectInCurrentStage(lightGO);
			StageUtility.PlaceGameObjectInCurrentStage(obj);

			//---Apply the object settings---//
			obj.transform.position = icon.iconSettings.objectPosition;
			obj.transform.localScale = icon.iconSettings.objectScale;
			obj.transform.eulerAngles = icon.iconSettings.objectRotation;

			//---Apply hierarchy settings---//
			List<string> keys = icon.iconSettings.subObjectEnables.Keys.ToList();
			foreach (string key in keys)
			{
				var indicies = key.Split('/');
				GameObject subObj = obj;
				for (int i = 2; i < indicies.Length; i++)
				{
					int idx = int.Parse(indicies[i]);
					subObj = subObj.transform.GetChild(idx).gameObject;
				}
				subObj.SetActive(icon.iconSettings.subObjectEnables[key]);
			}

			//---Add the camera component and apply camera settings---//
			cam = camGO.AddComponent<Camera>();
			cam.scene = this.scene;
			cam.clearFlags = CameraClearFlags.Nothing;
			cam.transform.position = icon.iconSettings.cameraPosition;
			cam.transform.LookAt(icon.iconSettings.cameraTarget);
			cam.orthographic = icon.iconSettings.cameraOrtho;
			cam.orthographicSize = icon.iconSettings.cameraSize;
			cam.orthographicSize /= icon.iconSettings.camerasScaleFactor;

			cam.fieldOfView = icon.iconSettings.cameraFov;
			float fov = 2 * Mathf.Rad2Deg * Mathf.Atan2(
				Mathf.Tan(cam.fieldOfView * Mathf.Deg2Rad / 2), icon.iconSettings.camerasScaleFactor
				);
			if (fov < 0)
				fov += 180;
			cam.fieldOfView = fov;

			cam.nearClipPlane = 0.001f;
			cam.farClipPlane = 10000;
			cam.depthTextureMode = DepthTextureMode.Depth;
			cam.clearFlags = CameraClearFlags.Color;
			//cam.GetUniversalAdditionalCameraData().renderPostProcessing = false; //URP only

			//---Add light componenet and apply lighting settings---//
			Light dirLight = lightGO.AddComponent<Light>();
			dirLight.type = LightType.Directional;
			dirLight.color = icon.iconSettings.lightColour;
			dirLight.transform.eulerAngles = icon.iconSettings.lightDir;
			dirLight.intensity = icon.iconSettings.lightIntensity;

			//---Store current environment settings---//
			ambientLightColour = RenderSettings.ambientLight;
			ambientMode = RenderSettings.ambientMode;
			fogEnabled = RenderSettings.fog;

			//---Apply environment settings---//
			RenderSettings.ambientLight = icon.iconSettings.ambientLightColour;
			RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
			RenderSettings.fog = false;

			//---Apply animation settings---//
			if (icon.iconSettings.animationClip != null)
			{
				float t = icon.iconSettings.animationClip.length * icon.iconSettings.animationOffset;
				icon.iconSettings.animationClip.SampleAnimation(obj, t);
			}
		}

		public Texture2D RenderIcon(int width, int height)
		{
			//---Setup render texture---//
			width = Mathf.Clamp(width, 8, 2048);
			height = Mathf.Clamp(height, 8, 2048);
			var rtd = new RenderTextureDescriptor(width, height) { depthBufferBits = 24, msaaSamples = 8, useMipMap = false, sRGB = true };
			var rt = new RenderTexture(rtd);
			rt.Create();

			//---Render camera to render texture---//
			cam.targetTexture = rt;
			cam.aspect = (float)width / (float)height;
			cam.Render();
			cam.targetTexture = null;
			cam.ResetAspect();

			//---Convert render texture to Texture2D---//
			Texture2D render = new Texture2D(width, height, TextureFormat.RGBA32, false, false);
			var oldActive = RenderTexture.active;
			RenderTexture.active = rt;
			render.ReadPixels(new Rect(0, 0, width, height), 0, 0);
			render.Apply();

			//---Cleanup render texture---//
			RenderTexture.active = oldActive;
			rt.Release();

			//---Restore environment settings---//
			RenderSettings.ambientLight = ambientLightColour;
			RenderSettings.ambientMode = ambientMode;
			RenderSettings.fog = fogEnabled;

			return render;
		}

		protected override GUIContent CreateHeaderContent()
		{
			GUIContent headerContent = new GUIContent();
			headerContent.text = "RapidIcon";
			return headerContent;
		}
	}
}