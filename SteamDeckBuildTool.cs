// Original by Wes Jurica - 2022, UNLICENSE
// https://gist.github.com/nikescar1/eba8d031f8366621ee48057b936cac97
// Modified:
// to take Product Name from Project Settings as devkit Title.
// to target both Windows and Linux builds.

namespace _0
{
	using System.Collections;
	using Unity.EditorCoroutines.Editor;
	using UnityEditor;
	using UnityEditor.Callbacks;
	using UnityEngine;
	using UnityEngine.Networking;

	/// <summary>
	/// Posts notification to locally running SteamOS DevKit Client.
	/// </summary>
	public static class SteamDeckBuildTool
	{
		static PostUploadToSteamDeckCoroutine postCoroutine;

		[PostProcessBuild(1000)]
		public static void UploadToSteamDeck(BuildTarget target, string pathToBuiltProject)
		{
			if (target is BuildTarget.StandaloneWindows64 or BuildTarget.StandaloneLinux64)
			{
				postCoroutine = new PostUploadToSteamDeckCoroutine();
				postCoroutine.Start();
			}
		}

		[System.Serializable]
		public class SteamDeckBuildJson
		{
			public string type = "build";
			public string status = "success";
			public string name = "RXC_Release"; // Enter "Name" from Steam Devkit Management Tool here
		}

		public class PostUploadToSteamDeckCoroutine
		{
			public void Start()
			{
				EditorCoroutineUtility.StartCoroutine(PostUploadToSteamDeck(), this);
			}
		}

		static IEnumerator PostUploadToSteamDeck()
		{
			string json = JsonUtility.ToJson(new SteamDeckBuildJson() { name = Application.productName });
			byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);

			using (UnityWebRequest webRequest = UnityWebRequest.PostWwwForm("http://127.0.0.1:32010/post_event", ""))
			{
				using (UploadHandlerRaw uH = new UploadHandlerRaw(bytes))
				{
					webRequest.uploadHandler = uH;
					webRequest.SetRequestHeader("Content-Type", "application/json");
					yield return webRequest.SendWebRequest();

					if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
					    webRequest.result == UnityWebRequest.Result.ProtocolError)
					{
						if (webRequest.error.Equals("Cannot connect to destination host"))
						{
							Debug.Log(
								"Steam Deck upload error: Cannot connect to Steam Deck. Is SteamOS Devkit Client running?");
						}
						else if (webRequest.error.Equals("HTTP/1.1 500 Internal Server Error"))
						{
							Debug.Log(
								"Steam Deck upload error: Cannot connect to Steam Deck. Is the Steam Deck powered on and in Game Mode?");
						}
						else
						{
							Debug.Log("Steam Deck upload error: " + webRequest.error);
						}
					}
					else
					{
						Debug.Log("Uploaded to Steam Deck");
					}
				}
			}
		}
	}
}
