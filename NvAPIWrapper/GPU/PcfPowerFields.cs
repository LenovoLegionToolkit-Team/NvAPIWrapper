using System;

namespace NvAPIWrapper.GPU
{
    /// <summary>
    ///     Specifies the power target fields within the PCF controller buffer to update.
    /// </summary>
    [Flags]
    public enum PcfPowerFields
    {
        /// <summary>
        ///     No fields selected.
        /// </summary>
        None = 0,

        /// <summary>
        ///     Offset field 0x2C (AC Target TPP Limit).
        /// </summary>
        ACTargetTPPLimit = 1,

        /// <summary>
        ///     Offset field 0x30 (AC Default GPU Limit).
        /// </summary>
        ACDefaultGPULimit = 2,

        /// <summary>
        ///     Offset field 0x34 (AC Min GPU Limit).
        /// </summary>
        ACMinGPULimit = 4,

        /// <summary>
        ///     Offset field 0x38 (AC Max GPU Limit).
        /// </summary>
        ACMaxGPULimit = 8,

        /// <summary>
        ///     All available power fields.
        /// </summary>
        All = ACTargetTPPLimit | ACDefaultGPULimit | ACMinGPULimit | ACMaxGPULimit
    }
}
