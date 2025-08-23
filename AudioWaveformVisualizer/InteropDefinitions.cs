using System;
using System.Runtime.InteropServices;

namespace AudioWaveformVisualizer
{
    public enum EDataFlow
    {
        eRender = 0,
        eCapture = 1,
        eAll = 2
    }

    public enum ERole
    {
        eConsole = 0,
        eMultimedia = 1,
        eCommunications = 2
    }

    public enum AUDCLNT_SHAREMODE
    {
        AUDCLNT_SHAREMODE_SHARED,
        AUDCLNT_SHAREMODE_EXCLUSIVE
    }

    [Flags]
    public enum AUDCLNT_STREAMFLAGS : uint
    {
        AUDCLNT_STREAMFLAGS_LOOPBACK = 0x00020000
    }

    public enum CLSCTX : uint
    {
        CLSCTX_ALL = 0x17
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public struct WAVEFORMATEX
    {
        public ushort wFormatTag;
        public ushort nChannels;
        public uint nSamplesPerSec;
        public uint nAvgBytesPerSec;
        public ushort nBlockAlign;
        public ushort wBitsPerSample;
        public ushort cbSize;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public struct WAVEFORMATEXTENSIBLE
    {
        public WAVEFORMATEX Format;
        public ushort wValidBitsPerSample;
        public uint dwChannelMask;
        public Guid SubFormat;
    }

    [ComImport]
    [Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
    public class MMDeviceEnumerator { }

    [ComImport]
    [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMMDeviceEnumerator
    {
        void EnumAudioEndpoints(EDataFlow dataFlow, uint dwStateMask, out IMMDeviceCollection ppDevices);
        void GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, out IMMDevice ppEndpoint);
        void GetDevice([MarshalAs(UnmanagedType.LPWStr)] string pwstrId, out IMMDevice ppDevice);
        [PreserveSig]
        int RegisterEndpointNotificationCallback(IMMNotificationClient pClient);
        [PreserveSig]
        int UnregisterEndpointNotificationCallback(IMMNotificationClient pClient);
    }

    [ComImport]
    [Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMMDeviceCollection
    {
        void GetCount(out uint pcDevices);
        void Item(uint nDevice, out IMMDevice ppDevice);
    }

    [ComImport]
    [Guid("D666063F-1587-4E43-81F1-B948E807363F")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMMDevice
    {
        void Activate(ref Guid iid, CLSCTX dwClsCtx, IntPtr pActivationParams, [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);
        void OpenPropertyStore(uint stgmAccess, [MarshalAs(UnmanagedType.Interface)] out IPropertyStore ppProperties);
        [PreserveSig]
        int GetId([MarshalAs(UnmanagedType.LPWStr)] out string ppstrId);
    }

    [ComImport]
    [Guid("1CB9AD4C-DBFA-4c32-B178-C2F568A703B2")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAudioClient
    {
        void Initialize(AUDCLNT_SHAREMODE ShareMode, AUDCLNT_STREAMFLAGS StreamFlags, long hnsBufferDuration, long hnsPeriodicity, IntPtr pFormat, ref Guid AudioSessionGuid);
        void GetBufferSize(out uint pNumBufferFrames);
        void GetStreamLatency(out long phnsLatency);
        void GetCurrentPadding(out uint pNumPaddingFrames);
        void IsFormatSupported(AUDCLNT_SHAREMODE ShareMode, IntPtr pFormat, out IntPtr ppClosestMatch);
        void GetMixFormat(out IntPtr ppDeviceFormat);
        void GetDevicePeriod(out long phnsDefaultDevicePeriod, out long phnsMinimumDevicePeriod);
        void Start();
        void Stop();
        void Reset();
        void SetEventHandle(IntPtr eventHandle);
        void GetService(ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppv);
    }

    [ComImport]
    [Guid("C8ADBD64-E71E-48a0-A4DE-185C395CD317")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAudioCaptureClient
    {
        void GetBuffer(out IntPtr ppData, out uint pNumFramesToRead, out uint pdwFlags, out long pu64DevicePosition, out long pu64QPCPosition);
        void ReleaseBuffer(uint numFramesRead);
        void GetNextPacketSize(out uint pNumFramesInNextPacket);
    }

    [ComImport]
    [Guid("7991EEC9-7E89-4D85-8390-6C703CEC60C0")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMMNotificationClient
    {
        void OnDeviceStateChanged([MarshalAs(UnmanagedType.LPWStr)] string? pwstrDeviceId, uint dwNewState);
        void OnDeviceAdded([MarshalAs(UnmanagedType.LPWStr)] string? pwstrDeviceId);
        void OnDeviceRemoved([MarshalAs(UnmanagedType.LPWStr)] string? pwstrDeviceId);
        void OnDefaultDeviceChanged(EDataFlow flow, ERole role, [MarshalAs(UnmanagedType.LPWStr)] string? pwstrDefaultDeviceId);
        void OnPropertyValueChanged([MarshalAs(UnmanagedType.LPWStr)] string? pwstrDeviceId, PROPERTYKEY key);
    }

    [ComImport]
    [Guid("886d8eeb-8cf2-4446-8d02-cdba1dbdcf99")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPropertyStore
    {
        [PreserveSig]
        int GetCount(out uint cProps);
        [PreserveSig]
        int GetAt(uint iProp, out PROPERTYKEY pkey);
        [PreserveSig]
        int GetValue(ref PROPERTYKEY key, out PROPVARIANT pv);
        [PreserveSig]
        int SetValue(ref PROPERTYKEY key, ref PROPVARIANT pv);
        [PreserveSig]
        int Commit();
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PROPERTYKEY
    {
        public Guid fmtid;
        public uint pid;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct PROPVARIANT
    {
        [FieldOffset(0)]
        public ushort vt;
        [FieldOffset(2)]
        public ushort reserved1;
        [FieldOffset(4)]
        public ushort reserved2;
        [FieldOffset(6)]
        public ushort reserved3;
        [FieldOffset(8)]
        public IntPtr pszVal;
    }
}