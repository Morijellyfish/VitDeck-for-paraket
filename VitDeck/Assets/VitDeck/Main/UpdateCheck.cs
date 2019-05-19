using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using VitDeck.Utility;

namespace VitDeck.Main
{
	/// <summary>
	/// VitDeckが最新版になっているかバージョンチェックを行うクラス。
	/// </summary>
	public static class UpdateCheck
	{
		public static bool IsLatest()
		{
			// テスト用
			var releaseUrl = "https://vkettools.github.io/VitDeckTest/releases/latest.json";

			var localVersion = VitDeck.GetVersion();
			var latestVersion = GetLatestVersion(releaseUrl);

			return string.Equals(localVersion, latestVersion);
		}

		public static string GetLatestVersion(string releaseUrl)
		{
			var release = ReleaseInfoCoroutine(releaseUrl);
			while (release.MoveNext()) { }
			var version = release.Current.ToString();

			return version;
		}

		static IEnumerator ReleaseInfoCoroutine(string releaseUrl)
		{
			var request = UnityWebRequest.Get(releaseUrl);
			request.downloadHandler = new DownloadHandlerBuffer();
			yield return request.SendWebRequest();

			while (!request.isDone)
			{
				yield return null;
			}

			if (request.isHttpError || request.isNetworkError)
			{
				Debug.Log(request.error);
			}
			else
			{
				var text = request.downloadHandler.text;
				var info = JsonUtility.FromJson<ReleaseInfo>(text);
				yield return info.tag_name;
			}
		}

		[Serializable]
		public class ReleaseInfo
		{
			public string tag_name;
		}
	}
}

