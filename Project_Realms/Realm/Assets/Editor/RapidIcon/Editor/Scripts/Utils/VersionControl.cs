using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RapidIcon_1_7_1
{
	public static class VersionControl
	{
		static Version thisVersion = new Version("1.7.1");

		public struct Version
		{
			public int major;
			public int minor;
			public int patch;

			public Version(int major, int minor, int patch)
			{
				this.major = major;
				this.minor = minor;
				this.patch = patch;
			}

			public Version(string version)
			{
				this = ConvertFromString(version);
			}

			public static Version ConvertFromString(string s)
			{
				Version version = new Version(0, 0, 0);

				string[] split = s.Split(".");
				if (split != null)
				{
					if (split.Length >= 1)
						int.TryParse(split[0], out version.major);

					if (split.Length >= 2)
						int.TryParse(split[1], out version.minor);

					if (split.Length >= 3)
						int.TryParse(split[2], out version.patch);
				}

				return version;
			}

			public static bool operator >(Version v1, Version v2)
			{
				if (v1.major > v2.major)
					return true;  //major is newer
				else if (v1.major < v2.major)
					return false; //major is older

				//major is equal

				if (v1.minor > v2.minor)
					return true;  //minor is newer
				else if (v1.minor < v2.minor)
					return false; //minor is older

				//minor is equal

				if (v1.patch > v2.patch)
					return true;  //patch is newer
				else if (v1.patch < v2.patch)
					return false; //patch is older

				//patch is equal, versions are equal
				return false;
			}

			public static bool operator <(Version v1, Version v2)
			{
				if (v1.major < v2.major)
					return true;  //major is older
				else if (v1.major > v2.major)
					return false; //major is newer

				//major is equal

				if (v1.minor < v2.minor)
					return true;  //minor is older
				else if (v1.minor > v2.minor)
					return false; //minor is newer

				//minor is equal

				if (v1.patch < v2.patch)
					return true;  //patch is older
				else if (v1.patch > v2.patch)
					return false; //patch is newer

				//patch is equal, versions are equal
				return false;
			}
		}

		public static string ConvertToString(this Version version)
		{
			return version.major + "." + version.minor + "." + version.patch;
		}

		public static Version GetStoredVersion()
		{
			string s = EditorPrefs.GetString(PlayerSettings.productName + "RapidIconVersion", thisVersion.ConvertToString());
			return Version.ConvertFromString(s);
		}

		public static void UpdateStoredVersion()
		{
			EditorPrefs.SetString(PlayerSettings.productName + "RapidIconVersion", thisVersion.ConvertToString());
		}

		public static bool IsStoredVersionOld()
		{
			Version version = GetStoredVersion();

			if (thisVersion > version)
				return true;

			return false;
		}

		public static void CheckUpdate(List<IconSet> iconSets)
		{
			Version lastVersion = GetStoredVersion();

			//---Pre 1.7.0 Updates---//
			//Versions before 1.7.0 are no longer supported
			if (lastVersion < new Version("1.7.0"))
			{
				Debug.LogWarning("RapidIcon versions prior to 1.7.0 are no longer supported");
				return;
			}
		}

		public static bool PreLoadCheck()
		{
			Version lastVersion = GetStoredVersion();

			//---1.7.0 Updates---//
			if (lastVersion < new Version("1.7.0"))
			{
				if (EditorUtility.DisplayDialog("Confirm", "Version 1.7.0 is not compatible with data from older version (" + lastVersion.ConvertToString() + "). Old data will be deleted, do you want to continue?", "Continue", "Cancel"))
				{
					EditorPrefs.DeleteKey(PlayerSettings.productName + "RapidIconOpenedFolders");
					EditorPrefs.DeleteKey(PlayerSettings.productName + "RapidIconSelectedFolders");

					EditorPrefs.DeleteKey(PlayerSettings.productName + "RapidIconSelectedAssets");
					EditorPrefs.DeleteKey(PlayerSettings.productName + "RapidIconAssetGridScroll");

					EditorPrefs.DeleteKey(PlayerSettings.productName + "RapidIconEditorTab");
					EditorPrefs.DeleteKey(PlayerSettings.productName + "RapidIconData");

					EditorPrefs.DeleteKey(PlayerSettings.productName + "RapidIconIconsRefreshed");

					EditorPrefs.DeleteKey(PlayerSettings.productName + "RapidIconFilterIdx");

					UpdateStoredVersion();
				}
				else
					return false;
			}

			return true;
		}
	}
}