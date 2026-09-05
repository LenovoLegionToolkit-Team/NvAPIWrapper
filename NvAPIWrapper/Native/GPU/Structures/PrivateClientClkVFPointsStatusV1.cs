using System.Linq;
using System.Runtime.InteropServices;
using NvAPIWrapper.Native.Attributes;
using NvAPIWrapper.Native.General.Structures;
using NvAPIWrapper.Native.Interfaces;

namespace NvAPIWrapper.Native.GPU.Structures
{
    /// <summary>
    ///     Contains information regarding GPU client clock V/F points status
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct PrivateClientClkVFPointsStatusV1 : IInitializable
    {
        internal const int MaxNumberOfPoints = 254;

        internal StructureVersion _Version;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
        internal uint[] _ReservedHeader;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxNumberOfPoints)]
        internal VFPoint[] _Points;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        internal uint[] _ReservedTrailing;

        /// <summary>
        ///     Gets a list of V/F curve points
        /// </summary>
        public VFPoint[] Points
        {
            get => _Points ?? new VFPoint[0];
        }

        /// <summary>
        ///     Contains information regarding a single V/F curve point
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct VFPoint
        {
            internal uint _FrequencyInkHz;
            internal uint _VoltageInMicroV;
            internal uint _Reserved1;
            internal uint _Reserved2;
            internal uint _Reserved3;
            internal uint _Reserved4;
            internal uint _Reserved5;

            /// <summary>
            ///     Gets the frequency in kHz
            /// </summary>
            public uint FrequencyInkHz
            {
                get => _FrequencyInkHz;
            }

            /// <summary>
            ///     Gets the voltage in microvolts
            /// </summary>
            public uint VoltageInMicroV
            {
                get => _VoltageInMicroV;
            }

            /// <summary>
            ///     Gets the voltage in millivolts
            /// </summary>
            public uint VoltageInMilliV
            {
                get => _VoltageInMicroV / 1000;
            }
        }
    }
}
