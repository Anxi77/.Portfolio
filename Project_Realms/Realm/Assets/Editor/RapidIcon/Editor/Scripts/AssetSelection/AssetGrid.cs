using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RapidIcon_1_7_1
{
	//---ObjectPathPair Definition---//
	public struct ObjectPathPair
	{
		public ObjectPathPair(UnityEngine.Object obj, string pth)
		{
			UnityEngine_object = obj;
			path = pth;
		}

		public UnityEngine.Object UnityEngine_object;
		public string path;
	};

	[Serializable]
	public class AssetGrid
	{
		//---PUBLIC---//
		//Icons
		public Dictionary<UnityEngine.Object, IconSet> objectIconSets;
		public Dictionary<string, IconSet> sortedIconSetsByPath;
		public List<IconSet> visibleIconSets;
		public List<IconSet> selectedIconSets;

		//Textures
		public Texture2D[] assetSelectionTextures;

		//Other
		public string rapidIconRootFolder;
		public bool assetGridFocused;
		public int previewSize;

		//---INTERNAL---//
		//GUI
		Vector2 scrollPosition;
		GUIStyle gridStyle, gridLabelStyle;

		//Selection
		int lastSelectedIconIndex;
		int selectionMinIndex;
		int selectionMaxIndex;
		string lastSelectedIndividualFolder;
		List<ObjectPathPair> objectsLoadedFromSelectedFolders;

		//RapidIcon Window Elements
		RapidIconWindow window;
		AssetList assetList;

		//Filter
		int filterIdx;
		string[] filters = new string[] { "t:model t:prefab", "t:prefab", "t:model" };
		string[] filterNames = new string[] { "Prefabs & Models", "Prefabs Only", "Models Only" };

		//Other
		Rect rect;
		bool iconsRefreshed;

		public AssetGrid(AssetList assets)
		{
			//---Initialise AssetGrid---//
			//Asset List
			assetList = assets;

			//Selection
			objectsLoadedFromSelectedFolders = new List<ObjectPathPair>();
			lastSelectedIconIndex = -1;
			selectionMinIndex = int.MaxValue;
			selectionMaxIndex = -1;

			//Icons
			objectIconSets = new Dictionary<UnityEngine.Object, IconSet>();
			sortedIconSetsByPath = new Dictionary<string, IconSet>();
			selectedIconSets = new List<IconSet>();

			//Styles
			previewSize = 128;
			gridStyle = new GUIStyle();
			gridStyle.fixedHeight = previewSize;
			gridStyle.fixedWidth = previewSize;
			gridStyle.margin.bottom = 16 + (int)EditorGUIUtility.singleLineHeight + 2;
			gridStyle.margin.left = 16;
			gridStyle.alignment = TextAnchor.MiddleCenter;
			gridLabelStyle = new GUIStyle(gridStyle);
			gridLabelStyle.margin.bottom = 16 + previewSize + 2;
			gridLabelStyle.fixedHeight = EditorGUIUtility.singleLineHeight;
			gridLabelStyle.alignment = TextAnchor.MiddleCenter;
			if (EditorGUIUtility.isProSkin)
				gridLabelStyle.normal.textColor = new Color32(192, 192, 192, 255);
			else
				gridLabelStyle.normal.textColor = Color.black;

			//Filter
			filterIdx = 0;

			//Textures
			assetSelectionTextures = new Texture2D[5];

			string[] split = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("RapidIconWindow")[0]).Split('/');
			rapidIconRootFolder = "";
			for (int i = 0; i < split.Length - 4; i++)
				rapidIconRootFolder += split[i] + "/";

			assetSelectionTextures[0] = (Texture2D)AssetDatabase.LoadMainAssetAtPath(rapidIconRootFolder + "Editor/UI/deselectedAsset.png");
			assetSelectionTextures[1] = (Texture2D)AssetDatabase.LoadMainAssetAtPath(rapidIconRootFolder + "Editor/UI/selectedAssetActiveDark.png");
			assetSelectionTextures[2] = (Texture2D)AssetDatabase.LoadMainAssetAtPath(rapidIconRootFolder + "Editor/UI/selectedAssetInactiveDark.png");
			assetSelectionTextures[3] = (Texture2D)AssetDatabase.LoadMainAssetAtPath(rapidIconRootFolder + "Editor/UI/selectedAssetActiveLight.png");
			assetSelectionTextures[4] = (Texture2D)AssetDatabase.LoadMainAssetAtPath(rapidIconRootFolder + "Editor/UI/selectedAssetInactiveLight.png");
			assetSelectionTextures[0].hideFlags = HideFlags.DontSave;
			assetSelectionTextures[1].hideFlags = HideFlags.DontSave;
			assetSelectionTextures[2].hideFlags = HideFlags.DontSave;
			assetList.lastNumberOfSelected = -1;

			//Other
			iconsRefreshed = true;
		}

		public void Draw(float width, RapidIconWindow w)
		{
			//---Check variables are set---//
			CheckAndSetWindow(w);

			//---Refresh icons after startup---//
			if (!iconsRefreshed && EditorApplication.timeSinceStartup > 15)
			{
				RefreshAllIcons();
				iconsRefreshed = true;
			}

			GUILayout.BeginVertical(GUILayout.Width(width));
			GUILayout.Space(4);

			//---Filter---//
			GUILayout.BeginHorizontal();
			GUILayout.Space(8);
			if (GUILayout.Button("Refresh"))
				ReloadObjects();
			if (GUILayout.Button("Filter: " + filterNames[filterIdx]))
			{
				filterIdx++;
				if (filterIdx == 3)
					filterIdx = 0;

				ReloadObjects();
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			//---Scroll view----//
			scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, false, GUIStyle.none, GUI.skin.verticalScrollbar);

			//---Draw icons---//
			DrawIcons(width);

			//---End GUI elements---//
			GUILayout.EndScrollView();
			GUILayout.EndVertical();

			//---Get last rect and check focus---//
			if (Event.current.type == EventType.Repaint)
				rect = new Rect(GUILayoutUtility.GetLastRect());
			CheckFocus(rect);
		}

		public void SaveData()
		{
			//---Save selected assets---//
			string selectedAssetsString = "";
			foreach (KeyValuePair<UnityEngine.Object, IconSet> iconSet in objectIconSets)
			{
				selectedAssetsString += "|-A-|" + iconSet.Value.assetPath + "|-S-|" + iconSet.Value.selected;
			}
			EditorPrefs.SetString(PlayerSettings.productName + "RapidIconSelectedAssets", selectedAssetsString);

			//---Save other variables---//
			EditorPrefs.SetFloat(PlayerSettings.productName + "RapidIconAssetGridScroll", scrollPosition.y);
			EditorPrefs.SetBool(PlayerSettings.productName + "RapidIconIconsRefreshed", iconsRefreshed);
			EditorPrefs.SetInt(PlayerSettings.productName + "RapidIconFilterIdx", filterIdx);
		}

		public void LoadData()
		{
			//---Close RapidIcon window if left open when Unity starts---//
			if (!SessionState.GetBool("rapidicon_loaded", false))
			{
				SessionState.SetBool("rapidicon_forceclose", true);
				SessionState.SetBool("rapidicon_loaded", true);
				return;
			}

			//---Load objects in selected folders---//
			objectsLoadedFromSelectedFolders = LoadObjectsInSelectedFolders();
			CreateIconSets();

			//---Load selected assets---//
			string selectedAssetsString = EditorPrefs.GetString(PlayerSettings.productName + "RapidIconSelectedAssets");
			string[] splitAssets = selectedAssetsString.Split(new string[] { "|-A-|" }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string s in splitAssets)
			{
				string[] splitS = s.Split(new string[] { "|-S-|" }, StringSplitOptions.RemoveEmptyEntries);
				string assetPath = splitS[0];
				if (splitS[1] == "True")
				{
					IconSet iconSet = GetIconSetFromPath(assetPath);
					if (iconSet != null)
					{
						iconSet.selected = true;
						selectedIconSets.Add(GetIconSetFromPath(assetPath));
					}
				}
			}

			//---Load other variables---//
			iconsRefreshed = EditorPrefs.GetBool(PlayerSettings.productName + "RapidIconIconsRefreshed");
			scrollPosition = new Vector2(0, EditorPrefs.GetFloat(PlayerSettings.productName + "RapidIconAssetGridScroll"));
			filterIdx = EditorPrefs.GetInt(PlayerSettings.productName + "RapidIconFilterIdx", 0);
		}

		void ReloadObjects()
		{
			//---Unload objects
			EditorUtility.UnloadUnusedAssetsImmediate();

			//---Reload the objects from selected folders---//
			objectsLoadedFromSelectedFolders = LoadObjectsInSelectedFolders();
			CreateIconSets();

			//---Add loaded icons to list---///
			sortedIconSetsByPath.Clear();
			foreach (ObjectPathPair loadedObject in objectsLoadedFromSelectedFolders)
			{
				IconSet iconSet = objectIconSets[loadedObject.UnityEngine_object];
				sortedIconSetsByPath.Add(iconSet.assetPath, iconSet);
			}

			//---Sort the list by path---//
			sortedIconSetsByPath = SortIconSetsByFolder(sortedIconSetsByPath);

			//---Update variables---//
			assetList.lastNumberOfSelected = assetList.selectedFolders.Count;
			lastSelectedIndividualFolder = assetList.selectedFolders[0];
		}

		public void RefreshAllIcons()
		{
			//---Loop through all icons---//
			int index = 1;
			foreach (IconSet iconSet in objectIconSets.Values)
			{
				//---Show a progress bar---//
				EditorUtility.DisplayProgressBar("Updating Icons (" + index++ + "/" + objectIconSets.Count + ")", iconSet.assetPath, (float)(index) / (float)(objectIconSets.Count));

				//---Update the icon renders---//
				Vector2Int renderResolution = Utils.MutiplyVector2IntByFloat(iconSet.GetCurrentIcon().iconSettings.exportResolution, window.iconEditor.resMultiplyers[window.iconEditor.resMultiplyerIndex]);
				iconSet.GetCurrentIcon().UpdateIcon(renderResolution, new Vector2Int(128, (int)(((float)renderResolution.y / (float)renderResolution.x) * 128)));
			}

			//---Clear the progress bar when done---//
			EditorUtility.ClearProgressBar();
		}

		void CheckAndSetWindow(RapidIconWindow w)
		{
			if (!window)
				window = w;
		}

		List<ObjectPathPair> LoadObjectsInSelectedFolders()
		{
			//---Get asset paths of all assets in selected folders---//
			string[] assetGUIDs = AssetDatabase.FindAssets(filters[filterIdx], assetList.selectedFolders.ToArray());
			string[] assetPaths = new string[assetGUIDs.Length];
			for (int i = 0; i < assetGUIDs.Length; i++)
				assetPaths[i] = AssetDatabase.GUIDToAssetPath(assetGUIDs[i]);

			List<ObjectPathPair> loadedObjectPathPairs = new List<ObjectPathPair>();
			foreach (string assetPath in assetPaths)
			{
				//---Get folder path from each of the asset paths---//
				string[] split = assetPath.Split('/');
				string folderPath = "";
				for (int i = 0; i < split.Length - 1; i++)
					folderPath += split[i] + (i < split.Length - 2 ? "/" : "");

				//---Load the asset if the path is in the selected folders list---//
				if (assetList.selectedFolders.Contains(folderPath))
				{
					ObjectPathPair objectPathPair = new ObjectPathPair();

					UnityEngine.Object o = AssetDatabase.LoadMainAssetAtPath(assetPath);
					objectPathPair.UnityEngine_object = o;
					objectPathPair.path = assetPath;
					loadedObjectPathPairs.Add(objectPathPair);
				}
			}
			return loadedObjectPathPairs;
		}

		void CreateIconSets()
		{
			int index = 1;
			foreach (ObjectPathPair loadedObject in objectsLoadedFromSelectedFolders)
			{
				if (!objectIconSets.ContainsKey(loadedObject.UnityEngine_object))
				{
					//---Create icon if doesn't already exist---//
					EditorUtility.DisplayProgressBar("Generating Icon Previews (" + index + " / " + (objectsLoadedFromSelectedFolders.Count) + ")", loadedObject.path, (float)(index++) / (float)objectsLoadedFromSelectedFolders.Count);
					objectIconSets.Add(loadedObject.UnityEngine_object, CreateIconSet(loadedObject));
				}
				else if (objectIconSets[loadedObject.UnityEngine_object].deleted)
				{
					objectIconSets[loadedObject.UnityEngine_object].deleted = false;
				}
				else
				{
					//---Update asset path if changed---//
					IconSet iconSet = objectIconSets[loadedObject.UnityEngine_object];

					string currentPath = AssetDatabase.GetAssetPath(loadedObject.UnityEngine_object);
					string savedPath = iconSet.assetPath;
					if (savedPath != currentPath)
					{
						Debug.LogWarning("Path updated for " + iconSet.assetName + " from " + savedPath + " to " + currentPath);
						iconSet.assetPath = currentPath;

						string[] split;
						split = iconSet.assetPath.Split('/');
						iconSet.assetName = split[split.Length - 1];
						if (iconSet.assetName.Length > 19)
							iconSet.assetShortenedName = iconSet.assetName.Substring(0, 16) + "...";
						else
							iconSet.assetShortenedName = iconSet.assetName;

						split = iconSet.assetPath.Split('/');
						iconSet.folderPath = "";
						for (int i = 0; i < split.Length - 1; i++)
							iconSet.folderPath += split[i] + (i < split.Length - 2 ? "/" : "");
					}
				}
			}
			EditorUtility.ClearProgressBar();
		}

		public IconSet CreateIconSet(ObjectPathPair objectPathPair)
		{
			//---Create a new icon set and icon objects---//
			IconSet iconSet = new IconSet(this, objectPathPair);
			return iconSet;
		}

		void DrawIcons(float gridWidth)
		{
			//---Draw margin---//
			GUILayout.Space(14);
			GUILayout.BeginHorizontal();
			GUILayout.Space(16);

			//---Create lists---//
			List<Texture2D> visibleIconRenders = new List<Texture2D>();
			List<Texture2D> visibleIconSelectionTextures = new List<Texture2D>();
			List<string> visibleIconLabels = new List<string>();
			visibleIconSets = new List<IconSet>();

			//---Reload objects if needed---//
			if (sortedIconSetsByPath.Count != objectsLoadedFromSelectedFolders.Count || assetList.selectedFolders.Count != assetList.lastNumberOfSelected || assetList.selectedFolders[0] != lastSelectedIndividualFolder)
			{
				ReloadObjects();
			}

			//---Deselect icons if no longer in selected folders / search (i.e. if not visible in the grid)---//
			foreach (IconSet iconSet in objectIconSets.Values)
			{
				if (!assetList.selectedFolders.Contains(iconSet.folderPath) || (assetList.doSearch && !assetList.searchFolders.Contains(iconSet.folderPath + "/" + iconSet.assetName)))
				{
					iconSet.selected = false;
					if (selectedIconSets.Contains(iconSet))
						selectedIconSets.Remove(iconSet);
				}
			}

			int index = 0;
			foreach (KeyValuePair<string, IconSet> iconSet in sortedIconSetsByPath)
			{
				//---Skip this icon if it's flagged as deleted---//
				if (iconSet.Value.deleted)
					continue;

				//---Flag  the icon as deleted if asset object is null---//
				else if (iconSet.Value.assetObject == null)
				{
					iconSet.Value.deleted = true;
					iconSet.Value.assetObject = null;
					selectedIconSets.Remove(iconSet.Value);
					continue;
				}

				//---Render the icon preview if it is missing---//
				if (iconSet.Value.GetCurrentIcon().previewRender == null)
				{
					EditorUtility.DisplayProgressBar("Generating Icon Previews (" + index + "/" + (sortedIconSetsByPath.Count) + ")", iconSet.Value.assetPath, ((float)index++ / sortedIconSetsByPath.Count));
					iconSet.Value.GetCurrentIcon().previewRender = Utils.RenderIcon(iconSet.Value.GetCurrentIcon(), previewSize, (int)(((float)iconSet.Value.GetCurrentIcon().iconSettings.exportResolution.y / (float)iconSet.Value.GetCurrentIcon().iconSettings.exportResolution.x) * previewSize));
				}

				//---Set the selection texture---//
				if (EditorGUIUtility.isProSkin)
					iconSet.Value.selectionTexture = iconSet.Value.selected ? (assetGridFocused ? assetSelectionTextures[1] : assetSelectionTextures[2]) : assetSelectionTextures[0];
				else
					iconSet.Value.selectionTexture = iconSet.Value.selected ? (assetGridFocused ? assetSelectionTextures[3] : assetSelectionTextures[4]) : assetSelectionTextures[0];

				//---Add the icon to visibleIcons if it's within the selected folders, or the search---//
				if (assetList.selectedFolders.Contains(iconSet.Value.folderPath) && (!assetList.doSearch || assetList.searchFolders.Contains(iconSet.Value.folderPath + "/" + iconSet.Value.assetName)))
				{
					visibleIconSets.Add(iconSet.Value);

					//Use warning image if animations enabled
					visibleIconRenders.Add(iconSet.Value.GetCurrentIcon().previewRender);

					visibleIconSelectionTextures.Add(iconSet.Value.selectionTexture);
					visibleIconLabels.Add(iconSet.Value.assetShortenedName);
				}
			}
			EditorUtility.ClearProgressBar();

			//---Draw the grid of icons---///
			int count = Mathf.FloorToInt((gridWidth - 16) / (previewSize + 16));
			count = Mathf.Min(count, visibleIconSets.Count);
			int clicked = GUILayout.SelectionGrid(-1, visibleIconRenders.ToArray(), count, gridStyle, GUILayout.Width(32 + count * (previewSize + 16)));
			Rect r = GUILayoutUtility.GetLastRect();
			r.y += previewSize + 2;

			//---Draw the label background textures on the grid---//
			int labelClicked = GUI.SelectionGrid(r, -1, visibleIconSelectionTextures.ToArray(), count, gridLabelStyle);

			//---Draw the label texts on the grid---//
			clicked = GUI.SelectionGrid(r, clicked, visibleIconLabels.ToArray(), count, gridLabelStyle);
			if (clicked == -1 && labelClicked != -1)
				clicked = labelClicked;

			//---Draw margin and end GUI elements---//
			GUILayout.Space(16);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			//---Check mouse clicks and arrow key presses for grid selection---//
			CheckMouseClicks(clicked, visibleIconSets);
			CheckArrowKeys(visibleIconSets, count);
		}

		void CheckMouseClicks(int clicked, List<IconSet> visibleIconSets)
		{
			if (clicked >= 0)
			{
				if (!Event.current.control && !Event.current.shift)
				{
					//---Regular click, no ctrl/shift - select the icon---//
					foreach (KeyValuePair<UnityEngine.Object, IconSet> iconSet in objectIconSets)
						iconSet.Value.selected = false;

					visibleIconSets[clicked].selected = true;
					visibleIconSets[clicked].assetGridIconIndex = clicked;
					selectedIconSets.Clear();
					selectionMinIndex = clicked;
					selectionMaxIndex = clicked;
				}
				else if (Event.current.control)
				{
					//---Ctrl click - add icon to existing selection---//
					visibleIconSets[clicked].selected = !visibleIconSets[clicked].selected;
					visibleIconSets[clicked].assetGridIconIndex = clicked;
				}
				else if (Event.current.shift)
				{
					//---Shift click - add all icons between clicks---//
					if (selectionMinIndex != -1 && selectionMaxIndex != -1 && clicked >= selectionMinIndex && clicked <= selectionMaxIndex)
					{
						for (int i = selectionMinIndex; i <= selectionMaxIndex; i++)
						{
							visibleIconSets[i].selected = false;
							if (selectedIconSets.Contains(visibleIconSets[i]))
								selectedIconSets.Remove(visibleIconSets[i]);
						}

						selectionMinIndex = Mathf.Min(lastSelectedIconIndex, clicked);
						selectionMaxIndex = Math.Max(lastSelectedIconIndex, clicked);
					}
					int minI = Mathf.Min(lastSelectedIconIndex, clicked);
					int maxI = Math.Max(lastSelectedIconIndex, clicked);
					if (minI < 0) minI = 0;
					if (maxI < 0) maxI = 0;

					for (int i = minI; i <= maxI; i++)
					{
						visibleIconSets[i].selected = true;
						visibleIconSets[i].assetGridIconIndex = i;
						if (!selectedIconSets.Contains(visibleIconSets[i]))
							selectedIconSets.Add(visibleIconSets[i]);
					}

				}

				//---If not shift click then toggle the icon from the selection---//
				if (!Event.current.shift)
				{
					if (visibleIconSets[clicked].selected && !selectedIconSets.Contains(visibleIconSets[clicked]))
						selectedIconSets.Add(visibleIconSets[clicked]);
					else if (selectedIconSets.Contains(visibleIconSets[clicked]))
						selectedIconSets.Remove(visibleIconSets[clicked]);
				}

				//---Sort the selected icons by grid index---//
				if (selectedIconSets.Count > 1)
					selectedIconSets = selectedIconSets.OrderBy(a => a.assetGridIconIndex).ToList();

				//---Update variables---//
				selectionMinIndex = Mathf.Min(selectionMinIndex, clicked);
				selectionMaxIndex = Mathf.Max(selectionMaxIndex, clicked);
				lastSelectedIconIndex = clicked;
				assetGridFocused = true;
				window.Repaint();
			}
			else if (Event.current.rawType == EventType.MouseDown && !window.leftSeparator.mouseOver && !window.rightSeparator.mouseOver)
			{
				//---Clear selection if mouse clicked in empty space in asset grid region---//
				Vector2 correctMousePos = Event.current.mousePosition + rect.position;
				if (rect.Contains(correctMousePos))
				{
					selectedIconSets.Clear();
					foreach (IconSet iconSet in visibleIconSets)
						iconSet.selected = false;
				}
			}
		}

		void CheckArrowKeys(List<IconSet> iconSets, int gridXIcons)
		{
			//---Check if a key is pressed---//
			if (assetGridFocused && Event.current.isKey && Event.current.type != EventType.KeyUp)
			{
				//---Right arrow key pressed---//
				if (Event.current.keyCode == KeyCode.RightArrow && lastSelectedIconIndex < iconSets.Count - 1)
				{
					if (!Event.current.shift && !Event.current.control)
					{
						//---Select only this icon if no shift/ctrl pressed---//
						foreach (IconSet iconSet in iconSets)
							iconSet.selected = false;
						selectedIconSets.Clear();

						iconSets[lastSelectedIconIndex + 1].selected = true;
						selectedIconSets.Add(iconSets[lastSelectedIconIndex + 1]);
					}
					else
					{
						//---Add to current selection if shift/ctrl pressed---///
						if (!iconSets[lastSelectedIconIndex + 1].selected)
						{
							iconSets[lastSelectedIconIndex + 1].selected = true;
							selectedIconSets.Add(iconSets[lastSelectedIconIndex + 1]);
						}
						else
						{
							iconSets[lastSelectedIconIndex].selected = false;
							selectedIconSets.Remove(iconSets[lastSelectedIconIndex]);
						}

					}
					lastSelectedIconIndex++;
				}
				//---Left arrow key pressed---//
				else if (Event.current.keyCode == KeyCode.LeftArrow && lastSelectedIconIndex > 0)
				{
					if (!Event.current.shift && !Event.current.control)
					{
						//---Select only this icon if no shift/ctrl pressed---//
						foreach (IconSet iconSet in iconSets)
							iconSet.selected = false;
						selectedIconSets.Clear();

						iconSets[lastSelectedIconIndex - 1].selected = true;
						selectedIconSets.Add(iconSets[lastSelectedIconIndex - 1]);
					}
					else
					{
						//---Add to current selection if shift/ctrl pressed---///
						if (!iconSets[lastSelectedIconIndex - 1].selected)
						{
							iconSets[lastSelectedIconIndex - 1].selected = true;
							selectedIconSets.Add(iconSets[lastSelectedIconIndex - 1]);
						}
						else
						{
							iconSets[lastSelectedIconIndex].selected = false;
							selectedIconSets.Remove(iconSets[lastSelectedIconIndex]);
						}
					}
					lastSelectedIconIndex--;
				}
				//---Down arrow key pressed---//
				else if (Event.current.keyCode == KeyCode.DownArrow)
				{
					if (lastSelectedIconIndex < iconSets.Count - gridXIcons)
					{
						if (!Event.current.shift && !Event.current.control)
						{
							//---Select only this icon if no shift/ctrl pressed---//
							foreach (IconSet iconSet in iconSets)
								iconSet.selected = false;
							selectedIconSets.Clear();

							iconSets[lastSelectedIconIndex + gridXIcons].selected = true;
							selectedIconSets.Add(iconSets[lastSelectedIconIndex + gridXIcons]);
						}
						else
						{
							//---Add to current selection if shift/ctrl pressed---///
							if (!iconSets[lastSelectedIconIndex + gridXIcons].selected)
							{
								iconSets[lastSelectedIconIndex + gridXIcons].selected = true;
								selectedIconSets.Add(iconSets[lastSelectedIconIndex + gridXIcons]);
								for (int i = lastSelectedIconIndex; i < lastSelectedIconIndex + gridXIcons; i++)
								{
									iconSets[i].selected = true;
									selectedIconSets.Add(iconSets[i]);
								}
							}
							else
							{
								iconSets[lastSelectedIconIndex].selected = false;
								selectedIconSets.Remove(iconSets[lastSelectedIconIndex]);
								for (int i = lastSelectedIconIndex; i < lastSelectedIconIndex + gridXIcons; i++)
								{
									iconSets[i].selected = false;
									selectedIconSets.Remove(iconSets[i]);
								}
							}

						}
						lastSelectedIconIndex += gridXIcons;
					}
					else if (lastSelectedIconIndex < Mathf.Floor((float)iconSets.Count / gridXIcons) * gridXIcons)
					{
						if (!Event.current.shift && !Event.current.control)
						{
							//---Select only this icon if no shift/ctrl pressed---//
							foreach (IconSet iconSet in iconSets)
								iconSet.selected = false;
							selectedIconSets.Clear();

							iconSets[iconSets.Count - 1].selected = true;
							selectedIconSets.Add(iconSets[iconSets.Count - 1]);
						}
						else
						{
							//---Add to current selection if shift/ctrl pressed---///
							if (!iconSets[iconSets.Count - 1].selected)
							{
								iconSets[iconSets.Count - 1].selected = true;
								selectedIconSets.Add(iconSets[iconSets.Count - 1]);
								for (int i = lastSelectedIconIndex; i < iconSets.Count; i++)
								{
									iconSets[i].selected = true;
									selectedIconSets.Add(iconSets[i]);
								}
							}
							else
							{
								iconSets[lastSelectedIconIndex].selected = false;
								selectedIconSets.Remove(iconSets[lastSelectedIconIndex]);
								for (int i = lastSelectedIconIndex; i < iconSets.Count - 1; i++)
								{
									iconSets[i].selected = false;
									selectedIconSets.Remove(iconSets[i]);
								}
							}

						}
						lastSelectedIconIndex = iconSets.Count - 1;
					}
				}
				//---Up arrow key pressed---//
				else if (Event.current.keyCode == KeyCode.UpArrow && lastSelectedIconIndex >= gridXIcons)
				{
					if (!Event.current.shift && !Event.current.control)
					{
						//---Select only this icon if no shift/ctrl pressed---//
						foreach (IconSet iconSet in iconSets)
							iconSet.selected = false;
						selectedIconSets.Clear();

						iconSets[lastSelectedIconIndex - gridXIcons].selected = true;
						selectedIconSets.Add(iconSets[lastSelectedIconIndex - gridXIcons]);
					}
					else
					{
						//---Add to current selection if shift/ctrl pressed---///
						if (!iconSets[lastSelectedIconIndex - gridXIcons].selected)
						{
							iconSets[lastSelectedIconIndex - gridXIcons].selected = true;
							selectedIconSets.Add(iconSets[lastSelectedIconIndex - gridXIcons]);
							for (int i = lastSelectedIconIndex; i > lastSelectedIconIndex - gridXIcons; i--)
							{
								iconSets[i].selected = true;
								selectedIconSets.Add(iconSets[i]);
							}
						}
						else
						{
							iconSets[lastSelectedIconIndex].selected = false;
							selectedIconSets.Remove(iconSets[lastSelectedIconIndex]);
							for (int i = lastSelectedIconIndex; i > lastSelectedIconIndex - gridXIcons; i--)
							{
								iconSets[i].selected = false;
								selectedIconSets.Remove(iconSets[i]);
							}
						}

					}
					lastSelectedIconIndex -= gridXIcons;
				}
				else if (Event.current.keyCode == KeyCode.A && Event.current.modifiers == EventModifiers.Control)
				{
					//---Select all if ctrl-A pressed---//
					selectedIconSets.Clear();
					foreach (KeyValuePair<string, IconSet> iconSet in sortedIconSetsByPath)
					{
						if (assetList.selectedFolders.Contains(iconSet.Value.folderPath))
						{
							iconSet.Value.selected = true;
							selectedIconSets.Add(iconSet.Value);
						}
					}
				}
			}
		}

		Dictionary<string, IconSet> SortIconSetsByFolder(Dictionary<string, IconSet> data)
		{
			//---Get a string array of asset paths---//
			string[] assetPaths = new string[data.Keys.Count];
			data.Keys.CopyTo(assetPaths, 0);

			//---Create a dictionary that will hold folder paths as keys, and a list of asset paths in the values (assets within the folder)---//
			Dictionary<string, List<string>> folders = new Dictionary<string, List<string>>();

			//---Create a list for just the folder names---//
			List<string> folderNames = new List<string>();

			foreach (string assetPath in assetPaths)
			{
				//---Get folder path from asset path---//
				string[] split = assetPath.Split('/');
				string folderPath = "";
				for (int i = 0; i < split.Length - 1; i++)
					folderPath += split[i] + (i < split.Length - 2 ? "/" : "");

				//---Add folder to folders dictionary if not already in there---//
				if (!folders.ContainsKey(folderPath))
				{
					folders.Add(folderPath, new List<string>());
					folderNames.Add(folderPath);
				}

				//---Add asset path in the list at the [folderPath] index of the folders dictionary---//
				folders[folderPath].Add(assetPath);
			}

			//---Sort the folder names---//
			folderNames.Sort();

			//---For each of the sorted folders, sort the assets within that folder---//
			string[] sortedAssetPaths = new string[assetPaths.Length];
			int index = 0;
			foreach (string folder in folderNames)
			{
				folders[folder].Sort();
				foreach (string assetPath in folders[folder])
				{
					//---Add the asset paths to the new list in sorted order---//
					sortedAssetPaths[index++] = assetPath;
				}
			}

			//---Use the sorted list of asset paths to create a dictionary of sorted icons---//
			Dictionary<string, IconSet> sortedData = new Dictionary<string, IconSet>();
			foreach (string assetPath in sortedAssetPaths)
			{
				sortedData.Add(assetPath, data[assetPath]);
			}

			return sortedData;
		}

		void CheckFocus(Rect checkRect)
		{
			//---Check if last mouse click was in the asset grid rect---//
			if (Event.current.rawType == EventType.MouseDown)
			{
				assetGridFocused = checkRect.Contains(Event.current.mousePosition);
			}

			//---Check the RapidIcon window is in focus---//
			if (EditorWindow.focusedWindow != null && EditorWindow.focusedWindow.GetType() != typeof(RapidIconWindow))
				assetGridFocused = false;
		}

		IconSet GetIconSetFromPath(string path)
		{
			//---Loop through icons and check if the path matches---//
			foreach (IconSet iconSet in objectIconSets.Values)
			{
				if (iconSet.assetPath == path)
					return iconSet;
			}

			return null;
		}
	}
}