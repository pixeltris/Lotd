using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Lotd
{
    public partial class MemTools
    {
        public double TimeMultiplier { get; private set; }
        public bool TimeMultiplierEnabled { get; private set; }
        private bool timeMultiplierHooksInitialized;

        public void SetTimeMultiplier(double multiplier, bool enable)
        {
            if (!UpdateState() || nativeScriptGlobalsAddress == IntPtr.Zero)
            {
                return;
            }

            // Suspend the process
            List<IntPtr> threadHandles = BeginSuspendProcess();

            NativeScript.Globals globals = ReadValue<NativeScript.Globals>(nativeScriptGlobalsAddress);

            bool firstRun = !timeMultiplierHooksInitialized;

            if (!timeMultiplierHooksInitialized)
            {
                if (globals.QueryPerformanceCounterFuncAddr != IntPtr.Zero && queryPerformanceCounterAddress != IntPtr.Zero)
                {
                    WriteValue(queryPerformanceCounterAddress, GetNativeScriptFunctionAddress("QueryPerformanceCounter_hook"));
                }
                if (globals.GetTickCount64FuncAddr != IntPtr.Zero && getTickCount64Address != IntPtr.Zero)
                {
                    WriteValue(getTickCount64Address, GetNativeScriptFunctionAddress("GetTickCount64_hook"));
                }
                if (globals.GetTickCountFuncAddr != IntPtr.Zero && getTickCountAddress != IntPtr.Zero)
                {
                    WriteValue(getTickCountAddress, GetNativeScriptFunctionAddress("GetTickCount_hook"));
                }
                if (globals.TimeGetTimeFuncAddr != IntPtr.Zero && timeGetTimeAddress != IntPtr.Zero)
                {
                    WriteValue(timeGetTimeAddress, GetNativeScriptFunctionAddress("timeGetTime_hook"));
                }
                timeMultiplierHooksInitialized = true;
            }

            double oldMultiplier = firstRun ? 1 : globals.TimeMultiplier.Multiplier;

            NativeScript.TimeMultiplierInfo timeMultiplier = globals.TimeMultiplier;
            timeMultiplier.Multiplier = enable ? multiplier : 1;
            timeMultiplier.Enabled = enable;

            // Rebase the initial times so that our new multiplier if offset correctly
            if (globals.QueryPerformanceCounterFuncAddr != IntPtr.Zero)
            {
                long performanceCounter;
                QueryPerformanceCounter(out performanceCounter);
                RebaseTimeMultiplier(ref timeMultiplier.InitialPerformanceCounter, ref timeMultiplier.OffsetPerformanceCounter,
                    performanceCounter, oldMultiplier, firstRun);
            }
            if (globals.GetTickCount64FuncAddr != IntPtr.Zero)
            {
                RebaseTimeMultiplier(ref timeMultiplier.InitialTickCount64, ref timeMultiplier.OffsetTickCount64,
                    GetTickCount64(), oldMultiplier, firstRun);
            }
            if (globals.GetTickCountFuncAddr != IntPtr.Zero)
            {
                RebaseTimeMultiplier(ref timeMultiplier.InitialTickCount, ref timeMultiplier.OffsetTickCount,
                    GetTickCount(), oldMultiplier, firstRun);
            }
            if (globals.TimeGetTimeFuncAddr != IntPtr.Zero)
            {
                RebaseTimeMultiplier(ref timeMultiplier.InitialTimeGetTime, ref timeMultiplier.OffsetTimeGetTime,
                    TimeGetTime(), oldMultiplier, firstRun);
            }

            CallNativeScriptFunctionWithStruct("SetTimeMultiplierInfo", timeMultiplier);

            TimeMultiplierEnabled = enable;
            TimeMultiplier = multiplier;

            // Resume the thread handles we managed to suspend
            EndSuspendProcess(threadHandles);
        }

        private void RebaseTimeMultiplier<T>(ref T initialTime, ref T offsetTime, T time, double oldMultiplier, bool firstRun) where T : struct
        {
            if (firstRun)
            {
                initialTime = time;
                offsetTime = time;
                return;
            }

            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Int32:
                    offsetTime = (T)(object)(int)((((int)(object)time - (int)(object)initialTime) * oldMultiplier) + (int)(object)offsetTime);
                    break;
                case TypeCode.UInt32:
                    offsetTime = (T)(object)(uint)((((uint)(object)time - (uint)(object)initialTime) * oldMultiplier) + (uint)(object)offsetTime);
                    break;
                case TypeCode.Int64:
                    offsetTime = (T)(object)(long)((((long)(object)time - (long)(object)initialTime) * oldMultiplier) + (long)(object)offsetTime);
                    break;
                case TypeCode.UInt64:
                    offsetTime = (T)(object)(ulong)((((ulong)(object)time - (ulong)(object)initialTime) * oldMultiplier) + (ulong)(object)offsetTime);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
