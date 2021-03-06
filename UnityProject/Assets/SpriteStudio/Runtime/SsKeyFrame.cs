/**
	SpriteStudioPlayer
	
	Base key frame class and inherited classes
	
	Copyright(C) Web Technology Corp. 
	
*/

using UnityEngine;
using System.Collections;

/// parameters for interpolation
[System.Serializable]
public class SsCurveParams
{
	public	SsInterpolationType	Type;
	public	float				StartT;	// start time offset at handle point
	public	float				StartV;	// start value offset at handle point
	public	float				EndT;	// end time offset at handle point
	public	float				EndV;	// end value offset at handle point

	public bool IsNone
	{
		get {return Type == SsInterpolationType.None;}
	}
	
	public bool Equals(SsCurveParams r)
	{
		if (Type	!= r.Type)		return false;
		if (StartT	!= r.StartT)	return false;
		if (StartV	!= r.StartV)	return false;
		if (EndT	!= r.EndT)		return false;
		if (EndV	!= r.EndV)		return false;
		return true;
	}

	public override string ToString()
	{
		return "Type: " + Type + ", StartT: " + StartT + ", StartV: " + StartV + ", EndT: " + EndT + ", EndV: " + EndV;
	}
};

/// key interface
public interface SsKeyFrameInterface
{
	SsKeyValueType	ValueType {get;set;}
	int				Time {get;set;}
	object			ObjectValue {get;set;}
	bool			EqualsValue(SsKeyFrameInterface rhs);

	// curve params is not used if the ValueType == Param or Sound, that is to say they can't be interpolated. 
	SsCurveParams	Curve {get;set;}
}

/// common information of any keys.
[System.Serializable]
public class SsKeyFrameBase<T> : SsKeyFrameInterface
{
	public	T				Value;

	[SerializeField]	int	_Time;
	public	int				Time {get {return _Time;} set {_Time = value;}}
	
	[SerializeField]	SsKeyValueType	_ValueType;
	public	SsKeyValueType	ValueType {get {return _ValueType;} set {_ValueType = value;}}
	
	[SerializeField]	SsCurveParams	_Curve;
	public	SsCurveParams	Curve {get {return _Curve;} set{_Curve = value;}}

	public	bool		EqualsValue(SsKeyFrameInterface rhs)
	{
		if (_Curve != null && rhs.Curve != null)
		{
			if (!_Curve.Equals(rhs.Curve))
			{
//				Debug.LogWarning("DIFFERENT!!: " + _Curve + " != " + rhs.Curve);
				return false;
			}
//			else
//				Debug.Log("Same: " + _Curve + " == " + rhs.Curve);
		}
		else if (_Curve != rhs.Curve)
			return false;
		
		SsKeyFrameBase<T> derivedRhs = (SsKeyFrameBase<T>)rhs;
		if (!Value.Equals(derivedRhs.Value))
		{
//			Debug.LogWarning("DIFFERENT!!: " + Value + " != " + derivedRhs.Value);
			return false;
		}
//		else
//			Debug.Log("Same: " + Value + " == " + derivedRhs.Value);
		return true;
	}

	// this does boxing
	public	object	ObjectValue {
		get{return Value;}
		set{Value = (T)value;}
	}

	// apply result into derived type value.
//	private void _ApplyValue(float value) {}
	
	public override string ToString()
	{
		return "MyType: " + typeof(T) + ", ValueType: " + ValueType + ", Time: " + Time + ", Value {" + Value + "}, Curve {" + Curve + "}\n";
	}
}

// declare inherited classes
[System.Serializable] public class SsBoolKeyFrame :			SsKeyFrameBase<bool> {}
[System.Serializable] public class SsIntKeyFrame :			SsKeyFrameBase<int> {}
[System.Serializable] public class SsFloatKeyFrame :		SsKeyFrameBase<float> {}
[System.Serializable] public class SsPointKeyFrame :		SsKeyFrameBase<SsPoint> {}
[System.Serializable] public class SsPaletteKeyFrame :		SsKeyFrameBase<SsPaletteKeyValue> {}
[System.Serializable] public class SsColorBlendKeyFrame :	SsKeyFrameBase<SsColorBlendKeyValue> {}
[System.Serializable] public class SsVertexKeyFrame :		SsKeyFrameBase<SsVertexKeyValue> {}
[System.Serializable] public class SsUserDataKeyFrame :		SsKeyFrameBase<SsUserDataKeyValue> {}
[System.Serializable] public class SsSoundKeyFrame :		SsKeyFrameBase<SsSoundKeyValue> {}

/// attribute value interface
public interface SsAttrValueInterface
{
	SsAttrValueInterface	Clone();
}

/// about palette change
[System.Serializable]
public class SsPaletteKeyValue : SsAttrValueInterface, SsInterpolatable
{
	public bool	Use;	// use palette anime? use the palette inside image file if false.
	public int	Page;	// page number if is has multi palettes.
	public byte	Block;	// block index of 16x16 mode.

	public SsPaletteKeyValue() {}

	public SsPaletteKeyValue(SsPaletteKeyValue r)
	{
		Use = r.Use;
		Page = r.Page;
		Block = r.Block;
	}
	
	public override string ToString()
	{
		return "Use: " + Use + ", Page: " + Page + ", Block: " + Block;
	}

	public SsAttrValueInterface	Clone()
	{
		return new SsPaletteKeyValue(this);
	}

	public SsInterpolatable GetInterpolated(SsCurveParams curve, float time, SsInterpolatable start, SsInterpolatable end, int startTime, int endTime)
	{
		var v = new SsPaletteKeyValue();
		return v.Interpolate(curve, time, start, end, startTime, endTime);
	}

	public SsInterpolatable Interpolate(SsCurveParams curve, float time, SsInterpolatable start_, SsInterpolatable end_, int startTime, int endTime)
	{
		var start = (SsPaletteKeyValue)start_;
		var end = (SsPaletteKeyValue)end_;
		Use = SsInterpolation.Interpolate(curve, time, start.Use ? 1f : 0f, end.Use ? 1f : 0f, startTime, endTime) >= 0.5f ? true : false;
		Page = SsInterpolation.Interpolate(curve, time, start.Page, end.Page, startTime, endTime);
		Block = (byte)SsInterpolation.Interpolate(curve, time, start.Block, end.Block, startTime, endTime);
		return this;
	}
}

/// about color blending
[System.Serializable]
public class SsColorBlendKeyValue : SsAttrValueInterface, SsInterpolatable
{
	public SsColorBlendTarget		Target;
	public SsColorBlendOperation	Operation;
	// Color.A is used as its own color rate to texel color.
	public SsColorRef[]				Colors;		// [0]:left top(or whole color) [1]:right top [2]:left bottom [3]:right bottom

	public SsColorBlendKeyValue(int colorsNum)
	{
		Target = SsColorBlendTarget.None;
		Operation = SsColorBlendOperation.Mix;
		if (colorsNum > 0)
			Colors = SsColorRef.CreateArray(colorsNum);
	}
	public SsColorBlendKeyValue(SsColorBlendKeyValue r)
	{
		Target = r.Target;
		Operation = r.Operation;
		Colors = (SsColorRef[])r.Colors.Clone();
	}
	public SsAttrValueInterface	Clone()
	{
		return new SsColorBlendKeyValue(this);
	}

	public SsInterpolatable GetInterpolated(SsCurveParams curve, float time, SsInterpolatable start, SsInterpolatable end_, int startTime, int endTime)
	{
		var end = (SsColorBlendKeyValue)end_;
		var v = new SsColorBlendKeyValue(end.Colors.Length);
		return v.Interpolate(curve, time, start, end, startTime, endTime);
	}

	public SsInterpolatable Interpolate(SsCurveParams curve, float time, SsInterpolatable start_, SsInterpolatable end_, int startTime, int endTime)
	{
		var start = (SsColorBlendKeyValue)start_;
		var end = (SsColorBlendKeyValue)end_;
		
		if (start.Target == SsColorBlendTarget.None
		&&	end.Target == SsColorBlendTarget.None)
		{
#if false // obsolete
			// act as nothing blending when the targets of both keys are none.
			for (int i = 0; i < Colors.Length; ++i)
				Colors[i].R = Colors[i].G = Colors[i].B = Colors[i].A = 0;
			this.Target = SsColorBlendTarget.None;
			this.Operation = SsColorBlendOperation.Non;
#else
			// not needs to interpolate, just refers to start key.
			Debug.LogError("Must not come here.");
#endif
		}
		else
		{
			// Interpolates weight from start to end with the specified curve type.
			float rate = SsInterpolation.Interpolate(curve, time, 0.0f, 1.0f, startTime, endTime);

			// RGBA and ratio are always interpolated by linear
			SsInterpolationType orgType = curve.Type;
			curve.Type = SsInterpolationType.Linear;

			for (int i = 0; i < Colors.Length; ++i)
				Colors[i].Interpolate(curve, rate, start.Colors[i], end.Colors[i], startTime, endTime);

			curve.Type = orgType;

			bool useStartParam = time < 1 ? true : false;
			if (start.Target == SsColorBlendTarget.None)
			{
				// use the end key colors when the target of start key is none
				for (int i = 0; i < Colors.Length; ++i)
				{
					Colors[i].R = end.Colors[i].R;
					Colors[i].G = end.Colors[i].G;
					Colors[i].B = end.Colors[i].B;
				}
				useStartParam = false;
			}
			else
			if (end.Target == SsColorBlendTarget.None)
			{
				// use the start key colors when the target of end key is none
				for (int i = 0; i < Colors.Length; ++i)
				{
					Colors[i].R = start.Colors[i].R;
					Colors[i].G = start.Colors[i].G;
					Colors[i].B = start.Colors[i].B;
				}
			}
			// inherit target and operation
			if (useStartParam)
			{
				this.Target = start.Target;
				this.Operation = start.Operation;
			}
			else
			{
				this.Target = end.Target;
				this.Operation = end.Operation;
			}
		}
		return this;
	}

	public override string ToString()
	{
		string s = "Target: " + Target + ", Operation: " + Operation;
		if (Colors != null)
		{
			int index = 0;
			foreach (var e in Colors)
			{
				s += ", Color[" + index + "]: " + e;
				++index;
			}
		}
		return s;
	}
}

/// for modifying vertices. 4 vertices are relative to part's pivot.
[System.Serializable]
public class SsVertexKeyValue : SsAttrValueInterface, SsInterpolatable
{
	public SsPoint[]		Vertices;	// relative displacement of each vertex. [0]:left top [1]:right top [2]:right bottom [3]: left bottom
	
	// Y axis direction is flipped.
	public Vector3 Vertex3(int i) {return new Vector3(Vertices[i].X, -Vertices[i].Y, 0f);}

	public SsVertexKeyValue(int verticesNum)
	{
		if (verticesNum > 0)
			Vertices = SsPoint.CreateArray(verticesNum);
	}

	public SsVertexKeyValue(SsVertexKeyValue r)
	{
		Vertices = (SsPoint[])r.Vertices.Clone();
	}
	public SsAttrValueInterface	Clone()
	{
		return new SsVertexKeyValue(this);
	}

	public SsInterpolatable GetInterpolated(SsCurveParams curve, float time, SsInterpolatable start, SsInterpolatable end, int startTime, int endTime)
	{
		var v = new SsVertexKeyValue(((SsVertexKeyValue)end).Vertices.Length);
		return v.Interpolate(curve, time, start, end, startTime, endTime);
	}

	public SsInterpolatable Interpolate(SsCurveParams curve, float time, SsInterpolatable start_, SsInterpolatable end_, int startTime, int endTime)
	{
		var start = (SsVertexKeyValue)start_;
		var end = (SsVertexKeyValue)end_;
		
		// use curve params as blend ratio between start key and end key.
		float rate = SsInterpolation.Interpolate(curve, time, 0.0f, 1.0f, startTime, endTime);
		
		// lerp value from start to end.
		SsInterpolationType orgType = curve.Type;
		curve.Type = SsInterpolationType.Linear;
		
		for (int i = 0; i < Vertices.Length; ++i)
		{
			Vertices[i].Interpolate(curve, rate, start.Vertices[i], end.Vertices[i], startTime, endTime);
		}

		curve.Type = orgType;

		return this;
	}

	public override string ToString()
	{
		string s = "Vertices: ";
		if (Vertices != null)
		{
			int index = 0;
			foreach (var e in Vertices)
			{
				s += "[" + index + "]: " + e;
				++index;
			}
		}
		return s;
	}
}

/// user custom information
[System.Serializable]
public class SsUserDataKeyValue : SsAttrValueInterface
{
	public bool	IsNum;
	public int	Num;	// uint is correct but it cannot be serialized.

	public bool	IsRect;
	public SsRect	Rect;

	public bool	IsPoint;
	public SsPoint	Point;

	public bool	IsString;
	public string	String;

	public SsUserDataKeyValue() {}

	public SsUserDataKeyValue(SsUserDataKeyValue r)
	{
		IsNum = r.IsNum;
		Num = r.Num;
		IsRect = r.IsRect;
		this.Rect = r.Rect.Clone();
		IsPoint = r.IsPoint;
		this.Point = r.Point.Clone();
		IsString = r.IsString;
		this.String = System.String.Copy(r.String);
	}
	public SsAttrValueInterface	Clone()
	{
		return new SsUserDataKeyValue(this);
	}

	public override string ToString()
	{
		return "IsNum: " + IsNum
			+ ", Num: " + Num
			+ ", IsRect: " + IsRect
			+ ", Rect: " + Rect
			+ ", IsPoint: " + IsPoint
			+ ", Point: " + Point
			+ ", IsString: " + IsString
			+ ", String: " + String;
	}

}

/// sound operation
[System.Serializable]
public class SsSoundKeyValue : SsAttrValueInterface
{
	public SsSoundKeyFlags	Flags;
	public byte			SoundId;
	public byte			NoteOn;
	public byte			Volume;
	public byte			LoopNum;
	public uint			UserData;
//	public uint			Reserve1;
//	public uint			Reserve2;
//	public uint			Reserve3;

	public SsSoundKeyValue() {}
	
	public SsSoundKeyValue(SsSoundKeyValue r)
	{
		Flags = r.Flags;
		SoundId = r.SoundId;
		NoteOn = r.NoteOn;
		Volume = r.Volume;
		LoopNum = r.LoopNum;
		UserData = r.UserData;
	}
	public SsAttrValueInterface	Clone()
	{
		return new SsSoundKeyValue(this);
	}

	public override string ToString()
	{
		return "Flags: " + Flags
			+ ", SoundId: " + SoundId
			+ ", NoteOn: " + NoteOn
			+ ", Volume: " + Volume
			+ ", LoopNum: " + LoopNum
			+ ", UserData: " + UserData
//			+ ", Reserve1: " + Reserve1
//			+ ", Reserve2: " + Reserve2
//			+ ", Reserve3: " + Reserve3
			;
	}
}
