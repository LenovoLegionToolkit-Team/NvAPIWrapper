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
        /// <param name="field2CInMilliwatts">The value of Field2C in milliwatts.</param>
        /// <param name="field30InMilliwatts">The value of Field30 in milliwatts.</param>
        /// <param name="field34InMilliwatts">The value of Field34 in milliwatts.</param>
        /// <param name="field38InMilliwatts">The value of Field38 in milliwatts.</param>
        public PcfPowerValues(uint field2CInMilliwatts, uint field30InMilliwatts, uint field34InMilliwatts, uint field38InMilliwatts)
        {
            Field2CInMilliwatts = field2CInMilliwatts;
            Field30InMilliwatts = field30InMilliwatts;
            Field34InMilliwatts = field34InMilliwatts;
            Field38InMilliwatts = field38InMilliwatts;
        }

        /// <summary>
        ///     Gets the Field2C power limit value in milliwatts (Primary GPU total processing power target).
        /// </summary>
        public uint Field2CInMilliwatts { get; }

        /// <summary>
        ///     Gets the Field30 power limit value in milliwatts (Baseline GPU power target).
        /// </summary>
        public uint Field30InMilliwatts { get; }

        /// <summary>
        ///     Gets the Field34 power limit value in milliwatts (Secondary power limit).
        /// </summary>
        public uint Field34InMilliwatts { get; }

        /// <summary>
        ///     Gets the Field38 power limit value in milliwatts (Auxiliary power limit).
        /// </summary>
        public uint Field38InMilliwatts { get; }
    }
}
