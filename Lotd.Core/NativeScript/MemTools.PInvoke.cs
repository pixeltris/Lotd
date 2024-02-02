using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Lotd
{
    public partial class MemTools
    {
        private static string processName = "YuGiOh";

        public static bool Available
        {
            get { return IntPtr.Size == 8; }
        }

        public static bool IsProcessAvailable
        {
            get
            {
                Process[] processes = Process.GetProcessesByName(processName);
                for (int i = 0; i < processes.Length; i++)
                {
                    processes[i].Close();
                }
                return processes.Length > 0;
            }
        }

        public Process Process { get; private set; }
        public IntPtr ProcessHandle { get; private set; }

        public bool HasProcessHandle
        {
            get { return ProcessHandle != IntPtr.Zero && !Process.HasExited; }
        }

        private IntPtr nativeScriptAddress = IntPtr.Zero;
        private IntPtr nativeScriptGlobalsAddress = IntPtr.Zero;

        public bool IsNativeScriptLoaded
        {
            get { return nativeScriptAddress != IntPtr.Zero; }
        }

        private bool UpdateState()
        {
            return HasProcessHandle || Open();
        }

        public void FocusWindow()
        {
            try
            {
                if (Process != null)
                {
                    IntPtr hwnd = Process.MainWindowHandle;
                    SetForegroundWindow(hwnd);
                }
            }
            catch
            {
            }
        }

        public bool Open()
        {
            Close();

            Process[] processes = Process.GetProcessesByName(processName);
            if (processes.Length > 0)
            {
                Process = processes[0];

                ProcessAccessFlags accessFlags =
                    ProcessAccessFlags.QueryInformation | ProcessAccessFlags.CreateThread |
                    ProcessAccessFlags.VMOperation | ProcessAccessFlags.VMRead | ProcessAccessFlags.VMWrite;

                ProcessHandle = OpenProcess(accessFlags, false, Process.Id);

                if (ProcessHandle == IntPtr.Zero)
                {
                    Process = null;
                }
            }

            for (int i = 0; i < processes.Length; i++)
            {
                if (Process == null || processes[i] != Process)
                {
                    processes[i].Close();
                }
            }

            if (Process != null)
            {
                Process.Exited += Process_Exited;
            }

            return HasProcessHandle;
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            Close();
        }

        public void Close()
        {
            if (ProcessHandle != IntPtr.Zero)
            {
                CloseHandle(ProcessHandle);
            }
            ProcessHandle = IntPtr.Zero;
            nativeScriptAddress = IntPtr.Zero;

            if (IsFullyLoaded && Unloaded != null)
            {
                Unloaded(this, EventArgs.Empty);
            }
            IsFullyLoaded = false;

            if (Process != null)
            {
                Process.Close();
                Process = null;
            }
        }

        private bool LoadNativeScript(ref NativeScript.Globals globals)
        {
            return LoadNativeScript(ref globals, true);
        }

        private bool LoadNativeScript(ref NativeScript.Globals globals, bool callEntryPoint)
        {
            if (!UpdateState() || IsNativeScriptLoaded)
            {
                return false;
            }

            IntPtr processHandle = ProcessHandle;

            // Buffer for the NativeScript code
            byte[] buffer = new byte[NativeScript.Scripts[Version].Buffer.Length];
            Buffer.BlockCopy(NativeScript.Scripts[Version].Buffer, 0, buffer, 0, buffer.Length);

            IntPtr address = VirtualAllocEx(processHandle, IntPtr.Zero, (IntPtr)buffer.Length,
                AllocationType.Commit, MemoryProtection.ExecuteReadWrite);

            if (address == IntPtr.Zero)
            {
                return false;
            }

            // Write the NativeScript code
            IntPtr bytesWritten;
            if (!WriteProcessMemoryEx(processHandle, (IntPtr)address.ToInt64(), buffer, (IntPtr)buffer.Length, out bytesWritten) ||
                bytesWritten.ToInt32() != buffer.Length)
            {
                return false;
            }

            // Set the Globals base address and write the Globals structure to memory
            globals.BaseAddress = address;

            byte[] globalsBuffer = StructToByteArray(globals);
            IntPtr globalsAddress = VirtualAllocEx(processHandle, IntPtr.Zero, (IntPtr)globalsBuffer.Length,
                AllocationType.Commit, MemoryProtection.ReadWrite);
            
            if (globalsAddress == IntPtr.Zero ||
                !WriteProcessMemoryEx(processHandle, globalsAddress, globalsBuffer, (IntPtr)globalsBuffer.Length, out bytesWritten) ||
                bytesWritten.ToInt32() != globalsBuffer.Length)
            {
                return false;
            }

            // Write the globals address to the NativeScript GetGlobals function
            byte[] globalsAddressBuffer = BitConverter.GetBytes(globalsAddress.ToInt64());
            Debug.Assert(globalsAddressBuffer.Length == 8);
            if (!WriteProcessMemoryEx(processHandle, (IntPtr)(address.ToInt64() + NativeScript.Scripts[Version].GlobalsAddressOffset), globalsAddressBuffer,
                (IntPtr)globalsAddressBuffer.Length, out bytesWritten) || bytesWritten.ToInt32() != globalsAddressBuffer.Length)
            {
                return false;
            }

            nativeScriptAddress = address;
            nativeScriptGlobalsAddress = globalsAddress;

            if (callEntryPoint)
            {
                CallNativeScriptFunction("EntryPoint");
            }

            return true;
        }

        private CallNativeFunctionResult CallNativeScriptFunctionWithStruct<T>(string functionName, ref T arg) where T : struct
        {
            int result;
            return CallNativeScriptFunction(functionName, ref arg, true, out result);
        }

        private CallNativeFunctionResult CallNativeScriptFunctionWithStruct<T>(string functionName, T arg) where T : struct
        {
            int result;
            return CallNativeScriptFunction(functionName, ref arg, true, out result);
        }

        private CallNativeFunctionResult CallNativeScriptFunctionWithStruct<T>(string functionName, ref T arg, out int result) where T : struct
        {
            return CallNativeScriptFunction(functionName, ref arg, true, out result);
        }

        private CallNativeFunctionResult CallNativeScriptFunctionWithStruct<T>(string functionName, T arg, out int result) where T : struct
        {
            return CallNativeScriptFunction(functionName, ref arg, true, out result);
        }

        private CallNativeFunctionResult CallNativeScriptFunction(string functionName)
        {
            int result;
            return CallNativeScriptFunction(functionName, 0, out result);
        }

        private CallNativeFunctionResult CallNativeScriptFunction(string functionName, out int result)
        {
            return CallNativeScriptFunction(functionName, 0, out result);
        }

        private CallNativeFunctionResult CallNativeScriptFunction(string functionName, long arg)
        {
            int result;
            return CallNativeScriptFunction(functionName, arg, out result);
        }

        private CallNativeFunctionResult CallNativeScriptFunction(string functionName, long arg, out int result)
        {
            return CallNativeScriptFunction(functionName, ref arg, false, out result);
        }

        private CallNativeFunctionResult CallNativeScriptFunction<T>(string functionName, ref T arg, bool isStructArg, out int result) where T : struct
        {
            result = 0;

            if (!UpdateState())
            {
                return CallNativeFunctionResult.ProcessClosed;
            }

            if (!IsNativeScriptLoaded)
            {
                return CallNativeFunctionResult.NativeScriptNotLoaded;
            }

            IntPtr processHandle = ProcessHandle;

            byte[] argStructBuffer = StructToByteArray(arg);            
            IntPtr argVal = IntPtr.Zero;
            bool isArgValRemoteAllocated = false;

            if (isStructArg)
            {
                // Allocate memory for the struct in the remote process
                argVal = VirtualAllocEx(processHandle, IntPtr.Zero, (IntPtr)argStructBuffer.Length,
                    AllocationType.Commit, MemoryProtection.ReadWrite);

                if (argVal == IntPtr.Zero)
                {
                    return CallNativeFunctionResult.ArgAllocateFailed;
                }

                // Write the struct bytes to the remote address
                IntPtr bytesWritten;
                if (!WriteProcessMemoryEx(processHandle, argVal, argStructBuffer, (IntPtr)argStructBuffer.Length, out bytesWritten) ||
                    bytesWritten.ToInt32() != argStructBuffer.Length)
                {
                    return CallNativeFunctionResult.ArgWriteFailed;
                }

                isArgValRemoteAllocated = true;
            }
            else
            {
                Debug.Assert(argStructBuffer.Length <= IntPtr.Size);
                byte[] argValBuffer = new byte[IntPtr.Size];
                Buffer.BlockCopy(argStructBuffer, 0, argValBuffer, 0, argStructBuffer.Length);
                argVal = new IntPtr(BitConverter.ToInt64(argValBuffer, 0));
            }

            IntPtr functionAddress = GetNativeScriptFunctionAddress(functionName);
            if (functionAddress == IntPtr.Zero)
            {
                return CallNativeFunctionResult.FindFunctionFailed;
            }

            IntPtr thread = CreateRemoteThread(processHandle, IntPtr.Zero, IntPtr.Zero, functionAddress, argVal, 0, IntPtr.Zero);

            CallNativeFunctionResult callResult = CallNativeFunctionResult.Success;

            if (thread == IntPtr.Zero)
            {
                callResult = CallNativeFunctionResult.CreateThreadFailed;
            }
            else
            {
                uint singleObject = WaitForSingleObject(thread, (uint)ThreadWaitValue.Infinite);
                if (!(singleObject == (uint)ThreadWaitValue.Object0 || singleObject == (uint)ThreadWaitValue.Timeout))
                {
                    callResult = CallNativeFunctionResult.WaitForThreadFailed;
                }
                else if (!GetExitCodeThread(thread, out result))
                {
                    callResult = CallNativeFunctionResult.GetThreadExitCodeFailed;
                }
            }

            if (isArgValRemoteAllocated)
            {
                // Read the struct back from memory
                IntPtr readBytes;
                if (ReadProcessMemoryEx(processHandle, argVal, argStructBuffer, (IntPtr)argStructBuffer.Length, out readBytes) &&
                    readBytes.ToInt32() == argStructBuffer.Length)
                {
                    arg = StructFromByteArray<T>(argStructBuffer);
                }

                // Free the address for the struct allocated in the remote process memory
                VirtualFreeEx(processHandle, argVal, (IntPtr)0, AllocationType.Release);
            }

            return callResult;
        }

        private IntPtr GetNativeScriptFunctionAddress(string functionName)
        {
            if (!IsNativeScriptLoaded)
            {
                return IntPtr.Zero;
            }

            int functionOffset;
            if (!NativeScript.Scripts[Version].Functions.TryGetValue(functionName, out functionOffset))
            {
                return IntPtr.Zero;
            }

            return nativeScriptAddress + functionOffset;
        }

        private bool HookFunction(long address, string functionName)
        {
            return HookFunction((IntPtr)address, functionName);
        }

        private bool HookFunction(IntPtr address, string functionName)
        {
            if (!UpdateState())
            {
                return false;
            }

            // Just a straight up JMP and we lose the original bytes. No way to call the original method.

            // JMP [RIP+0] followed by the jmp address
            byte[] buffer = { 0xFF, 0x25, 0, 0, 0, 0,    0, 0, 0, 0, 0, 0, 0, 0 };

            IntPtr hookAddress = GetNativeScriptFunctionAddress(functionName);
            byte[] hookAddressBytes = BitConverter.GetBytes(hookAddress.ToInt64());
            Buffer.BlockCopy(hookAddressBytes, 0, buffer, 6, hookAddressBytes.Length);

            IntPtr writtenBytes;
            return WriteProcessMemoryEx(ProcessHandle, address, buffer, (IntPtr)buffer.Length, out writtenBytes) &&
                writtenBytes.ToInt32() == buffer.Length;
        }

        private List<IntPtr> BeginSuspendProcess()
        {
            // Should we do multiple checks to ensure all threads are suspended?
            List<IntPtr> threadHandles = new List<IntPtr>();
            try
            {
                Process process = Process;

                if (process != null)
                {
                    foreach (ProcessThread thread in process.Threads)
                    {
                        IntPtr handle = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)thread.Id);
                        if (handle != IntPtr.Zero)
                        {
                            SuspendThread(handle);
                            threadHandles.Add(handle);
                        }
                    }
                }
            }
            catch
            {
            }
            return threadHandles;
        }

        private void EndSuspendProcess(List<IntPtr> threadHandles)
        {            
            foreach (IntPtr handle in threadHandles)
            {
                ResumeThread(handle);
                CloseHandle(handle);
            }
        }

        private void SuspendProcess()
        {
            try
            {
                Process process = Process;

                if (process != null)
                {
                    foreach (ProcessThread thread in process.Threads)
                    {
                        IntPtr handle = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)thread.Id);
                        if (handle != IntPtr.Zero)
                        {
                            SuspendThread(handle);
                            CloseHandle(handle);
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private void ResumeProcess()
        {
            try
            {
                Process process = Process;

                if (process != null)
                {
                    foreach (ProcessThread thread in process.Threads)
                    {
                        IntPtr handle = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)thread.Id);
                        if (handle != IntPtr.Zero)
                        {
                            ResumeThread(handle);
                            CloseHandle(handle);
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private string ReadStringASCII(long address)
        {
            return ReadStringUnicode((IntPtr)address);
        }

        private string ReadStringASCII(IntPtr address)
        {
            StringBuilder result = new StringBuilder();
            while (true)
            {
                int chunkLen = 128;
                byte[] buffer = ReadBytes(address, chunkLen);
                for (int i = 0; i < buffer.Length; i++)
                {
                    if (buffer[i] == 0)
                    {
                        return result.ToString();
                    }
                    else
                    {
                        result.Append((char)buffer[i]);
                    }
                }
                address += chunkLen;
            }
        }

        private string ReadStringUnicode(long address)
        {
            return ReadStringUnicode((IntPtr)address);
        }

        private string ReadStringUnicode(IntPtr address)
        {
            StringBuilder result = new StringBuilder();
            while (true)
            {
                int chunkLen = 128;
                byte[] buffer = ReadBytes(address, chunkLen);
                for (int i = 0; i < buffer.Length - 1; i += 2)
                {
                    if (buffer[i] == 0 && buffer[i + 1] == 0)
                    {
                        return result.ToString();
                    }
                    else
                    {
                        result.Append((char)BitConverter.ToInt16(buffer, i));
                    }
                }
                address += chunkLen;
            }
        }

        /// <summary>
        /// Allocate a buffer in the remote process memory
        /// </summary>
        private IntPtr AllocateRemoteBuffer(byte[] buffer)
        {
            if (!UpdateState())
            {
                return IntPtr.Zero;
            }

            IntPtr processHandle = ProcessHandle;

            IntPtr address = VirtualAllocEx(processHandle, IntPtr.Zero, (IntPtr)buffer.Length,
               AllocationType.Commit, MemoryProtection.ExecuteReadWrite);

            if (address == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }

            IntPtr bytesWritten;
            if (!WriteProcessMemoryEx(processHandle, (IntPtr)address.ToInt64(), buffer, (IntPtr)buffer.Length, out bytesWritten) ||
                bytesWritten.ToInt32() != buffer.Length)
            {
                return IntPtr.Zero;
            }

            return address;
        }

        /// <summary>
        /// Destroy a buffer we allocated in the remote process memory with AllocateRemoteBuffer
        /// </summary>
        private void DestroyRemoteBuffer(IntPtr address)
        {
            VirtualFreeEx(ProcessHandle, address, (IntPtr)0, AllocationType.Release);
        }

        private byte[] ReadBytes(long address, int count)
        {
            return ReadBytes((IntPtr)address, count);
        }

        private byte[] ReadBytes(IntPtr handle, long address, int count)
        {
            return ReadBytes(handle, address, count);
        }

        private byte[] ReadBytes(IntPtr address, int count)
        {
            return ReadBytes(ProcessHandle, address, count);
        }

        private byte[] ReadBytes(IntPtr handle, IntPtr address, int count)
        {
            byte[] buffer = new byte[count];

            IntPtr bytesRead;
            ReadProcessMemoryEx(handle, address, buffer, (IntPtr)buffer.Length, out bytesRead);

            return buffer;
        }

        private void WriteBytes(long address, byte[] value)
        {
            WriteBytes((IntPtr)address, value);
        }

        private void WriteBytes(IntPtr handle, long address, byte[] value)
        {
            WriteBytes(handle, (IntPtr)address, value);
        }

        private void WriteBytes(IntPtr address, byte[] value)
        {
            WriteBytes(ProcessHandle, address, value);
        }

        private void WriteBytes(IntPtr handle, IntPtr address, byte[] value)
        {
            if (!UpdateState())
            {
                return;
            }

            IntPtr bytesWritten;
            WriteProcessMemoryEx(handle, address, value, (IntPtr)value.Length, out bytesWritten);
        }

        private T ReadValue<T>(long address) where T : struct
        {
            return ReadValue<T>((IntPtr)address);
        }

        private T ReadValue<T>(IntPtr address) where T : struct
        {
            return ReadValue<T>(ProcessHandle, address);
        }

        private T ReadValue<T>(IntPtr handle, long address) where T : struct
        {
            return ReadValue<T>(handle, (IntPtr)address);
        }

        private T ReadValue<T>(IntPtr handle, IntPtr address) where T : struct
        {
            if (!UpdateState())
            {
                return default(T);
            }

            byte[] buffer = new byte[Marshal.SizeOf(typeof(T))];

            IntPtr bytesRead;
            ReadProcessMemoryEx(handle, address, buffer, (IntPtr)buffer.Length, out bytesRead);

            return StructFromByteArray<T>(buffer);
        }

        private T[] ReadValues<T>(long address, int count) where T : struct
        {
            return ReadValues<T>(ProcessHandle, address, count);
        }

        private T[] ReadValues<T>(IntPtr address, int count) where T : struct
        {
            return ReadValues<T>(ProcessHandle, address, count);
        }

        private T[] ReadValues<T>(IntPtr handle, long address, int count) where T : struct
        {
            return ReadValues<T>(handle, (IntPtr)address, count);
        }

        private T[] ReadValues<T>(IntPtr handle, IntPtr address, int count) where T : struct
        {
            if (!UpdateState())
            {
                return null;
            }

            byte[] buffer = new byte[Marshal.SizeOf(typeof(T)) * count];

            IntPtr bytesRead;
            if (!ReadProcessMemoryEx(handle, address, buffer, (IntPtr)buffer.Length, out bytesRead) || bytesRead.ToInt64() == 0)
            {
                return null;
            }

            return StructsFromByteArray<T>(buffer);
        }

        private void WriteValue<T>(long address, T value) where T : struct
        {
            WriteValue(ProcessHandle, address, value);
        }

        private void WriteValue<T>(IntPtr address, T value) where T : struct
        {
            WriteValue(ProcessHandle, address, value);
        }

        private void WriteValue<T>(IntPtr handle, long address, T value) where T : struct
        {
            WriteValue(handle, (IntPtr)address, value);
        }

        private void WriteValue<T>(IntPtr handle, IntPtr address, T value) where T : struct
        {
            if (!UpdateState())
            {
                return;
            }

            byte[] buffer = StructToByteArray(value);

            IntPtr bytesWritten;
            WriteProcessMemoryEx(handle, address, buffer, (IntPtr)buffer.Length, out bytesWritten);
        }

        private void WriteValues<T>(long address, T[] values) where T : struct
        {
            WriteValues(ProcessHandle, address, values);
        }

        private void WriteValues<T>(IntPtr address, T[] values) where T : struct
        {
            WriteValues(ProcessHandle, address, values);
        }

        private void WriteValues<T>(IntPtr handle, long address, T[] values) where T : struct
        {
            WriteValues(handle, (IntPtr)address, values);
        }

        private void WriteValues<T>(IntPtr handle, IntPtr address, T[] values) where T : struct
        {
            if (!UpdateState())
            {
                return;
            }

            byte[] buffer = StructsToByteArray(values);

            IntPtr bytesWritten;
            WriteProcessMemoryEx(handle, address, buffer, (IntPtr)buffer.Length, out bytesWritten);
        }

        private T[] StructsFromByteArray<T>(byte[] buffer) where T : struct
        {
            int structSize = Marshal.SizeOf(typeof(T));
            Debug.Assert(buffer.Length % buffer.Length == 0);

            T[] result = new T[buffer.Length / structSize];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = StructFromByteArray<T>(buffer, i * structSize);
            }
            return result;
        }

        public static T StructFromByteArray<T>(byte[] value) where T : struct
        {
            return StructFromByteArray<T>(value, 0);
        }

        private static T StructFromByteArray<T>(byte[] value, int index) where T : struct
        {
            int structSize = Marshal.SizeOf(typeof(T));
            IntPtr ptr = Marshal.AllocHGlobal(structSize);
            Marshal.Copy(value, index, ptr, structSize);
            return (T)Marshal.PtrToStructure(ptr, typeof(T));
        }

        private byte[] StructsToByteArray<T>(T[] value) where T : struct
        {
            int structSize = Marshal.SizeOf(typeof(T));
            byte[] result = new byte[structSize * value.Length];
            for (int i = 0; i < value.Length; i++)
            {
                StructToByteArray(value[i], result, i * structSize);
            }
            return result;
        }

        public static byte[] StructToByteArray<T>(T value) where T : struct
        {
            byte[] buffer = new byte[Marshal.SizeOf(typeof(T))];
            StructToByteArray(value, buffer, 0);
            return buffer;
        }

        private static void StructToByteArray<T>(T value, byte[] buffer, int index) where T : struct
        {
            int structSize = Marshal.SizeOf(typeof(T));
            IntPtr ptr = Marshal.AllocHGlobal(structSize);
            Marshal.StructureToPtr(value, ptr, false);
            Marshal.Copy(ptr, buffer, index, structSize);
            Marshal.FreeHGlobal(ptr);
        }

        /// <summary>
        /// Writes memory to a process and attempts to temporarily change memory page protection if it fails the first time
        /// </summary>
        private bool WriteProcessMemoryEx(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, IntPtr nSize, out IntPtr lpNumberOfBytesWritten)
        {
            bool success = WriteProcessMemory(hProcess, lpBaseAddress, lpBuffer, nSize, out lpNumberOfBytesWritten);
            if (!success)
            {
                // We probably don't have access to this page. Change the page protection to gain access
                PageProtection oldPageProtection = default(PageProtection);
                if (!VirtualProtectEx(hProcess, lpBaseAddress, nSize, PageProtection.PAGE_EXECUTE_READWRITE, out oldPageProtection))
                {
                    // We failed to obtain access
                    return false;
                }

                success = WriteProcessMemory(hProcess, lpBaseAddress, lpBuffer, nSize, out lpNumberOfBytesWritten);
                
                // Restore the old page protection
                VirtualProtectEx(hProcess, lpBaseAddress, nSize, oldPageProtection, out oldPageProtection);
            }

            return success;
        }

        /// <summary>
        /// Read memory from a process and attempts to temporarily change memory protection if it fails the first time
        /// </summary>
        private bool ReadProcessMemoryEx(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, IntPtr dwSize, out IntPtr lpNumberOfBytesRead)
        {
            bool success = ReadProcessMemory(hProcess, lpBaseAddress, lpBuffer, dwSize, out lpNumberOfBytesRead);
            if (!success)
            {
                MEMORY_BASIC_INFORMATION64 mbi;
                if (VirtualQueryEx(hProcess, lpBaseAddress, out mbi, MEMORY_BASIC_INFORMATION64.Size) > 0)
                {
                    bool changePageProtection = true;
                    PageProtection newPageProtection = default(PageProtection);

                    // Attempt to give reasonable read access based on the current page protection
                    switch (mbi.Protect)
                    {
                        case PageProtection.PAGE_NOACCESS:
                            newPageProtection = PageProtection.PAGE_READONLY;
                            break;
                        case PageProtection.PAGE_EXECUTE:
                            newPageProtection = PageProtection.PAGE_EXECUTE_READ;
                            break;
                        case PageProtection.PAGE_EXECUTE_WRITECOPY:
                            newPageProtection = PageProtection.PAGE_EXECUTE_READWRITE;
                            break;
                        case PageProtection.PAGE_WRITECOPY:
                            newPageProtection = PageProtection.PAGE_READWRITE;
                            break;
                        default:
                            changePageProtection = false;
                            break;
                    }

                    if (changePageProtection)
                    {
                        PageProtection oldPageProtection = default(PageProtection);
                        if (!VirtualProtectEx(hProcess, lpBaseAddress, dwSize, newPageProtection, out oldPageProtection))
                        {
                            // We failed to obtain access
                            return false;
                        }

                        success = ReadProcessMemory(hProcess, lpBaseAddress, lpBuffer, dwSize, out lpNumberOfBytesRead);

                        // Restore the old page protection
                        VirtualProtectEx(hProcess, lpBaseAddress, dwSize, oldPageProtection, out oldPageProtection);
                    }
                }
            }

            return success;
        }

        private IntPtr GetProcAddress(string moduleName, string functionName)
        {
            IntPtr moduleHandle = GetModuleHandle(moduleName);
            if (moduleHandle == IntPtr.Zero)
            {
                moduleHandle = LoadLibraryEx(moduleName, IntPtr.Zero, LoadLibraryExFlags.DontResolveDllReferences);
            }
            if (moduleHandle != IntPtr.Zero)
            {
                return GetProcAddress(moduleHandle, functionName);
            }
            return IntPtr.Zero;
        }

        [DllImport("kernel32.dll")]
        static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION64 lpBuffer, IntPtr dwLength);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize, PageProtection flNewProtect, out PageProtection lpflOldProtect);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, IntPtr nSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [In, Out] byte[] lpBuffer, IntPtr dwSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
        static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize, AllocationType flAllocationType, MemoryProtection flProtect);

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize, AllocationType dwFreeType);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, IntPtr dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, [Out] IntPtr lpThreadId);

        [DllImport("kernel32")]
        static extern uint WaitForSingleObject(IntPtr hObject, uint dwMilliseconds);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetExitCodeThread(IntPtr hThread, out int lpExitCode);

        [DllImport("kernel32.dll", EntryPoint = "GetModuleHandleW", SetLastError = true)]
        static extern IntPtr GetModuleHandle([MarshalAs(UnmanagedType.LPWStr)] string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", EntryPoint = "LoadLibraryExW", SetLastError = true)]
        public static extern IntPtr LoadLibraryEx([MarshalAs(UnmanagedType.LPWStr)] string lpFileName, IntPtr hFile, LoadLibraryExFlags dwFlags);

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

        [DllImport("kernel32.dll")]
        public static extern uint SuspendThread(IntPtr hThread);

        [DllImport("kernel32.dll")]
        public static extern int ResumeThread(IntPtr hThread);

        [DllImport("kernel32.dll")]
        static extern void SetLastError(int dwErrorCode);

        [DllImport("kernel32.dll")]
        static extern int GetLastError();

        [DllImport("kernel32.dll")]
        static extern bool QueryPerformanceCounter(out long lpPerformanceCounter);

        [DllImport("kernel32.dll")]
        static extern ulong GetTickCount64();

        [DllImport("kernel32.dll")]
        static extern int GetTickCount();

        [DllImport("winmm.dll", EntryPoint = "timeGetTime")]
        static extern uint TimeGetTime();

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hwnd);

        [StructLayout(LayoutKind.Sequential)]
        struct MEMORY_BASIC_INFORMATION64
        {
            public static IntPtr Size = (IntPtr)48;

            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public PageProtection AllocationProtect;
            public int __alignment1;
            public long RegionSize;
            public uint State;
            public PageProtection Protect;
            public uint Type;
            public int __alignment2;
        }

        [Flags]
        enum PageProtection : uint
        {
            PAGE_EXECUTE = 0x10,
            PAGE_EXECUTE_READ = 0x20,
            PAGE_EXECUTE_READWRITE = 0x40,
            PAGE_EXECUTE_WRITECOPY = 0x80,
            PAGE_NOACCESS = 0x01,
            PAGE_READONLY = 0x02,
            PAGE_READWRITE = 0x04,
            PAGE_WRITECOPY = 0x08,
        }

        [Flags]
        enum ProcessAccessFlags : uint
        {
            Terminate = 1u,
            CreateThread = 2u,
            SetSessionID = 4u,
            VMOperation = 8u,
            VMRead = 16u,
            VMWrite = 32u,
            DUPHandle = 64u,
            CreateProcess = 128u,
            SetQuota = 256u,
            SetInformation = 512u,
            QueryInformation = 1024u,
            SuspendResume = 2048u,
            QueryLimitedInformation = 4096u,
            AllAccess = 2097151u,
            Synchronize = 1048576u,
            StandardRightsRequired = 983040u
        }

        [Flags]
        enum AllocationType : uint
        {
            Commit = 4096u,
            Reserve = 8192u,
            Decommit = 16384u,
            Release = 32768u,
            Free = 65536u,
            Private = 131072u,
            Mapped = 262144u,
            Reset = 524288u,
            TopDown = 1048576u,
            WriteWatch = 2097152u,
            Physical = 4194304u,
            Rotate = 8388608u,
            LargePages = 536870912u,
            FourMbPages = 2147483648u
        }

        enum MemoryProtection : uint
        {
            NoAccess = 1u,
            ReadOnly,
            ReadWrite = 4u,
            WriteCopy = 8u,
            Execute = 16u,
            ExecuteRead = 32u,
            ExecuteReadWrite = 64u,
            ExecuteWriteCopy = 128u,
            PageGuard = 256u,
            NoCache = 512u,
            WriteCombine = 1024u
        }

        enum ThreadWaitValue : uint
        {
            Object0,
            Abandoned = 128u,
            Timeout = 258u,
            Failed = 4294967295u,
            Infinite = 4294967295u
        }

        [Flags]
        public enum LoadLibraryExFlags : uint
        {
            DontResolveDllReferences = 1u,
            LoadLibraryAsDatafile = 2u,
            LoadLibraryWithAlteredSearchPath = 8u,
            LoadIgnoreCodeAuthzLevel = 16u,
            LoadLibraryAsImageResource = 32u,
            LoadLibraryAsDatafileExclusive = 64u
        }

        [Flags]
        public enum ThreadAccess : int
        {
            TERMINATE = (0x0001),
            SUSPEND_RESUME = (0x0002),
            GET_CONTEXT = (0x0008),
            SET_CONTEXT = (0x0010),
            SET_INFORMATION = (0x0020),
            QUERY_INFORMATION = (0x0040),
            SET_THREAD_TOKEN = (0x0080),
            IMPERSONATE = (0x0100),
            DIRECT_IMPERSONATION = (0x0200)
        }

        enum CallNativeFunctionResult
        {
            ProcessClosed,           
            NativeScriptNotLoaded,
            FindFunctionFailed,
            ArgAllocateFailed,
            ArgWriteFailed,
            CreateThreadFailed,
            WaitForThreadFailed,
            GetThreadExitCodeFailed,
            Success
        }
    }
}
