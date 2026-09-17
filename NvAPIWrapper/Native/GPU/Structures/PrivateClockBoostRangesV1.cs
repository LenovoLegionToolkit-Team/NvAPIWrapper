using System.Linq;
using System.Runtime.InteropServices;
using NvAPIWrapper.Native.Attributes;
using NvAPIWrapper.Native.General.Structures;
using NvAPIWrapper.Native.Interfaces;

namespace NvAPIWrapper.Native.GPU.Structures
{
    /// <summary>
    ///     Contains information regarding GPU clock boost ranges
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct PrivateClockBoostRangesV1 : IInitializable
    {
        internal const int MaxNumberOfClocksPerGPU = ClockFrequenciesV1.MaxClocksPerGPU;
        internal const int MaxNumberOfUnknown = 8;

        internal StructureVersion _Version;
        internal uint _ClockDomainMask;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxNumberOfUnknown)]
        internal uint[] _Unknown;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxNumberOfClocksPerGPU)]
        internal ClockBoostRange[] _ClockBoostRanges;

        /// <summary>
        ///     Gets a list of clock boost ranges, one per clock domain present on the GPU
        /// </summary>
        public ClockBoostRange[] ClockBoostRanges
        {
            get
            {
                var present = 0;
                for (var mask = _ClockDomainMask; mask != 0; mask >>= 1)
                {
                    present += (int) (mask & 1);
                }

                return _ClockBoostRanges.Take(present).ToArray();
            }
        }

        /// <summary>
        ///     Contains information regarding a clock boost range
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct ClockBoostRange
        {
            internal uint _Unknown1;
            internal PublicClockDomain _ClockDomain;
            internal uint _Unknown2;
            internal uint _Unknown3;
            internal uint _Unknown4;
            internal uint _Unknown5;
            internal uint _Unknown6;
            internal uint _Unknown7;
            internal uint _Unknown8;
            internal uint _Unknown9;
            internal int _RangeMaximumInkHz;
            internal int _RangeMinimumInkHz;
            internal int _PointIndexRange;
            internal uint _Unknown10;
            internal uint _Unknown11;
            internal uint _Unknown12;
            internal uint _Unknown13;
            internal uint _Unknown14;

            /// <summary>
            ///     Gets the clock domain
            /// </summary>
            public PublicClockDomain ClockDomain
            {
                get => _ClockDomain;
            }

            /// <summary>
            ///     Gets the maximum allowed frequency offset in kHz for this clock domain
            /// </summary>
            public int MaximumInkHz
            {
                get => _RangeMaximumInkHz;
            }

            /// <summary>
            ///     Gets the minimum allowed frequency offset in kHz for this clock domain
            /// </summary>
            public int MinimumInkHz
            {
                get => _RangeMinimumInkHz;
            }

            /// <summary>
            ///     Gets the index of the first V/F point of this clock domain inside the client clock boost table
            /// </summary>
            public byte FirstPointIndex
            {
                get => (byte) (_PointIndexRange & 0xFF);
            }

            /// <summary>
            ///     Gets the index of the last V/F point of this clock domain inside the client clock boost table
            /// </summary>
            public byte LastPointIndex
            {
                get => (byte) ((_PointIndexRange >> 8) & 0xFF);
            }
        }
    }
}