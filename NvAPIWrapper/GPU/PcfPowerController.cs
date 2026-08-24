using System;
using NvAPIWrapper.Native;
using NvAPIWrapper.Native.Exceptions;

#pragma warning disable CS1591

namespace NvAPIWrapper.GPU
{
    public sealed class PcfPowerController : IDisposable
    {
        private sealed class ControllerLayout(PcfPowerLayout type, uint version, int size, int field2COffset, int field30Offset, int field34Offset, int field38Offset)
        {
            public PcfPowerLayout Type { get; } = type;
            public uint Version { get; } = version;
            public int Size { get; } = size;
            public int Field2COffset { get; } = field2COffset;
            public int Field30Offset { get; } = field30Offset;
            public int Field34Offset { get; } = field34Offset;
            public int Field38Offset { get; } = field38Offset;
        }

        private static readonly ControllerLayout[] KnownLayouts =
        [
            new ControllerLayout(PcfPowerLayout.V1, 0x00010C88, 0xC88, 0x2C, 0x30, 0x34, 0x38),
            new ControllerLayout(PcfPowerLayout.V2, 0x00021640, 0x1640, 0x48, 0x4C, 0x50, 0x54)
        ];

        private readonly object _sync = new();
        private readonly ControllerLayout _layout;
        private readonly byte[] _startupBuffer;
        private bool _disposed;

        public PcfPowerController()
        {
            NVIDIA.Initialize();

            try
            {
                PcfApi.InitializeMode1();

                foreach (var layout in KnownLayouts)
                {
                    try
                    {
                        var buffer = PcfApi.GetControllerBuffer(layout.Version, layout.Size);
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

        public PcfPowerLayout Layout => _layout.Type;

        public uint LayoutVersion => _layout.Version;

        public PcfPowerValues GetPowerValues()
        {
            lock (_sync)
            {
                ThrowIfDisposed();
                return Decode(PcfApi.GetControllerBuffer(_layout.Version, _layout.Size));
            }
        }

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

                var buffer = PcfApi.GetControllerBuffer(_layout.Version, _layout.Size);
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

        public bool GetDynamicBoostEnabled()
        {
            lock (_sync)
            {
                ThrowIfDisposed();
                return PcfApi.GetDynamicBoostStatus();
            }
        }

        public void SetDynamicBoostEnabled(bool enabled)
        {
            lock (_sync)
            {
                ThrowIfDisposed();
                PcfApi.SetDynamicBoostStatus(enabled);
            }
        }

        public void RestoreStartupSnapshot()
        {
            lock (_sync)
            {
                ThrowIfDisposed();
                PcfApi.SetControllerBuffer(_startupBuffer);
            }
        }

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

    public enum PcfPowerLayout
    {
        V1,
        V2
    }

    [Flags]
    public enum PcfPowerFields
    {
        None = 0,
        Field2C = 1,
        Field30 = 2,
        Field34 = 4,
        Field38 = 8,
        All = Field2C | Field30 | Field34 | Field38
    }

    public sealed class PcfPowerValues(uint field2CInMilliwatts, uint field30InMilliwatts, uint field34InMilliwatts, uint field38InMilliwatts)
    {
        public uint Field2CInMilliwatts { get; } = field2CInMilliwatts;
        public uint Field30InMilliwatts { get; } = field30InMilliwatts;
        public uint Field34InMilliwatts { get; } = field34InMilliwatts;
        public uint Field38InMilliwatts { get; } = field38InMilliwatts;
    }
}
