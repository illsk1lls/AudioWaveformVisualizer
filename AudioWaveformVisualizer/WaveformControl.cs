using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AudioWaveformVisualizer
{
	public class WaveformControl : UserControl
	{
		private System.Windows.Forms.Timer timer;
		public AudioCapture? AudioCapture { get; set; }

		public WaveformControl()
		{
			this.DoubleBuffered = true;
			this.BackColor = Color.Black;

			timer = new System.Windows.Forms.Timer();
			timer.Interval = 60;
			timer.Tick += Timer_Tick;
			timer.Start();
		}

		private void Timer_Tick(object? sender, EventArgs? e)
		{
			this.Invalidate();
		}

		protected override void OnPaint(PaintEventArgs? e)
		{
			if (e == null) return;
			base.OnPaint(e);
			Graphics g = e.Graphics;
			g.SmoothingMode = SmoothingMode.AntiAlias;

			float centerY = this.Height / 2f;

			if (AudioCapture != null)
			{
				double[] samples = AudioCapture.GetRecentSamples();
				if (samples.Length >= 2)
				{
					PointF[] points = new PointF[samples.Length];
					for (int i = 0; i < samples.Length; i++)
					{
						float x = (float)i / (samples.Length - 1) * this.Width;
						float amplitude = (float)(samples[i] * (this.Height / 2.0));
						float y = centerY - amplitude;
						points[i] = new PointF(x, y);
					}

					using (Pen waveformPen = new Pen(Color.FromArgb(255, 0, 191, 255), 1f))
					{
						g.DrawLines(waveformPen, points);
					}
				}
			}
		}

		protected override void OnResize(EventArgs? e)
		{
			if (e == null) return;
			base.OnResize(e);
		}
	}
}