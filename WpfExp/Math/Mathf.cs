using System;

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
	}
}

