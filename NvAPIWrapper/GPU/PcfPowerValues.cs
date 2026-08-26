namespace NvAPIWrapper.GPU
{
    /// <summary>
    ///     Represents power limit values decoded from an NVIDIA PCF controller buffer.
    /// </summary>
    public sealed class PcfPowerValues
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PcfPowerValues"/> class.
        /// </summary>
        /// <param name="acTargetTPPLimit">The value of AC Target TPP Limit in milliwatts.</param>
        /// <param name="acDefaultGPULimit">The value of AC Default GPU Limit in milliwatts.</param>
        /// <param name="acMinGPULimit">The value of AC Min GPU Limit in milliwatts.</param>
        /// <param name="acMaxGPULimit">The value of AC Max GPU Limit in milliwatts.</param>
        public PcfPowerValues(uint acTargetTPPLimit, uint acDefaultGPULimit, uint acMinGPULimit, uint acMaxGPULimit)
        {
            ACTargetTPPLimitInMilliwatts = acTargetTPPLimit;
            ACDefaultGPULimitInMilliwatts = acDefaultGPULimit;
            ACMinGPULimitInMilliwatts = acMinGPULimit;
            ACMaxGPULimitInMilliwatts = acMaxGPULimit;
        }

        /// <summary>
        ///     Gets the AC Target TPP Limit value in milliwatts.
        /// </summary>
        public uint ACTargetTPPLimitInMilliwatts { get; }

        /// <summary>
        ///     Gets the AC Default GPU Limit value in milliwatts.
        /// </summary>
        public uint ACDefaultGPULimitInMilliwatts { get; }

        /// <summary>
        ///     Gets the AC Min GPU Limit value in milliwatts.
        /// </summary>
        public uint ACMinGPULimitInMilliwatts { get; }

        /// <summary>
        ///     Gets the AC Max GPU Limit value in milliwatts.
        /// </summary>
        public uint ACMaxGPULimitInMilliwatts { get; }
    }
}
