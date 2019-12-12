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
		int pixelStride;
		Int32Rect rect;
		Matrix4x4 model = Matrix4x4.CreateWorldMatrix(Vector3.right * 0.25f, Vector3.up * 0.5f, Vector3.forward * 0.25f, new Vector3(0, 0, 0));
		Matrix4x4 view = Matrix4x4.CreateWorldMatrix(Vector3.right, Vector3.up, Vector3.forward, Vector3.forward * 4f);
		Matrix4x4 projection = Matrix4x4.CreateProjectionMatrix(60f, 0.1f, 1000f);

		Vector3 pos;
		Vector3 rot;
		

		public MainWindow()
		{
			InitializeComponent();

			int width = (int)img.Width;
			int height = (int)img.Height;
			rect = new Int32Rect(0, 0, width, height);

			display = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr32, null);
			img.Source = display;

			pixels = new byte[width * height * (display.Format.BitsPerPixel / 8)];

			pixelStride = (display.PixelWidth * display.Format.BitsPerPixel) / 8;

			CompositionTarget.Rendering += CompositionTarget_Rendering;
			KeyDown += MainWindow_KeyDown;
		}

		private void MainWindow_KeyDown(object sender, KeyEventArgs e)
		{
			Vector3 translate = Vector3.zero;
			Vector3 rotate = Vector3.zero;
			if (e.Key == Key.Right) translate += Vector3.right;
			if (e.Key == Key.Left) translate += Vector3.left;
			if (e.Key == Key.Up) translate += Vector3.up;
			if (e.Key == Key.Down) translate += Vector3.down;
			if (e.Key == Key.PageUp) translate += Vector3.forward;
			if (e.Key == Key.PageDown) translate += Vector3.back;

			if (e.Key == Key.W) rotate += Vector3.right;
			if (e.Key == Key.S) rotate += Vector3.left;
			if (e.Key == Key.A) rotate += Vector3.down;
			if (e.Key == Key.D) rotate += Vector3.up;

			pos += translate * 0.1f;
			rot += rotate * 3f;
		}

		private void CompositionTarget_Rendering(object sender, EventArgs e)
		{
			ClearScreen(new Color32(0,0,0,0));

			Vector4[] vs = new Vector4[]
			{
				new Vector4(0, 1, 0, 1),
				new Vector4(1, 0, 1, 1),
				new Vector4(-1, 0, 1, 1),
				new Vector4(-1, 0, -1, 1),
				new Vector4(1, 0, -1, 1)
			};
			IEnumerable<int> indices = new int[] { 2, 0, 1, 0, 4, 0, 3, 0, 2, 1, 4, 3, 2 };
			IEnumerator<int> en = indices.GetEnumerator();
			en.MoveNext();
			int last = en.Current;

			while (en.MoveNext())
			{
				Matrix4x4 control = Matrix4x4.CreateRotationMatrix(rot) * Matrix4x4.CreateTranslationMatrix(pos);
				Matrix4x4 mvp = model * control * view * projection;

				Vector4 lastVert = vs[last];
				Vector4 currentVert = vs[en.Current];

				Vector4 v = mvp * currentVert;
				Vector4 v_last = mvp * lastVert;

				var vint = ViewportToScren(v);
				var vint_last = ViewportToScren(v_last);
				WriteLine(vint, vint_last, new Color32(0, 255, 0, 255));

				last = en.Current;

				textBox.Content = $"model : \n{(model * control).ToString()}\n\nview : \n{view}\n\nmodel * view :\n{model * control * view}\n\nvert : {currentVert}\n\nmodel * vert : \n{(model * control * currentVert).ToString()}\n\nmv * vert : \n{(model * control * view * currentVert).ToString()}\n\n{vint}";
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
		private void WritePixel(int x, int y, Color32 color)
		{
			if (rect.Width > x && rect.Height > y && y >= 0 && x >= 0)
			{
				int pixelOffset = (x + y * rect.Width) * 32 / 8;
				pixels[pixelOffset] = color.b;
				pixels[pixelOffset + 1] = color.g;
				pixels[pixelOffset + 2] = color.r;
				pixels[pixelOffset + 3] = color.a;
			}
		}
		private Vector2Int ViewportToScren(Vector3 v)
		{
			v.y = -v.y;
			Vector3 s = (v + new Vector3(1f, 1f, 0f)) * 0.5f * new Vector3(rect.Width, rect.Height, 1f);
			return new Vector2Int((int)s.x, (int)s.y);
		}
		private Vector2Int ViewportToScren(Vector4 v)
		{
			return ViewportToScren((Vector3)v);
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
