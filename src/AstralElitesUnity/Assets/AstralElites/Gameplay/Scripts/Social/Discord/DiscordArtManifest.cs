using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using System.Runtime.Serialization.Json;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

[DataContract]
public struct DiscordArtAssetData
{
    [DataMember]
    public int type;
    [DataMember]
    public string id;
    [DataMember]
    public string name;

    public override string ToString()
    {
        return string.Format("{0} (ID: {1})", name, id);
    }
}

[Serializable]
public class DiscordArtAsset
{
    public string Name;
    public string Id;
    public Texture2D Image;

    public DiscordArtAsset(string key, string id, Texture2D image)
    {
        Name = key;
        Id = id;
        Image = image;
    }
}

[CreateAssetMenu]
public class DiscordArtManifest : ScriptableObject
{
    public List<DiscordArtAsset> Assets;
}

#if UNITY_EDITOR
[CustomEditor(typeof(DiscordArtManifest))]
public class DiscordArtManifestEditor : Editor
{
	public const string AssetListEndpoint = "https://discordapp.com/api/oauth2/applications/487740611378675713/assets";
	public const string ArtAssetEndpoint = "https://cdn.discordapp.com/app-assets/487740611378675713/{0}";

	private GUIStyle labelStyle = null;

	public override void OnInspectorGUI()
	{
		if (labelStyle == null)
		{
			labelStyle = new GUIStyle(EditorStyles.label)
			{
				alignment = TextAnchor.MiddleLeft
			};
		}

		DrawDefaultInspector();

		var manifest = (DiscordArtManifest)target;

		if (manifest.Assets != null)
		{
			foreach (var element in manifest.Assets)
			{
				EditorGUILayout.BeginVertical(EditorStyles.helpBox);

				var elementRect = GUILayoutUtility.GetRect(0, 42);

				var iconRect = new Rect(elementRect.x, elementRect.y, elementRect.height, elementRect.height);
				var labelRect = new Rect(iconRect.xMax + 8, elementRect.y, elementRect.xMax - iconRect.xMax - 8, elementRect.height);

				if (element.Image != null)
				{
					GUI.DrawTexture(iconRect, element.Image);
				}

				EditorGUI.LabelField(labelRect, element.Name, labelStyle);

				EditorGUILayout.EndVertical();
			}
		}

		if (GUILayout.Button("Update"))
		{
			EditorCoroutine.Start(Download());
		}
	}

	private IEnumerator Download()
	{
		ClearManifest();
		var manifest = (DiscordArtManifest)target;
		manifest.Assets = new List<DiscordArtAsset>();

		var assetListRequest = UnityWebRequest.Get(AssetListEndpoint);
		var listRequest = assetListRequest.SendWebRequest();
		while (!listRequest.isDone)
		{
			yield return null;
		}

		var assetData = (DiscordArtAssetData[])new DataContractJsonSerializer(typeof(DiscordArtAssetData[]))
			.ReadObject(new MemoryStream(assetListRequest.downloadHandler.data));

		foreach (var data in assetData)
		{
			string formattedArtAssetEndpoint = string.Format(ArtAssetEndpoint, data.id);

			// var artAssetDownload = UnityWebRequestTexture.GetTexture(formattedArtAssetEndpoint);
			// 
			// var request = artAssetDownload.SendWebRequest();
			// 
			// while (!request.isDone)
			// {
			// 	yield return null;
			// }
			// 
			// var artAssetTexture = ((DownloadHandlerTexture)artAssetDownload.downloadHandler).texture;
			// 
			// artAssetTexture = ChangeFormat(artAssetTexture, TextureFormat.RGB24);
			// artAssetTexture.name = ObjectNames.NicifyVariableName(data.name);
			// 
			// manifest.Assets.Add(new DiscordArtAsset(data.name, data.id, artAssetTexture));
			// AssetDatabase.AddObjectToAsset(artAssetTexture, target);
		}
		EditorUtility.SetDirty(target);
		AssetDatabase.SaveAssets();
	}

	private void ClearManifest()
	{
		var objects = AssetDatabase.LoadAllAssetsAtPath(
			AssetDatabase.GetAssetPath(target));

		for (int i = 0; i < objects.Length; i++)
		{
			var obj = objects[i];

			if (obj.GetType() == typeof(Texture2D))
			{
				DestroyImmediate(obj, true);
			}
		}

		EditorUtility.SetDirty(target);
	}

	public static Texture2D ChangeFormat(Texture2D oldTexture, TextureFormat newFormat)
	{
		var newTex = new Texture2D(oldTexture.width, oldTexture.height, newFormat, false);
		newTex.SetPixels(oldTexture.GetPixels());
		newTex.Apply();

		return newTex;
	}
}
#endif
