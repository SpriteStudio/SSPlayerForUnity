/**
	SpriteStudioPlayer
	
	An anime parts resource
	
	Copyright(C) Web Technology Corp. 
	
*/

//#define _DEBUG

using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

[System.Serializable]
public class SsInheritanceParam
{
	public	bool	Use;	// Is this value used?
	public	float	Rate;	// how rate a value inherits from parent. 0 <= 10000 <= 20000

	public override string ToString()
	{
		return "Use: " + Use + ", Rate: " + Rate;
	}
};

[System.Serializable]
public class SsInheritanceState
{
	public	SsInheritanceType		Type;
	public	SsInheritanceParam[]	Values;
	
	public SsInheritanceState()
	{
		Values = new SsInheritanceParam[(int)SsKeyAttr.Num];
	}

	// initialize value depending on file format version
	public void Initialize( int iVersion, bool imageFlip )
	{
		Type = SsInheritanceType.Parent;	// first, set parent's value as intial value.
		for(int i = 0; i < Values.Length; i++ )
		{
			Values[i] = new SsInheritanceParam();
			SsKeyAttr attr = (SsKeyAttr)i;
#if false
			// obsolete
			if( iVersion == 0 )
			{
				// just to get info?
				Values[i].Use = true;
			}
			else if( iVersion == 1 )
			{
				// compatible with Ver.1
				Type = SsInheritanceType.Self; // use myself value
	//			if( i <= KEYFRAME_PRIO )
				if( (attr == SsKeyAttr.FlipV) || (attr == SsKeyAttr.FlipH) || (attr == SsKeyAttr.Hide) )
				{
					Values[i].Use = false;
				}
				else
				{
					Values[i].Use = true;
				}
			}
			else
			{
				// compatible with Ver.2
				if( !imageFlip )
				{
					Values[i].Use = true;
				}
				else
				{
					if( (attr == SsKeyAttr.FlipV) || (attr == SsKeyAttr.FlipH) || (attr == SsKeyAttr.Hide) )
					{
						Values[i].Use = false;
					}
					else
					{
						Values[i].Use = true;
					}
				}
			}
#else
			if (attr == SsKeyAttr.FlipV
			||	attr == SsKeyAttr.FlipH
			||	attr == SsKeyAttr.Hide)
			{
				Values[i].Use = false;
				Values[i].Rate = 0f;
			}
			else
			{
				Values[i].Use = true;
				Values[i].Rate = 1f;
			}
#endif
		}
	}

	public override string ToString()
	{
		string s = "Type: " + Type + "\n";
		int index = 0;
		foreach (var e in Values)
		{
			s += "\tValues[" + Enum.GetName(typeof(SsKeyAttr), index) +"]: " + e + "\n";
			++index;
		}
		s += "\n";
		return s;
	}
}

// attribute value interface
public interface SsAttrValue
{
	bool	HasValue {get;}
	int		RefKeyIndex {get;set;}
	void	SetValue(object v);
}

// this value will be stored in every frame generalized from key frame information.
[System.Serializable]
public class SsAttrValueBase<T, KeyType> : SsAttrValue
	where KeyType : SsKeyFrameInterface
{
	static public bool operator true(SsAttrValueBase<T, KeyType> p){return p != null;}
	static public bool operator false(SsAttrValueBase<T, KeyType> p){return p == null;}

	public	bool	HasValue {
		get{return _RefKeyIndex == -1;}
	}

	public	int		RefKeyIndex {
		get{return _RefKeyIndex;} 
		set{_RefKeyIndex = value;}
	}
	[SerializeField] int	_RefKeyIndex;

	public	T		Value {
		get {return _Value;}
		set {_Value = value; _RefKeyIndex = -1;}
	}
	[SerializeField] T		_Value;

	public	void	SetValue(object v)
	{
		Value = (T)v;
	}
}

// declare inherited classes to serialize through Unity.
[System.Serializable] public class SsBoolAttrValue :		SsAttrValueBase<bool, SsBoolKeyFrame> {}
[System.Serializable] public class SsIntAttrValue :			SsAttrValueBase<int, SsIntKeyFrame> {}
[System.Serializable] public class SsFloatAttrValue :		SsAttrValueBase<float, SsFloatKeyFrame> {}
[System.Serializable] public class SsPointAttrValue :		SsAttrValueBase<SsPoint, SsPointKeyFrame> {}
[System.Serializable] public class SsPaletteAttrValue :		SsAttrValueBase<SsPaletteKeyValue, SsPaletteKeyFrame> {}
[System.Serializable] public class SsColorBlendAttrValue :	SsAttrValueBase<SsColorBlendKeyValue, SsColorBlendKeyFrame> {}
[System.Serializable] public class SsVertexAttrValue :		SsAttrValueBase<SsVertexKeyValue, SsVertexKeyFrame> {}
[System.Serializable] public class SsUserDataAttrValue :	SsAttrValueBase<SsUserDataKeyValue, SsUserDataKeyFrame> {}
[System.Serializable] public class SsSoundAttrValue :		SsAttrValueBase<SsSoundKeyValue, SsSoundKeyFrame> {}

/// Part resource info
[System.Serializable]
public class SsPartRes
{
	[HideInInspector] public SsAnimation AnimeRes;	///< the animation which this part belongs to
	public	string					Name;
	public	SsPartType				Type;
	public	SsRect					PicArea;		///< area of source image to refer
	public	int 					OriginX;		///< pivot. offset from left top (as pixel count).
	public	int 					OriginY;
	public	int 					MyId;
	public	int 					ParentId;
	public	int 					ChildNum;
	public	int 					SrcObjId;		///< source image ID
	public	SsSourceObjectType		SrcObjType;		///< for scene(.sssx file) only.
	public	SsAlphaBlendOperation 	AlphaBlendType;
	public	SsInheritanceState		InheritState;

	public	SsInheritanceParam		InheritParam(SsKeyAttr attr)
	{
		return InheritState.Values[(int)attr];
	}
	
	public	bool					Inherits(SsKeyAttr attr)
	{
		return InheritParam(attr).Use;
	}

	public	float					InheritRate(SsKeyAttr attr)
	{
#if false
		// actual values are already inherited at import statically
		if (InheritState.Type == SsInheritanceType.Parent)
		{
			//... some code to get parent's value recursively.
		}
#endif
		SsInheritanceParam	p = InheritParam(attr);
		if (!p.Use) return 0f;
		return p.Rate;
	}
	
	// now these arrays which contain List or array inside can't be serialized.
	//public	SsAttrValue[][]			AttrValueArrays;	// jagged array can't be serialized in Unity
	
	// temporary to make individual lists.
	// this field is only available at the moment when the animation is imported.
	//List<SsKeyFrameInterface>[]			keyFrameLists;
	
	// these key arrays are just to serialize.
	public	List<SsFloatKeyFrame>		PosXKeys;
	public	List<SsFloatKeyFrame>		PosYKeys;
	public	List<SsFloatKeyFrame>		AngleKeys;
	public	List<SsFloatKeyFrame>		ScaleXKeys;
	public	List<SsFloatKeyFrame>		ScaleYKeys;
	public	List<SsFloatKeyFrame>		TransKeys;
	public	List<SsIntKeyFrame>			PrioKeys;
	public	List<SsBoolKeyFrame>		FlipHKeys;
	public	List<SsBoolKeyFrame>		FlipVKeys;
	public	List<SsBoolKeyFrame>		HideKeys;
	public	List<SsColorBlendKeyFrame>	PartsColKeys;
	public	List<SsPaletteKeyFrame>		PartsPalKeys;
	public	List<SsVertexKeyFrame>		VertexKeys;
	public	List<SsUserDataKeyFrame>	UserKeys;
	public	List<SsSoundKeyFrame>		SoundKeys;
	public	List<SsIntKeyFrame>			ImageOffsetXKeys;
	public	List<SsIntKeyFrame>			ImageOffsetYKeys;
	public	List<SsIntKeyFrame>			ImageOffsetWKeys;
	public	List<SsIntKeyFrame>			ImageOffsetHKeys;
	public	List<SsIntKeyFrame>			OriginOffsetXKeys;
	public	List<SsIntKeyFrame>			OriginOffsetYKeys;

	// these attribute arrays are just to serialize.
	public	SsFloatAttrValue[]		PosXValues;
	public	SsFloatAttrValue[]		PosYValues;
	public	SsFloatAttrValue[]		AngleValues;
	public	SsFloatAttrValue[]		ScaleXValues;
	public	SsFloatAttrValue[]		ScaleYValues;
	public	SsFloatAttrValue[]		TransValues;
	public	SsIntAttrValue[]		PrioValues;
	public	SsBoolAttrValue[]		FlipHValues;
	public	SsBoolAttrValue[]		FlipVValues;
	public	SsBoolAttrValue[]		HideValues;
	public	SsColorBlendAttrValue[]	PartsColValues;
	public	SsPaletteAttrValue[]	PartsPalValues;
	public	SsVertexAttrValue[]		VertexValues;
	public	SsUserDataAttrValue[]	UserValues;
	public	SsSoundAttrValue[]		SoundValues;
	public	SsIntAttrValue[]		ImageOffsetXValues;
	public	SsIntAttrValue[]		ImageOffsetYValues;
	public	SsIntAttrValue[]		ImageOffsetWValues;
	public	SsIntAttrValue[]		ImageOffsetHValues;
	public	SsIntAttrValue[]		OriginOffsetXValues;
	public	SsIntAttrValue[]		OriginOffsetYValues;
	
	[SerializeField]	private	SsKeyAttrFlags			_hasAttrFlags = new SsKeyAttrFlags();
	public	bool					HasAttrFlags(SsKeyAttrFlags masks)
	{
		return (_hasAttrFlags & masks) != 0;
	}
	public	bool					HasTrancparency;
	public	int						FrameNum;
	public	SsImageFile				imageFile;
	public	Vector2[]				UVs;
	public	Vector3[]				OrgVertices;		///< original 4 vertices will be not modified. it consists of OriginX/Y and PicArea.WH.
//	public	Vector3[,]				AnimatedVertices;	///< 4 vertices that will animate if this resource has vertex animes.
	
	public	bool	IsRoot {get {return MyId == 0;}}
	public	bool	HasParent {get {return ParentId >= 0;}}
	
	public
	SsPartRes()
	{
		Name = null;
		PicArea = new SsRect();
		InheritState = new SsInheritanceState();
		
		// create individual key frame list
		PosXKeys			= new List<SsFloatKeyFrame>();
		PosYKeys			= new List<SsFloatKeyFrame>();
		AngleKeys			= new List<SsFloatKeyFrame>();
		ScaleXKeys			= new List<SsFloatKeyFrame>();
		ScaleYKeys			= new List<SsFloatKeyFrame>();
		TransKeys			= new List<SsFloatKeyFrame>();
		PrioKeys			= new List<SsIntKeyFrame>();
		FlipHKeys			= new List<SsBoolKeyFrame>();
		FlipVKeys			= new List<SsBoolKeyFrame>();
		HideKeys			= new List<SsBoolKeyFrame>();
		PartsColKeys		= new List<SsColorBlendKeyFrame>();
		PartsPalKeys		= new List<SsPaletteKeyFrame>();
		VertexKeys			= new List<SsVertexKeyFrame>();
		UserKeys			= new List<SsUserDataKeyFrame>();
		SoundKeys			= new List<SsSoundKeyFrame>();
		ImageOffsetXKeys	= new List<SsIntKeyFrame>();
		ImageOffsetYKeys	= new List<SsIntKeyFrame>();
		ImageOffsetWKeys	= new List<SsIntKeyFrame>();
		ImageOffsetHKeys	= new List<SsIntKeyFrame>();
		OriginOffsetXKeys	= new List<SsIntKeyFrame>();
		OriginOffsetYKeys	= new List<SsIntKeyFrame>();
	}
	
	public Vector2[]
	CalcUVs(int ofsX, int ofsY, int ofsW, int ofsH)
	{
		if (Type != SsPartType.Normal) return null;

		float texW = imageFile.width;
		float texH = imageFile.height;
		if (UVs == null)
			UVs = new Vector2[4]{new Vector2(),new Vector2(),new Vector2(),new Vector2()};
		var rc = (Rect)PicArea;
		rc.xMin += ofsX;
		rc.yMin += ofsY;
		rc.xMax += ofsX + ofsW;
		rc.yMax += ofsY + ofsH;
		//--------- Left-Top based, clockwise
		UVs[0].x = UVs[3].x = rc.xMin / texW;		// Left
		UVs[0].y = UVs[1].y = 1 - rc.yMin / texH;	// Top
		UVs[1].x = UVs[2].x = rc.xMax / texW;		// Right
		UVs[2].y = UVs[3].y = 1 - rc.yMax / texH;	// Bottom
		return UVs;
	}
	
	public Vector3[]
	GetVertices(Vector2 size)
	{
		Vector2 offset = new Vector2(OriginX, OriginY);    // Offset of sprite from center of client GameObject
		Vector3[] verts = new Vector3[4]
		{
			new Vector3(-offset.x,			offset.y, 0f),
			new Vector3(size.x - offset.x,	offset.y, 0f),
			new Vector3(size.x - offset.x,	-(size.y -offset.y), 0f),
			new Vector3(-offset.x,			-(size.y -offset.y), 0f)
		};
		for (int i = 0; i < 4; ++i)
			verts[i] *= AnimeRes.ScaleFactor;
		return verts;
	}
	
	// precalculate attribute values each frame to decrease runtime cost
	public void
	CreateAttrValues(int frameNum, SsAssetDatabase database)
	{
		if (frameNum <= 0)
		{
			Debug.LogWarning("No keys to precalculate.");
			return;
		}
		FrameNum = frameNum;
//		if (keyFrameLists == null)
//		{
//			Debug.LogError("Key frame list must be prepared.");
//			return;
//		}
#if _DEBUG
		SsDebugLog.PrintLn("CreateAttrValues()");
		SsDebugLog.PrintLn("Name: " + Name);
#endif

		// create temporary key frame list
//		keyFrameLists = new List<SsKeyFrameInterface>[(int)SsKeyAttr.Num];
//		keyFrameLists[(int)SsKeyAttr.PosX] = PosXKeys;

		// create attribute value lists
		for (int attrIdx = 0; attrIdx < (int)SsKeyAttr.Num; ++attrIdx)
		{
			SsKeyAttr attr = (SsKeyAttr)attrIdx;
			int keyNum = GetKeyNumOf(attr);

			// skip non-key attribute
			if (keyNum <= 0) continue;
			
			int curKeyIndex = 0;
			SsKeyFrameInterface curKey = GetKey(attr, 0);
			int nextKeyIndex = 0;
			SsKeyFrameInterface nextKey = null;
			int valuesNum = 1;
			if (keyNum > 1)
			{
				nextKey = GetKey(attr, 1);
				nextKeyIndex = 1;
				// consolidate identical values if possible
				for (int i = 1; i < keyNum; ++i)
				{
					SsKeyFrameInterface key = GetKey(attr, i);
					if (!key.EqualsValue(curKey))
					{
						// give up consolidating
						valuesNum = frameNum;
						break;
					}
				}
			}
			else if (keyNum == 1)
			{
				if (attr == SsKeyAttr.Hide)
				{
					SsBoolKeyFrame hideKey = (SsBoolKeyFrame)curKey;
					if (hideKey.Value == false)
					{
						valuesNum = frameNum;
					}
				}
			}
			
			SsKeyAttrDesc	attrDesc = SsKeyAttrDescManager.GetById(attr);
			switch (attr)
			{
			case SsKeyAttr.PosX:	PosXValues = new SsFloatAttrValue[valuesNum];		break;
			case SsKeyAttr.PosY:	PosYValues = new SsFloatAttrValue[valuesNum];		break;
			case SsKeyAttr.Angle:	AngleValues = new SsFloatAttrValue[valuesNum];	break;
			case SsKeyAttr.ScaleX:	ScaleXValues = new SsFloatAttrValue[valuesNum];	break;
			case SsKeyAttr.ScaleY:	ScaleYValues = new SsFloatAttrValue[valuesNum];	break;
			case SsKeyAttr.Trans:	TransValues = new SsFloatAttrValue[valuesNum];	break;
			case SsKeyAttr.Prio:	PrioValues = new SsIntAttrValue[valuesNum];		break;
			case SsKeyAttr.FlipH:	FlipHValues = new SsBoolAttrValue[valuesNum];	break;
			case SsKeyAttr.FlipV:	FlipVValues = new SsBoolAttrValue[valuesNum];	break;
			case SsKeyAttr.Hide:	HideValues = new SsBoolAttrValue[valuesNum];		break;
			case SsKeyAttr.PartsCol:PartsColValues = new SsColorBlendAttrValue[valuesNum];break;
			case SsKeyAttr.PartsPal:PartsPalValues = new SsPaletteAttrValue[valuesNum];break;
			case SsKeyAttr.Vertex:	VertexValues = new SsVertexAttrValue[valuesNum];	break;
			case SsKeyAttr.User:	UserValues = new SsUserDataAttrValue[valuesNum];	break;
			case SsKeyAttr.Sound:	SoundValues = new SsSoundAttrValue[valuesNum];	break;
			case SsKeyAttr.ImageOffsetX:	ImageOffsetXValues = new SsIntAttrValue[valuesNum];	break;
			case SsKeyAttr.ImageOffsetY:	ImageOffsetYValues = new SsIntAttrValue[valuesNum];	break;
			case SsKeyAttr.ImageOffsetW:	ImageOffsetWValues = new SsIntAttrValue[valuesNum];	break;
			case SsKeyAttr.ImageOffsetH:	ImageOffsetHValues = new SsIntAttrValue[valuesNum];	break;
			case SsKeyAttr.OriginOffsetX:	OriginOffsetXValues = new SsIntAttrValue[valuesNum];	break;
			case SsKeyAttr.OriginOffsetY:	OriginOffsetYValues = new SsIntAttrValue[valuesNum];	break;
			}
			// mark that this attribute is used.
			_hasAttrFlags |= (SsKeyAttrFlags)(1 << attrIdx);
#if _DEBUG
			SsDebugLog.Print(string.Format("\tAttr[{0}]: {1}\n", attrIdx, attrDesc.Attr));
#endif
			for (int frame = 0; frame < valuesNum; ++frame)
			{
				if (nextKey != null
				&&	frame >= nextKey.Time)
				{
					// advance to next keyed frame
					curKeyIndex = nextKeyIndex;
					curKey = nextKey;
					++nextKeyIndex;
					nextKey = nextKeyIndex < keyNum ? GetKey(attr, nextKeyIndex) : null;
				}
				
				// base typed value
				SsAttrValue	v = null;
				// create new value to add
				SsBoolAttrValue			boolValue = null;
				SsIntAttrValue			intValue = null;
				SsFloatAttrValue		floatValue = null;
//				SsPointAttrValue		pointValue = null;
				SsPaletteAttrValue		paletteValue = null;
				SsColorBlendAttrValue	colorBlendValue = null;
				SsVertexAttrValue		vertexValue = null;
				SsUserDataAttrValue		userValue = null;
				SsSoundAttrValue		soundValue = null;
				switch (attrDesc.ValueType)
				{
				case SsKeyValueType.Data:		///< actually decimal or integer
					switch (attrDesc.CastType)
					{
					default:
						v = intValue = new SsIntAttrValue();
						break;
					case SsKeyCastType.Float:
					case SsKeyCastType.Degree:
						v = floatValue = new SsFloatAttrValue();
						break;
					}
					break;
				case SsKeyValueType.Param:		///< actually boolean
					v = boolValue = new SsBoolAttrValue();
					break;
//				case SsKeyValueType.Point:		///< x,y
//					v = pointValue = new SsPointAttrValue();
//					break;
				case SsKeyValueType.Palette:	///< left,top,right,bottom
					v = paletteValue = new SsPaletteAttrValue();
					break;
				case SsKeyValueType.Color:		///< single or vertex colors
					v = colorBlendValue = new SsColorBlendAttrValue();
					break;
				case SsKeyValueType.Vertex:		///< vertex positions relative to origin
					v = vertexValue = new SsVertexAttrValue();
					break;
				case SsKeyValueType.User:		///< user defined data(numeric|point|rect|string...)
					v = userValue = new SsUserDataAttrValue();
					break;
				case SsKeyValueType.Sound:		///< sound id, volume, note on...
					v = soundValue = new SsSoundAttrValue();
					break;
				}

#if false	// move this care to runtime
				if (attrDesc.Attr == SsKeyAttr.Hide
				&&	frame < curKey.Time)
				{
					// "hide" needs special care, it will be true before first key.
					boolValue.Value = true;
				}
				else
#endif
				if (attrDesc.NeedsInterpolatable && nextKey != null && frame >= curKey.Time)
				{
					bool doInterpolate = true;
					
					if (attrDesc.Attr == SsKeyAttr.PartsCol)
					{
						var curCbk = (SsColorBlendKeyValue)curKey.ObjectValue;
						var nextCbk = (SsColorBlendKeyValue)nextKey.ObjectValue;
						// if current and next key has no target, new value simply refers current key.
						if (curCbk.Target == SsColorBlendTarget.None
						&&	nextCbk.Target == SsColorBlendTarget.None)
						{
							v.RefKeyIndex = curKeyIndex;
							doInterpolate = false;
						}
					}

					if (doInterpolate)
					{
						// needs interpolation
						object res;
						if (frame == curKey.Time)
						{
							// use start key value as is.
							res = curKey.ObjectValue;
						}
						else
						{
							// interpolate curKey~nextKey
							res = SsInterpolation.InterpolateKeyValue(curKey, nextKey, frame);
						}
						try{
							// can't restore primitive type from the boxed through casting.
							if (boolValue)
								boolValue.Value = System.Convert.ToBoolean(res);
							else if (intValue)
								intValue.Value = System.Convert.ToInt32(res);
							else if (floatValue)
							{
								if ((attrDesc.Attr == SsKeyAttr.PosX || attrDesc.Attr == SsKeyAttr.PosY)
								&&	!database.NotIntegerizeInterpolatotedXYValues)
									floatValue.Value = System.Convert.ToInt32(res);
								else
									floatValue.Value = System.Convert.ToSingle(res);
							}
							else
								v.SetValue(res);
						}catch{
							Debug.LogError("[INTERNAL] failed to unbox: " + res);
						}
						
						if (attrDesc.Attr == SsKeyAttr.PartsCol)
						{
							var curCbk = (SsColorBlendKeyValue)curKey.ObjectValue;
							var nextCbk = (SsColorBlendKeyValue)nextKey.ObjectValue;
							// if current or next key has vertex colors, key between them should have vertex colors.
							if (curCbk.Target == SsColorBlendTarget.Vertex
							||	nextCbk.Target == SsColorBlendTarget.Vertex)
							{
								var newCbk = colorBlendValue.Value;
								newCbk.Target = SsColorBlendTarget.Vertex;
								// use next key operation.
								newCbk.Operation = nextCbk.Operation;
							}
						}
					}
				}
				else
				{
					// just use the value at last referred key frame.
					v.RefKeyIndex = curKeyIndex;
				}
#if _DEBUG
				if (v.Value != null)
					SsDebugLog.Print(string.Format("\t\tframe[{0}]: {1}\n", frame, v.Value));
#endif
				// add value to the relative array
				switch (attr)
				{
				case SsKeyAttr.PosX:			PosXValues[frame] = floatValue;		break;
				case SsKeyAttr.PosY:			PosYValues[frame] = floatValue;		break;
				case SsKeyAttr.Angle:			AngleValues[frame] = floatValue;	break;
				case SsKeyAttr.ScaleX:			ScaleXValues[frame] = floatValue;	break;
				case SsKeyAttr.ScaleY:			ScaleYValues[frame] = floatValue;	break;
				case SsKeyAttr.Trans:			TransValues[frame] = floatValue;	break;
				case SsKeyAttr.Prio:			PrioValues[frame] = intValue;		break;
				case SsKeyAttr.FlipH:			FlipHValues[frame] = boolValue;		break;
				case SsKeyAttr.FlipV:			FlipVValues[frame] = boolValue;		break;
				case SsKeyAttr.Hide:			HideValues[frame] = boolValue;		break;
				case SsKeyAttr.PartsCol:		PartsColValues[frame] = colorBlendValue;break;
				case SsKeyAttr.PartsPal:		PartsPalValues[frame] = paletteValue;	break;
				case SsKeyAttr.Vertex:			VertexValues[frame] = vertexValue;	break;
				case SsKeyAttr.User:			UserValues[frame] = userValue;		break;
				case SsKeyAttr.Sound:			SoundValues[frame] = soundValue;	break;
				case SsKeyAttr.ImageOffsetX:	ImageOffsetXValues[frame] = intValue;	break;
				case SsKeyAttr.ImageOffsetY:	ImageOffsetYValues[frame] = intValue;	break;
				case SsKeyAttr.ImageOffsetW:	ImageOffsetWValues[frame] = intValue;	break;
				case SsKeyAttr.ImageOffsetH:	ImageOffsetHValues[frame] = intValue;	break;
				case SsKeyAttr.OriginOffsetX:	OriginOffsetXValues[frame] = intValue;	break;
				case SsKeyAttr.OriginOffsetY:	OriginOffsetYValues[frame] = intValue;	break;
				}
			}
		}
		
		// set having transparency flag
		HasTrancparency = false;
		if (HasAttrFlags(SsKeyAttrFlags.Trans))
		{
			// if opacity value under 1 exists in some keys, we recognize this will be transparent at some frames
			foreach (SsFloatKeyFrame e in TransKeys)
			{
				if (e.Value < 1f)
				{
					HasTrancparency = true;
					break;
				}
			}
		}
	}
	
	public bool
	HasColorBlendKey()
	{
		return HasAttrFlags(SsKeyAttrFlags.PartsCol);
	}
	
	public void
	AddKeyFrame(SsKeyAttr attr, SsKeyFrameInterface key)
	{
		switch (attr)
		{
		case SsKeyAttr.PosX:			PosXKeys.Add((SsFloatKeyFrame)key); return;
		case SsKeyAttr.PosY:			PosYKeys.Add((SsFloatKeyFrame)key); return;
		case SsKeyAttr.Angle:			AngleKeys.Add((SsFloatKeyFrame)key); return;
		case SsKeyAttr.ScaleX:			ScaleXKeys.Add((SsFloatKeyFrame)key); return;
		case SsKeyAttr.ScaleY:			ScaleYKeys.Add((SsFloatKeyFrame)key); return;
		case SsKeyAttr.Trans:			TransKeys.Add((SsFloatKeyFrame)key); return;
		case SsKeyAttr.Prio:			PrioKeys.Add((SsIntKeyFrame)key); return;
		case SsKeyAttr.FlipH:			FlipHKeys.Add((SsBoolKeyFrame)key); return;
		case SsKeyAttr.FlipV:			FlipVKeys.Add((SsBoolKeyFrame)key); return;
		case SsKeyAttr.Hide:			HideKeys.Add((SsBoolKeyFrame)key); return;
		case SsKeyAttr.PartsCol:		PartsColKeys.Add((SsColorBlendKeyFrame)key); return;
		case SsKeyAttr.PartsPal:		PartsPalKeys.Add((SsPaletteKeyFrame)key); return;
		case SsKeyAttr.Vertex:			VertexKeys.Add((SsVertexKeyFrame)key); return;
		case SsKeyAttr.User:			UserKeys.Add((SsUserDataKeyFrame)key); return;
		case SsKeyAttr.Sound:			SoundKeys.Add((SsSoundKeyFrame)key); return;
		case SsKeyAttr.ImageOffsetX:	ImageOffsetXKeys.Add((SsIntKeyFrame)key); return;
		case SsKeyAttr.ImageOffsetY:	ImageOffsetYKeys.Add((SsIntKeyFrame)key); return;
		case SsKeyAttr.ImageOffsetW:	ImageOffsetWKeys.Add((SsIntKeyFrame)key); return;
		case SsKeyAttr.ImageOffsetH:	ImageOffsetHKeys.Add((SsIntKeyFrame)key); return;
		case SsKeyAttr.OriginOffsetX:	OriginOffsetXKeys.Add((SsIntKeyFrame)key); return;
		case SsKeyAttr.OriginOffsetY:	OriginOffsetYKeys.Add((SsIntKeyFrame)key); return;
		}
		Debug.LogError("Unknown attribute: " + attr);
	}
	
	public SsKeyFrameInterface
	GetKey(SsKeyAttr attr, int index)
	{
		switch (attr)
		{
		case SsKeyAttr.PosX:			return PosXKeys[index];
		case SsKeyAttr.PosY:			return PosYKeys[index];
		case SsKeyAttr.Angle:			return AngleKeys[index];
		case SsKeyAttr.ScaleX:			return ScaleXKeys[index];
		case SsKeyAttr.ScaleY:			return ScaleYKeys[index];
		case SsKeyAttr.Trans:			return TransKeys[index];
		case SsKeyAttr.Prio:			return PrioKeys[index];
		case SsKeyAttr.FlipH:			return FlipHKeys[index];
		case SsKeyAttr.FlipV:			return FlipVKeys[index];
		case SsKeyAttr.Hide:			return HideKeys[index];
		case SsKeyAttr.PartsCol:		return PartsColKeys[index];
		case SsKeyAttr.PartsPal:		return PartsPalKeys[index];
		case SsKeyAttr.Vertex:			return VertexKeys[index];
		case SsKeyAttr.User:			return UserKeys[index];
		case SsKeyAttr.Sound:			return SoundKeys[index];
		case SsKeyAttr.ImageOffsetX:	return ImageOffsetXKeys[index];
		case SsKeyAttr.ImageOffsetY:	return ImageOffsetYKeys[index];
		case SsKeyAttr.ImageOffsetW:	return ImageOffsetWKeys[index];
		case SsKeyAttr.ImageOffsetH:	return ImageOffsetHKeys[index];
		case SsKeyAttr.OriginOffsetX:	return OriginOffsetXKeys[index];
		case SsKeyAttr.OriginOffsetY:	return OriginOffsetYKeys[index];
		}
		Debug.LogError("Unknown attribute: " + attr);
		return null;
	}

	public int
	GetKeyNumOf(SsKeyAttr attr)
	{
		switch (attr)
		{
		case SsKeyAttr.PosX:			return PosXKeys.Count;
		case SsKeyAttr.PosY:			return PosYKeys.Count;
		case SsKeyAttr.Angle:			return AngleKeys.Count;
		case SsKeyAttr.ScaleX:			return ScaleXKeys.Count;
		case SsKeyAttr.ScaleY:			return ScaleYKeys.Count;
		case SsKeyAttr.Trans:			return TransKeys.Count;
		case SsKeyAttr.Prio:			return PrioKeys.Count;
		case SsKeyAttr.FlipH:			return FlipHKeys.Count;
		case SsKeyAttr.FlipV:			return FlipVKeys.Count;
		case SsKeyAttr.Hide:			return HideKeys.Count;
		case SsKeyAttr.PartsCol:		return PartsColKeys.Count;
		case SsKeyAttr.PartsPal:		return PartsPalKeys.Count;
		case SsKeyAttr.Vertex:			return VertexKeys.Count;
		case SsKeyAttr.User:			return UserKeys.Count;
		case SsKeyAttr.Sound:			return SoundKeys.Count;
		case SsKeyAttr.ImageOffsetX:	return ImageOffsetXKeys.Count;
		case SsKeyAttr.ImageOffsetY:	return ImageOffsetYKeys.Count;
		case SsKeyAttr.ImageOffsetW:	return ImageOffsetWKeys.Count;
		case SsKeyAttr.ImageOffsetH:	return ImageOffsetHKeys.Count;
		case SsKeyAttr.OriginOffsetX:	return OriginOffsetXKeys.Count;
		case SsKeyAttr.OriginOffsetY:	return OriginOffsetYKeys.Count;
		}
		Debug.LogError("Unknown attribute: " + attr);
		return 0;
	}
	
	public SsAttrValue[]
	GetAttrValues(SsKeyAttr attr)
	{
		switch (attr)
		{
		case SsKeyAttr.PosX:			return PosXValues;
		case SsKeyAttr.PosY:			return PosYValues;
		case SsKeyAttr.Angle:			return AngleValues;
		case SsKeyAttr.ScaleX:			return ScaleXValues;
		case SsKeyAttr.ScaleY:			return ScaleYValues;
		case SsKeyAttr.Trans:			return TransValues;
		case SsKeyAttr.Prio:			return PrioValues;
		case SsKeyAttr.FlipH:			return FlipHValues;
		case SsKeyAttr.FlipV:			return FlipVValues;
		case SsKeyAttr.Hide:			return HideValues;
		case SsKeyAttr.PartsCol:		return PartsColValues;
		case SsKeyAttr.PartsPal:		return PartsPalValues;
		case SsKeyAttr.Vertex:			return VertexValues;
		case SsKeyAttr.User:			return UserValues;
		case SsKeyAttr.Sound:			return SoundValues;
		case SsKeyAttr.ImageOffsetX:	return ImageOffsetXValues;
		case SsKeyAttr.ImageOffsetY:	return ImageOffsetYValues;
		case SsKeyAttr.ImageOffsetW:	return ImageOffsetWValues;
		case SsKeyAttr.ImageOffsetH:	return ImageOffsetHValues;
		case SsKeyAttr.OriginOffsetX:	return OriginOffsetXValues;
		case SsKeyAttr.OriginOffsetY:	return OriginOffsetYValues;
		}
		Debug.LogError("Unknown attribute: " + attr);
		return null;
	}
	
	public T
	AttrValue<T,AttrType,KeyType>(SsKeyAttr attr, int frame, List<KeyType> keyList)
		where AttrType : SsAttrValueBase<T, KeyType>
		where KeyType : SsKeyFrameBase<T>
	{
		SsAttrValue[] values = GetAttrValues(attr);
		if (values.Length == 1)
		{
			// always use the value at frame 0 because this attribute has only one constant value.
			frame = 0;
		}
		SsAttrValue v = values[frame];
		if (v.HasValue)
		{
			// has direct value
#if _DEBUG
			AttrType derivedAttr;
			try{
				derivedAttr = (AttrType)v;
				return derivedAttr.Value;
			}catch{
				Debug.LogError("derived VALUE can't be casted!!");
			}
			derivedAttr = (AttrType)v;
			return derivedAttr.Value;
#else
			return ((AttrType)v).Value;
#endif
		}
		else
		{
			// use raw value of the key
			KeyType key = keyList[v.RefKeyIndex];
			return key.Value;
		}
	}
	
	//--------- attribute value getters
	public	float					PosX(int frame)
	{
		if (!HasAttrFlags(SsKeyAttrFlags.PosX)) return 0;
		return AttrValue<float, SsFloatAttrValue, SsFloatKeyFrame>(SsKeyAttr.PosX, frame, PosXKeys);
	}
	public	float					PosY(int frame)
	{
		if (!HasAttrFlags(SsKeyAttrFlags.PosY)) return 0;
		return AttrValue<float, SsFloatAttrValue, SsFloatKeyFrame>(SsKeyAttr.PosY, frame, PosYKeys);
	}
	public	float					Angle(int frame)
	{
		if (!HasAttrFlags(SsKeyAttrFlags.Angle)) return 0f;
		return AttrValue<float, SsFloatAttrValue, SsFloatKeyFrame>(SsKeyAttr.Angle, frame, AngleKeys);
	}
	public	float					ScaleX(int frame)
	{
		if (!HasAttrFlags(SsKeyAttrFlags.ScaleX)) return 1f;
		return AttrValue<float, SsFloatAttrValue, SsFloatKeyFrame>(SsKeyAttr.ScaleX, frame, ScaleXKeys);
	}
	public	float					ScaleY(int frame)
	{
		if (!HasAttrFlags(SsKeyAttrFlags.ScaleY)) return 1f;
		return AttrValue<float, SsFloatAttrValue, SsFloatKeyFrame>(SsKeyAttr.ScaleY, frame, ScaleYKeys);
	}
	public	float					Trans(int frame)
	{
		if (!HasAttrFlags(SsKeyAttrFlags.Trans)) return 1f;
		return AttrValue<float, SsFloatAttrValue, SsFloatKeyFrame>(SsKeyAttr.Trans, frame, TransKeys);
	}
	public	float					Prio(int frame)
	{
		if (!HasAttrFlags(SsKeyAttrFlags.Prio)) return 0;
		return AttrValue<int, SsIntAttrValue, SsIntKeyFrame>(SsKeyAttr.Prio, frame, PrioKeys);
	}
	public	bool					FlipH(int frame)
	{
		if (!HasAttrFlags(SsKeyAttrFlags.FlipH)) return false;
		return AttrValue<bool, SsBoolAttrValue, SsBoolKeyFrame>(SsKeyAttr.FlipH, frame, FlipHKeys);
	}
	public	bool					FlipV(int frame)
	{
		if (!HasAttrFlags(SsKeyAttrFlags.FlipV)) return false;
		return AttrValue<bool, SsBoolAttrValue, SsBoolKeyFrame>(SsKeyAttr.FlipV, frame, FlipVKeys);
	}
	public	bool					Hide(int frame)
	{
		if (IsBeforeFirstKey(frame)) return true;
		return AttrValue<bool, SsBoolAttrValue, SsBoolKeyFrame>(SsKeyAttr.Hide, frame, HideKeys);
	}
	public	bool					IsBeforeFirstKey(int frame)
	{
		// Hide this part if this has no Hide keys.
		if (!HasAttrFlags(SsKeyAttrFlags.Hide)) return true;
		// Also hide before first key frame.
		return frame < HideKeys[0].Time;
	}
	public	SsColorBlendKeyValue	PartsCol(int frame)
	{
		if (!HasAttrFlags(SsKeyAttrFlags.PartsCol)) return null;
		if (frame < GetKey(SsKeyAttr.PartsCol, 0).Time) return null;
		return AttrValue<SsColorBlendKeyValue, SsColorBlendAttrValue, SsColorBlendKeyFrame>(SsKeyAttr.PartsCol, frame, PartsColKeys);
	}
	public	SsPaletteKeyValue		PartsPal(int frame)
	{
		if (!HasAttrFlags(SsKeyAttrFlags.PartsPal)) return null;
		return AttrValue<SsPaletteKeyValue, SsPaletteAttrValue, SsPaletteKeyFrame>(SsKeyAttr.PartsPal, frame, PartsPalKeys);
	}
	public	SsVertexKeyValue		Vertex(int frame)
	{
		if (!HasAttrFlags(SsKeyAttrFlags.Vertex)) return null;
		return AttrValue<SsVertexKeyValue, SsVertexAttrValue, SsVertexKeyFrame>(SsKeyAttr.Vertex, frame, VertexKeys);
	}
	public	SsUserDataKeyValue		User(int frame)
	{
		if (!HasAttrFlags(SsKeyAttrFlags.User)) return null;
		return AttrValue<SsUserDataKeyValue, SsUserDataAttrValue, SsUserDataKeyFrame>(SsKeyAttr.User, frame, UserKeys);
	}
	public	SsSoundKeyValue			Sound(int frame)
	{
		if (!HasAttrFlags(SsKeyAttrFlags.Sound)) return null;
		return AttrValue<SsSoundKeyValue, SsSoundAttrValue, SsSoundKeyFrame>(SsKeyAttr.Sound, frame, SoundKeys);
	}
	public	int						ImageOffsetX(int frame)
	{
		if (!HasAttrFlags(SsKeyAttrFlags.ImageOffsetX)) return 0;
		return AttrValue<int, SsIntAttrValue, SsIntKeyFrame>(SsKeyAttr.ImageOffsetX, frame, ImageOffsetXKeys);
	}
	public	int						ImageOffsetY(int frame)
	{
		if (!HasAttrFlags(SsKeyAttrFlags.ImageOffsetY)) return 0;
		return AttrValue<int, SsIntAttrValue, SsIntKeyFrame>(SsKeyAttr.ImageOffsetY, frame, ImageOffsetYKeys);
	}
	public	int						ImageOffsetW(int frame)
	{
		if (!HasAttrFlags(SsKeyAttrFlags.ImageOffsetW)) return 0;
		return AttrValue<int, SsIntAttrValue, SsIntKeyFrame>(SsKeyAttr.ImageOffsetW, frame, ImageOffsetWKeys);
	}
	public	int						ImageOffsetH(int frame)
	{
		if (!HasAttrFlags(SsKeyAttrFlags.ImageOffsetH)) return 0;
		return AttrValue<int, SsIntAttrValue, SsIntKeyFrame>(SsKeyAttr.ImageOffsetH, frame, ImageOffsetHKeys);
	}
	public	int						OriginOffsetX(int frame)
	{
		if (!HasAttrFlags(SsKeyAttrFlags.OriginOffsetX)) return 0;
		return AttrValue<int, SsIntAttrValue, SsIntKeyFrame>(SsKeyAttr.OriginOffsetX, frame, OriginOffsetXKeys);
	}
	public	int						OriginOffsetY(int frame)
	{
		if (!HasAttrFlags(SsKeyAttrFlags.OriginOffsetY)) return 0;
		return AttrValue<int, SsIntAttrValue, SsIntKeyFrame>(SsKeyAttr.OriginOffsetY, frame, OriginOffsetYKeys);
	}

	public override string
	ToString()
	{
		string s = String.Format(@"
Name:		{0}
Type:		{1}
PicArea:	{2}
OriginX:	{3}
OriginY:	{4}
MyID:		{5}
ParentID:	{6}
ChildNum:	{7}
SrcObjId:	{8}
SrcObjType:	{9}
BlendType:	{10}
",
			Name,
			Type,
			PicArea,
			OriginX,
			OriginY,
			MyId,
			ParentId,
			ChildNum,
			SrcObjId,
			SrcObjType,
			AlphaBlendType);
		s += "InheritState:\n" + InheritState;
		
#if false
		if (keyFrameLists != null)
		{
			int	attrIndex = 0;
			foreach (var list in keyFrameLists)
			{
				int index = 0;
				foreach (var e in list)
				{
					s += "keyFrameLists["+ Enum.GetName(typeof(SsKeyAttr), attrIndex) + "]" + "[" + index +"]: " + e;
					++index;
				}
				++attrIndex;
			}
		}
#endif
		s += "\n";
		return s;
	}
}
