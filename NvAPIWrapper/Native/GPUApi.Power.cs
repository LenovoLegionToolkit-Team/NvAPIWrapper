using System;
using NvAPIWrapper.Native.Exceptions;
using NvAPIWrapper.Native.General;
using NvAPIWrapper.Native.GPU;
using NvAPIWrapper.Native.GPU.Structures;
using NvAPIWrapper.Native.Helpers;
using NvAPIWrapper.Native.Helpers.Structures;
using NvAPIWrapper.Native.Interfaces.GPU;

namespace NvAPIWrapper.Native
{
    public static partial class GPUApi
    {
        /// <summary>
        ///     [PRIVATE]
        ///     Retrieves the coprocessor / RTD3 power status information for a physical GPU.
        /// </summary>
        /// <param name="physicalGPUHandle">Handle of the physical GPU.</param>
        /// <returns>The coprocessor information structure.</returns>
        public static ICoprocInfo GetCoprocInfo(PhysicalGPUHandle physicalGPUHandle)
        {
            var getCoprocInfo = DelegateFactory.GetDelegate<Delegates.GPU.NvAPI_GPU_GetCoprocInfo>();

            foreach (var acceptType in getCoprocInfo.Accepts())
            {
                var instance = acceptType.Instantiate<ICoprocInfo>();

                using (var coprocInfoRef = ValueTypeReference.FromValueType(instance, acceptType))
                {
                    var status = getCoprocInfo(physicalGPUHandle, coprocInfoRef);

                    if (status == Status.IncompatibleStructureVersion)
                    {
                        continue;
                    }

                    if (status != Status.Ok)
                    {
                        throw new NVIDIAApiException(status);
                    }

                    return coprocInfoRef.ToValueType<ICoprocInfo>(acceptType);
                }
            }

            throw new NVIDIANotSupportedException("GetCoprocInfo is not supported or structure version mismatch.");
        }

        /// <summary>
        ///     [PRIVATE]
        ///     Retrieves GC6 diagnostic and support information for a physical GPU.
        /// </summary>
        /// <param name="physicalGPUHandle">Handle of the physical GPU.</param>
        /// <returns>The GC6 debug info structure.</returns>
        public static GC6DebugInfoV2 GetGC6DebugInfo(PhysicalGPUHandle physicalGPUHandle)
        {
            var getGC6DebugInfo = DelegateFactory.GetDelegate<Delegates.GPU.NvAPI_GPU_GetGC6DebugInfo>();

            foreach (var acceptType in getGC6DebugInfo.Accepts())
            {
                var instance = acceptType.Instantiate<GC6DebugInfoV2>();

                using (var debugInfoRef = ValueTypeReference.FromValueType(instance, acceptType))
                {
                    var status = getGC6DebugInfo(physicalGPUHandle, debugInfoRef);

                    if (status == Status.IncompatibleStructureVersion)
                    {
                        continue;
                    }

                    if (status != Status.Ok)
                    {
                        throw new NVIDIAApiException(status);
                    }

                    return debugInfoRef.ToValueType<GC6DebugInfoV2>(acceptType);
                }
            }

            throw new NVIDIANotSupportedException("GetGC6DebugInfo is not supported or structure version mismatch.");
        }
    }
}
