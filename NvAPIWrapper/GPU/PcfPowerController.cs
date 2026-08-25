using System;
using NvAPIWrapper.Native;
using NvAPIWrapper.Native.Exceptions;

namespace NvAPIWrapper.GPU
{
    /// <summary>
    ///     Provides access to NVIDIA PCF (Platform Control Framework) power management features on supported laptop GPUs.
    /// </summary>
    public sealed class PcfPowerController : IDisposable
    {
        private sealed class ControllerLayout
        {
            public ControllerLayout(PcfPowerLayout type, uint version, int size, int field2COffset, int field30Offset, int field34Offset, int field38Offset)
            {
                Type = type;
                Version = version;
                Size = size;
                Field2COffset = field2COffset;
                Field30Offset = field30Offset;
                Field34Offset = field34Offset;
                Field38Offset = field38Offset;
            }

            public PcfPowerLayout Type { get; }
            public uint Version { get; }
            public int Size { get; }
            public int Field2COffset { get; }
            public int Field30Offset { get; }
            public int Field34Offset { get; }
            public int Field38Offset { get; }
        }

        private static readonly ControllerLayout[] KnownLayouts =
        {
            new ControllerLayout(PcfPowerLayout.V1, 0x00010C88, 0xC88, 0x2C, 0x30, 0x34, 0x38),
            new ControllerLayout(PcfPowerLayout.V2, 0x00021640, 0x1640, 0x48, 0x4C, 0x50, 0x54)
        };

        private readonly object _sync = new object();
        private readonly ControllerLayout _layout;
        private readonly byte[] _startupBuffer;
        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PcfPowerController"/> class, automatically discovering the active Dynamic Boost controller.
        /// </summary>
        /// <exception cref="NVIDIANotSupportedException">The PCF controller does not accept a known buffer layout.</exception>
        /// <exception cref="NVIDIAApiException">An error occurred while communicating with the NVIDIA driver.</exception>
        public PcfPowerController() : this(PcfApi.GetDynamicBoostControllerIndex())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PcfPowerController"/> class targeting the specified controller index.
        /// </summary>
        /// <param name="controllerIndex">The 0-based index of the target PCF controller.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="controllerIndex"/> is greater than or equal to 32.</exception>
        /// <exception cref="NVIDIANotSupportedException">The PCF controller does not accept a known buffer layout.</exception>
        /// <exception cref="NVIDIAApiException">An error occurred while communicating with the NVIDIA driver.</exception>
        public PcfPowerController(uint controllerIndex)
        {
            if (controllerIndex >= 32)
            {
                throw new ArgumentOutOfRangeException(nameof(controllerIndex), "Controller index must be between 0 and 31.");
            }

            ControllerIndex = controllerIndex;
            ControllerMask = 1u << (int)controllerIndex;

            NVIDIA.Initialize();

            try
            {
                PcfApi.InitializeMode1();

                foreach (var layout in KnownLayouts)
                {
                    try
                    {
                        var buffer = PcfApi.GetControllerBuffer(layout.Version, layout.Size, ControllerMask);
                        _layout = layout;
                        _startupBuffer = (byte[])buffer.Clone();
                        return;
                    }
                    catch (NVIDIAApiException) { /* Ignore */ }
                }

                throw new NVIDIANotSupportedException("PCF controller does not accept a known buffer layout.");
            }
            catch
            {
                TryUnload();
                throw;
            }
        }

        /// <summary>
        ///     Gets the 0-based index of the target PCF controller.
        /// </summary>
        public uint ControllerIndex { get; }

        /// <summary>
        ///     Gets the bitmask identifying the target PCF controller.
        /// </summary>
        public uint ControllerMask { get; }

        /// <summary>
        ///     Gets the active PCF power controller buffer layout version type.
        /// </summary>
        public PcfPowerLayout Layout => _layout.Type;

        /// <summary>
        ///     Gets the raw version identifier of the active layout.
        /// </summary>
        public uint LayoutVersion => _layout.Version;

        /// <summary>
        ///     Retrieves the current power limit values from the PCF controller buffer.
        /// </summary>
        /// <returns>A <see cref="PcfPowerValues"/> instance containing the current power limits.</returns>
        /// <exception cref="ObjectDisposedException">The controller instance has been disposed.</exception>
        /// <exception cref="NVIDIAApiException">An error occurred while reading from the NVIDIA driver.</exception>
        public PcfPowerValues GetPowerValues()
        {
            lock (_sync)
            {
                ThrowIfDisposed();
                return Decode(PcfApi.GetControllerBuffer(_layout.Version, _layout.Size, ControllerMask));
            }
        }

        /// <summary>
        ///     Updates specified power limit fields in the PCF controller buffer.
        /// </summary>
        /// <param name="fields">The fields to update.</param>
        /// <param name="values">The power limit values to apply.</param>
        /// <exception cref="ArgumentOutOfRangeException">No fields were specified.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="values"/> is null.</exception>
        /// <exception cref="ObjectDisposedException">The controller instance has been disposed.</exception>
        /// <exception cref="NVIDIAApiException">An error occurred while writing to the NVIDIA driver.</exception>
        public void SetPowerValues(PcfPowerFields fields, PcfPowerValues values)
        {
            if (fields == PcfPowerFields.None)
            {
                throw new ArgumentOutOfRangeException(nameof(fields));
            }

            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            lock (_sync)
            {
                ThrowIfDisposed();

                var buffer = PcfApi.GetControllerBuffer(_layout.Version, _layout.Size, ControllerMask);
                if ((fields & PcfPowerFields.Field2C) != 0)
                {
                    SetUInt32(buffer, _layout.Field2COffset, values.Field2CInMilliwatts);
                }

                if ((fields & PcfPowerFields.Field30) != 0)
                {
                    SetUInt32(buffer, _layout.Field30Offset, values.Field30InMilliwatts);
                }

                if ((fields & PcfPowerFields.Field34) != 0)
                {
                    SetUInt32(buffer, _layout.Field34Offset, values.Field34InMilliwatts);
                }

                if ((fields & PcfPowerFields.Field38) != 0)
                {
                    SetUInt32(buffer, _layout.Field38Offset, values.Field38InMilliwatts);
                }

                PcfApi.SetControllerBuffer(buffer);
            }
        }

        /// <summary>
        ///     Retrieves the current Total Processing Power Target offset from baseline in Watts.
        /// </summary>
        /// <returns>The power target offset in Watts (>= 0).</returns>
        /// <exception cref="ObjectDisposedException">The controller instance has been disposed.</exception>
        /// <exception cref="NVIDIAApiException">An error occurred while reading from the NVIDIA driver.</exception>
        public int GetTargetProcessingPowerOffsetInWatts()
        {
            var values = GetPowerValues();
            if (values.Field2CInMilliwatts == uint.MaxValue || values.Field2CInMilliwatts <= values.Field30InMilliwatts)
            {
                return 0;
            }

            var offsetInMilliwatts = (long)values.Field2CInMilliwatts - values.Field30InMilliwatts;
            return checked((int)(offsetInMilliwatts / 1000));
        }

        /// <summary>
        ///     Updates the Total Processing Power Target offset from baseline in Watts.
        /// </summary>
        /// <param name="offsetInWatts">The offset in Watts to add to the baseline TGP (must be non-negative).</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offsetInWatts"/> is negative.</exception>
        /// <exception cref="ObjectDisposedException">The controller instance has been disposed.</exception>
        /// <exception cref="NVIDIAApiException">An error occurred while writing to the NVIDIA driver.</exception>
        public void SetTargetProcessingPowerOffsetInWatts(int offsetInWatts)
        {
            if (offsetInWatts < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offsetInWatts), "Offset in Watts must be non-negative.");
            }

            if (offsetInWatts == 0)
            {
                ResetTargetProcessingPowerOffset();
                return;
            }

            lock (_sync)
            {
                ThrowIfDisposed();

                var values = Decode(PcfApi.GetControllerBuffer(_layout.Version, _layout.Size, ControllerMask));
                var field2CInMilliwatts = checked((uint)((long)values.Field30InMilliwatts + (long)offsetInWatts * 1000L));
                var updatedValues = new PcfPowerValues(
                    field2CInMilliwatts,
                    values.Field30InMilliwatts,
                    values.Field34InMilliwatts,
                    values.Field38InMilliwatts);

                SetPowerValues(PcfPowerFields.Field2C, updatedValues);
            }
        }

        /// <summary>
        ///     Resets the Total Processing Power Target override, releasing Channel 1 and restoring native dynamic EC thermal scaling.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The controller instance has been disposed.</exception>
        /// <exception cref="NVIDIAApiException">An error occurred while writing to the NVIDIA driver.</exception>
        public void ResetTargetProcessingPowerOffset()
        {
            lock (_sync)
            {
                ThrowIfDisposed();

                var values = Decode(PcfApi.GetControllerBuffer(_layout.Version, _layout.Size, ControllerMask));
                var updatedValues = new PcfPowerValues(
                    uint.MaxValue,
                    values.Field30InMilliwatts,
                    values.Field34InMilliwatts,
                    values.Field38InMilliwatts);

                SetPowerValues(PcfPowerFields.Field2C, updatedValues);
            }
        }

        /// <summary>
        ///     Gets a value indicating whether Dynamic Boost is currently enabled.
        /// </summary>
        /// <returns><c>true</c> if Dynamic Boost is enabled; otherwise, <c>false</c>.</returns>
        /// <exception cref="ObjectDisposedException">The controller instance has been disposed.</exception>
        /// <exception cref="NVIDIAApiException">An error occurred while querying Dynamic Boost status.</exception>
        public bool GetDynamicBoostEnabled()
        {
            lock (_sync)
            {
                ThrowIfDisposed();
                return PcfApi.GetDynamicBoostStatus();
            }
        }

        /// <summary>
        ///     Enables or disables Dynamic Boost.
        /// </summary>
        /// <param name="enabled"><c>true</c> to enable Dynamic Boost; <c>false</c> to disable.</param>
        /// <exception cref="ObjectDisposedException">The controller instance has been disposed.</exception>
        /// <exception cref="NVIDIAApiException">An error occurred while setting Dynamic Boost status.</exception>
        public void SetDynamicBoostEnabled(bool enabled)
        {
            lock (_sync)
            {
                ThrowIfDisposed();
                PcfApi.SetDynamicBoostStatus(enabled);
            }
        }

        /// <summary>
        ///     Restores the initial PCF controller buffer snapshot captured at initialization.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The controller instance has been disposed.</exception>
        /// <exception cref="NVIDIAApiException">An error occurred while restoring the snapshot.</exception>
        public void RestoreStartupSnapshot()
        {
            lock (_sync)
            {
                ThrowIfDisposed();
                PcfApi.SetControllerBuffer(_startupBuffer);
            }
        }

        /// <summary>
        ///     Releases all resources used by the <see cref="PcfPowerController"/> and unloads the PCF session.
        /// </summary>
        public void Dispose()
        {
            lock (_sync)
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;
                TryUnload();
            }
        }

        private PcfPowerValues Decode(byte[] buffer)
        {
            return new PcfPowerValues(
                BitConverter.ToUInt32(buffer, _layout.Field2COffset),
                BitConverter.ToUInt32(buffer, _layout.Field30Offset),
                BitConverter.ToUInt32(buffer, _layout.Field34Offset),
                BitConverter.ToUInt32(buffer, _layout.Field38Offset));
        }

        private static void SetUInt32(byte[] buffer, int offset, uint value)
        {
            Buffer.BlockCopy(BitConverter.GetBytes(value), 0, buffer, offset, sizeof(uint));
        }

        private void TryUnload()
        {
            try
            {
                PcfApi.UnloadMode1();
            }
            catch { /* Ignore */ }

            try
            {
                PcfApi.UnloadNvApi();
            }
            catch { /* Ignore */ }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(PcfPowerController));
            }
        }
    }
}
