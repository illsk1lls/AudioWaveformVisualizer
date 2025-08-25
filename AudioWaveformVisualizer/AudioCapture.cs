using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections.Generic;

namespace AudioWaveformVisualizer
{
	public class AudioCapture
	{
		[DllImport("ole32.dll")]
		public static extern void CoTaskMemFree(IntPtr pv);

		private IAudioClient? audioClient;
		private IAudioCaptureClient? captureClient;
		private WAVEFORMATEX waveFormat;
		private Thread? captureThread;
		private bool isCapturing;
		private readonly object lockObject = new object();
		private bool isFloat;
		private ushort bitsPerSample;
		private Queue<double> sampleBuffer = new Queue<double>();
		private const int maxBufferSize = 4096;

		public double[] GetRecentSamples()
		{
			lock (lockObject)
			{
				return sampleBuffer.ToArray();
			}
		}

		public void Initialize(IMMDevice device)
		{
			try
			{
				if (device == null) throw new ArgumentNullException(nameof(device));

				Guid iidIAudioClient = typeof(IAudioClient).GUID;
				device.Activate(ref iidIAudioClient, CLSCTX.CLSCTX_ALL, IntPtr.Zero, out object o);
				audioClient = (IAudioClient)o;

				if (audioClient == null) throw new Exception("Failed to activate audio client");

				audioClient.GetMixFormat(out IntPtr pFormat);
				if (pFormat == IntPtr.Zero) throw new Exception("Failed to get mix format");

				waveFormat = (WAVEFORMATEX)Marshal.PtrToStructure(pFormat, typeof(WAVEFORMATEX))!;

				isFloat = false;
				bitsPerSample = waveFormat.wBitsPerSample;
				if (waveFormat.wFormatTag == 0xFFFE)
				{
					WAVEFORMATEXTENSIBLE extFormat = (WAVEFORMATEXTENSIBLE)Marshal.PtrToStructure(pFormat, typeof(WAVEFORMATEXTENSIBLE))!;
					if (extFormat.SubFormat == new Guid("00000003-0000-0010-8000-00aa00389b71"))
					{
						isFloat = true;
					}
					else if (extFormat.SubFormat == new Guid("00000001-0000-0010-8000-00aa00389b71"))
					{
						isFloat = false;
					}
					else
					{
						CoTaskMemFree(pFormat);
						throw new Exception("Unsupported subformat in WAVEFORMATEXTENSIBLE");
					}
					bitsPerSample = extFormat.Format.wBitsPerSample;
				}
				else if (waveFormat.wFormatTag == 3)
				{
					isFloat = true;
				}
				else if (waveFormat.wFormatTag == 1)
				{
					isFloat = false;
				}
				else
				{
					CoTaskMemFree(pFormat);
					throw new Exception("Unsupported format tag");
				}

				if ((isFloat && bitsPerSample != 32 && bitsPerSample != 64) ||
					(!isFloat && bitsPerSample != 16 && bitsPerSample != 32))
				{
					CoTaskMemFree(pFormat);
					throw new Exception("Unsupported bit depth");
				}

				long bufferDuration = 10000000L / 10;
				Guid emptyGuid = Guid.Empty;
				audioClient.Initialize(AUDCLNT_SHAREMODE.AUDCLNT_SHAREMODE_SHARED, AUDCLNT_STREAMFLAGS.AUDCLNT_STREAMFLAGS_LOOPBACK, bufferDuration, 0, pFormat, ref emptyGuid);
				CoTaskMemFree(pFormat);

				Guid iidIAudioCaptureClient = typeof(IAudioCaptureClient).GUID;
				audioClient.GetService(ref iidIAudioCaptureClient, out object occ);
				captureClient = (IAudioCaptureClient)occ;

				if (captureClient == null) throw new Exception("Failed to get capture client");
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error initializing capture: " + ex.Message);
			}
		}

		public void Start()
		{
			if (audioClient == null) throw new InvalidOperationException("Audio client not initialized");

			audioClient.Start();

			isCapturing = true;
			captureThread = new Thread(CaptureLoop);
			captureThread.Start();
		}

		public void Stop()
		{
			isCapturing = false;
			captureThread?.Join();
			audioClient?.Stop();
		}

		private void CaptureLoop()
		{
			while (isCapturing)
			{
				Thread.Sleep(10);

				if (captureClient == null) continue;

				captureClient.GetBuffer(out IntPtr pData, out uint numFrames, out uint flags, out long pos, out long qpc);

				if (numFrames > 0)
				{
					int byteLength = (int)(numFrames * waveFormat.nBlockAlign);
					byte[] buffer = new byte[byteLength];
					Marshal.Copy(pData, buffer, 0, byteLength);

					int bytesPerSample = bitsPerSample / 8;

					List<double> newFrameAvgs = new List<double>();

					for (uint f = 0; f < numFrames; f++)
					{
						double frameSum = 0.0;
						for (ushort ch = 0; ch < waveFormat.nChannels; ch++)
						{
							int offset = (int)(f * waveFormat.nBlockAlign + ch * bytesPerSample);
							double sample;
							if (isFloat)
							{
								if (bitsPerSample == 32)
								{
									sample = BitConverter.ToSingle(buffer, offset);
								}
								else
								{
									sample = BitConverter.ToDouble(buffer, offset);
								}
							}
							else
							{
								if (bitsPerSample == 16)
								{
									sample = BitConverter.ToInt16(buffer, offset) / 32768.0;
								}
								else
								{
									sample = BitConverter.ToInt32(buffer, offset) / 2147483648.0;
								}
							}
							frameSum += sample;
						}
						double frameAvg = frameSum / waveFormat.nChannels;
						newFrameAvgs.Add(frameAvg);
					}

					lock (lockObject)
					{
						foreach (var avg in newFrameAvgs)
						{
							sampleBuffer.Enqueue(avg);
							if (sampleBuffer.Count > maxBufferSize)
							{
								sampleBuffer.Dequeue();
							}
						}
					}
				}

				captureClient.ReleaseBuffer(numFrames);
			}
		}
	}
}