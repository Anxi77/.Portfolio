using System;
using UnityEditor;
using UnityEngine;

namespace RapidIcon_1_7_1
{
	public enum SeparatorTypes { Vertical, Horizontal };

	[Serializable]
	public class DraggableSeparator
	{
		//---PUBLIC---//
		public float value;
		public Rect rect;
		public bool mouseOver;

		//---INTERNAL---//
		SeparatorTypes separatorType;
		bool dragging;
		Texture2D separatorTex;
		RapidIconWindow window;

		public DraggableSeparator(SeparatorTypes sepType)
		{
			//---Initialise DraggableSeparator---//
			//Set type (horizontal/vertical)
			separatorType = sepType;

			//Create texture, use correct colour for light/dark mode
			if (EditorGUIUtility.isProSkin)
				separatorTex = Utils.CreateColourTexture(2, 2, new Color32(31, 31, 31, 255));
			else
				separatorTex = Utils.CreateColourTexture(2, 2, new Color32(153, 153, 153, 255));

			separatorTex.hideFlags = HideFlags.DontSave;
		}

		public void Draw(float minValue, float maxValue, RapidIconWindow w)
		{
			//---Check variables are set---//
			CheckAndSetWindow(w);
			CheckPosition(minValue, maxValue);

			//---Set seperator length---//
			if (separatorType == SeparatorTypes.Vertical)
				SetLength(window.position.height);
			else
				SetLength(window.position.width - window.leftSeparator.value);

			//---Draw the separator---//
			float currentSepPos = value;
			Rect drawRect = new Rect(rect);
			if (separatorType == SeparatorTypes.Vertical)
				drawRect.width = 1;
			else if (separatorType == SeparatorTypes.Horizontal)
				drawRect.height = 1;
			GUI.DrawTexture(drawRect, separatorTex);

			//---Create mouse area rect, slightly larger than the draw rect---//
			Rect mouseArea = new Rect(rect);
			if (separatorType == SeparatorTypes.Vertical)
				mouseArea.position -= new Vector2(3, 0);
			else if (separatorType == SeparatorTypes.Horizontal)
				mouseArea.position -= new Vector2(0, 4);

			//---Set cursor if mouse over mouse area---//
			if (separatorType == SeparatorTypes.Vertical)
				EditorGUIUtility.AddCursorRect(mouseArea, MouseCursor.ResizeHorizontal);
			else if (separatorType == SeparatorTypes.Horizontal)
				EditorGUIUtility.AddCursorRect(mouseArea, MouseCursor.ResizeVertical);

			//---Detect dragging of separator---//
			if (mouseArea.Contains(Event.current.mousePosition))
			{
				mouseOver = true;
				if (Event.current.rawType == EventType.MouseDown)
					dragging = true;
			}
			else
				mouseOver = false;

			//---Move the separator with mouse if dragging---//
			if (dragging)
			{
				if (separatorType == SeparatorTypes.Vertical)
				{
					currentSepPos = Mathf.Clamp(Event.current.mousePosition.x, minValue, maxValue);
					rect.Set(currentSepPos, rect.y, rect.width, rect.height);
				}
				else if (separatorType == SeparatorTypes.Horizontal)
				{
					currentSepPos = Mathf.Clamp(Event.current.mousePosition.y, minValue, maxValue);
					rect.Set(rect.x, currentSepPos, rect.width, rect.height);
				}

				value = currentSepPos;
				window.Repaint();
			}

			//---Detect dragging of seperator stopped---//
			if (Event.current.rawType == EventType.MouseUp)
				dragging = false;
		}

		public void SaveData(string name)
		{
			//---Save separator position value---//
			EditorPrefs.SetFloat(PlayerSettings.productName + name, value);
		}

		public void LoadData(string name, int defaultValue)
		{
			//---Load separator position value---//
			if (separatorType == SeparatorTypes.Vertical)
			{
				rect = new Rect(EditorPrefs.GetFloat(PlayerSettings.productName + name, defaultValue), 0, 8f, 600);
				value = rect.position.x;
			}
			else if (separatorType == SeparatorTypes.Horizontal)
			{
				rect = new Rect(0, EditorPrefs.GetFloat(PlayerSettings.productName + name, defaultValue), 400, 6f);
				value = rect.position.y;
			}
		}

		void CheckAndSetWindow(RapidIconWindow w)
		{
			if (!window)
				window = w;
		}

		void SetLength(float length)
		{
			if (window)
			{
				if (separatorType == SeparatorTypes.Vertical)
					rect.height = length;
				else if (separatorType == SeparatorTypes.Horizontal)
					rect.width = length;
			}
		}

		void CheckPosition(float min, float max)
		{
			//---Apply min/max limits to separator position---//
			float chk = value;
			if (separatorType == SeparatorTypes.Vertical)
			{
				rect.position = new Vector2(Mathf.Clamp(rect.position.x, min, max), rect.position.y);
				value = rect.position.x;
			}
			else if (separatorType == SeparatorTypes.Horizontal)
			{
				rect.position = new Vector2(rect.position.x, Mathf.Clamp(rect.position.y, min, max));
				value = rect.position.y;
			}

			//---Repaint if separator has been moved---//
			if (window && value != chk)
				window.Repaint();
		}
	}
}