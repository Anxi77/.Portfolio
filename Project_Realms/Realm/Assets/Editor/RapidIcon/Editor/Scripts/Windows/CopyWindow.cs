using UnityEditor;
using UnityEngine;

namespace RapidIcon_1_7_1
{
	public class CopyWindow : EditorWindow
	{
		public static CopyWindow window;
		private static int option;
		private static int subOption;
		private static int selectedVariantIdx;
		private static IconEditor iconEditor;

		public static void Init(IconEditor editor)
		{
			window = (CopyWindow)GetWindow(typeof(CopyWindow), true, "Copy to other icons");
			window.position = new Rect(Screen.currentResolution.width / 2 - 230, Screen.currentResolution.height / 2 - 64, 460, 128);

			iconEditor = editor;
			selectedVariantIdx = iconEditor.currentIconSet.icons.IndexOf(iconEditor.currentIcon);

			window.Show();
		}

		void OnGUI()
		{
			GUILayout.Space(4);
			GUILayout.Label("Copy to");

			//---Draw dropdown list of selected assets---//

			string[] options = new string[] { "All variants of this icon", "Currently selected variant of selected icons", "All variants of selected icons", "Specific variant of selected icons" };
			option = EditorGUILayout.Popup(option, options);

			string[] subOptions = new string[] { "Current tab only", "All tabs" };
			subOption = EditorGUILayout.Popup(subOption, subOptions);

			if (option == 3)
			{
				selectedVariantIdx = EditorGUILayout.IntField("Variant Index", selectedVariantIdx);
				if (selectedVariantIdx < 0)
					selectedVariantIdx = 0;

				bool showWarning = false;
				foreach (IconSet iconSet in iconEditor.assetGrid.selectedIconSets)
				{
					if (selectedVariantIdx >= iconSet.icons.Count)
					{
						showWarning = true;
						break;
					}
				}

				if (showWarning)
				{
					GUIStyle s = new GUIStyle(GUI.skin.label);
					s.normal.textColor = new Color(1.0f, 0.5f, 0.0f, 1.0f);
					GUILayout.Label("Not all selected icons have a variant at index " + selectedVariantIdx + ", a new variant will be created", s);
				}
			}

			GUILayout.FlexibleSpace();

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Copy"))
			{
				DoCopy();
				window.Close();
			}
			GUILayout.EndHorizontal();
		}

		private void DoCopy()
		{
			switch (option)
			{
				case 0:
					int idx = 0;
					int count = iconEditor.currentIconSet.icons.Count;
					foreach (Icon icon in iconEditor.currentIconSet.icons)
					{
						EditorUtility.DisplayProgressBar("Copy to Other Icons", "Copying to icon (" + idx + "/" + count + ")", (float)(idx++)/count);
						Utils.CopyIconSettings(iconEditor.currentIcon, icon, subOption == 0 ? iconEditor.tab : -1);
						Utils.UpdateIcon(icon, iconEditor);
					}
					EditorUtility.ClearProgressBar();
					break;

				case 1:
					idx = 0;
					count = iconEditor.assetGrid.selectedIconSets.Count;
					foreach (IconSet iconSet in iconEditor.assetGrid.selectedIconSets)
					{
						EditorUtility.DisplayProgressBar("Copy to Other Icons", "Copying to icon (" + idx + "/" + count + ")", (float)(idx++) / count);
						Icon icon = iconSet.GetCurrentIcon();
						Utils.CopyIconSettings(iconEditor.currentIcon, icon, subOption == 0 ? iconEditor.tab : -1);
						Utils.UpdateIcon(icon, iconEditor);
					}
					EditorUtility.ClearProgressBar();
					break;

				case 2:
					idx = 0;
					count = iconEditor.assetGrid.selectedIconSets.Count;
					foreach (IconSet iconSet in iconEditor.assetGrid.selectedIconSets)
					{
						EditorUtility.DisplayProgressBar("Copy to Other Icons", "Copying to icon (" + idx + "/" + count + ")", (float)(idx++) / count);
						foreach (Icon icon in iconSet.icons)
						{
							Utils.CopyIconSettings(iconEditor.currentIcon, icon, subOption == 0 ? iconEditor.tab : -1);
							Utils.UpdateIcon(icon, iconEditor);
						}
					}
					EditorUtility.ClearProgressBar();
					break;

				case 3:
					idx = 0;
					count = iconEditor.assetGrid.selectedIconSets.Count;
					foreach (IconSet iconSet in iconEditor.assetGrid.selectedIconSets)
					{
						EditorUtility.DisplayProgressBar("Copy to Other Icons", "Copying to icon (" + idx + "/" + count + ")", (float)(idx++) / count);
						Icon icon;
						if (iconSet.icons.Count > selectedVariantIdx)
							icon = iconSet.icons[selectedVariantIdx];
						else
						{
							icon = iconSet.AddDefaultIcon(iconEditor.assetGrid, new ObjectPathPair { path = iconSet.assetPath, UnityEngine_object = iconSet.assetObject });
							iconSet.iconIndex = iconSet.icons.Count - 1;
							icon.iconSettings.exportName += " (" + iconSet.iconIndex + ")";
						}

						Utils.CopyIconSettings(iconEditor.currentIcon, icon, subOption == 0 ? iconEditor.tab : -1);
						Utils.UpdateIcon(icon, iconEditor);
					}
					EditorUtility.ClearProgressBar();
					break;
			}
		}
	}
}