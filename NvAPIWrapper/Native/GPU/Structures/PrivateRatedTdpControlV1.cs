using System.Runtime.InteropServices;
using NvAPIWrapper.Native.Attributes;
using NvAPIWrapper.Native.General.Structures;
using NvAPIWrapper.Native.Helpers;
using NvAPIWrapper.Native.Interfaces;

namespace NvAPIWrapper.Native.GPU.Structures
{
    /// <summary>
    ///     Contains information regarding GPU rated TDP control
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct PrivateRatedTdpControlV1 : IInitializable
    {
        internal StructureVersion _Version;
        internal uint _Flag;
        internal uint _Mode;

        /// <summary>
        ///     Gets the control flag
        /// </summary>
        public uint Flag
        {
            get => _Flag;
        }

        /// <summary>
        ///     Gets the control mode
        /// </summary>
        public uint Mode
        {
            get => _Mode;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="PrivateRatedTdpControlV1" />
        /// </summary>
        /// <param name="flag">The control flag.</param>
        /// <param name="mode">The control mode.</param>
        public PrivateRatedTdpControlV1(uint flag, uint mode)
        {
            this = typeof(PrivateRatedTdpControlV1).Instantiate<PrivateRatedTdpControlV1>();
            _Flag = flag;
            _Mode = mode;
        }

        /// <summary>
        ///     Creates an instance configured to enable rated TDP
        /// </summary>
        public static PrivateRatedTdpControlV1 EnableRatedTdp() => new PrivateRatedTdpControlV1(1, 3);

        /// <summary>
        ///     Creates an instance configured to clear rated TDP
        /// </summary>
        public static PrivateRatedTdpControlV1 ClearRatedTdp() => new PrivateRatedTdpControlV1(1, 0);
    }
}
