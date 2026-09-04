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
            get => _ClockBoostLocks.Take((int) _ClockBoostLocksCount).ToArray();
        }

        /// <summary>
        ///     Creates a new instance of <see cref="PrivateClockBoostLockV2" />
        /// </summary>
        /// <param name="clockBoostLocks">The list of clock boost locks</param>
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
            _ClockBoostLocksCount = (uint) clockBoostLocks.Length;
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
        ///     Creates a reset configuration to return clock and performance state domains (0, 1, 4, 5) to dynamic management.
        ///     To reset voltage lock (Domain 6), use <see cref="CreateVoltageReset" />.
        /// </summary>
        public static PrivateClockBoostLockV2 CreateDynamicReset()
        {
            return new PrivateClockBoostLockV2(new[]
            {
                ClockBoostLock.CreateDynamicReset(0),
                ClockBoostLock.CreateDynamicReset(1),
                ClockBoostLock.CreateDynamicReset(4),
                ClockBoostLock.CreateDynamicReset(5),
            });
        }

        /// <summary>
        ///     Creates a lock configuration for a target voltage (Domain 6)
        /// </summary>
        /// <param name="voltageInMicroV">The target voltage in uV.</param>
        public static PrivateClockBoostLockV2 CreateVoltageLock(uint voltageInMicroV)
        {
            return new PrivateClockBoostLockV2(new[]
            {
                ClockBoostLock.CreateVoltageLock(voltageInMicroV)
            });
        }

        /// <summary>
        ///     Creates a reset configuration for voltage (Domain 6) to return to dynamic management
        /// </summary>
        public static PrivateClockBoostLockV2 CreateVoltageReset()
        {
            return new PrivateClockBoostLockV2(new[]
            {
                ClockBoostLock.CreateVoltageReset()
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
            ///     Gets the clock domain
            /// </summary>
            public PublicClockDomain ClockDomain
            {
                get => _ClockDomain;
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
            /// <param name="clockDomain">The public clock domain.</param>
            /// <param name="lockMode">The clock lock mode.</param>
            /// <param name="voltageInMicroV">The locked voltage in uV.</param>
            public ClockBoostLock(PublicClockDomain clockDomain, ClockLockMode lockMode, uint voltageInMicroV) : this()
            {
                _ClockDomain = clockDomain;
                _Mode = 0;
                _LockMode = lockMode;
                _Value = 0;
                _VoltageInMicroV = voltageInMicroV;
                _Flag = 0;
            }

            /// <summary>
            ///     Creates a frequency lock entry
            /// </summary>
            /// <param name="domain">The clock domain.</param>
            /// <param name="frequencyInKhz">The target frequency in kHz.</param>
            public static ClockBoostLock CreateFrequencyLock(uint domain, uint frequencyInKhz)
            {
                var lockEntry = new ClockBoostLock();
                lockEntry._ClockDomain = (PublicClockDomain)domain;
                lockEntry._Mode = 0;
                lockEntry._LockMode = (ClockLockMode)2;
                lockEntry._Value = 0;
                lockEntry._VoltageInMicroV = frequencyInKhz;
                lockEntry._Flag = 0;
                return lockEntry;
            }

            /// <summary>
            ///     Creates a performance state lock entry
            /// </summary>
            /// <param name="domain">The clock domain.</param>
            /// <param name="pstateId">The target performance state ID.</param>
            public static ClockBoostLock CreatePStateLock(uint domain, PerformanceStateId pstateId)
            {
                var lockEntry = new ClockBoostLock();
                lockEntry._ClockDomain = (PublicClockDomain)domain;
                lockEntry._Mode = 0;
                lockEntry._LockMode = (ClockLockMode)1;
                lockEntry._Value = 0;
                lockEntry._VoltageInMicroV = (uint)pstateId;
                lockEntry._Flag = 0;
                return lockEntry;
            }

            /// <summary>
            ///     Creates a reset entry
            /// </summary>
            /// <param name="domain">The clock domain.</param>
            public static ClockBoostLock CreateDynamicReset(uint domain)
            {
                var lockEntry = new ClockBoostLock();
                lockEntry._ClockDomain = (PublicClockDomain)domain;
                lockEntry._Mode = 0;
                lockEntry._LockMode = 0;
                lockEntry._Value = 0;
                lockEntry._VoltageInMicroV = 0;
                lockEntry._Flag = 0;
                return lockEntry;
            }

            /// <summary>
            ///     Creates a voltage lock entry (Domain 6, Manual mode)
            /// </summary>
            /// <param name="voltageInMicroV">The target voltage in uV.</param>
            public static ClockBoostLock CreateVoltageLock(uint voltageInMicroV)
            {
                var lockEntry = new ClockBoostLock();
                lockEntry._ClockDomain = PublicClockDomain.Voltage;
                lockEntry._Mode = 0;
                lockEntry._LockMode = ClockLockMode.Manual;
                lockEntry._Value = 0;
                lockEntry._VoltageInMicroV = voltageInMicroV;
                lockEntry._Flag = 0;
                return lockEntry;
            }

            /// <summary>
            ///     Creates a voltage reset entry (Domain 6, Dynamic mode)
            /// </summary>
            public static ClockBoostLock CreateVoltageReset()
            {
                var lockEntry = new ClockBoostLock();
                lockEntry._ClockDomain = PublicClockDomain.Voltage;
                lockEntry._Mode = 0;
                lockEntry._LockMode = ClockLockMode.None;
                lockEntry._Value = 0;
                lockEntry._VoltageInMicroV = 0;
                lockEntry._Flag = 0;
                return lockEntry;
            }
        }
    }
}