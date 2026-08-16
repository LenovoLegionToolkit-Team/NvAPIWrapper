using System.Runtime.InteropServices;
using NvAPIWrapper.Native.Attributes;
using NvAPIWrapper.Native.General.Structures;
using NvAPIWrapper.Native.Helpers;
using NvAPIWrapper.Native.Interfaces;

namespace NvAPIWrapper.Native.GPU.Structures
{
    /// <summary>
    ///     [PRIVATE]
    ///     Holds diagnostic and support information for the GC6 (RTD3) sleep state.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    [StructureVersion(2)]
    public struct GC6DebugInfoV2 : IInitializable
    {
        internal StructureVersion _Version;
        internal readonly uint _Flags;
        internal readonly uint _BlockerFlags;

        /// <summary>
        ///     Gets a value indicating whether the VBIOS supports GC6 power transitions.
        /// </summary>
        public bool HasVbiosSupport => (_Flags & 0x1) != 0 || (_Flags & 0x2) != 0;

        /// <summary>
        ///     Gets a value indicating whether the System BIOS (SBIOS) supports GC6 power transitions.
        /// </summary>
        public bool HasSbiosSupport => (_Flags & 0x4) != 0;

        /// <summary>
        ///     Gets a value indicating whether both VBIOS and SBIOS support GC6.
        /// </summary>
        public bool IsGc6Supported => HasVbiosSupport && HasSbiosSupport;

        /// <summary>
        ///     Gets the raw blocker flags preventing GC6 transition.
        /// </summary>
        public uint BlockerFlags => _BlockerFlags;

        /// <summary>
        ///     Gets the typed blocker reasons preventing GC6 transition.
        /// </summary>
        public GC6BlockerReason BlockerReasons => (GC6BlockerReason)_BlockerFlags;
    }
}
