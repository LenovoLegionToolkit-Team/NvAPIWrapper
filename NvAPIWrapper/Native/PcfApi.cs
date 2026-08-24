using System;
using System.Runtime.InteropServices;
using NvAPIWrapper.Native.Exceptions;
using NvAPIWrapper.Native.General;
using NvAPIWrapper.Native.Helpers;

namespace NvAPIWrapper.Native
{
    internal static class PcfApi
    {
        private const int ExtraBufferBytes = 256;

        public static void InitializeMode1()
        {
            EnsureOk(DelegateFactory.GetDelegate<Delegates.Pcf.NvAPI_PCF_Mode1Initialize>()(1));
        }

        public static void UnloadMode1()
        {
            EnsureOk(DelegateFactory.GetDelegate<Delegates.Pcf.NvAPI_PCF_Mode1Unload>()(1));
        }

        public static void UnloadNvApi()
        {
            EnsureOk(DelegateFactory.GetDelegate<Delegates.Pcf.NvAPI_PCF_GlobalUnload>()());
        }

        public static byte[] GetControllerBuffer(uint version, int size)
        {
            var buffer = new byte[size];
            var pointer = Marshal.AllocHGlobal(size + ExtraBufferBytes);

            try
            {
                Zero(pointer, size + ExtraBufferBytes);
                Marshal.WriteInt32(pointer, 0, 1);
                Marshal.WriteInt32(pointer, 4, unchecked((int)version));

                EnsureOk(DelegateFactory.GetDelegate<Delegates.Pcf.NvAPI_PCF_ControllerGetControl>()(pointer));

                Marshal.Copy(pointer, buffer, 0, buffer.Length);
                return buffer;
            }
            finally
            {
                Marshal.FreeHGlobal(pointer);
            }
        }

        public static void SetControllerBuffer(byte[] buffer)
        {
            var pointer = Marshal.AllocHGlobal(buffer.Length + ExtraBufferBytes);

            try
            {
                Zero(pointer, buffer.Length + ExtraBufferBytes);
                Marshal.Copy(buffer, 0, pointer, buffer.Length);
                EnsureOk(DelegateFactory.GetDelegate<Delegates.Pcf.NvAPI_PCF_ControllerSetControl>()(pointer));
            }
            finally
            {
                Marshal.FreeHGlobal(pointer);
            }
        }

        public static bool GetDynamicBoostStatus()
        {
            var pointer = Marshal.AllocHGlobal(16);

            try
            {
                Zero(pointer, 16);
                EnsureOk(DelegateFactory.GetDelegate<Delegates.Pcf.NvAPI_PCF_DynamicBoostGetStatus>()(pointer));
                return Marshal.ReadByte(pointer) != 0;
            }
            finally
            {
                Marshal.FreeHGlobal(pointer);
            }
        }

        public static void SetDynamicBoostStatus(bool enabled)
        {
            EnsureOk(DelegateFactory.GetDelegate<Delegates.Pcf.NvAPI_PCF_DynamicBoostSetStatus>()((byte)(enabled ? 1 : 0)));
        }

        private static void Zero(IntPtr pointer, int length)
        {
            Marshal.Copy(new byte[length], 0, pointer, length);
        }

        private static void EnsureOk(Status status)
        {
            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }
    }
}
