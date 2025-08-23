using System;
using System.Runtime.InteropServices;

namespace AudioWaveformVisualizer
{
    public class DeviceTracker : IMMNotificationClient
    {
        private IMMDeviceEnumerator? deviceEnumerator;
        private AudioCapture capture;

        public DeviceTracker(AudioCapture captureInstance)
        {
            capture = captureInstance ?? throw new ArgumentNullException(nameof(captureInstance));
        }

        [DllImport("ole32.dll")]
        public static extern int ReleaseComObject(object o);

        public void Start()
        {
            deviceEnumerator = (IMMDeviceEnumerator)new MMDeviceEnumerator();
            deviceEnumerator.RegisterEndpointNotificationCallback(this);

            deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eConsole, out IMMDevice device);

            capture.Initialize(device!);
            capture.Start();
        }

        public void Stop(bool final = false)
        {
            capture.Stop();

            if (final && deviceEnumerator != null)
            {
                deviceEnumerator.UnregisterEndpointNotificationCallback(this);
                ReleaseComObject(deviceEnumerator);
                deviceEnumerator = null;
            }
        }

        public void OnDeviceStateChanged(string? pwstrDeviceId, uint dwNewState)
        {
        }

        public void OnDeviceAdded(string? pwstrDeviceId)
        {
        }

        public void OnDeviceRemoved(string? pwstrDeviceId)
        {
        }

        public void OnDefaultDeviceChanged(EDataFlow flow, ERole role, string? pwstrDefaultDeviceId)
        {
            if (flow == EDataFlow.eRender && role == ERole.eConsole)
            {
                capture.Stop();
                deviceEnumerator!.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eConsole, out IMMDevice device);
                capture.Initialize(device!);
                capture.Start();
            }
        }

        public void OnPropertyValueChanged(string? pwstrDeviceId, PROPERTYKEY key)
        {
        }
    }
}