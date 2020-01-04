using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EnginePart;

namespace WpfExp
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		WriteableBitmap display;
		byte[] pixels;
		float[] zBuffer;
		int pixelStride;
		Int32Rect rect;
		Matrix4x4 model = Matrix4x4.CreateWorldMatrix(Vector3.right, Vector3.up, Vector3.forward, new Vector3(0, 0, 0));
		Matrix4x4 view = Matrix4x4.CreateWorldMatrix(Vector3.right, Vector3.up, Vector3.forward, Vector3.forward * 4f);
		Matrix4x4 projection;

		Vector3 pos;
		Vector3 rot;
		Vector3 rotateAround;
		float fieldOfView = 120f;
		

		public MainWindow()
		{
			InitializeComponent();

			int width = (int)this.Width;
			int height = (int)this.Height;
			
			rect = new Int32Rect(0, 0, width, height);

			textBox.Content = $"{rect.Width} {rect.Height}";

			display = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr32, null);
			img.Source = display;

			pixels = new byte[width * height * (display.Format.BitsPerPixel / 8)];

			zBuffer = new float[width * height];

			pixelStride = (display.PixelWidth * display.Format.BitsPerPixel) / 8;

			CompositionTarget.Rendering += CompositionTarget_Rendering;
			KeyDown += MainWindow_KeyDown;
		}

		private void MainWindow_KeyDown(object sender, KeyEventArgs e)
		{
			Vector3 translate = Vector3.zero;
			Vector3 rotate = Vector3.zero;
			Vector3 camRotate = Vector3.zero;
			if (e.Key == Key.NumPad6) translate += Vector3.right;
			if (e.Key == Key.NumPad4) translate += Vector3.left;
			if (e.Key == Key.NumPad8) translate += Vector3.up;
			if (e.Key == Key.NumPad2) translate += Vector3.down;
			if (e.Key == Key.PageUp) translate += Vector3.forward;
			if (e.Key == Key.PageDown) translate += Vector3.back;

			if (e.Key == Key.W) rotate += Vector3.right;
			if (e.Key == Key.S) rotate += Vector3.left;
			if (e.Key == Key.A) rotate += Vector3.down;
			if (e.Key == Key.D) rotate += Vector3.up;

			if (e.Key == Key.Left) camRotate += Vector3.down;
			if (e.Key == Key.Right) camRotate += Vector3.up;
			if (e.Key == Key.Up) camRotate += Vector3.right;
			if (e.Key == Key.Down) camRotate += Vector3.left;

			if (e.Key == Key.Home) fieldOfView--;
			if (e.Key == Key.End) fieldOfView++;

			pos += (Vector3)(view * translate * 0.1f);
			rot += rotate * 3f;
			rotateAround += camRotate * 3f;
		}

		private void CompositionTarget_Rendering(object sender, EventArgs e)
		{
			ClearScreen(new Color32(0,0,0,0));
			ClearzBuffer();

			projection = Matrix4x4.CreateProjectionMatrix(fieldOfView, (float)rect.Width / rect.Height, 0.1f, 1000f);

			Matrix4x4 control = Matrix4x4.CreateRotationMatrix(rot) * Matrix4x4.CreateTranslationMatrix(pos);
			Matrix4x4 view = Matrix4x4.RotateAround(rotateAround, Vector3.zero) * this.view;

			try
			{
				Vector4[] vs = new Vector4[]
			{
				new Vector4(0, 1, 0, 1),
				new Vector4(1, 0, 1, 1),
				new Vector4(-1, 0, 1, 1),
				new Vector4(-1, 0, -1, 1),
				new Vector4(1, 0, -1, 1)
			};
				int[] indices = { 0, 3, 2, 0, 2, 1, 0, 1, 4, 0, 4, 3, 1, 3, 4, 2, 3, 1 };


				for (int i = 0; i < indices.Length; i += 3)
				{

					
					Matrix4x4 mvp = model * control * view;

					Vector4 vector_0 = mvp * vs[indices[i]];
					Vector4 vector_1 = mvp * vs[indices[i + 1]];
					Vector4 vector_2 = mvp * vs[indices[i + 2]];



					Vector3 normal = Vector3.Cross((Vector3)(vector_0 - vector_1), (Vector3)(vector_0 - vector_2)).normalized;
					float light = Vector3.Dot(normal, Vector3.forward);

					if (light > 0)
					{
						light = (light + 1) * 0.5f;
						var t_0 = ViewportToScren(projection * vector_0);
						var t_1 = ViewportToScren(projection * vector_1);
						var t_2 = ViewportToScren(projection * vector_2);

						DrawTriangle(t_0, t_1, t_2, new Color32(255, 255, 255, 255) * light);
					}
				}
				Matrix4x4 vp = view * projection;
				WriteLine(ViewportToScren(Vector3.zero), ViewportToScren(vp * Vector3.right), Color32.red);
				WriteLine(ViewportToScren(Vector3.zero), ViewportToScren(vp * Vector3.up), Color32.green);
				WriteLine(ViewportToScren(Vector3.zero), ViewportToScren(vp * Vector3.forward), Color32.blue);
			}
			catch (Exception ex)
			{
				textBox.Content = ex.ToString();
			}


			ApplyWrite();
		}
		private void ApplyWrite()
		{
			display.WritePixels(rect, pixels, pixelStride, 0);
		}
		private void ClearScreen(Color32 clearColor)
		{
			for (int x = 0; x < rect.Width; x++)
			{
				for (int y = 0; y < rect.Height; y++)
				{
					WritePixel(x, y, clearColor);
				}
			}
			ApplyWrite();
		}
		private void ClearzBuffer()
		{
			for (int i = 0; i < zBuffer.Length; i++)
			{
				zBuffer[i] = float.NegativeInfinity;
			}
		}

		private void WritePixel(float x, float y, Color32 color)
		{
			WritePixel((int)x, (int)y, color);
		}
		private Color32 PickPixel(int x, int y)
		{
			Color32 color = new Color32 (255, 255, 255, 255);
			if (rect.Width > x && rect.Height > y && y >= 0 && x >= 0)
			{
				int pixelOffset = (x + y * rect.Width) * 32 / 8;
				color.b = pixels[pixelOffset];
				color.g = pixels[pixelOffset + 1];
				color.r = pixels[pixelOffset + 2];
				color.a = pixels[pixelOffset + 3];
			}
			return color;
		}
		private void WritePixel(int x, int y, Color32 color, bool add = false)
		{
			y = rect.Height - y;
			if (add)
			{
				color = color + PickPixel(x, y);
			}

			if (rect.Width > x && rect.Height > y && y >= 0 && x >= 0)
			{
				int pixelOffset = (x + y * rect.Width) * 32 / 8;
				pixels[pixelOffset] = color.b;
				pixels[pixelOffset + 1] = color.g;
				pixels[pixelOffset + 2] = color.r;
				pixels[pixelOffset + 3] = color.a;
			}
		}
		private Vector3Int ViewportToScren(Vector3 v)
		{
			Vector3 s = (v + new Vector3(1f, 1f, 0f)) * 0.5f * new Vector3(rect.Width, rect.Height, 1f);
			s.z = v.z;
			return (Vector3Int)s;
		}
		private Vector3Int ViewportToScren(Vector4 v)
		{
			return ViewportToScren((Vector3)v);
		}
		private void DrawTrianglePart (Vector3 a, Vector3 b, int height, int totalHeight, int xStart, int yStart, int ySign, Color32 color)
		{
			int yMin, yMax;

			yMin = 0;
			yMax = height;

			for (int y = yMin; y <= yMax; y++)
			{
				float progress_a = y / (float)totalHeight;
				float progress_b = y / (float)height;

				float x_a = a.x * progress_a;
				float x_b = b.x * progress_b;

				int start = (int)Mathf.Min(x_a, x_b) + xStart;
				int end = (int)Mathf.Max(x_a, x_b) + xStart;

				start = start.Clamp(15, rect.Width - 15);
				end = end.Clamp(15, rect.Width - 15);

				textBox.Content = rect.Width;

				for (int x = start; x <= end; x++)
				{
					WritePixel(x, yStart + y * ySign, color);
				}
			}
		}
		private void DrawTriangle(Vector3 t0, Vector3 t1, Vector3 t2, Color32 color)
		{
			if (t0.y < t1.y) this.Swap(ref t0, ref t1);
			if (t0.y < t2.y) this.Swap(ref t0, ref t2);
			if (t1.y < t2.y) this.Swap(ref t1, ref t2);

			int totalHeight = (int)(t0.y - t2.y);
			int height = (int)(t0.y - t1.y);
			int yStart = (int)t0.y;
			int xStart = (int)t0.x;

			Vector3 a = t2 - t0;
			Vector3 b = t1 - t0;

			DrawTrianglePart(a, b, height, totalHeight, xStart, yStart, -1, color);

			height = (int)(t1.y - t2.y);
			xStart = (int)t2.x;
			yStart = (int)t2.y;
			a = t1 - t2;
			b = t0 - t2;

			DrawTrianglePart(b, a, height, totalHeight, xStart, yStart, 1, color);


			WriteLine(t0, t1, Color32.red);
			WriteLine(t0, t2, Color32.red);
			WriteLine(t2, t1, Color32.red);
		}
		private void WriteLine(Vector3 coord_0, Vector3 coord_1, Color32 color)
		{
			WriteLine((Vector2Int)coord_0, (Vector2Int)coord_1, color);
		}
		private void WriteLine(Vector4 coord_0, Vector4 coord_1, Color32 color)
		{
			WriteLine((Vector2Int)coord_0, (Vector2Int)coord_1, color);
		}
		private void WriteLine(Vector2Int coord_0, Vector2Int coord_1, Color32 color)
		{
			int pixelWidth = rect.Width;
			int pixelHeight = rect.Height;

			int x0 = coord_0.x;
			int y0 = coord_0.y;
			int x1 = coord_1.x;
			int y1 = coord_1.y;

			int dx = Math.Abs(x1 - x0);
			int sx = x0 < x1 ? 1 : -1;

			int dy = Math.Abs(y1 - y0);
			int sy = y0 < y1 ? 1 : -1;

			int err = (dx > dy ? dx : -dy) / 2;
			int e2;

			for (; ; )
			{

				if (!(x0 >= pixelWidth || y0 >= pixelHeight || x0 < 0 || y0 < 0))
					WritePixel(x0, y0, color);

				if (x0 == x1 && y0 == y1)
					break;

				e2 = err;

				if (e2 > -dx)
				{
					err -= dy;
					x0 += sx;
				}

				if (e2 < dy)
				{
					err += dx;
					y0 += sy;
				}
			}
		}
	}
}
