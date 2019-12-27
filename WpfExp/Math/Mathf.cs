using System;
using System.Linq;

namespace EnginePart
{
	public static class Mathf
	{
		public const float PI = (float)Math.PI;
		public const float Deg2Rad = PI / 180f;
		public const float Rad2Deg = 180f / PI;

		public static float Sin (this float a)
		{
			return (float)Math.Sin (a * Deg2Rad);
		}
		public static float Cos (this float a)
		{
			return (float)Math.Cos (a * Deg2Rad);
		}
		public static float Sqrt (this float a)
		{
			return (float)Math.Sqrt (a);
		}
		public static float ASin (this float a)
		{
			return (float)Math.Asin (a) * Rad2Deg;
		}
		public static float ACos (this float a)
		{
			return (float)Math.Acos (a) * Rad2Deg;
		}
		public static float Sign (this float a)
		{
			return Math.Sign (a);
		}

		public static float Tan(float v)
		{
			return Sin(v) / Cos(v);
		}

		public static void Swap<T>(this object obj, ref T a, ref T b)
		{
			T temp = a;
			a = b;
			b = temp;
		}

		public static float Min (float a, float b)
		{
			if (a > b) return b;
			return a;
		}
		public static float Min(float a, float b, float c)
		{
			if (a <= b && a <= c) return a;
			if (b <= c && b <= a) return b;
			return c;
		}
		public static float Max(float a, float b)
		{
			if (a < b) return b;
			return a;
		}
		public static float Max(float a, float b, float c)
		{
			if (a >= b && a >= c) return a;
			if (b >= c && b >= a) return b;
			return c;
		}
		public static float Abs (this float a)
		{
			if (a < 0) return -a;
			return a;
		}
		public static Vector3 ToVector (this Color32 color)
		{
			return new Vector3(color.r, color.g, color.b);
		}
		public static Color32 ToColor(this Vector3 vector)
		{
			return new Color32(vector.x, vector.y, vector.z, 255f);
		}
		public static float Determinant (float a1, float b1, float a2, float b2)
		{
			return a1 * b2 - a2 * b1;
		}
		public static float Determinant (Vector2 axis_a, Vector2 axis_b)
		{
			return Determinant(axis_a.x, axis_b.x, axis_a.y, axis_b.y);
		}
	}
}

