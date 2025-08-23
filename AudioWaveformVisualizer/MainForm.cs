using System;
using System.Drawing;
using System.Windows.Forms;

namespace AudioWaveformVisualizer
{
    public class MainForm : Form
    {
        private WaveformControl? waveformfield;
        private AudioCapture? capture;
        private DeviceTracker? tracker;

        public MainForm()
        {
            this.Text = "Audio Waveform Visualizer";
            this.Size = new Size(600, 400);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            waveformfield = new WaveformControl();
            waveformfield.Dock = DockStyle.Fill;
            this.Controls.Add(waveformfield);

            capture = new AudioCapture();
            tracker = new DeviceTracker(capture);
            tracker.Start();

            if (waveformfield != null)
            {
                waveformfield.AudioCapture = capture;
            }

            this.FormClosing += MainForm_FormClosing;
        }

        private void MainForm_FormClosing(object? sender, FormClosingEventArgs? e)
        {
            tracker?.Stop(true);
        }
    }
}