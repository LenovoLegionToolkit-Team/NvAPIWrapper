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
            public ControllerLayout(PcfPowerLayout type, uint version, int size, int stride, int flagOffset, int acTargetTppLimit, int acDefaultGpuLimit, int acMinGpuLimit, int acMaxGpuLimit)
            {
                Type = type;
                Version = version;
                Size = size;
                Stride = stride;
                FlagOffset = flagOffset;
                ACTargetTPPLimit = acTargetTppLimit;
                ACDefaultGPULimit = acDefaultGpuLimit;
                ACMinGPULimit = acMinGpuLimit;
                ACMaxGPULimit = acMaxGpuLimit;
            }

            public PcfPowerLayout Type { get; }
            public uint Version { get; }
            public int Size { get; }
            public int Stride { get; }
            public int FlagOffset { get; }
            public int ACTargetTPPLimit { get; }
            public int ACDefaultGPULimit { get; }
            public int ACMinGPULimit { get; }
            public int ACMaxGPULimit { get; }
        }

        private static readonly ControllerLayout[] KnownLayouts =
        {
            new ControllerLayout(PcfPowerLayout.V1, 0x00010C88, 0xC88, 100, 0x08, 0x2C, 0x30, 0x34, 0x38),
            new ControllerLayout(PcfPowerLayout.V2, 0x00021640, 0x1640, 176, 0x40, 0x4C, 0x50, 0x54, 0x58)
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
        ///     Updates one power limit field in the PCF controller buffer in milliwatts.
        /// </summary>
        /// <param name="field">The field to update.</param>
        /// <param name="milliwatts">The power limit value to apply in milliwatts.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="field"/> does not specify exactly one field.</exception>
        /// <exception cref="ObjectDisposedException">The controller instance has been disposed.</exception>
        /// <exception cref="NVIDIAApiException">An error occurred while writing to the NVIDIA driver.</exception>
        public void SetPowerField(PcfPowerFields field, uint milliwatts)
        {
            if (field == PcfPowerFields.None)
                throw new ArgumentOutOfRangeException(nameof(field));

            SetPowerValues(field, GetPowerValues().With(field, milliwatts));
        }

        /// <summary>
        ///     Updates all PCF power limit fields in milliwatts.
        /// </summary>
        /// <param name="values">The power limit values to apply in milliwatts.</param>
        /// <exception cref="ArgumentNullException"><paramref name="values"/> is null.</exception>
        /// <exception cref="ObjectDisposedException">The controller instance has been disposed.</exception>
        /// <exception cref="NVIDIAApiException">An error occurred while writing to the NVIDIA driver.</exception>
        public void SetPowerLimits(PcfPowerValues values)
        {
            SetPowerValues(PcfPowerFields.All, values);
        }

        /// <summary>
        ///     Resets one PCF power limit override by writing the driver's release value.
        /// </summary>
        /// <param name="field">Exactly one power limit field to reset.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="field"/> does not specify exactly one field.</exception>
        /// <exception cref="ObjectDisposedException">The controller instance has been disposed.</exception>
        /// <exception cref="NVIDIAApiException">An error occurred while writing to the NVIDIA driver.</exception>
        public void ResetPowerField(PcfPowerFields field)
        {
            lock (_sync)
            {
                ThrowIfDisposed();

                var values = GetPowerValues();
                SetPowerValues(field, values.With(field, uint.MaxValue));
            }
        }

        /// <summary>
        ///     Resets all overrides, releasing Channel 1 and restoring native dynamic EC thermal scaling.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The controller instance has been disposed.</exception>
        /// <exception cref="NVIDIAApiException">An error occurred while writing to the NVIDIA driver.</exception>
        public void ResetAllOverrides()
        {
            lock (_sync)
            {
                ThrowIfDisposed();

                var values = GetPowerValues();
                var updatedValues = new PcfPowerValues(
                    uint.MaxValue,
                    uint.MaxValue,
                    uint.MaxValue,
                    uint.MaxValue);

                SetPowerValues(PcfPowerFields.All, updatedValues);
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

        /// <summary>
        ///     Updates specified power limit fields in the PCF controller buffer.
        /// </summary>
        /// <param name="fields">The fields to update.</param>
        /// <param name="values">The power limit values to apply.</param>
        /// <exception cref="ArgumentOutOfRangeException">No fields were specified.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="values"/> is null.</exception>
        /// <exception cref="ObjectDisposedException">The controller instance has been disposed.</exception>
        /// <exception cref="NVIDIAApiException">An error occurred while writing to the NVIDIA driver.</exception>
        private void SetPowerValues(PcfPowerFields fields, PcfPowerValues values)
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

                var buffer = new byte[_layout.Size];
                SetUInt32(buffer, 0, ControllerMask);
                SetUInt32(buffer, 4, _layout.Version);

                var controllerOffset = (int)(ControllerIndex * _layout.Stride);
                buffer[_layout.FlagOffset + controllerOffset] = 1;

                if ((fields & PcfPowerFields.ACTargetTPPLimit) != 0)
                {
                    SetUInt32(buffer, _layout.ACTargetTPPLimit + controllerOffset, values.ACTargetTPPLimitInMilliwatts);
                }

                if ((fields & PcfPowerFields.ACDefaultGPULimit) != 0)
                {
                    SetUInt32(buffer, _layout.ACDefaultGPULimit + controllerOffset, values.ACDefaultGPULimitInMilliwatts);
                }

                if ((fields & PcfPowerFields.ACMinGPULimit) != 0)
                {
                    SetUInt32(buffer, _layout.ACMinGPULimit + controllerOffset, values.ACMinGPULimitInMilliwatts);
                }

                if ((fields & PcfPowerFields.ACMaxGPULimit) != 0)
                {
                    SetUInt32(buffer, _layout.ACMaxGPULimit + controllerOffset, values.ACMaxGPULimitInMilliwatts);
                }

                PcfApi.SetControllerBuffer(buffer);
            }
        }

        private PcfPowerValues Decode(byte[] buffer)
        {
            var controllerOffset = (int)(ControllerIndex * _layout.Stride);
            return new PcfPowerValues(
                BitConverter.ToUInt32(buffer, _layout.ACTargetTPPLimit + controllerOffset),
                BitConverter.ToUInt32(buffer, _layout.ACDefaultGPULimit + controllerOffset),
                BitConverter.ToUInt32(buffer, _layout.ACMinGPULimit + controllerOffset),
                BitConverter.ToUInt32(buffer, _layout.ACMaxGPULimit + controllerOffset));
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
