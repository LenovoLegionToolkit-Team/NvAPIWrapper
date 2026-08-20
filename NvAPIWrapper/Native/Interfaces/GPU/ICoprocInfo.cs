using NvAPIWrapper.Native.GPU;

namespace NvAPIWrapper.Native.Interfaces.GPU
{
    /// <summary>
    ///     Interface for all CoprocInfo structures holding coprocessor / RTD3 power status information.
    /// </summary>
    public interface ICoprocInfo
    {
        /// <summary>
        ///     Gets the raw coprocessor power state.
        /// </summary>
        CoprocPowerState PowerState { get; }

        /// <summary>
        ///     Gets a value indicating whether the GPU graphics/compute pipeline is actively busy rendering.
        /// </summary>
        bool IsPipelineActive { get; }

        /// <summary>
        ///     Gets a value indicating whether GC6 (RTD3 D3cold) is enabled.
        /// </summary>
        bool IsGc6Enabled { get; }

        /// <summary>
        ///     Gets a value indicating whether RTD3 is supported and enabled.
        /// </summary>
        bool IsRtd3Enabled { get; }

        /// <summary>
        ///     Gets the GC-OFF capability version (0 = None, 1 = GC-OFF 1.0, 2 = GC-OFF 3.0).
        /// </summary>
        uint GcOffVersion { get; }

        /// <summary>
        ///     Gets the reason flags preventing entering GC6.
        /// </summary>
        uint NotEnterGC6Reason { get; }

        /// <summary>
        ///     Gets the reason flags preventing entering GCOFF.
        /// </summary>
        uint NotEnterGCOFFReason { get; }
    }
}
