using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Lotd
{
    /// <summary>
    /// This is a basic tool to handle compiling a c file using cl.exe and then using dumpbin.exe to get the
    /// code from the .obj which is then processed slightly to produce valid CALL instructions. The final byte
    /// array is then outputted as generated code along with an offset to a globals address and an offset to an
    /// entry point function. The globals address needs to be allocated and the address at the offset overwritten
    /// at runtime.
    /// 
    /// What this gives is a byte array which can be written to memory in a target process to execute arbitrary code
    /// without injecting a dll. All code must be stack based other than that 1 global address which can be used to hold
    /// anything. The entry point function can be called using CreateRemoteThread.
    /// 
    /// Reasons for doing this rather than just injecting a C/C++ written dll:
    /// 1) No dll needs to be injected into the target process.
    /// 2) We can create a self-hosting C# injector without depending on a native dll to be injected to host C#.
    /// 3) More control over the execution of our code and no c runtime bloat (though this means no c runtime functions)
    /// 
    /// Really this code should allow any compiler (not just cl.exe) and use a COFF loader rather than dumpbin.exe
    /// To keep code brief cl.exe / dumpbin.exe will do for now.
    /// </summary>
    class NativeScriptCompiler
    {
        private static string scriptFileDirName = "NativeScript";
        private static string scriptFileName = "NativeScript.c";
        private static string outputFileName = "NativeScript.Generated.cs";

        private static string GetScriptPath()
        {
            if (File.Exists(scriptFileName))
            {
                return Path.GetFullPath(scriptFileName);
            }

            if (File.Exists(Path.Combine(scriptFileDirName, scriptFileName)))
            {
                return Path.GetFullPath(Path.Combine(scriptFileDirName, scriptFileName));
            }

            return GetSourceFilePath(scriptFileName);
        }

        private static string GetSourceFilePath(string fileName)
        {
            string currentFilePath = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
            if (!string.IsNullOrEmpty(currentFilePath) && File.Exists(currentFilePath))
            {
                return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(currentFilePath), fileName));
            }
            return null;
        }

        private static string GetScriptHash()
        {
            string path = GetSourceFilePath(scriptFileName);
            if (File.Exists(path))
            {
                using (SHA256 hashAlgorithm = SHA256.Create())
                {
                    return BitConverter.ToString(hashAlgorithm.ComputeHash(File.ReadAllBytes(path))).Replace("-", string.Empty);
                }
            }
            return null;
        }

        public static void CompileIfChanged()
        {
            if (NativeScript.ScriptHash != GetScriptHash())
            {
                Compile();
            }
        }

        public static void Compile()
        {
            Compile(false);
        }

        public static void Compile(bool optimize)
        {
            string scriptPath = GetScriptPath();
            if (!string.IsNullOrEmpty(scriptPath) && File.Exists(scriptPath))
            {
                string[] lines = File.ReadAllLines(scriptPath);

                if (lines.Length < 2)
                {
                    return;
                }

                string compiler = lines[0].Trim('/', ' ');
                string dumpbin = lines[1].Trim('/', ' ');

                if (string.IsNullOrEmpty(compiler) || string.IsNullOrEmpty(dumpbin))
                {
                    return;
                }

                if (!File.Exists(compiler))
                {
                    compiler = Path.Combine(Path.GetDirectoryName(scriptPath), compiler);
                }

                if (!File.Exists(dumpbin))
                {
                    dumpbin = Path.Combine(Path.GetDirectoryName(scriptPath), dumpbin);
                }

                if (!File.Exists(compiler) || !File.Exists(dumpbin))
                {
                    return;
                }

                string scriptDir = Path.GetDirectoryName(scriptPath);
                string objPath = Path.ChangeExtension(scriptPath, ".obj");

                string additionalArgs = "/GS- ";// disable buffer security check (security cookie stuff)
                if (optimize)
                {
                    additionalArgs += "/O2";
                }

                string compileOutput, compileError;
                if (RunProcess(compiler, "/c " + additionalArgs + " \"" + scriptPath + "\"", scriptDir, out compileOutput, out compileError) &&
                    !compileOutput.Contains("error") && File.Exists(objPath))
                {
                    // Compile was successful, get the disasm

                    string disasmOutput, disasmError;
                    if (RunProcess(dumpbin, "\"" + objPath + "\" /disasm", scriptDir, out disasmOutput, out disasmError) &&
                        !disasmOutput.Contains("fatal error"))
                    {
                        string lastLine = string.Empty;

                        Module module = new Module();
                        Function currentFunction = null;
                        string instructionStr = string.Empty;
                        string instructionBytes = string.Empty;

                        string[] splitted = disasmOutput.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string line in splitted)
                        {
                            if (line.StartsWith("  0"))
                            {
                                if (lastLine.EndsWith(":") && !lastLine.StartsWith(" "))
                                {
                                    Debug.Assert(currentFunction == null);
                                    currentFunction = new Function(lastLine.Substring(0, lastLine.Length - 1));
                                    module.Functions.Add(currentFunction.Name, currentFunction);
                                }

                                if (!string.IsNullOrEmpty(instructionStr))
                                {
                                    currentFunction.AddInstruction(instructionBytes, instructionStr);
                                }

                                instructionBytes = string.Empty;
                                instructionStr = string.Empty;
                                string str = line.Substring(line.IndexOf(':') + 1);
                                int bytesEnd = str.IndexOf("  ");
                                instructionBytes = str.Substring(0, bytesEnd).Trim();
                                instructionStr = str.Substring(bytesEnd).Trim();

                                // Remove additional spacing between the mnemonic and the rest of the instruction
                                int firstSpace = instructionStr.IndexOf(' ');
                                while (true)
                                {
                                    if (instructionStr[firstSpace + 1] == ' ')
                                    {
                                        instructionStr = instructionStr.Remove(firstSpace + 1, 1);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                            else if (line == "  Summary")
                            {
                                break;
                            }
                            else if (line.StartsWith(" "))
                            {
                                instructionBytes += line.Trim();
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(instructionStr) && currentFunction != null)
                                {
                                    currentFunction.AddInstruction(instructionBytes, instructionStr);
                                }

                                currentFunction = null;
                                instructionBytes = string.Empty;
                                instructionStr = string.Empty;
                            }

                            lastLine = line;
                        }

                        if (!string.IsNullOrEmpty(instructionStr))
                        {
                            currentFunction.AddInstruction(instructionBytes, instructionStr);
                        }

                        module.Build();
                    }
                }
                else
                {
                    Debug.WriteLine("Compile failed " + compileOutput);
                    Debugger.Break();
                }
            }
        }

        private static bool RunProcess(string exePath, string args, string workingDir, out string output, out string error)
        {
            using (Process process = new Process())
            {
                process.StartInfo = new ProcessStartInfo
                {
                    WorkingDirectory = workingDir,
                    FileName = exePath,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                };

                StringBuilder outputStringBuilder = new StringBuilder();
                StringBuilder errorStringBuilder = new StringBuilder();

                using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
                using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
                {
                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                        {
                            outputWaitHandle.Set();
                        }
                        else
                        {
                            outputStringBuilder.AppendLine(e.Data);
                        }
                    };
                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                        {
                            errorWaitHandle.Set();
                        }
                        else
                        {
                            errorStringBuilder.AppendLine(e.Data);
                        }
                    };

                    process.Start();

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    int timeout = 60000;
                    if (process.WaitForExit(timeout) &&
                        outputWaitHandle.WaitOne(timeout) &&
                        errorWaitHandle.WaitOne(timeout))
                    {
                        output = outputStringBuilder.ToString();
                        error = errorStringBuilder.ToString();
                        return true;
                    }
                    else
                    {
                        output = outputStringBuilder.ToString();
                        error = errorStringBuilder.ToString();
                        Console.WriteLine("Failed to wait for process to complete '" + exePath + "'");
                        return false;
                    }
                }
            }
        }

        class Module
        {
            public Dictionary<string, Function> Functions { get; private set; }
            public Dictionary<Function, int> FunctionOffsets { get; private set; }
            public byte[] Buffer { get; private set; }
            
            private static string getGlobalsFunctionName = "GetGlobals";
            public Function GetGlobalsFunction { get; private set; }
            public int GlobalsAddressOffset { get; private set; }

            public string Code { get; private set; }

            public Module()
            {
                Functions = new Dictionary<string, Function>();
                FunctionOffsets = new Dictionary<Function, int>();
            }

            public void Build()
            {
                List<byte> buffer = new List<byte>();

                FunctionOffsets.Clear();

                GetGlobalsFunction = null;
                GlobalsAddressOffset = -1;

                int offset = 0;
                foreach (Function function in Functions.Values)
                {
                    if (function.Name == getGlobalsFunctionName)
                    {
                        GetGlobalsFunction = function;
                        GlobalsAddressOffset = offset + 2;
                        Debug.Assert(function.Instructions[0].CompleteInstruction == "mov rax,0AAAAAAAAAAAAAAAAh");
                        for (int i = 0; i < 8; i++)
                        {
                            function.Instructions[0].Bytes[i + 2] = 0;
                        }
                    }

                    int functionLen = 0;
                    foreach (Instruction instruction in function.Instructions)
                    {
                        functionLen += instruction.Bytes.Length;
                    }
                    FunctionOffsets.Add(function, offset);
                    offset += functionLen;
                }

                foreach (Function function in Functions.Values)
                {
                    foreach (Instruction instruction in function.Instructions)
                    {
                        if (instruction.IsCall && instruction.Bytes[0] == 0xE8)
                        {
                            string functionName = instruction.CompleteInstruction.Substring(
                                instruction.CompleteInstruction.IndexOf(' ') + 1).Trim();

                            int instructionOffset = FunctionOffsets[function] + instruction.Offset;
                            int targetFuncOffset = FunctionOffsets[Functions[functionName]];
                            
                            byte[] relativeAddr = BitConverter.GetBytes(targetFuncOffset - instructionOffset - 5);
                            instruction.Bytes[1] = relativeAddr[0];
                            instruction.Bytes[2] = relativeAddr[1];
                            instruction.Bytes[3] = relativeAddr[2];
                            instruction.Bytes[4] = relativeAddr[3];
                        }
                    }

                    function.BuildBytes();
                    buffer.AddRange(function.Bytes);
                }

                Buffer = buffer.ToArray();
                Code = GenerateCode();

                string outputFilePath = GetSourceFilePath(outputFileName);
                if (File.Exists(outputFilePath))
                {
                    string oldCode = File.ReadAllText(outputFilePath);
                    if (oldCode != Code)
                    {
                        File.WriteAllText(outputFilePath, Code);
                        Debugger.Break();
                    }
                }
                else
                {
                    Debug.WriteLine(Code);
                    Debugger.Break();
                }                
            }

            private string GenerateCode()
            {
                return GenerateCode(typeof(NativeScriptCompiler).Namespace);
            }

            private string GenerateCode(string namespaceName)
            {
                StringBuilder result = new StringBuilder();
                result.AppendLine("using System;");
                result.AppendLine("using System.Collections.Generic;");
                result.AppendLine();
                result.AppendLine("namespace " + namespaceName);
                result.AppendLine("{");
                result.AppendLine("    partial class NativeScript");
                result.AppendLine("    {");
                result.AppendLine("        public const string ScriptHash = \"" + GetScriptHash() + "\";");
                result.AppendLine("        public const int GlobalsAddressOffset = " + GlobalsAddressOffset + ";");
                result.AppendLine("        public static Dictionary<string, int> Functions = new Dictionary<string, int>()");
                result.AppendLine("        {");
                foreach (KeyValuePair<Function, int> functionOffset in FunctionOffsets)
                {
                    result.AppendLine("            { \"" + functionOffset.Key.Name + "\", " + functionOffset.Value + " },");
                }
                result.AppendLine("        };");
                result.AppendLine();
                result.AppendLine("        public static byte[] Buffer =");
                result.AppendLine("        {");
                result.Append("            ");
                for (int i = 0; i < Buffer.Length; i++)
                {
                    result.Append("0x" + Buffer[i].ToString("X2"));
                    if (i < Buffer.Length - 1)
                    {
                        result.Append(",");
                    }
                    if ((i + 1) % 16 == 0)
                    {
                        result.AppendLine();
                        result.Append("            ");
                    }
                }
                result.AppendLine();
                result.AppendLine("        };");
                result.AppendLine("    }");
                result.AppendLine("}");
                return result.ToString();
            }
        }

        class Function
        {
            public string Name { get; set; }
            public List<Instruction> Instructions { get; private set; }
            public byte[] Bytes { get; private set; }

            public Function(string name)
            {
                Name = name;
                Instructions = new List<Instruction>();
            }

            public void AddInstruction(string bytes, string completeInstruction)
            {
                Instruction instruction = new Instruction();

                Instruction lastInstruction = Instructions.Count > 0 ? Instructions[Instructions.Count - 1] : null;
                if (lastInstruction != null)
                {
                    instruction.Offset = lastInstruction.Offset + lastInstruction.Bytes.Length;
                }

                bytes = bytes.Replace(" ", string.Empty);
                Debug.Assert(bytes.Length % 2 == 0);
                instruction.Bytes = new byte[bytes.Length / 2];
                
                for (int i = 0; i < instruction.Bytes.Length; i++)
                {
                    instruction.Bytes[i] = byte.Parse(bytes.Substring(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                }

                instruction.CompleteInstruction = completeInstruction;

                int firstSpace = completeInstruction.IndexOf(' ');
                if (firstSpace >= 0)
                {
                    instruction.Mnemonic = completeInstruction.Substring(0, firstSpace);
                }

                Instructions.Add(instruction);
            }

            public void BuildBytes()
            {
                List<byte> bytes = new List<byte>();
                foreach (Instruction instruction in Instructions)
                {
                    bytes.AddRange(instruction.Bytes);
                }
                Bytes = bytes.ToArray();
            }
        }

        class Instruction
        {
            public byte[] Bytes { get; set; }
            public string Mnemonic { get; set; }
            public string CompleteInstruction { get; set; }

            /// <summary>
            /// Offset into the owning function (if this is part of a function)
            /// </summary>
            public int Offset { get; set; }

            public bool IsCall
            {
                get { return Mnemonic != null && Mnemonic.ToLower() == "call"; }
            }

            public override string ToString()
            {
                return CompleteInstruction;
            }
        }
    }
}
