/**
	SpriteStudio
	
	SpriteStudio database
 
	Copyright(C) Web Technology Corp. 
	
*/

//#define _BUILD_UNIFIED_SHADERS

using UnityEngine;
using System.Collections.Generic;

public class SsAssetDatabase : MonoBehaviour
{
	public	const string fileName = "SpriteStudioDatabase";
	public	const string filePath = "Assets/SpriteStudioDatabase.prefab";
	
	[HideInInspector]	public	float	ScaleFactor = 1f;
	[HideInInspector]	public	bool	AngleCurveParamAsRadian = true;
	[HideInInspector]	public	bool	RefersToIndividualInheritValueForRootPart = true;	// if true, refers to individual inheritance settings on SS5, otherwise always refers to default settings as SS4.
	[HideInInspector]	public	bool	NotIntegerizeInterpolatotedXYValues = true;
#if _BUILD_UNIFIED_SHADERS
	public	bool	UseUnifiedShader = false;
#endif
	public	List<SsAnimation> animeList = new List<SsAnimation>();

	static	public	SsAssetDatabase	Instance;
	
	static	public	void	CreateNewObject()
	{
		var go = new GameObject(fileName);
		go.AddComponent<SsAssetDatabase>();
	}

	void OnEnable()
	{
		if (!Instance)
		{
			// cannot find with this function
//			Instance = GameObject.FindObjectOfType(typeof(SsAssetDatabase)) as SsAssetDatabase;
			GameObject go = GameObject.Find(fileName);
			Instance = go.GetComponent<SsAssetDatabase>();
			if (!Instance)
			{
				Debug.Log("Not found " + fileName + " in this scene");
				return;
			}
		}
		animeList = Instance.animeList;
	}

	public SsAnimation[] GetAnimeArray()
	{
		return animeList.ToArray();
	}
	
	public void AddAnime(SsAnimation anm)
	{
		CleanupAnimeList();
		foreach (var e in animeList)
			if (e == anm) return;
		animeList.Add(anm);
	}
	
	public SsAnimation GetAnime(string name)
	{
		foreach (var e in animeList)
			if (e.name == name) return e;
		return null;
	}
	
	public void CleanupAnimeList()
	{
		animeList.RemoveAll(e => e == null);
	}
}
