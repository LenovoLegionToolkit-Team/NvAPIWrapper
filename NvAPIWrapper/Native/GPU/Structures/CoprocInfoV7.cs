using System.Runtime.InteropServices;
using NvAPIWrapper.Native.Attributes;
using NvAPIWrapper.Native.General.Structures;
using NvAPIWrapper.Native.Helpers;
using NvAPIWrapper.Native.Interfaces;
using NvAPIWrapper.Native.Interfaces.GPU;

namespace NvAPIWrapper.Native.GPU.Structures
{
    /// <summary>
    ///     [PRIVATE]
    ///     Holds coprocessor / RTD3 power status information for the GPU (Version 7 - 96 bytes).
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 96)]
    [StructureVersion(7)]
    public struct CoprocInfoV7 : IInitializable, ICoprocInfo
    {
        [FieldOffset(0)]
        internal StructureVersion _Version;

        [FieldOffset(4)]
        internal readonly uint _Status;

        [FieldOffset(8)]
        internal readonly uint _Flags;

        [FieldOffset(0x24)]
        internal readonly byte _RawPowerState;

        [FieldOffset(0x28)]
        internal readonly uint _NotEnterGC6Reason;

        [FieldOffset(0x2C)]
        internal readonly uint _NotEnterGCOFFReason;

        [FieldOffset(0x30)]
        internal readonly uint _Rtd3Flags;

        [FieldOffset(0x38)]
        internal readonly uint _Gc6Flags;

        [FieldOffset(0x3C)]
        internal readonly uint _GcOffVersion;

        /// <inheritdoc />
        public CoprocPowerState PowerState => _RawPowerState switch
        {
            0 => CoprocPowerState.GcOff,
            1 => IsPipelineActive ? CoprocPowerState.Active : CoprocPowerState.Idle,
            2 => CoprocPowerState.Gc6,
            3 => CoprocPowerState.Idle,
            _ => CoprocPowerState.Unknown
        };

        /// <inheritdoc />
        public bool IsPipelineActive
        {
            get
            {
                var isIdle = ((_Flags >> 29) & 1) != 0 && ((_Flags >> 1) & 1) != 0;
                return !isIdle;
            }
        }

        /// <inheritdoc />
        public bool IsGc6Enabled => (_Gc6Flags & 1) != 0;

        /// <inheritdoc />
        public bool IsRtd3Enabled => (_Rtd3Flags & 1) != 0;

        /// <inheritdoc />
        public uint GcOffVersion => _GcOffVersion;

        /// <inheritdoc />
        public uint NotEnterGC6Reason => _NotEnterGC6Reason;

        /// <inheritdoc />
        public uint NotEnterGCOFFReason => _NotEnterGCOFFReason;
    }
}
