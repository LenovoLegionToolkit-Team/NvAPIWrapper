using System;
using System.Linq;
using System.Runtime.InteropServices;
using NvAPIWrapper.Native.Attributes;
using NvAPIWrapper.Native.General.Structures;
using NvAPIWrapper.Native.Helpers;
using NvAPIWrapper.Native.Interfaces;

namespace NvAPIWrapper.Native.GPU.Structures
{
    /// <summary>
    ///     Contains information regarding the GPU clock boost locks
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(2)]
    public struct PrivateClockBoostLockV2 : IInitializable
    {
        internal const int MaxNumberOfClocksPerGPU = ClockFrequenciesV1.MaxClocksPerGPU;

        internal StructureVersion _Version;
        internal uint _Unknown1;
        internal uint _ClockBoostLocksCount;
        internal uint _Unknown2;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxNumberOfClocksPerGPU)]
        internal ClockBoostLock[] _ClockBoostLocks;

        /// <summary>
        ///     Gets the list of clock boost locks
        /// </summary>
        public ClockBoostLock[] ClockBoostLocks
        {
            get => _ClockBoostLocks?.Take((int)_ClockBoostLocksCount).ToArray() ?? Array.Empty<ClockBoostLock>();
        }

        /// <summary>
        ///     Creates a new instance of <see cref="PrivateClockBoostLockV2" />
        /// </summary>
        /// <param name="clockBoostLocks">The list of clock boost locks.</param>
        public PrivateClockBoostLockV2(ClockBoostLock[] clockBoostLocks)
        {
            if (clockBoostLocks?.Length > MaxNumberOfClocksPerGPU)
            {
                throw new ArgumentException($"Maximum of {MaxNumberOfClocksPerGPU} clocks are configurable.",
                    nameof(clockBoostLocks));
            }

            if (clockBoostLocks == null || clockBoostLocks.Length == 0)
            {
                throw new ArgumentException("Array is null or empty.", nameof(clockBoostLocks));
            }

            this = typeof(PrivateClockBoostLockV2).Instantiate<PrivateClockBoostLockV2>();
            _ClockBoostLocksCount = (uint)clockBoostLocks.Length;
            Array.Copy(clockBoostLocks, 0, _ClockBoostLocks, 0, clockBoostLocks.Length);
        }

        /// <summary>
        ///     Creates a lock configuration for a target performance state and frequency
        /// </summary>
        public static PrivateClockBoostLockV2 CreatePStateAndFrequencyLock(PerformanceStateId stateId, uint frequencyInKhz)
        {
            return new PrivateClockBoostLockV2(new[]
            {
                ClockBoostLock.CreateFrequencyLock(PublicClockDomain.Graphics, frequencyInKhz, 1),
                ClockBoostLock.CreateFrequencyLock(PublicClockDomain.Graphics, frequencyInKhz, 4),
                ClockBoostLock.CreatePStateLock(PublicClockDomain.Graphics, stateId, 5),
                ClockBoostLock.CreatePStateLock(PublicClockDomain.Graphics, stateId, 1),
            });
        }

        /// <summary>
        ///     Creates a reset configuration to return clocks to dynamic management
        /// </summary>
        public static PrivateClockBoostLockV2 CreateDynamicReset()
        {
            return new PrivateClockBoostLockV2(new[]
            {
                ClockBoostLock.CreateResetEntry(PublicClockDomain.Graphics, 1),
                ClockBoostLock.CreateResetEntry(PublicClockDomain.Graphics, 4),
                ClockBoostLock.CreateResetEntry(PublicClockDomain.Graphics, 5),
                ClockBoostLock.CreateResetEntry(PublicClockDomain.Graphics, 1),
            });
        }

        /// <summary>
        ///     Contains information regarding a clock boost lock entry
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct ClockBoostLock
        {
            internal PublicClockDomain _ClockDomain;
            internal uint _Mode;
            internal ClockLockMode _LockMode;
            internal uint _Value;
            internal uint _VoltageInMicroV;
            internal uint _Flag;

            /// <summary>
            ///     Gets the public clock domain
            /// </summary>
            public PublicClockDomain ClockDomain
            {
                get => _ClockDomain;
            }

            /// <summary>
            ///     Gets the lock mode
            /// </summary>
            public uint Mode
            {
                get => _Mode;
            }

            /// <summary>
            ///     Gets the clock lock mode
            /// </summary>
            public ClockLockMode LockMode
            {
                get => _LockMode;
            }

            /// <summary>
            ///     Gets the value
            /// </summary>
            public uint Value
            {
                get => _Value;
            }

            /// <summary>
            ///     Gets the locked voltage in uV
            /// </summary>
            public uint VoltageInMicroV
            {
                get => _VoltageInMicroV;
            }

            /// <summary>
            ///     Gets the entry flag
            /// </summary>
            public uint Flag
            {
                get => _Flag;
            }

            /// <summary>
            ///     Creates a new instance of <see cref="ClockBoostLock" />
            /// </summary>
            /// <param name="clockDomain">The public clock domain.</param>
            /// <param name="lockMode">The clock lock mode.</param>
            /// <param name="voltageInMicroV">The locked voltage in uV.</param>
            public ClockBoostLock(PublicClockDomain clockDomain, ClockLockMode lockMode, uint voltageInMicroV) : this(
                clockDomain,
                0,
                lockMode,
                0,
                voltageInMicroV,
                0)
            {
            }

            /// <summary>
            ///     Creates a new instance of <see cref="ClockBoostLock" />
            /// </summary>
            public ClockBoostLock(
                PublicClockDomain clockDomain,
                uint mode,
                ClockLockMode lockMode,
                uint value,
                uint voltageInMicroV,
                uint flag) : this()
            {
                _ClockDomain = clockDomain;
                _Mode = mode;
                _LockMode = lockMode;
                _Value = value;
                _VoltageInMicroV = voltageInMicroV;
                _Flag = flag;
            }

            /// <summary>
            ///     Creates a frequency lock entry
            /// </summary>
            public static ClockBoostLock CreateFrequencyLock(PublicClockDomain clockDomain, uint frequencyInKhz, uint flag)
            {
                return new ClockBoostLock(clockDomain, 2, ClockLockMode.None, frequencyInKhz, 0, flag);
            }

            /// <summary>
            ///     Creates a performance state lock entry
            /// </summary>
            public static ClockBoostLock CreatePStateLock(PublicClockDomain clockDomain, PerformanceStateId pstateId, uint flag)
            {
                return new ClockBoostLock(clockDomain, 1, ClockLockMode.None, (uint)pstateId, 0, flag);
            }

            /// <summary>
            ///     Creates a reset entry
            /// </summary>
            public static ClockBoostLock CreateResetEntry(PublicClockDomain clockDomain, uint flag)
            {
                return new ClockBoostLock(clockDomain, 0, ClockLockMode.None, 0, 0, flag);
            }
        }
    }
}