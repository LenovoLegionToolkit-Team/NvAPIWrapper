namespace NvAPIWrapper.Native.GPU
{
    /// <summary>
    ///     Holds a list of known memory types
    /// </summary>
    public enum GPUMemoryType : uint
    {
        /// <summary>
        ///     Unknown memory type
        /// </summary>
        Unknown = 0,

        /// <summary>
        ///     Synchronous dynamic random-access memory
        /// </summary>
        SDRAM = 1,

        /// <summary>
        ///     Double Data Rate Synchronous Dynamic Random-Access Memory
        /// </summary>
        DDR1 = 2,

        /// <summary>
        ///     Double Data Rate 2 Synchronous Dynamic Random-Access Memory
        /// </summary>
        DDR2 = 3,

        /// <summary>
        ///     Graphics Double Data Rate 2 Synchronous Dynamic Random-Access Memory
        /// </summary>
        GDDR2 = 4,

        /// <summary>
        ///     Graphics Double Data Rate 3 Synchronous Dynamic Random-Access Memory
        /// </summary>
        GDDR3 = 5,

        /// <summary>
        ///     Graphics Double Data Rate 4 Synchronous Dynamic Random-Access Memory
        /// </summary>
        GDDR4 = 6,

        /// <summary>
        ///     Double Data Rate 3 Synchronous Dynamic Random-Access Memory
        /// </summary>
        DDR3 = 7,

        /// <summary>
        ///     Graphics Double Data Rate 5 Synchronous Dynamic Random-Access Memory
        /// </summary>
        GDDR5 = 8,

        /// <summary>
        ///     Lowe Power Double Data Rate 2 Synchronous Dynamic Random-Access Memory
        /// </summary>
        LPDDR2,

        /// <summary>
        ///     Graphics Double Data Rate 5X Synchronous Dynamic Random-Access Memory
        /// </summary>
        GDDR5X = 10,

        /// <summary>
        ///     Graphics Double Data Rate 6 Synchronous Dynamic Random-Access Memory
        /// </summary>
        GDDR6 = 14,

        /// <summary>
        ///     Graphics Double Data Rate 6X Synchronous Dynamic Random-Access Memory
        /// </summary>
        GDDR6X = 15,

        /// <summary>
        ///     Graphics Double Data Rate 7X Synchronous Dynamic Random-Access Memory
        /// </summary>
        GDDR7 = 16,
    }
}