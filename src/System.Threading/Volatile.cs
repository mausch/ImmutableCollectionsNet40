using System;
using System.Runtime;
using System.Runtime.ConstrainedExecution;
using System.Security;
namespace System.Threading
{
    internal static class Volatile
    {
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static short Read(ref short location)
        {
            short result = location;
            Thread.MemoryBarrier();
            return result;
        }

       
        [CLSCompliant(false), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static UIntPtr Read(ref UIntPtr location)
        {
            UIntPtr result = location;
            Thread.MemoryBarrier();
            return result;
        }
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static float Read(ref float location)
        {
            float result = location;
            Thread.MemoryBarrier();
            return result;
        }
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static double Read(ref double location)
        {
            return Interlocked.CompareExchange(ref location, 0.0, 0.0);
        }
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical]
        public static T Read<T>(ref T location) where T : class
        {
            T result = location;
            Thread.MemoryBarrier();
            return result;
        }
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static void Write(ref bool location, bool value)
        {
            Thread.MemoryBarrier();
            location = value;
        }
        [CLSCompliant(false), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static void Write(ref sbyte location, sbyte value)
        {
            Thread.MemoryBarrier();
            location = value;
        }
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static void Write(ref byte location, byte value)
        {
            Thread.MemoryBarrier();
            location = value;
        }
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static void Write(ref short location, short value)
        {
            Thread.MemoryBarrier();
            location = value;
        }
        [CLSCompliant(false), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static void Write(ref ushort location, ushort value)
        {
            Thread.MemoryBarrier();
            location = value;
        }
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static void Write(ref int location, int value)
        {
            Thread.MemoryBarrier();
            location = value;
        }
        [CLSCompliant(false), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static void Write(ref uint location, uint value)
        {
            Thread.MemoryBarrier();
            location = value;
        }
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static void Write(ref long location, long value)
        {
            Interlocked.Exchange(ref location, value);
        }
        //[CLSCompliant(false), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical]
        //public unsafe static void Write(ref ulong location, ulong value)
        //{
        //    fixed (ulong* ptr = &location)
        //    {
        //        Interlocked.Exchange(ref *(long*)ptr, (long)value);
        //    }
        //}
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static void Write(ref IntPtr location, IntPtr value)
        {
            Thread.MemoryBarrier();
            location = value;
        }
        [CLSCompliant(false), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static void Write(ref UIntPtr location, UIntPtr value)
        {
            Thread.MemoryBarrier();
            location = value;
        }
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static void Write(ref float location, float value)
        {
            Thread.MemoryBarrier();
            location = value;
        }
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static void Write(ref double location, double value)
        {
            Interlocked.Exchange(ref location, value);
        }
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical]
        public static void Write<T>(ref T location, T value) where T : class
        {
            Thread.MemoryBarrier();
            location = value;
        }
    }
}
