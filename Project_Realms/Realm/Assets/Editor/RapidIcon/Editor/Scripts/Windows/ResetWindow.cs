using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RapidIcon_1_7_1
{
	public class ResetWindow : EditorWindow
	{
		public static ResetWindow window;
		private static int option;
		private static int subOption;
		private static IconEditor iconEditor;

		public static void Init(IconEditor editor)
		{
			window = (ResetWindow)GetWindow(typeof(ResetWindow), true, "Reset");
			window.position = new Rect(Screen.currentResolution.width / 2 - 200, Screen.currentResolution.height / 2 - 50, 400, 100);

			iconEditor = editor;

			window.Show();
		}

		void OnGUI()
		{
			GUILayout.Space(4);
			GUILayout.Label("Reset");

			//---Draw dropdown list of selected assets---//

			string[] options = new string[] { "Currently selected variant only", "All variants of currently selected icon", "Currently selected variant of all selected icons", "All variants of all selected icons", "Reset and delete all variants (this icon only)", "Reset and delete all variants (all selected icons)" };
			option = EditorGUILayout.Popup(option, options);

			if (option <= 3)
			{
				string[] subOptions = new string[] { "Current tab only", "All tabs" };
				subOption = EditorGUILayout.Popup(subOption, subOptions);
			}

			GUILayout.FlexibleSpace();

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Reset"))
			{
				DoReset();
				window.Close();
			}
			GUILayout.EndHorizontal();
		}

		private void DoReset()
		{
			switch (option)
			{
				case 0:
					ResetIcon(iconEditor.currentIcon);
					break;

				case 1:
					int idx = 0;
					int count = iconEditor.currentIconSet.icons.Count;
					foreach (Icon icon in iconEditor.currentIconSet.icons)
					{
						EditorUtility.DisplayProgressBar("Reset Icons", "Resetting icon (" + idx + "/" + count + ")", (float)(idx++) / count);
						ResetIcon(icon);
					}
					EditorUtility.ClearProgressBar();
					break;

				case 2:
					idx = 0;
					count = iconEditor.assetGrid.selectedIconSets.Count;
					foreach (IconSet iconSet in iconEditor.assetGrid.selectedIconSets)
					{
						EditorUtility.DisplayProgressBar("Reset Icons", "Resetting icon (" + idx + "/" + count + ")", (float)(idx++) / count);
						Icon icon = iconSet.GetCurrentIcon();
						ResetIcon(icon);
					}
					EditorUtility.ClearProgressBar();
					break;

				case 3:
					idx = 0;
					count = iconEditor.assetGrid.selectedIconSets.Count;
					foreach (IconSet iconSet in iconEditor.assetGrid.selectedIconSets)
					{
						EditorUtility.DisplayProgressBar("Reset Icons", "Resetting icon (" + idx + "/" + count + ")", (float)(idx++) / count);
						foreach (Icon icon in iconSet.icons)
						{
							ResetIcon(icon);
						}
					}
					EditorUtility.ClearProgressBar();
					break;
				case 4:
					iconEditor.currentIconSet.icons.Clear();

					ObjectPathPair currrntSetObj = new ObjectPathPair(iconEditor.currentIconSet.assetObject, iconEditor.currentIconSet.assetPath);
					iconEditor.currentIconSet.AddDefaultIcon(iconEditor.assetGrid, currrntSetObj);

					iconEditor.currentIconSet.iconIndex = 0;
					break;

				case 5:
					idx = 0;
					count = iconEditor.assetGrid.selectedIconSets.Count;
					foreach (IconSet iconSet in iconEditor.assetGrid.selectedIconSets)
					{
						EditorUtility.DisplayProgressBar("Reset Icons", "Resetting icon (" + idx + "/" + count + ")", (float)(idx++) / count);
						iconSet.icons.Clear();

						ObjectPathPair setObj = new ObjectPathPair(iconSet.assetObject, iconSet.assetPath);
						iconSet.AddDefaultIcon(iconEditor.assetGrid, setObj);

						iconSet.iconIndex = 0;
					}
					EditorUtility.ClearProgressBar();
					break;
			}

		}

		private void ResetIcon(Icon icon)
		{
			//---Create a reset icon---//
			ObjectPathPair obj = new ObjectPathPair(icon.parentIconSet.assetObject, icon.parentIconSet.assetPath);
			IconSet newIconSet = iconEditor.assetGrid.CreateIconSet(obj);

			Utils.CopyIconSettings(newIconSet.GetCurrentIcon(), icon, subOption == 0 ? iconEditor.tab : -1);
			ResetHierarchy(icon);

			Utils.UpdateIcon(icon, iconEditor);
		}

		private void ResetHierarchy(Icon icon)
		{
			List<string> keys = icon.iconSettings.subObjectEnables.Keys.ToList();
			foreach (string s in keys)
				icon.iconSettings.subObjectEnables[s] = true;
		}
	}
}