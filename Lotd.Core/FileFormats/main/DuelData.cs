using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Lotd.FileFormats
{
    /// <summary>
    /// Holds information about duels in the campain duels list
    /// </summary>
    public class DuelData : FileData
    {
        static Encoding encoding1 = Encoding.ASCII;
        static Encoding encoding2 = Encoding.Unicode;
        public Dictionary<int, Item> Items { get; private set; }

        public override bool IsLocalized
        {
            get { return true; }
        }

        public DuelData()
        {
            Items = new Dictionary<int, Item>();
        }

        public override void Load(BinaryReader reader, long length, Language language)
        {
            long fileStartPos = reader.BaseStream.Position;

            uint count = (uint)reader.ReadUInt64();
            for (uint i = 0; i < count; i++)
            {                
                int id = reader.ReadInt32();
                DuelSeries series = (DuelSeries)reader.ReadInt32();
                int displayIndex = reader.ReadInt32();
                int playerCharId = reader.ReadInt32();
                int opponentCharId = reader.ReadInt32();
                int playerDeckId = reader.ReadInt32();
                int opponentDeckId = reader.ReadInt32();
                int arenaId = reader.ReadInt32();
                int unk8 = reader.ReadInt32();
                int dlcId = reader.ReadInt32();
                long codeNameOffset = reader.ReadInt64();
                long playerAlternateSkinOffset = reader.ReadInt64();
                long opponentAlternateSkinOffset = reader.ReadInt64();
                long nameOffset = reader.ReadInt64();
                long descriptionOffset = reader.ReadInt64();
                long tipOffset = reader.ReadInt64();

                long tempOffset = reader.BaseStream.Position;

                reader.BaseStream.Position = fileStartPos + codeNameOffset;
                string codeName = reader.ReadNullTerminatedString(encoding1);

                reader.BaseStream.Position = fileStartPos + playerAlternateSkinOffset;
                string playerAlternateSkin = reader.ReadNullTerminatedString(encoding1);

                reader.BaseStream.Position = fileStartPos + opponentAlternateSkinOffset;
                string opponentAlternateSkin = reader.ReadNullTerminatedString(encoding1);

                reader.BaseStream.Position = fileStartPos + nameOffset;
                string name = reader.ReadNullTerminatedString(encoding2);

                reader.BaseStream.Position = fileStartPos + descriptionOffset;
                string description = reader.ReadNullTerminatedString(encoding2);

                reader.BaseStream.Position = fileStartPos + tipOffset;
                string tipStr = reader.ReadNullTerminatedString(encoding2);

                reader.BaseStream.Position = tempOffset;

                Item item;
                if (!Items.TryGetValue(id, out item))
                {
                    item = new Item(id, series, displayIndex, playerCharId, opponentCharId, playerDeckId, opponentDeckId, arenaId, unk8, dlcId);
                    Items.Add(item.Id, item);
                }
                item.CodeName.SetText(language, codeName);
                item.PlayerAlternateSkin.SetText(language, playerAlternateSkin);
                item.OpponentAlternateSkin.SetText(language, opponentAlternateSkin);
                item.Name.SetText(language, name);
                item.Description.SetText(language, description);
                item.Tip.SetText(language, tipStr);
            }
        }

        public override void Save(BinaryWriter writer, Language language)
        {
            int firstChunkItemSize = 88;// Size of each item in the first chunk
            long fileStartPos = writer.BaseStream.Position;

            writer.Write((ulong)Items.Count);

            long offsetsOffset = writer.BaseStream.Position;
            writer.Write(new byte[Items.Count * firstChunkItemSize]);

            int index = 0;
            foreach (Item item in Items.Values)
            {
                int codeNameLen = GetStringSize(item.CodeName.GetText(language), encoding1);
                int playerAlternateSkinLen = GetStringSize(item.PlayerAlternateSkin.GetText(language), encoding1);
                int opponentAlternateSkinLen = GetStringSize(item.OpponentAlternateSkin.GetText(language), encoding1);
                int nameLen = GetStringSize(item.Name.GetText(language), encoding2);
                int descriptionLen = GetStringSize(item.Description.GetText(language), encoding2);
                long tempOffset = writer.BaseStream.Position;

                writer.BaseStream.Position = offsetsOffset + (index * firstChunkItemSize);
                writer.Write(item.Id);
                writer.Write((int)item.Series);
                writer.Write(item.DisplayIndex);
                writer.Write(item.PlayerCharId);
                writer.Write(item.OpponentCharId);
                writer.Write(item.PlayerDeckId);
                writer.Write(item.OpponentDeckId);
                writer.Write(item.ArenaId);
                writer.Write(item.Unk8);
                writer.Write(item.DlcId);
                writer.WriteOffset(fileStartPos, tempOffset);
                writer.WriteOffset(fileStartPos, tempOffset + codeNameLen);
                writer.WriteOffset(fileStartPos, tempOffset + codeNameLen + playerAlternateSkinLen);
                writer.WriteOffset(fileStartPos, tempOffset + codeNameLen + playerAlternateSkinLen + opponentAlternateSkinLen);
                writer.WriteOffset(fileStartPos, tempOffset + codeNameLen + playerAlternateSkinLen + opponentAlternateSkinLen + nameLen);
                writer.WriteOffset(fileStartPos, tempOffset + codeNameLen + playerAlternateSkinLen + opponentAlternateSkinLen + nameLen + descriptionLen);
                writer.BaseStream.Position = tempOffset;

                writer.WriteNullTerminatedString(item.CodeName.GetText(language), encoding1);
                writer.WriteNullTerminatedString(item.PlayerAlternateSkin.GetText(language), encoding1);
                writer.WriteNullTerminatedString(item.OpponentAlternateSkin.GetText(language), encoding1);
                writer.WriteNullTerminatedString(item.Name.GetText(language), encoding2);
                writer.WriteNullTerminatedString(item.Description.GetText(language), encoding2);
                writer.WriteNullTerminatedString(item.Tip.GetText(language), encoding2);

                index++;
            }
        }

        public class Item
        {
            public int Id { get; set; }

            /// <summary>
            /// The series this duel belongs to (Yu-Gi-Oh!, GX, 5D's, ZEXAL, ARC-V)
            /// </summary>
            public DuelSeries Series { get; set; }

            /// <summary>
            /// The index at which this duel will be displayed in the list (unclear what happens if there are duplicates)
            /// Note that this starts at 1 for each series and increments.
            /// Note that in the original data there are some out of order but there aren't any duplicates (GX).
            /// </summary>
            public int DisplayIndex { get; set; }

            /// <summary>
            /// Your character id. This is an id which maps into CharData. Use that structure get to the character info.
            /// </summary>
            public int PlayerCharId { get; set; }

            /// <summary>
            /// Your opponents character id. This is an id which maps into CharData. Use that structure get to the character info.
            /// </summary>
            public int OpponentCharId { get; set; }

            /// <summary>
            /// Your deck id. This is an id which maps into DeckData. Use that structure to get the deck info.
            /// </summary>
            public int PlayerDeckId { get; set; }

            /// <summary>
            /// Your opponents deck id. This is an id which maps into DeckData. Use that structure to get the deck info.
            /// </summary>
            public int OpponentDeckId { get; set; }

            /// <summary>
            /// The arena this duel takes place. This is an id which maps into ArenaData. Use that structure to get the arena info.
            /// </summary>
            public int ArenaId { get; set; }

            public int Unk8 { get; set; }
            public int DlcId { get; set; }

            /// <summary>
            /// The code name for this duel. This is likely used internally. It is usually the duel name with spaces.
            /// e.g. "TheDuelistKingdom", "TheHeartOfTheCards", "TheUltimateGreatMoth"
            /// </summary>
            public LocalizedText CodeName { get; set; }

            /// <summary>
            /// Alternate skin / style for your character.
            /// These can be seen if you open the 'busts' folder and see prefixes before "_neutral"
            /// e.g. "alternate", "barian", "dark", "glasses", "blue", "notattoo"
            /// </summary>
            public LocalizedText PlayerAlternateSkin { get; set; }

            /// <summary>
            /// Alternate skin / style for your opponents character.
            /// </summary>
            public LocalizedText OpponentAlternateSkin { get; set; }

            /// <summary>
            /// The name of the duel. This is the name that is displayed in the campaign duels list.
            /// e.g. "The Duelist Kingdom", "The heart of the Cards", "The Ultimate Great Moth"
            /// </summary>
            public LocalizedText Name { get; set; }

            /// <summary>
            /// The description / reason for the duel (this isn't displayed anywhere in-game?).
            /// Note that some of these are blank, code names ("ARCVDuel_03_1") or the same as the duel name
            /// "Help Yugi Muto explain the rules of Duel Monsters to Joey Wheeler."
            /// "Seto Kaiba defeated Solomon Muto and has destroyed his rare Blue-Eyes White Dragon card!"
            /// "The Duelist Tournament has begun! In the first round it is Yugi Muto versus the underhanded and sneaky Weevil Underwood."
            /// </summary>
            public LocalizedText Description { get; set; }

            /// <summary>
            /// This tip is displayed when you lose the duel (in the blue box on the right - above the rewards)
            /// </summary>
            public LocalizedText Tip { get; set; }

            public Item(int id, DuelSeries series, int displayIndex, int playerCharId, int opponentCharId,
                int playerDeckId, int opponentDeckId, int duelArena, int unk8, int dlcId)
            {                
                Id = id;
                Series = series;
                DisplayIndex = displayIndex;
                PlayerCharId = playerCharId;
                OpponentCharId = opponentCharId;
                PlayerDeckId = playerDeckId;
                OpponentDeckId = opponentDeckId;
                ArenaId = duelArena;
                Unk8 = unk8;
                DlcId = dlcId;
                CodeName = new LocalizedText();
                PlayerAlternateSkin = new LocalizedText();
                OpponentAlternateSkin = new LocalizedText();
                Name = new LocalizedText();
                Description = new LocalizedText();
                Tip = new LocalizedText();
            }

            public override string ToString()
            {
                return "id: " + Id +
                    " series: " + Series + " displayIndex: " + DisplayIndex +
                    " playerCharId: " + PlayerCharId + " opponentCharId: " + OpponentCharId +
                    " playerDeckId: " + PlayerDeckId + " opponentDeckId: " + OpponentDeckId +
                    " arenaId: " + ArenaId + " unk8: " + Unk8 + " dlcId: " + DlcId +
                    
                    " codeName: '" + CodeName + "' playerAlternateSkin: '" + PlayerAlternateSkin +
                    "' opponentAlternateSkin: '" + OpponentAlternateSkin + "' name: '" + Name +
                    "' unkStr5: '" + Description + "' tip: '" + Tip + "'";
            }
        }
    }
}
