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
        internal uint _Unknown;
        internal uint _ClockBoostLocksCount;

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
                ClockBoostLock.CreateFrequencyLock(0, frequencyInKhz),
                ClockBoostLock.CreateFrequencyLock(1, frequencyInKhz),
                ClockBoostLock.CreatePStateLock(4, stateId),
                ClockBoostLock.CreatePStateLock(5, stateId),
            });
        }

        /// <summary>
        ///     Creates a reset configuration to return clocks to dynamic management
        /// </summary>
        public static PrivateClockBoostLockV2 CreateDynamicReset()
        {
            return new PrivateClockBoostLockV2(new[]
            {
                ClockBoostLock.CreateResetEntry(0),
                ClockBoostLock.CreateResetEntry(1),
                ClockBoostLock.CreateResetEntry(4),
                ClockBoostLock.CreateResetEntry(5),
            });
        }

        /// <summary>
        ///     Contains information regarding a clock boost lock entry
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct ClockBoostLock
        {
            internal uint _Flag;
            internal uint _Unknown1;
            internal uint _Mode;
            internal ClockLockMode _LockMode;
            internal uint _Value;
            internal uint _VoltageInMicroV;

            /// <summary>
            ///     Gets the entry flag
            /// </summary>
            public uint Flag
            {
                get => _Flag;
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
            ///     Creates a new instance of <see cref="ClockBoostLock" />
            /// </summary>
            /// <param name="flag">The entry flag.</param>
            /// <param name="mode">The lock mode.</param>
            /// <param name="value">The frequency in kHz or performance state ID.</param>
            public ClockBoostLock(uint flag, uint mode, uint value) : this()
            {
                _Flag = flag;
                _Unknown1 = 0;
                _Mode = mode;
                _LockMode = ClockLockMode.None;
                _Value = value;
                _VoltageInMicroV = 0;
            }

            /// <summary>
            ///     Creates a new instance of <see cref="ClockBoostLock" />
            /// </summary>
            /// <param name="clockDomain">The public clock domain.</param>
            /// <param name="lockMode">The clock lock mode.</param>
            /// <param name="voltageInMicroV">The locked voltage in uV.</param>
            public ClockBoostLock(PublicClockDomain clockDomain, ClockLockMode lockMode, uint voltageInMicroV) : this()
            {
                _Flag = (uint)clockDomain;
                _Unknown1 = 0;
                _Mode = 0;
                _LockMode = lockMode;
                _Value = 0;
                _VoltageInMicroV = voltageInMicroV;
            }

            /// <summary>
            ///     Creates a frequency lock entry
            /// </summary>
            /// <param name="flag">The entry flag.</param>
            /// <param name="frequencyInKhz">The target frequency in kHz.</param>
            public static ClockBoostLock CreateFrequencyLock(uint flag, uint frequencyInKhz)
            {
                return new ClockBoostLock(flag, 2u, frequencyInKhz);
            }

            /// <summary>
            ///     Creates a performance state lock entry
            /// </summary>
            /// <param name="flag">The entry flag.</param>
            /// <param name="pstateId">The target performance state ID.</param>
            public static ClockBoostLock CreatePStateLock(uint flag, PerformanceStateId pstateId)
            {
                return new ClockBoostLock(flag, 1u, (uint)pstateId);
            }

            /// <summary>
            ///     Creates a reset entry
            /// </summary>
            /// <param name="flag">The entry flag.</param>
            public static ClockBoostLock CreateResetEntry(uint flag)
            {
                return new ClockBoostLock(flag, 0u, 0u);
            }
        }
    }
}