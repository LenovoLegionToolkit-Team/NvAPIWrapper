using System;
using System.Runtime.InteropServices;
using NvAPIWrapper.Native.Exceptions;
using NvAPIWrapper.Native.General;
using NvAPIWrapper.Native.Helpers;

namespace NvAPIWrapper.Native
{
    /// <summary>
    ///     Contains native Platform Control Framework (PCF) functions used by laptop GPU drivers.
    /// </summary>
    public static class PcfApi
    {
        private const int ExtraBufferBytes = 256;
        private const uint MasterInfoVersion = 0x00033208;
        private const int MasterInfoSize = 0x3208;
        private const int MasterInfoMaxControllers = 32;
        private const int MasterInfoControllerEntrySize = 400;
        private const int MasterInfoHeaderSize = 8;
        private const byte ControllerTypeSbios = 1;
        private const byte ControllerTypeDynamicBoost = 5;

        /// <summary>
        ///     Initializes PCF Mode 1 session.
        /// </summary>
        /// <exception cref="NVIDIAApiException">An error occurred while initializing PCF Mode 1.</exception>
        public static void InitializeMode1()
        {
            EnsureOk(DelegateFactory.GetDelegate<Delegates.Pcf.NvAPI_PCF_Mode1Initialize>()(1));
        }

        /// <summary>
        ///     Unloads PCF Mode 1 session.
        /// </summary>
        /// <exception cref="NVIDIAApiException">An error occurred while unloading PCF Mode 1.</exception>
        public static void UnloadMode1()
        {
            EnsureOk(DelegateFactory.GetDelegate<Delegates.Pcf.NvAPI_PCF_Mode1Unload>()(1));
        }

        /// <summary>
        ///     Unloads PCF NVAPI session globally.
        /// </summary>
        /// <exception cref="NVIDIAApiException">An error occurred while unloading PCF globally.</exception>
        public static void UnloadNvApi()
        {
            EnsureOk(DelegateFactory.GetDelegate<Delegates.Pcf.NvAPI_PCF_GlobalUnload>()());
        }

        /// <summary>
        ///     Retrieves the PCF master info buffer containing platform controller topology.
        /// </summary>
        /// <param name="version">The master info version identifier (default is <c>0x00033208</c>).</param>
        /// <param name="size">The expected size of the master info buffer in bytes (default is <c>0x3208</c> / 12,808 bytes).</param>
        /// <returns>A byte array containing the master info table.</returns>
        /// <exception cref="NVIDIAApiException">An error occurred while reading the master info table.</exception>
        public static byte[] GetMasterInfoBuffer(uint version = MasterInfoVersion, int size = MasterInfoSize)
        {
            var buffer = new byte[size];
            var pointer = Marshal.AllocHGlobal(size + ExtraBufferBytes);

            try
            {
                Zero(pointer, size + ExtraBufferBytes);
                Marshal.WriteInt32(pointer, 4, unchecked((int)version));

                EnsureOk(DelegateFactory.GetDelegate<Delegates.Pcf.NvAPI_PCF_GetMasterInfo>()(pointer));

                Marshal.Copy(pointer, buffer, 0, buffer.Length);
                return buffer;
            }
            finally
            {
                Marshal.FreeHGlobal(pointer);
            }
        }

        /// <summary>
        ///     Attempts to discover the primary Dynamic Boost (QBoost) controller index from the PCF master info table.
        /// </summary>
        /// <returns>The 0-based controller index, or <c>0</c> if master info is unavailable or unsupported.</returns>
        public static uint GetDynamicBoostControllerIndex()
        {
            try
            {
                var buffer = GetMasterInfoBuffer();

                int? sbiosIndex = null;

                for (var i = 0; i < MasterInfoMaxControllers; i++)
                {
                    var offset = MasterInfoHeaderSize + (i * MasterInfoControllerEntrySize);
                    if (offset >= buffer.Length)
                    {
                        break;
                    }

                    var controllerType = buffer[offset];
                    if (controllerType == ControllerTypeDynamicBoost)
                    {
                        return (uint)i;
                    }

                    if (controllerType == ControllerTypeSbios && !sbiosIndex.HasValue)
                    {
                        sbiosIndex = i;
                    }
                }

                if (sbiosIndex.HasValue)
                {
                    return (uint)sbiosIndex.Value;
                }
            }
            catch
            {
            }

            return 0;
        }

        /// <summary>
        ///     Retrieves the PCF controller buffer for the specified layout version, size, and controller mask.
        /// </summary>
        /// <param name="version">The layout version identifier.</param>
        /// <param name="size">The expected size of the buffer in bytes.</param>
        /// <param name="controllerMask">The bitmask identifying the target controller (e.g. <c>1 &lt;&lt; controllerIndex</c>).</param>
        /// <returns>A byte array containing the controller buffer data.</returns>
        /// <exception cref="NVIDIAApiException">An error occurred while reading the controller buffer.</exception>
        public static byte[] GetControllerBuffer(uint version, int size, uint controllerMask = 1)
        {
            var buffer = new byte[size];
            var pointer = Marshal.AllocHGlobal(size + ExtraBufferBytes);

            try
            {
                Zero(pointer, size + ExtraBufferBytes);
                Marshal.WriteInt32(pointer, 0, unchecked((int)controllerMask));
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

        /// <summary>
        ///     Writes the PCF controller buffer back to the driver.
        /// </summary>
        /// <param name="buffer">The buffer data to write.</param>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
        /// <exception cref="NVIDIAApiException">An error occurred while writing the controller buffer.</exception>
        public static void SetControllerBuffer(byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

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

        /// <summary>
        ///     Gets the current Dynamic Boost enabled status.
        /// </summary>
        /// <returns><c>true</c> if Dynamic Boost is enabled; otherwise, <c>false</c>.</returns>
        /// <exception cref="NVIDIAApiException">An error occurred while retrieving Dynamic Boost status.</exception>
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

        /// <summary>
        ///     Sets the Dynamic Boost status.
        /// </summary>
        /// <param name="enabled"><c>true</c> to enable Dynamic Boost; otherwise, <c>false</c>.</param>
        /// <exception cref="NVIDIAApiException">An error occurred while setting Dynamic Boost status.</exception>
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
