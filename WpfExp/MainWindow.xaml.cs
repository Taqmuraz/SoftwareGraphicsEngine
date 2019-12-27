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
		Matrix4x4 model = Matrix4x4.CreateWorldMatrix(Vector3.right * 0.25f, Vector3.up * 0.5f, Vector3.forward * 0.25f, new Vector3(0, 0, 0));
		Matrix4x4 view = Matrix4x4.CreateWorldMatrix(Vector3.right, Vector3.up, Vector3.forward, Vector3.forward * 4f);
		Matrix4x4 projection = Matrix4x4.CreateProjectionMatrix(60f, 0.1f, 1000f);

		Vector3 pos;
		Vector3 rot;
		Vector3 rotateAround;
		

		public MainWindow()
		{
			InitializeComponent();

			int width = (int)img.Width;
			int height = (int)img.Height;
			rect = new Int32Rect(0, 0, width, height);

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

			pos += translate * 0.1f;
			rot += rotate * 3f;
			rotateAround += camRotate;
		}

		private void CompositionTarget_Rendering(object sender, EventArgs e)
		{
			ClearScreen(new Color32(0,0,0,0));
			ClearzBuffer();

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
				int[] indices = new int[] { 0, 3, 2, 0, 2, 1, 0, 1, 4, 0, 4, 3, 1, 3, 4, 2, 3, 1 };

				
				for (int i = 0; i < indices.Length; i += 3)
				{

					Matrix4x4 control = Matrix4x4.CreateRotationMatrix(rot) * Matrix4x4.CreateTranslationMatrix(pos);
					Matrix4x4 mvp = model * control * view * projection;

					Vector4 vector_0 = mvp * vs[indices[i]];
					Vector4 vector_1 = mvp * vs[indices[i + 1]];
					Vector4 vector_2 = mvp * vs[indices[i + 2]];

					Vector3 normal = Vector3.Cross((Vector3)(vector_0 - vector_1), (Vector3)(vector_0 - vector_2)).normalized;
					float light = Vector3.Dot(normal, Vector3.forward);

					if (light > 0)
					{
						var t_0 = ViewportToScren(vector_0);
						var t_1 = ViewportToScren(vector_1);
						var t_2 = ViewportToScren(vector_2);

						DrawTriangle(t_0, t_1, t_2, new Color32(255, 255, 255, 255) * light);
					}

					//textBox.Content = $"model : \n{(model * control).ToString()}\n\nview : \n{view}\n\nmodel * view :\n{model * control * view}\n\nvert : {currentVert}\n\nmodel * vert : \n{(model * control * currentVert).ToString()}\n\nmv * vert : \n{(model * control * view * currentVert).ToString()}\n\n{vint}";
				}
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
			v.y = -v.y;
			Vector3 s = (v + new Vector3(1f, 1f, 0f)) * 0.5f * new Vector3(rect.Width, rect.Height, 1f);
			s.z = v.z;
			return (Vector3Int)s;
		}
		private Vector3Int ViewportToScren(Vector4 v)
		{
			return ViewportToScren((Vector3)v);
		}
		private bool TriangleCheck (Vector2 a, Vector2 b, Vector2 c, Vector2 p)
		{
			float q1 = Triangle_Q_Check(a, b, p);
			float q2 = Triangle_Q_Check(b, c, p);
			float q3 = Triangle_Q_Check(c, a, p);

			return ((q1 >= 0) && (q2 >= 0) && (q3 >= 0)) || ((q1 < 0) && (q2 < 0) && (q3 < 0));
		}
		private float Triangle_Q_Check (Vector2 a, Vector2 b, Vector2 at)
		{
			return at.x * (b.y - a.y) + at.y * (a.x - b.x) + a.y * b.x - a.x * b.y;
		}
		private void DrawTriangle(Vector3 t0, Vector3 t1, Vector3 t2, Color32 color)
		{
			Vector3[] vs = new Vector3[3] { t0, t1, t2 };
			vs = vs.OrderByDescending(t => t.y).ToArray();

			t0 = vs[0];
			t1 = vs[1];
			t2 = vs[2];
			if (t1.x > t2.x) this.Swap(ref t1, ref t2);

			float minX = Mathf.Min(t0.x, t1.x, t2.x);
			float maxX = Mathf.Max(t0.x, t1.x, t2.x);
			float minY = Mathf.Min(t0.y, t1.y, t2.y);
			float maxY = Mathf.Max(t0.y, t1.y, t2.y);

			Int32Rect bounds = new Int32Rect((int)minX, (int)minY, (int)(maxX - minX), (int)(maxY - minY));


			for (int x = 0; x <= bounds.Width; x++)
			{
				for (int y = 0; y <= bounds.Height; y++)
				{
					int screenX = bounds.X + x;
					int screenY = bounds.Y + y;
					Vector2 point = new Vector2(screenX, screenY);

					if (TriangleCheck((Vector2)t0, (Vector2)t1, (Vector2)t2, point))
					{
						WritePixel (screenX, screenY, color);
					}
				}
			}

			WriteLine(t0, t1, new Color32(255, 0, 0, 255));
			WriteLine(t1, t2, new Color32(0, 255, 0, 255));
			WriteLine(t0, t2, new Color32(0, 0, 255, 255));

			Vector2 test_a = Vector2.one.normalized;
			Vector2 test_b = new Vector2(-1, 1).normalized;
			float d_test = 1f / Mathf.Determinant(test_a, test_b);
			Vector2 t = new Vector2 (7.07f, 7.07f);
			Vector2 test = new Vector2 (Mathf.Determinant(t, test_b) * d_test, Mathf.Determinant(test_a, t) * d_test);
			textBox.Content = $"a = {test_a}\nb = {test_b}\nt = {t}\ntest = {test}";
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
