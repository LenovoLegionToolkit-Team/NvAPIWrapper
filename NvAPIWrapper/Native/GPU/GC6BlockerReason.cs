using System;

namespace NvAPIWrapper.Native.GPU
{
    /// <summary>
    ///     [PRIVATE]
    ///     Represents reasons and flags preventing the GPU from entering the GC6 (RTD3) sleep state.
    /// </summary>
    [Flags]
    public enum GC6BlockerReason : uint
    {
        /// <summary>
        ///     No sleep blockers active.
        /// </summary>
        None = 0,

        /// <summary>
        ///     Miscellaneous internal driver hold.
        /// </summary>
        Misc = 1 << 0,

        /// <summary>
        ///     Sleep is disabled via software/driver configuration.
        /// </summary>
        SoftwareDisabled = 1 << 1,

        /// <summary>
        ///     Direct display interface (DDI) acquired.
        /// </summary>
        DdiAcquire = 1 << 2,

        /// <summary>
        ///     PCIe bus scan in progress.
        /// </summary>
        BusScan = 1 << 3,

        /// <summary>
        ///     System power event transition pending.
        /// </summary>
        PowerEvent = 1 << 4,

        /// <summary>
        ///     Power state rule restriction.
        /// </summary>
        PsRule = 1 << 5,

        /// <summary>
        ///     ACPI state lock active.
        /// </summary>
        Acpi = 1 << 6,

        /// <summary>
        ///     Device handle held by driver.
        /// </summary>
        Device = 1 << 7,

        /// <summary>
        ///     Outstanding display or memory lock.
        /// </summary>
        OutstandingLock = 1 << 8,

        /// <summary>
        ///     CPU-visible surface buffer mapped to dGPU.
        /// </summary>
        CpuVisibleSurface = 1 << 9,

        /// <summary>
        ///     Self-refresh-less monitor active.
        /// </summary>
        SrlessMonitor = 1 << 10,

        /// <summary>
        ///     NVIDIA High Definition Audio controller active.
        /// </summary>
        AudioMonitor = 1 << 11,

        /// <summary>
        ///     Display swapchain flip in progress.
        /// </summary>
        Flip = 1 << 12,

        /// <summary>
        ///     Command buffer execution pending.
        /// </summary>
        CommandBuffer = 1 << 13,

        /// <summary>
        ///     Deferred work queue active in driver.
        /// </summary>
        DeferredWork = 1 << 14,

        /// <summary>
        ///     Self-refresh inactive.
        /// </summary>
        SrInactive = 1 << 15,

        /// <summary>
        ///     Resource Manager reference held.
        /// </summary>
        RefResourceManager = 1 << 16,

        /// <summary>
        ///     Microsoft Hybrid Graphics (MSHybrid) reference held.
        /// </summary>
        RefMsHybrid = 1 << 17
    }
}
