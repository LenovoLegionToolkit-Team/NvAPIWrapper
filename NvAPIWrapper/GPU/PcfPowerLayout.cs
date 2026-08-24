namespace NvAPIWrapper.GPU
{
    /// <summary>
    ///     Specifies the recognized memory buffer layouts for NVIDIA PCF (Platform Control Framework) controllers.
    /// </summary>
    public enum PcfPowerLayout
    {
        /// <summary>
        ///     Version 1 PCF controller layout (0x00010C88, size 0xC88).
        /// </summary>
        V1,

        /// <summary>
        ///     Version 2 PCF controller layout (0x00021640, size 0x1640).
        /// </summary>
        V2
    }
}
