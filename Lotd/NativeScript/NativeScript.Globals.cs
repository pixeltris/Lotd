using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Lotd
{
    partial class NativeScript
    {
        // Must be the exact same struct as NativeScript.c
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct Globals
        {
            public IntPtr BaseAddress;

            public IntPtr GetCurrentProcessFuncAddr;
            public IntPtr WriteProcessMemoryFuncAddr;
            public IntPtr VirtualProtectFuncAddr;
            public IntPtr EnterCriticalSectionFuncAddr;
            public IntPtr LeaveCriticalSectionFuncAddr;
            public IntPtr srandFuncAddr;
            public IntPtr QueryPerformanceCounterFuncAddr;
            public IntPtr GetTickCount64FuncAddr;
            public IntPtr GetTickCountFuncAddr;
            public IntPtr TimeGetTimeFuncAddr;

            public TimeMultiplierInfo TimeMultiplier;

            public RandSeed Seed;

            private int useScreenStateTransitions;
            public bool UseScreenStateTransitions
            {
                get { return useScreenStateTransitions != 0; }
                set { useScreenStateTransitions = value ? 1 : 0; }
            }

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x70)]
            public byte[] BlockedActionIds;// The ids are the index
            public IntPtr ActionHandlerHookAddress;// The address of the action handler hook in NativeScript.c
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 90)]
            public byte[] BlockedAnimationIds;// The ids are the index
            public IntPtr AnimationHandlerHookAddress;// The address of the animation hanlder hook in NativeScript.c
            public int CurrentAnimationId;// The currently updating animation id

            public int DuelSeed;

            public IntPtr DuelInitDeckHandLPHookAddress;// For speed duel (otherwise it will be cleared)
            private int isNextDuelSpeedDuel;
            public bool IsNextDuelSpeedDuel
            {
                get { return isNextDuelSpeedDuel != 0; }
                set { isNextDuelSpeedDuel = value ? 1 : 0; }
            }
            public int NextDuelHandCount;// Number of cards to draw for the next duel (set to -1 to disable)

            public IntPtr LoadBattlePackYdcHookAddress;// For custom ydc battle packs
            private int customYdcBattlePacksEnabled;
            public bool CustomYdcBattlePacksEnabled
            {
                get { return customYdcBattlePacksEnabled != 0; }
                set { customYdcBattlePacksEnabled = value ? 1 : 0; }
            }
            public const int NumCustomBattlePackDecks = 100;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = NumCustomBattlePackDecks)]
            public MemTools.YdcDeck[] BattlePackDecks;
            
            public bool IsValid
            {
                get
                {
                    return
                        GetCurrentProcessFuncAddr != IntPtr.Zero &&
                        WriteProcessMemoryFuncAddr != IntPtr.Zero &&
                        VirtualProtectFuncAddr != IntPtr.Zero &&
                        EnterCriticalSectionFuncAddr != IntPtr.Zero &&
                        LeaveCriticalSectionFuncAddr != IntPtr.Zero &&
                        srandFuncAddr != IntPtr.Zero;
                }
            }

            public void InitializeSeed()
            {
                Seed = RandSeed.Create();
            }

            public static int OffsetActionHandlerHookAddress
            {
                get { return Marshal.OffsetOf(typeof(Globals), "ActionHandlerHookAddress").ToInt32(); }
            }

            public static int OffsetAnimationHandlerHookAddress
            {
                get { return Marshal.OffsetOf(typeof(Globals), "AnimationHandlerHookAddress").ToInt32(); }
            }

            public static int OffsetBlockedActionIds
            {
                get { return Marshal.OffsetOf(typeof(Globals), "BlockedActionIds").ToInt32(); }
            }

            public static int OffsetBlockedAnimationIds
            {
                get { return Marshal.OffsetOf(typeof(Globals), "BlockedAnimationIds").ToInt32(); }
            }

            public static int OffsetCurrentAnimationId
            {
                get { return Marshal.OffsetOf(typeof(Globals), "CurrentAnimationId").ToInt32(); }
            }

            public static int OffsetDuelInitDeckHandLPHookAddress
            {
                get { return Marshal.OffsetOf(typeof(Globals), "DuelInitDeckHandLPHookAddress").ToInt32(); }
            }

            public static int OffsetIsNextDuelSpeedDuel
            {
                get { return Marshal.OffsetOf(typeof(Globals), "isNextDuelSpeedDuel").ToInt32(); }
            }

            public static int OffsetNextDuelHandCount
            {
                get { return Marshal.OffsetOf(typeof(Globals), "NextDuelHandCount").ToInt32(); }
            }

            public static int OffsetLoadBattlePackYdcHookAddress
            {
                get { return Marshal.OffsetOf(typeof(Globals), "LoadBattlePackYdcHookAddress").ToInt32(); }
            }

            public static int OffsetCustomYdcBattlePacksEnabled
            {
                get { return Marshal.OffsetOf(typeof(Globals), "customYdcBattlePacksEnabled").ToInt32(); }
            }

            public static int OffsetBattlePackDecks
            {
                get { return Marshal.OffsetOf(typeof(Globals), "BattlePackDecks").ToInt32(); }
            }

            public static int OffsetDuelSeed
            {
                get { return Marshal.OffsetOf(typeof(Globals), "DuelSeed").ToInt32(); }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RandSeed
        {
            public uint Seed1;
            public uint Seed2;
            public uint Seed3;
            public uint Seed4;

            public static RandSeed Create(int seedSeed)
            {
                return Create(seedSeed, true);
            }

            public static RandSeed Create()
            {
                return Create(0, false);
            }

            private static RandSeed Create(int seedSeed, bool useSeedSeed)
            {
                Random rand = useSeedSeed ? new Random(seedSeed) : new Random();
                RandSeed seed = new RandSeed();
                unchecked
                {
                    // These need to be larger than certain values. See NativeScript.c
                    seed.Seed1 = (uint)rand.Next(5, int.MaxValue);
                    seed.Seed2 = (uint)rand.Next(8, int.MaxValue);
                    seed.Seed3 = (uint)rand.Next(24, int.MaxValue);
                    seed.Seed4 = (uint)rand.Next(176, int.MaxValue);
                }
                return seed;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TimeMultiplierInfo
        {
            // The initial values on first hook
            public long InitialPerformanceCounter;
            public ulong InitialTickCount64;
            public int InitialTickCount;
            public uint InitialTimeGetTime;

            // The offsets which are set every time the multiplier is changed
            public long OffsetPerformanceCounter;
            public ulong OffsetTickCount64;
            public int OffsetTickCount;
            public uint OffsetTimeGetTime;

            public double Multiplier;
            private int enabled;

            public bool Enabled
            {
                get { return enabled != 0; }
                set { enabled = value ? 1 : 0; }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct StructSizeInfo
        {
            public int Self;
            public int Globals;
            public int RandSeed;
            public int TimeMultiplierInfo;
            public int ScreenStateInfo;
            public int ActionState;
            public int ActionElement;
            public int ActionInfo;
            public int DeckEditFilterCards;
            public int CardShopOpenPackInfo;
        }
    }
}
