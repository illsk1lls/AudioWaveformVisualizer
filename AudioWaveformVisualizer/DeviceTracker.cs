using System;
using System.Runtime.InteropServices;

namespace AudioWaveformVisualizer
{
    public class DeviceTracker : IMMNotificationClient
    {
        private IMMDeviceEnumerator? deviceEnumerator;
        private AudioCapture capture;
        private HandlerRoutine? consoleHandler;

        public DeviceTracker(AudioCapture captureInstance)
        {
            capture = captureInstance ?? throw new ArgumentNullException(nameof(captureInstance));
        }

        [DllImport("Kernel32.dll")]
        public static extern bool SetConsoleCtrlHandler(HandlerRoutine? Handler, bool Add);

        public delegate bool HandlerRoutine(CtrlTypes ctrlType);

        public enum CtrlTypes
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        public void Start()
        {
            consoleHandler = new HandlerRoutine(ConsoleCtrlCheck);
            SetConsoleCtrlHandler(consoleHandler, true);

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
                deviceEnumerator = null;
            }
        }

        private bool ConsoleCtrlCheck(CtrlTypes ctrlType)
        {
            switch (ctrlType)
            {
                case CtrlTypes.CTRL_C_EVENT:
                case CtrlTypes.CTRL_BREAK_EVENT:
                case CtrlTypes.CTRL_CLOSE_EVENT:
                case CtrlTypes.CTRL_LOGOFF_EVENT:
                case CtrlTypes.CTRL_SHUTDOWN_EVENT:
                    Stop(true);
                    break;
            }
            return false;
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