namespace NvAPIWrapper.Native.GPU
{
    /// <summary>
    ///     Represents the coprocessor / RTD3 power state of the GPU.
    /// </summary>
    public enum CoprocPowerState : byte
    {
        /// <summary>
        ///     Unknown power state.
        /// </summary>
        Unknown = 255,

        /// <summary>
        ///     GPU is fully powered off / ACPI power-gated (GCOFF).
        /// </summary>
        GcOff = 0,

        /// <summary>
        ///     GPU is in D0 and actively rendering / executing workloads.
        /// </summary>
        Active = 1,

        /// <summary>
        ///     GPU is in RTD3 / D3cold deep power-gated sleep (GC6).
        /// </summary>
        Gc6 = 2,

        /// <summary>
        ///     GPU is powered on in D0, but currently idle / inactive.
        /// </summary>
        Idle = 3
    }
}
