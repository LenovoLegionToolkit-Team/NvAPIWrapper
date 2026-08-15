using System.Runtime.InteropServices;
using NvAPIWrapper.Native.Attributes;
using NvAPIWrapper.Native.General.Structures;
using NvAPIWrapper.Native.Helpers;
using NvAPIWrapper.Native.Interfaces;

namespace NvAPIWrapper.Native.GPU.Structures
{
    /// <summary>
    ///     Contains information regarding GPU P-State limit
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct PrivatePStateLimitV1 : IInitializable
    {
        internal StructureVersion _Version;
        internal uint _PStateLimit;
        internal uint _Reserved1;
        internal uint _Reserved2;
        internal uint _Reserved3;
        internal uint _Reserved4;
        internal uint _Reserved5;
        internal uint _Reserved6;
        internal uint _Reserved7;
        internal uint _Reserved8;

        /// <summary>
        ///     Gets the P-State limit index
        /// </summary>
        public uint PStateLimit
        {
            get => _PStateLimit;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="PrivatePStateLimitV1" />
        /// </summary>
        /// <param name="pstateLimit">The P-State limit.</param>
        public PrivatePStateLimitV1(uint pstateLimit)
        {
            this = typeof(PrivatePStateLimitV1).Instantiate<PrivatePStateLimitV1>();
            _PStateLimit = pstateLimit;
            _Reserved1 = 0;
            _Reserved2 = 0;
            _Reserved3 = 0;
            _Reserved4 = 0;
            _Reserved5 = 0;
            _Reserved6 = 0;
            _Reserved7 = 0;
            _Reserved8 = 0;
        }

        /// <summary>
        ///     Creates an instance configured to unlock higher P-States
        /// </summary>
        public static PrivatePStateLimitV1 UnlockHigherPStates() => new PrivatePStateLimitV1(0xFF);
    }
}
