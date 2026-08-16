using System.Runtime.InteropServices;
using NvAPIWrapper.Native.Attributes;
using NvAPIWrapper.Native.General.Structures;
using NvAPIWrapper.Native.Helpers;
using NvAPIWrapper.Native.Interfaces;

namespace NvAPIWrapper.Native.GPU.Structures
{
    /// <summary>
    ///     [PRIVATE]
    ///     Holds coprocessor / RTD3 power status information for the GPU.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 108)]
    [StructureVersion(8)]
    public struct CoprocInfoV8 : IInitializable
    {
        [FieldOffset(0)]
        internal StructureVersion _Version;

        [FieldOffset(4)]
        internal readonly uint _Status;

        [FieldOffset(8)]
        internal readonly uint _Flags;

        [FieldOffset(0x24)]
        internal readonly byte _RawPowerState;

        [FieldOffset(0x30)]
        internal readonly uint _Rtd3Flags;

        [FieldOffset(0x38)]
        internal readonly uint _Gc6Flags;

        [FieldOffset(0x3c)]
        internal readonly uint _GcOffVersion;

        /// <summary>
        ///     Gets the raw coprocessor power state.
        /// </summary>
        public CoprocPowerState PowerState => _RawPowerState switch
        {
            0 => CoprocPowerState.GcOff,
            1 => IsPipelineActive ? CoprocPowerState.Active : CoprocPowerState.Idle,
            2 => CoprocPowerState.Gc6,
            3 => CoprocPowerState.Idle,
            _ => CoprocPowerState.Unknown
        };

        /// <summary>
        ///     Gets a value indicating whether the GPU graphics/compute pipeline is actively busy rendering.
        /// </summary>
        public bool IsPipelineActive
        {
            get
            {
                var isIdle = ((_Flags >> 29) & 1) != 0 && ((_Flags >> 1) & 1) != 0;
                return !isIdle;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether GC6 (RTD3 D3cold) is enabled.
        /// </summary>
        public bool IsGc6Enabled => (_Gc6Flags & 1) != 0;
        
        /// <summary>
        ///     Gets a value indicating whether RTD3 is supported and enabled.
        /// </summary>
        public bool IsRtd3Enabled => (_Rtd3Flags & 1) != 0;

        /// <summary>
        ///     Gets the GC-OFF capability version (0 = None, 1 = GC-OFF 1.0, 2 = GC-OFF 3.0).
        /// </summary>
        public uint GcOffVersion => _GcOffVersion;
    }
}
