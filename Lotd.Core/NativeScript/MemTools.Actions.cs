using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Lotd
{
    public partial class MemTools
    {
        // Using these addresses is very unsafe without acquiring the lock belonging to "YGO:funcThreadDuel"

        // Address of the current action / target player of that action
        // See YGOfuncThreadDuel.c
        static IntPtr currentActionAddress = (IntPtr)0x1410D7FC0;
        static IntPtr hasActiveActionAddress = (IntPtr)0x141180BF8;
        static IntPtr numQueuedActionsAddress = (IntPtr)0x1410D87C8;
        // Address which holds state information about the current action (some actions don't use this - IDA sees QWORD/HIDWORD/LODWORD)
        static IntPtr currentActionStateAddress1 = (IntPtr)0x1410D87CC;
        static IntPtr currentActionStateAddress2 = (IntPtr)0x1410D87D0;

        private void ReplayActions(string hexStrings)
        {
            string[] lines = hexStrings.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                string hex = line.Replace(" ", string.Empty);
                if (hex.Length % 2 == 0)
                {
                    byte[] buffer = new byte[hex.Length / 2];
                    bool isValidBuffer = true;

                    for (int i = 0; i < buffer.Length; i++)
                    {
                        byte b;
                        if (byte.TryParse(hex.Substring(i * 2, 2), NumberStyles.HexNumber, null, out b))
                        {
                            buffer[i] = b;
                        }
                        else
                        {
                            isValidBuffer = false;
                            break;
                        }
                    }

                    if (isValidBuffer && buffer.Length == 8)
                    {
                        ActionInfo actionInfo = new ActionInfo();
                        actionInfo.Action.Value = BitConverter.ToInt64(buffer, 0);                        
                        
                        LogAction(buffer, ref actionInfo);

                        QueueAction(actionInfo);
                    }
                }
            }
        }

        private void LogAction(ActionInfo actionInfo)
        {
            LogAction(BitConverter.GetBytes(actionInfo.Action.Value), ref actionInfo);
        }

        private void LogAction(byte[] buffer, ref ActionInfo actionInfo)
        {
            string hex = BitConverter.ToString(buffer).Replace("-", " ");

            switch ((ActionId)DecodeActionId(actionInfo.ActionId))
            {
                //case ActionId.XyzSummon:
                case ActionId.SummonMonster:
                case ActionId.UseMagicCard:
                    // Validate our decoder
                    Actions.PlaceCardOnFieldInfo.Validate(actionInfo);

                    Actions.PlaceCardOnFieldInfo info = Actions.PlaceCardOnFieldInfo.FromAction(actionInfo);
                    //info.CardPosition = Actions.CardPosition.Pos7;
                    info.ToAction(ref actionInfo);

                    // Validate our encoder
                    Actions.PlaceCardOnFieldInfo.Validate(info);

                    Console.WriteLine(hex + " - { " + info + " }");

                    break;
                default:
                    Console.WriteLine(hex);
                    break;
            }
        }

        private ushort EncodeActionId(Player target, ushort actionId)
        {
            int targetVal = target == Player.Opponent ? -1 : 0;
            return (ushort)((targetVal << 15) | actionId);
        }

        private void DecodeActionId(ushort value, out Player target, out ushort actionId)
        {
            target = (value >> 15) == -1 ? Player.Opponent : Player.Self;
            actionId = (ushort)(value & 0xFFF);
        }

        private ushort DecodeActionId(ushort value)
        {
            Player target;
            ushort actionId;
            DecodeActionId(value, out target, out actionId);
            return actionId;
        }

        private ushort EncodeActionId(Player target, ActionId actionId)
        {
            return EncodeActionId(target, (ushort)actionId);
        }

        private void DecodeActionId(ushort value, out Player target, out ActionId actionId)
        {
            DecodeActionId(value, out target, out actionId);
        }

        private bool DoAction(ActionInfo actionInfo)
        {
            return CallNativeScriptFunctionWithStruct("DoAction", actionInfo) == CallNativeFunctionResult.Success;
        }

        private bool QueueAction(ActionInfo actionInfo)
        {
            actionInfo.Type = DoActionType.Queue;
            return DoAction(actionInfo);
        }

        public bool InjectAction(ActionInfo actionInfo, int index)
        {
            actionInfo.Type = DoActionType.Inject;
            actionInfo.CustomData = index;
            return DoAction(actionInfo);
        }

        public bool ForceAction(ActionInfo actionInfo)
        {
            return ForceAction(actionInfo, 1);
        }

        public bool ForceAction(ActionInfo actionInfo, int count)
        {
            actionInfo.Type = DoActionType.Forced;
            actionInfo.CustomData = count;
            return DoAction(actionInfo);
        }

        public bool SetCurrentAction(ActionInfo actionInfo)
        {
            actionInfo.Type = DoActionType.Overwrite;
            return DoAction(actionInfo);
        }

        public bool ClearCurrentAction()
        {
            return DoAction(new ActionInfo() { Type = DoActionType.ClearCurrent });
        }

        public bool ClearActionQueue()
        {
            return DoAction(new ActionInfo() { Type = DoActionType.ClearQueue });
        }

        public bool ClearAllActions()
        {
            return DoAction(new ActionInfo() { Type = DoActionType.ClearAll });
        }

        public void SetActionState(int state1, int state2)
        {
            ActionInfo actionInfo = new ActionInfo();

            actionInfo.SetState = true;
            actionInfo.State.State1 = state1;
            actionInfo.State.State2 = state2;

            DoAction(actionInfo);
        }

        //////////////////////////////////////////////////////////////////////////////////
        // Blocking / unblocking action and animation ids to avoid long animations
        // - These use function hooks and may potentially cause problems
        //////////////////////////////////////////////////////////////////////////////////

        public void UnblockActionIds(params ActionId[] actionIds)
        {
            BlockUnblockActionIds(actionIds, false);
        }

        public void BlockActionIds(params ActionId[] actionIds)
        {
            BlockUnblockActionIds(actionIds, true);
        }

        public void UnblockActionIds(params int[] actionIds)
        {
            BlockUnblockActionIds(actionIds, false);
        }

        public void BlockActionIds(params int[] actionIds)
        {
            BlockUnblockActionIds(actionIds, true);
        }

        private void BlockUnblockActionIds(ActionId[] actionIds, bool block)
        {
            int[] ids = new int[actionIds.Length];
            for (int i = 0; i < ids.Length; i++)
            {
                ids[i] = (byte)actionIds[i];
            }
            BlockUnblockActionIds(ids, block);
        }

        private void BlockUnblockActionIds(int[] actionIds, bool block)
        {
            if (!UpdateState() || actionIds == null)
            {
                return;
            }

            NativeScript.Globals globals = ReadValue<NativeScript.Globals>(nativeScriptGlobalsAddress);
            byte[] buffer = globals.BlockedActionIds.ToArray();

            foreach (byte actionId in actionIds)
            {
                if (actionId < buffer.Length)
                {
                    buffer[actionId] = (byte)(block ? 1 : 0);
                }
            }

            IntPtr written;
            WriteProcessMemoryEx(ProcessHandle, nativeScriptGlobalsAddress + NativeScript.Globals.OffsetBlockedActionIds,
                buffer, (IntPtr)buffer.Length, out written);
        }

        public void ClearBlockedActionIds()
        {
            NativeScript.Globals globals = ReadValue<NativeScript.Globals>(nativeScriptGlobalsAddress);
            byte[] blockedActionIds = globals.BlockedActionIds;
            if (blockedActionIds == null)
            {
                return;
            }

            byte[] buffer = new byte[globals.BlockedActionIds.Length];

            IntPtr written;
            WriteProcessMemoryEx(ProcessHandle, nativeScriptGlobalsAddress + NativeScript.Globals.OffsetBlockedActionIds,
                buffer, (IntPtr)buffer.Length, out written);
        }

        public ActionId[] GetBlockedActionIds()
        {
            List<ActionId> result = new List<ActionId>();
            if (!UpdateState())
            {
                return result.ToArray();
            }

            NativeScript.Globals globals = ReadValue<NativeScript.Globals>(nativeScriptGlobalsAddress);
            for (int i = 0; i < globals.BlockedActionIds.Length; i++)
            {
                if (globals.BlockedActionIds[i] != 0)
                {
                    result.Add((ActionId)i);
                }
            }

            return result.ToArray();
        }

        public void UnblockAnimationIds(params AnimationId[] animationIds)
        {
            BlockUnblockAnimationIds(animationIds, false);
        }

        public void BlockAnimationIds(params AnimationId[] animationIds)
        {
            BlockUnblockAnimationIds(animationIds, true);
        }

        public void UnblockAnimationIds(params int[] animationIds)
        {
            BlockUnblockAnimationIds(animationIds, false);
        }

        public void BlockAnimationIds(params int[] animationIds)
        {
            BlockUnblockAnimationIds(animationIds, true);
        }

        private void BlockUnblockAnimationIds(AnimationId[] animationIds, bool block)
        {
            int[] ids = new int[animationIds.Length];
            for (int i = 0; i < ids.Length; i++)
            {
                ids[i] = (int)animationIds[i];
            }
            BlockUnblockAnimationIds(ids, block);
        }

        private void BlockUnblockAnimationIds(int[] animationIds, bool block)
        {
            if (!UpdateState() || animationIds == null)
            {
                return;
            }

            NativeScript.Globals globals = ReadValue<NativeScript.Globals>(nativeScriptGlobalsAddress);
            byte[] buffer = globals.BlockedAnimationIds.ToArray();

            foreach (byte animationId in animationIds)
            {
                if (animationId < buffer.Length)
                {
                    buffer[animationId] = (byte)(block ? 1 : 0);
                }
            }
            
            IntPtr written;
            WriteProcessMemoryEx(ProcessHandle, nativeScriptGlobalsAddress + NativeScript.Globals.OffsetBlockedAnimationIds,
                buffer, (IntPtr)buffer.Length, out written);
        }

        public void ClearBlockedAnimationIds()
        {
            NativeScript.Globals globals = ReadValue<NativeScript.Globals>(nativeScriptGlobalsAddress);
            byte[] blockedAnimationIds = globals.BlockedAnimationIds;
            if (blockedAnimationIds == null)
            {
                return;
            }

            byte[] buffer = new byte[globals.BlockedAnimationIds.Length];

            IntPtr written;
            WriteProcessMemoryEx(ProcessHandle, nativeScriptGlobalsAddress + NativeScript.Globals.OffsetBlockedAnimationIds,
                buffer, (IntPtr)buffer.Length, out written);
        }

        public byte[] GetBlockedAnimationIds()
        {
            List<byte> result = new List<byte>();
            if (!UpdateState())
            {
                return result.ToArray();
            }

            NativeScript.Globals globals = ReadValue<NativeScript.Globals>(nativeScriptGlobalsAddress);
            for (int i = 0; i < globals.BlockedAnimationIds.Length; i++)
            {
                if (globals.BlockedAnimationIds[i] != 0)
                {
                    result.Add((byte)i);
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// Gets the currently active animation id
        /// </summary>
        /// <returns></returns>
        public int GetCurrentAnimationId()
        {
            IntPtr animationIdAddress = nativeScriptGlobalsAddress + NativeScript.Globals.OffsetCurrentAnimationId;
            return ReadValue<int>(animationIdAddress);
        }

        /// <summary>
        /// Gets the currently active action id
        /// </summary>
        public ushort GetCurrentActionId()
        {
            ushort currentActionId = ReadValue<ushort>(currentActionAddress);
            return DecodeActionId(currentActionId);
        }

        ////////////////////////////////////////////////////////////////////////////////////
        // Unsafe functions (using read/write memory instead of gaining thread control)
        ////////////////////////////////////////////////////////////////////////////////////

        public bool IsActionActive()
        {
            return ReadValue<int>(hasActiveActionAddress) != 0;
        }

        public int GetNumQueuedActions()
        {
            return ReadValue<int>(numQueuedActionsAddress);
        }

        /// <summary>
        /// Sets the action state for the currently active action
        /// 0=stopped? 1=active? 2=complete?
        /// </summary>
        public void SetActionStateUnsafe(int state1, int state2)
        {
            WriteValue(currentActionStateAddress1, state1);
            WriteValue(currentActionStateAddress2, state2);
        }

        /// <summary>
        /// Sets the currently active action (note that this will kill any action which is currently active)
        /// </summary>
        public void SetActionUnsafe(Player target, ushort action)
        {
            SetActionUnsafe(target, action, 0, 0, 0);
        }

        public void SetActionUnsafe(Player target, ushort action, ushort data1, ushort data2, ushort data3)
        {
            ushort encodedAction = EncodeActionId(target, action);

            byte[] buffer = new byte[8];
            Buffer.BlockCopy(BitConverter.GetBytes(encodedAction), 0, buffer, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(data1), 0, buffer, 2, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(data2), 0, buffer, 4, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(data3), 0, buffer, 6, 2);
            WriteBytes(currentActionAddress, buffer);

            WriteValue(hasActiveActionAddress, 1);
        }

        public void SetActionUnsafe(Player target, ushort action, byte[] data)
        {
            Debug.Assert(data.Length == 6);

            ushort encodedAction = EncodeActionId(target, action);

            byte[] buffer = new byte[8];
            Buffer.BlockCopy(BitConverter.GetBytes(encodedAction), 0, buffer, 0, 2);
            Buffer.BlockCopy(data, 0, buffer, 2, 6);
            WriteBytes(currentActionAddress, buffer);

            WriteValue(hasActiveActionAddress, 1);
        }

        /// <summary>
        /// Clear the currently active action
        /// </summary>
        public void ClearActionUnsafe()
        {
            WriteValue(hasActiveActionAddress, 0);
        }

        /// <summary>
        /// Clears the action queue (but not the currently active action)
        /// </summary>
        public void ClearActionQueueUnsafe()
        {
            WriteValue(numQueuedActionsAddress, 0);
        }

        /// <summary>
        /// Clear the currently active action and the action queue
        /// </summary>
        public void ClearAllActionsUnsafe()
        {
            ClearActionQueueUnsafe();
            ClearActionUnsafe();
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////
    // Custom structures used for modifying the action queue safetly
    ////////////////////////////////////////////////////////////////////////////////////

    [StructLayout(LayoutKind.Sequential)]
    public struct ActionState
    {
        public int State1;
        public int State2;
    }

    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public struct ActionElement
    {
        [FieldOffset(0)]
        public ushort ActionId;

        [FieldOffset(2)]
        public ushort ActionData1;
        [FieldOffset(4)]
        public ushort ActionData2;
        [FieldOffset(6)]
        public ushort ActionData3;

        [FieldOffset(2)]
        public byte ActionDataByte1;
        [FieldOffset(3)]
        public byte ActionDataByte2;
        [FieldOffset(4)]
        public byte ActionDataByte3;
        [FieldOffset(5)]
        public byte ActionDataByte4;
        [FieldOffset(6)]
        public byte ActionDataByte5;
        [FieldOffset(7)]
        public byte ActionDataByte6;

        [FieldOffset(0)]
        public long Value;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ActionInfo
    {
        // Should be bool / MarshalAs(I4) but there are some issues with the marshaler
        private int setState;

        public bool SetState
        {
            get { return setState != 0; }
            set { setState = value ? 1 : 0; }
        }

        public ActionState State;
        public ActionElement Action;

        public DoActionType Type;

        /// <summary>
        /// Count for DoActionType.Forced
        /// InjectIndex for DoActionType.Inject
        /// </summary>
        public int CustomData;

        public int State1
        {
            get { return State.State1; }
            set { State.State1 = value; }
        }
        public int State2
        {
            get { return State.State2; }
            set { State.State2 = value; }
        }

        public ushort ActionId
        {
            get { return Action.ActionId; }
            set { Action.ActionId = value; }
        }
        public byte[] ActionData
        {
            get { return BitConverter.GetBytes(Action.Value); }
            set { Action.Value = BitConverter.ToInt64(value, 0); }
        }
        public ushort ActionData1
        {
            get { return Action.ActionData1; }
            set { Action.ActionData1 = value; }
        }
        public ushort ActionData2
        {
            get { return Action.ActionData2; }
            set { Action.ActionData2 = value; }
        }
        public ushort ActionData3
        {
            get { return Action.ActionData3; }
            set { Action.ActionData3 = value; }
        }
        public byte ActionDataByte1
        {
            get { return Action.ActionDataByte1; }
            set { Action.ActionDataByte1 = value; }
        }
        public byte ActionDataByte2
        {
            get { return Action.ActionDataByte2; }
            set { Action.ActionDataByte2 = value; }
        }
        public byte ActionDataByte3
        {
            get { return Action.ActionDataByte3; }
            set { Action.ActionDataByte3 = value; }
        }
        public byte ActionDataByte4
        {
            get { return Action.ActionDataByte4; }
            set { Action.ActionDataByte4 = value; }
        }
        public byte ActionDataByte5
        {
            get { return Action.ActionDataByte5; }
            set { Action.ActionDataByte5 = value; }
        }
        public byte ActionDataByte6
        {
            get { return Action.ActionDataByte6; }
            set { Action.ActionDataByte6 = value; }
        }
    }

    public enum DoActionType : int
    {
        None,
        Queue,
        Overwrite,
        Inject,
        Forced,
        ClearCurrent,
        ClearQueue,
        ClearAll,
        InitHooks
    }

    public class Actions
    {
        public struct PlaceCardOnFieldInfo
        {
            public ushort CardId;

            private ushort owner;
            private ushort controller;
            private ushort cardPosition;

            /// <summary>
            /// The target card / field slot
            /// </summary>
            public ushort TargetSlot;            

            public ushort Unk2;
            public ushort CardObjectId;// The object id for this card (every card will have its own internal id for this game)

            /// <summary>
            /// Who will control the card / whose side of the field the card will go
            /// </summary>
            public MemTools.Player Controller
            {
                get { return (MemTools.Player)controller; }
                set { controller = (byte)(value == MemTools.Player.Self ? 0 : 1); }
            }

            /// <summary>
            /// Who this card belongs to
            /// </summary>            
            public MemTools.Player Owner
            {
                get { return (MemTools.Player)owner; }
                set { owner = (byte)(value == MemTools.Player.Self ? 0 : 1); }
            }
            
            /// <summary>
            /// How the card is positioned (face up, face down, face up defense, etc)
            /// </summary>
            public CardPosition CardPosition
            {
                get { return (CardPosition)cardPosition; }
                set { cardPosition = (ushort)value; }
            }

            // ActionData2:
            // xxxxxxxxxxxxxxx1 - controller (offset:0 bits:1 mask:0x1)
            // xxxxxxxxxx11111x - targetSlot (offset:1 bits:5 mask:0x1F)
            // xxxx111111xxxxxx - ? (offset:6 bits:6 mask:0x3F)
            // xxx1xxxxxxxxxxxx - owner (offset:7 bits:1 mask:0x1)
            // 111xxxxxxxxxxxxx - cardPosition (offset:13 bits:3 mask:0x7)

            // ActionData3:
            // xxxxxxxxxxxxxxx1 - ? (offset:0 bits:1 mask:0x1)
            // 111111111111111x - cardObjectId (offset:1 bits:15 mask:0x7FF)

            // Decoder / action handler can be found in sub_14049B350
            public static PlaceCardOnFieldInfo FromAction(ActionInfo actionInfo)
            {
                PlaceCardOnFieldInfo result = new PlaceCardOnFieldInfo();

                result.CardId = actionInfo.ActionData1;

                result.controller = (ushort)(actionInfo.ActionData2 & 1);
                result.TargetSlot = (ushort)((actionInfo.ActionData2 >> 1) & 0x1F);
                result.owner = (ushort)(actionInfo.ActionData2 >> 12);
                result.cardPosition = (ushort)(actionInfo.ActionData2 >> 13);

                result.Unk2 = (ushort)(actionInfo.ActionData3 & 1);
                result.CardObjectId = (ushort)(actionInfo.ActionData3 >> 1);

                return result;
            }

            // Encoder / action creator can be found sub_140101280 (search "v53 = 81;")
            public void ToAction(ref ActionInfo actionInfo)
            {
                actionInfo.ActionData1 = 0;
                actionInfo.ActionData2 = 0;
                actionInfo.ActionData3 = 0;

                actionInfo.ActionData1 = CardId;
                
                actionInfo.ActionData2 |= (ushort)(controller & 1);
                actionInfo.ActionData2 |= (ushort)((TargetSlot & 0x1F) << 1);
                actionInfo.ActionData2 |= (ushort)(owner << 12);
                actionInfo.ActionData2 |= (ushort)(cardPosition << 13);

                actionInfo.ActionData3 |= (ushort)(Unk2 & 1);
                actionInfo.ActionData3 |= (ushort)(CardObjectId << 1);
            }

            public static void Validate(PlaceCardOnFieldInfo info)
            {
                ActionInfo actionInfo = new ActionInfo();
                info.ToAction(ref actionInfo);
                ValidateUnknownBits(actionInfo);

                PlaceCardOnFieldInfo newInfo = FromAction(actionInfo);
                Debug.Assert(newInfo.Equals(info));
            }

            public static void Validate(ActionInfo actionInfo)
            {
                ValidateUnknownBits(actionInfo);

                ActionInfo oldAction = actionInfo;
                PlaceCardOnFieldInfo info = FromAction(actionInfo);
                info.ToAction(ref actionInfo);
                Debug.Assert(oldAction.Equals(actionInfo));
            }

            private static void ValidateUnknownBits(ActionInfo actionInfo)
            {
                // These bits seem to be used, validate that they are 0
                int unknownBits = (actionInfo.ActionData2 >> 6) & 0x3F;
                Debug.Assert(unknownBits == 0);
                if (unknownBits != 0)
                {
                    Console.WriteLine("Unknown bits: " + unknownBits + " - " +
                        BitConverter.ToString(actionInfo.ActionData).Replace("-", " "));
                }
            }

            public override string ToString()
            {
                return "cardId: " + CardId + " controller: " + Controller + " targetSlot: " + TargetSlot +
                    " cardPosition: " + CardPosition + " unk2: " + Unk2 + " cardObjectId: " + CardObjectId;
            }
        }

        // Limit of 7 positions defined by the bit masks
        public enum CardPosition
        {
            FaceDown = 0,
            FaceDownWithFlipEffect = 1,
            FaceUp = 2,
            Pos3 = 3,// Face up
            FaceDownDefense = 4,
            FaceUpDefense = 5,
            Pos6 = 6,// Face up defense
            Pos7 = 7,// Face up defense
        }
    }

    public enum ActionId : ushort
    {
        TurnChange = 4,// 04 00 00 00 00 00 00 00 - animation only (or requires a mouse click?)
        MainPhase = 0x0C,// 0C 00 00 00 00 00 00 00
        EndPhase = 0x0F,// 0F 00 00 00 00 00 00 00

        /// <summary>
        /// Attaking and taking damage from that attack
        /// </summary>
        Attack = 0x24,// 24 00 [A4 06] [01 00] [B8 25] - [lpDamage] [?] [attackerCardId]

        /// <summary>
        /// Directly sets a players life points to a specific value
        /// </summary>
        SetLifePoints = 0x25,// 25 00 [40 1F] [40 1F] [01 00] - [LP] [LP] [shouldTargetBothPlayers]

        /// <summary>
        /// The 3d model animation attack used for some cards e.g. dark magician
        /// </summary>
        Phyre3dAttackAnimation = 0x31,

        DrawCard = 0x57,// 57 00 [FF FF] [04 00] [00 00] - [FFFF=startingDraw?] [numCardsToDraw] [?]

        /// <summary>
        /// Used by both XYZ and pendulum summons
        /// </summary>
        XyzOrPendulumSummon = 0x4F,

        SummonMonster = 0x51,// 51 00 [F4 16] [04 40] [09 00] - [CardId] [ownerControllerSlotPos] [cardObjectId]
        UseMagicCard = 0x52,// 51 00 [F4 16] [04 40] [09 00] - [CardId] [ownerControllerSlotPos] [cardObjectId]
    }

    // Duel Links calls this "ViewType"
    public enum AnimationId
    {
        ///// <summary>
        ///// Used by both XYZ and pendulum summons
        ///// </summary>
        //XyzOrPendulumSummon = 59,

        //Phyre3dAttackAnimation = 33

        Null = 0,
        DuelStart = 1,
        DuelEnd = 2,
        WaitFrame = 3,
        WaitInput = 4,
        PhaseChange = 5,
        TurnChange = 6,
        FieldChange = 7,
        CursorSet = 8,
        BgmUpdate = 9,
        BattleInit = 10,
        BattleSelect = 11,
        BattleAttack = 12,
        BattleRun = 13,
        BattleEnd = 14,
        LifeSet = 15,
        LifeDamage = 16,
        LifeReset = 17,
        HandShuffle = 18,
        HandShow = 19,
        HandOpen = 20,
        DeckShuffle = 21,
        DeckReset = 22,
        DeckFlipTop = 23,
        GraveTop = 24,
        CardLockon = 25,
        CardMove = 26,
        CardSwap = 27,
        CardFlipTurn = 28,
        CardCheat = 29,
        CardSet = 30,
        CardVanish = 31,
        CardBreak = 32,
        CardExplosion = 33,
        CardExclude = 34,
        CardHappen = 35,
        CardDisable = 36,
        CardEquip = 37,
        CardIncTurn = 38,
        CardUpdate = 39,
        ManaSet = 40,
        MonstDeathTurn = 41,
        MonstShuffle = 42,
        TributeSet = 43,
        TributeReset = 44,
        TributeRun = 45,
        MaterialSet = 46,
        MaterialReset = 47,
        MaterialRun = 48,
        TuningSet = 49,
        TuningReset = 50,
        TuningRun = 51,
        ChainSet = 52,
        ChainRun = 53,
        RunSurrender = 54,
        RunDialog = 55,
        RunList = 56,
        RunSummon = 57,
        RunSpSummon = 58,
        RunFusion = 59,
        RunDetail = 60,
        RunCoin = 61,
        RunDice = 62,
        RunYujyo = 63,
        RunSpecialWin = 64,
        RunVija = 65,
        RunExtra = 66,
        RunCommand = 67,
        CutinDraw = 68,
        CutinSummon = 69,
        CutinFusion = 70,
        CutinChain = 71,
        CutinActivate = 72,
        CutinSet = 73,
        CutinReverse = 74,
        CutinTurn = 75,
        CutinFlip = 76,
        CutinTurnEnd = 77,
        CutinDamage = 78,
        CutinBreak = 79,
        CpuThinking = 80,
        HandRundom = 81,
        OverlaySet = 82,
        OverlayReset = 83,
        OverlayRun = 84,
        CutinSuccess = 85,
        ChainEnd = 86,
        LinkSet = 87,
        LinkReset = 88,
        LinkRun = 89,
        RunJanken = 90,
        CutinCoinDice = 91
    }
}
