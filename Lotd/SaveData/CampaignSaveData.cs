using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Lotd
{
    /// <summary>
    /// Represents all campaign save data
    /// </summary>
    public class CampaignSaveData : SaveDataChunk
    {
        /// <summary>
        /// Note that the first item MUST be "Available" or the series buttons aren't clickable
        /// </summary>
        public Dictionary<DuelSeries, Duel[]> DuelsBySeries { get; private set; }

        public const int DuelsPerSeries = 50;

        public CampaignSaveData(GameSaveData owner)
            : base(owner)
        {
            DuelsBySeries = new Dictionary<DuelSeries, Duel[]>();
            DuelsBySeries.Add(DuelSeries.YuGiOh, new Duel[DuelsPerSeries]);
            DuelsBySeries.Add(DuelSeries.YuGiOhGX, new Duel[DuelsPerSeries]);
            DuelsBySeries.Add(DuelSeries.YuGiOh5D, new Duel[DuelsPerSeries]);
            DuelsBySeries.Add(DuelSeries.YuGiOhZEXAL, new Duel[DuelsPerSeries]);
            DuelsBySeries.Add(DuelSeries.YuGiOhARCV, new Duel[DuelsPerSeries]);
            DuelsBySeries.Add(DuelSeries.YuGiOhVRAINS, new Duel[DuelsPerSeries]);

            foreach (KeyValuePair<DuelSeries, Duel[]> seriesDuels in DuelsBySeries)
            {
                for (int i = 0; i < DuelsPerSeries; i++)
                {
                    seriesDuels.Value[i] = new Duel();
                }
            }
        }

        public override void Clear()
        {
            foreach (KeyValuePair<DuelSeries, Duel[]> seriesDuels in DuelsBySeries)
            {
                for (int i = 0; i < DuelsPerSeries; i++)
                {
                    // Note that the first item MUST be "Available" or the series buttons aren't clickable

                    Duel duel = seriesDuels.Value[i];
                    duel.State = i == 0 ? CampaignDuelState.Available : CampaignDuelState.Locked;
                    duel.ReverseDuelState = CampaignDuelState.Locked;
                    duel.Unk1 = 0;
                    duel.Unk2 = 0;
                    duel.Unk3 = 0;
                    duel.Unk4 = 0;
                }
            }
        }

        public override void Load(BinaryReader reader)
        {
            reader.ReadInt32();
            reader.ReadInt32();

            for (int i = 0; i < Constants.GetNumDuelSeries(Version); i++)
            {
                DuelSeries series = IndexToSeries(i);

                Duel[] duels = DuelsBySeries[series];
                for (int j = 0; j < DuelsPerSeries; j++)
                {
                    duels[j].Read(reader);
                    if (j == 0)
                    {
                        reader.ReadInt32();// 0?
                        reader.ReadInt32();// 0?
                    }
                }
            }
        }

        public override void Save(BinaryWriter writer)
        {
            writer.Write(0);// 0?
            writer.Write(1);// 1 on a clean save (2 on first series complete?)

            for (int i = 0; i < Constants.GetNumDuelSeries(Version); i++)
            {
                DuelSeries series = IndexToSeries(i);

                Duel[] duels;
                DuelsBySeries.TryGetValue(series, out duels);

                for (int j = 0; j < DuelsPerSeries; j++)
                {
                    duels[j].Write(writer);
                    if (j == 0)
                    {
                        writer.Write((uint)0);
                        writer.Write((uint)0);
                    }
                }
            }
        }

        private int SeriesToIndex(DuelSeries series)
        {
            return (int)(series);
        }

        private DuelSeries IndexToSeries(int index)
        {
            return (DuelSeries)(index);
        }

        /// <summary>
        /// Represents an individual campaign duel save data
        /// </summary>
        public class Duel
        {
            public CampaignDuelState State { get; set; }
            public CampaignDuelState ReverseDuelState { get; set; }
            public int Unk1 { get; set; }// Some id, not sure which (duel id? char id? deck id?) - can be 0 and still works
            public int Unk2 { get; set; }
            public int Unk3 { get; set; }
            public int Unk4 { get; set; }

            public void Read(BinaryReader reader)
            {
                State = (CampaignDuelState)reader.ReadInt32();
                ReverseDuelState = (CampaignDuelState)reader.ReadInt32();
                Unk1 = reader.ReadInt32();
                Unk2 = reader.ReadInt32();
                Unk3 = reader.ReadInt32();
                Unk4 = reader.ReadInt32();

                //Debug.WriteLine(State + " " + ReverseDuelState + " " + Unk1 + " " + Unk2 + " " + Unk3 + " " + Unk4);
            }

            public void Write(BinaryWriter writer)
            {
                writer.Write((int)State);
                writer.Write((int)ReverseDuelState);
                writer.Write(Unk1);
                writer.Write(Unk2);
                writer.Write(Unk3);
                writer.Write(Unk4);
            }
        }
    }

    public enum CampaignDuelState
    {
        /// <summary>
        /// Locked / unavailable (characters are blacked out, padlock on the duel name, no '!' mark)
        /// </summary>
        Locked = 0,

        /// <summary>
        /// Available (characters are blacked out, '!' mark)
        /// </summary>
        Available = 1,

        /// <summary>
        /// Available and an attempt has been made to complete this duel (character are visible, '!' mark)
        /// </summary>
        AvailableAttempted = 2,

        /// <summary>
        /// Complete (characters are visible, no '!' mark)
        /// </summary>
        Complete = 3,

        /// <summary>
        /// Available (characters are blacked out, no '!' mark)
        /// Note that anything 4+ seems to give the same result
        /// </summary>
        AvailableAlt = 4,
    }
}
