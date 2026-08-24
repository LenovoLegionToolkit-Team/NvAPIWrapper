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
        ///     Offset field 0x2C (Field2C / Primary GPU total processing power target).
        /// </summary>
        Field2C = 1,

        /// <summary>
        ///     Offset field 0x30 (Field30 / Baseline GPU power target).
        /// </summary>
        Field30 = 2,

        /// <summary>
        ///     Offset field 0x34 (Field34 / Secondary power limit).
        /// </summary>
        Field34 = 4,

        /// <summary>
        ///     Offset field 0x38 (Field38 / Auxiliary power limit).
        /// </summary>
        Field38 = 8,

        /// <summary>
        ///     All available power fields.
        /// </summary>
        All = Field2C | Field30 | Field34 | Field38
    }
}
