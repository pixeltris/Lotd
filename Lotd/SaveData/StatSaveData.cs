using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Lotd
{
    public class StatSaveData : SaveDataChunk
    {
        public Dictionary<StatSaveType, long> Stats { get; private set; }

        static int numStats = 43;

        public StatSaveData()
        {
            Stats = new Dictionary<StatSaveType, long>();
        }

        public override void Clear()
        {
            foreach (StatSaveType statType in Enum.GetValues(typeof(StatSaveType)))
            {
                Stats[statType] = 0;
            }
        }

        public override void Load(BinaryReader reader)
        {
            for (int i = 0; i < numStats; i++)
            {
                Stats[(StatSaveType)i] = reader.ReadInt64();
            }
        }

        public override void Save(BinaryWriter writer)
        {
            for (int i = 0; i < numStats; i++)
            {
                long value;
                Stats.TryGetValue((StatSaveType)i, out value);
                writer.Write(value);
            }
        }
    }

    /// <summary>
    /// The stat type. See SAVESTAT_XXXXX in the native binary
    /// </summary>
    public enum StatSaveType
    {
        Games_Campaign,
        Games_Campaign_Normal,
        Games_Campaign_Reverse,
        Games_Challenge,
        Games_Multiplayer,
        Games_Multiplayer_1V1,
        Games_Multiplayer_Tag,
        Games_Multiplayer_Ranked,
        Games_Multiplayer_Friendly,
        Games_Multiplayer_Battlepack_Any,
        Games_Multiplayer_Battlepack_1,
        Games_Multiplayer_Battlepack_2,
        Games_Multiplayer_Battlepack_3,
        Wins_Campaign,
        Wins_Campaign_Normal,
        Wins_Campaign_Reverse,
        Wins_Challenge,
        Wins_Multiplayer,
        Wins_Multiplayer_1V1,
        Wins_Multiplayer_Tag,
        Wins_Multiplayer_Ranked,
        Wins_Multiplayer_Friendly,
        Wins_Multiplayer_Battlepack_Any,
        Wins_Multiplayer_Battlepack_1,
        Wins_Multiplayer_Battlepack_2,
        Wins_Multiplayer_Battlepack_3,
        Wins_Match,
        Wins_Nonmatch,
        Summons_Normal,
        Summons_Tribute,
        Summons_Ritual,
        Summons_Fusion,
        Summons_Xyz,
        Summons_Synchro,
        Summons_Pendulum,
        Damage_Any,
        Damage_Battle,
        Damage_Direct,
        Damage_Effect,
        Damage_Reflect,
        Chains,
        Decks_Created,

        /// <summary>
        /// There seems to be one additional value.
        /// Is this actually part of the stat data or something seperate?
        /// </summary>
        Unknown
    }
}
