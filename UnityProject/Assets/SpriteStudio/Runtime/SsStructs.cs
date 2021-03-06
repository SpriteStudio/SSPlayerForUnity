/**
	SpriteStudioPlayer
	
	Common structures
	
	Copyright(C) Web Technology Corp. 
	
*/

//#define _BUILD_UNIFIED_SHADERS

using UnityEngine;

[System.Serializable]
public class SsPoint : SsInterpolatable
{
	public	int	X;
	public	int	Y;

	public override string ToString()
	{
		return "X: " + X + ", Y: " + Y;
	}
	
	static public SsPoint[] CreateArray(int num)
	{
		var a = new SsPoint[num];
		for (int i = 0; i < a.Length; ++i)
			a[i] = new SsPoint();
		return a;
	}

	public SsPoint() {}

	public SsPoint(SsPoint r)
	{
		X = r.X;
		Y = r.Y;
	}

	public SsPoint Clone()
	{
		return new SsPoint(this);
	}

	public SsInterpolatable GetInterpolated(SsCurveParams curve, float time, SsInterpolatable start, SsInterpolatable end, int startTime, int endTime)
	{
		var v = new SsPoint();
		return v.Interpolate(curve, time, start, end, startTime, endTime);
	}

	public SsInterpolatable Interpolate(SsCurveParams curve, float time, SsInterpolatable start_, SsInterpolatable end_, int startTime, int endTime)
	{
		var start = (SsPoint)start_;
		var end = (SsPoint)end_;
		X = SsInterpolation.Interpolate(curve, time, start.X, end.X, startTime, endTime);
		Y = SsInterpolation.Interpolate(curve, time, start.Y, end.Y, startTime, endTime);
		return this;
	}
	
	public void Scale(float s)
	{
		X = (int)((float)X * s);
		Y = (int)((float)Y * s);
	}
}

[System.Serializable]
public class SsRect : SsInterpolatable
{
	public	int	Left;
	public	int	Top;
	public	int	Right;
	public	int	Bottom;

	static public SsRect[] CreateArray(int num)
	{
		var a = new SsRect[num];
		for (int i = 0; i < a.Length; ++i)
			a[i] = new SsRect();
		return a;
	}

	static public explicit operator UnityEngine.Rect(SsRect s)
	{
		var d = new UnityEngine.Rect();
		d.xMin = s.Left;
		d.xMax = s.Right;
		d.yMin = s.Top;
		d.yMax = s.Bottom;
		return d;
	}

	public SsRect() {}

	public SsRect(SsRect r)
	{
		Left = r.Left;
		Top = r.Top;
		Right = r.Right;
		Bottom = r.Bottom;
	}

	public SsRect Clone()
	{
		return new SsRect(this);
	}
	
	public int Width {get {return Right - Left;}}
	public int Height {get {return Bottom - Top;}}

	public Vector2 WH()
	{
		return new Vector2(Right - Left, Bottom - Top);
	}
	
	public override string ToString()
	{
		return "Left: " + Left + ", Top: " + Top + ", Right: " + Right + ", Bottom: " + Bottom;
	}

	public SsInterpolatable GetInterpolated(SsCurveParams curve, float time, SsInterpolatable start, SsInterpolatable end, int startTime, int endTime)
	{
		var v = new SsRect();
		return v.Interpolate(curve, time, start, end, startTime, endTime);
	}

	public SsInterpolatable Interpolate(SsCurveParams curve, float time, SsInterpolatable start_, SsInterpolatable end_, int startTime, int endTime)
	{
		var start = (SsRect)start_;
		var end = (SsRect)end_;
		Left = SsInterpolation.Interpolate(curve, time, start.Left, end.Left, startTime, endTime);
		Top = SsInterpolation.Interpolate(curve, time, start.Top, end.Top, startTime, endTime);
		Right = SsInterpolation.Interpolate(curve, time, start.Right, end.Right, startTime, endTime);
		Bottom = SsInterpolation.Interpolate(curve, time, start.Bottom, end.Bottom, startTime, endTime);
		return this;
	}
}

[System.Serializable]
public class SsColorRef : SsInterpolatable
{
	public byte	R;
	public byte	G;
	public byte	B;
	public byte	A;

	public SsColorRef(SsColorRef r)
	{
		R = r.R;
		G = r.G;
		B = r.B;
		A = r.A;
	}

	static public SsColorRef[] CreateArray(int num)
	{
		var a = new SsColorRef[num];
		for (int i = 0; i < a.Length; ++i)
			a[i] = new SsColorRef();
		return a;
	}

	public SsColorRef()	{}
	
	public SsColorRef Clone()
	{
		return new SsColorRef(this);
	}

	// get normalized color
	static public explicit operator UnityEngine.Color(SsColorRef s)
	{
		var d = new UnityEngine.Color();
		d.r = (float)s.R / 255;
		d.g = (float)s.G / 255;
		d.b = (float)s.B / 255;
		d.a = (float)s.A / 255;
		return d;
	}

	public override string ToString()
	{
		return "R: " + R + ", G: " + G + ", B: " + B + ", A: " + A;
	}

	public SsInterpolatable GetInterpolated(SsCurveParams curve, float time, SsInterpolatable start, SsInterpolatable end, int startTime, int endTime)
	{
		var v = new SsColorRef();
		return v.Interpolate(curve, time, start, end, startTime, endTime);
	}

	public SsInterpolatable Interpolate(SsCurveParams curve, float time, SsInterpolatable start_, SsInterpolatable end_, int startTime, int endTime)
	{
		var start = (SsColorRef)start_;
		var end = (SsColorRef)end_;
		int r = SsInterpolation.Interpolate(curve, time, (int)start.R, (int)end.R, startTime, endTime);
		int g = SsInterpolation.Interpolate(curve, time, (int)start.G, (int)end.G, startTime, endTime);
		int b = SsInterpolation.Interpolate(curve, time, (int)start.B, (int)end.B, startTime, endTime);
		int a = SsInterpolation.Interpolate(curve, time, (int)start.A, (int)end.A, startTime, endTime);
		R = (byte)Mathf.Clamp(r, 0, 255);
		G = (byte)Mathf.Clamp(g, 0, 255);
		B = (byte)Mathf.Clamp(b, 0, 255);
		A = (byte)Mathf.Clamp(a, 0, 255);
		return this;
	}
}

[System.Serializable]
public class SsImageFile
{
	public string			path;
	public Texture2D		texture;
	public Material[]		materials;
	public int				width;
	public int				height;
	public int				bpp;
#if _BUILD_UNIFIED_SHADERS
	public bool				useUnifiedShader;
#endif
	
	public Material		GetMaterial(SsShaderType t)
	{
		if (materials == null) return null;
		int index = SsShaderManager.ToSerial(t);
		if (index >= materials.Length) return null;
		return materials[index];
	}
	
	static public SsImageFile	invalidInstance;

	static SsImageFile()
	{
		invalidInstance = new SsImageFile();
		invalidInstance.path = "INVALID";
		invalidInstance.width = 8;
		invalidInstance.height = 8;
		invalidInstance.bpp = 8;
	}
}
