/**
	SpriteStudioPlayer
	
	Key attribute descriptions
	
	Copyright(C) Web Technology Corp. 
	
*/

using UnityEngine;
using System;
using System.Collections.Generic;

public class SsKeyAttrDesc
{	
//	public	string			Name {get;set;}	// verbose because this is identical to the key string.
	public	SsKeyAttr		Attr {get;set;}
	public	SsKeyValueType	ValueType {get;set;}
	public	SsKeyCastType	CastType {get;set;}
	public	bool			NeedsInterpolatable
	{
		get
		{
			switch (ValueType)
			{
			case SsKeyValueType.Param:
			case SsKeyValueType.User:
			case SsKeyValueType.Sound:
				return false;
			}
			return true;
		}
	}
	
	public SsKeyAttrDesc(SsKeyAttr a, SsKeyValueType v, SsKeyCastType c)
	{
		Attr = a;
		ValueType = v;
		CastType = c;
	}
}

static public class SsKeyAttrDescManager
{
	static private	Dictionary<string, SsKeyAttrDesc> _list = new Dictionary<string, SsKeyAttrDesc>
	{
		{"POSX",	new SsKeyAttrDesc(SsKeyAttr.PosX,	SsKeyValueType.Data,	SsKeyCastType.Float)},
		{"POSY",	new SsKeyAttrDesc(SsKeyAttr.PosY,	SsKeyValueType.Data,	SsKeyCastType.Float)},

		{"ANGL",	new SsKeyAttrDesc(SsKeyAttr.Angle,	SsKeyValueType.Data,	SsKeyCastType.Degree)},

		{"SCAX",	new SsKeyAttrDesc(SsKeyAttr.ScaleX,	SsKeyValueType.Data,	SsKeyCastType.Float)},
		{"SCAY",	new SsKeyAttrDesc(SsKeyAttr.ScaleY,	SsKeyValueType.Data,	SsKeyCastType.Float)},
		{"TRAN",	new SsKeyAttrDesc(SsKeyAttr.Trans,	SsKeyValueType.Data,	SsKeyCastType.Float)},

		{"PRIO",	new SsKeyAttrDesc(SsKeyAttr.Prio,	SsKeyValueType.Data,	SsKeyCastType.Int)},

		{"FLPH",	new SsKeyAttrDesc(SsKeyAttr.FlipH,	SsKeyValueType.Param,	SsKeyCastType.Bool)},
		{"FLPV",	new SsKeyAttrDesc(SsKeyAttr.FlipV,	SsKeyValueType.Param,	SsKeyCastType.Bool)},
		{"HIDE",	new SsKeyAttrDesc(SsKeyAttr.Hide,	SsKeyValueType.Param,	SsKeyCastType.Bool)},

		{"PCOL",	new SsKeyAttrDesc(SsKeyAttr.PartsCol,	SsKeyValueType.Color,	SsKeyCastType.Other)},
		{"PALT",	new SsKeyAttrDesc(SsKeyAttr.PartsPal,	SsKeyValueType.Palette,	SsKeyCastType.Other)},

		{"VERT",	new SsKeyAttrDesc(SsKeyAttr.Vertex,	SsKeyValueType.Vertex,	SsKeyCastType.Other)},

		{"UDAT",	new SsKeyAttrDesc(SsKeyAttr.User,	SsKeyValueType.User,	SsKeyCastType.Other)},

		{"IMGX",	new SsKeyAttrDesc(SsKeyAttr.ImageOffsetX,	SsKeyValueType.Data,	SsKeyCastType.Int)},
		{"IMGY",	new SsKeyAttrDesc(SsKeyAttr.ImageOffsetY,	SsKeyValueType.Data,	SsKeyCastType.Int)},
		{"IMGW",	new SsKeyAttrDesc(SsKeyAttr.ImageOffsetW,	SsKeyValueType.Data,	SsKeyCastType.Int)},
		{"IMGH",	new SsKeyAttrDesc(SsKeyAttr.ImageOffsetH,	SsKeyValueType.Data,	SsKeyCastType.Int)},

		{"ORFX",	new SsKeyAttrDesc(SsKeyAttr.OriginOffsetX,	SsKeyValueType.Data,	SsKeyCastType.Int)},
		{"ORFY",	new SsKeyAttrDesc(SsKeyAttr.OriginOffsetY,	SsKeyValueType.Data,	SsKeyCastType.Int)},

		{"SNDF",	new SsKeyAttrDesc(SsKeyAttr.Sound,	SsKeyValueType.Sound,	SsKeyCastType.Other)},
#if false
		// the followings are not contained as attribute so far. for .sssx only
		{"AREA",	new SsKeyAttrDesc(SsKeyAttr.Num,	SsKeyValueType.Num,	SsKeyCastType.Num)},
		{"ORGX",	new SsKeyAttrDesc(SsKeyAttr.Num,	SsKeyValueType.Num,	SsKeyCastType.Num)},
		{"ORGY",	new SsKeyAttrDesc(SsKeyAttr.Num,	SsKeyValueType.Num,	SsKeyCastType.Num)},
		{"TBDT",	new SsKeyAttrDesc(SsKeyAttr.Num,	SsKeyValueType.Num,	SsKeyCastType.Num)},
		{"MYID",	new SsKeyAttrDesc(SsKeyAttr.Num,	SsKeyValueType.Num,	SsKeyCastType.Num)},
		{"PAID",	new SsKeyAttrDesc(SsKeyAttr.Num,	SsKeyValueType.Num,	SsKeyCastType.Num)},
		{"CHID",	new SsKeyAttrDesc(SsKeyAttr.Num,	SsKeyValueType.Num,	SsKeyCastType.Num)},
		{"PCID",	new SsKeyAttrDesc(SsKeyAttr.Num,	SsKeyValueType.Data,	SsKeyCastType.Int)},
#endif
	};

	static public SsKeyAttrDesc Get(string tagName)
	{
		SsKeyAttrDesc attr;
		if (_list.TryGetValue(tagName, out attr))
		{
			return attr;
		}
		else
		{
			// not found, could be not yet supported type...
			return null;
		}
	}

	static public SsKeyAttrDesc GetById(SsKeyAttr attr)
	{
		foreach (SsKeyAttrDesc e in _list.Values)
			if (e.Attr == attr)
				return e;
		// not found, fatal error!! must add element to the list
		return null;
	}
}
