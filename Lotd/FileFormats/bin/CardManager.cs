using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Lotd.FileFormats
{
    // TODO: Split some of this into data files

    public class CardManager
    {
        public Manager Manager { get; private set; }
        public Dictionary<short, CardInfo> Cards { get; private set; }
        public List<CardInfo> CardsByIndex { get; private set; }

        // Card name types / archetypes
        public Dictionary<CardNameType, HashSet<short>> CardNameTypes { get; private set; }

        // Tags used for finding related cards which is used to display related cards on the left/right panels in deck edit
        public List<CardTagInfo> Tags { get; private set; }

        private Dictionary<Language, Dictionary<string, CardInfo>> cardsByName;

        public CardManager(Manager manager)
        {
            Manager = manager;
            Cards = new Dictionary<short, CardInfo>();
            CardsByIndex = new List<CardInfo>();
            Tags = new List<CardTagInfo>();
            cardsByName = new Dictionary<Language, Dictionary<string, CardInfo>>();
            CardNameTypes = new Dictionary<CardNameType, HashSet<short>>();
        }

        public CardInfo FindCardByName(Language language, string name)
        {
            CardInfo cardInfo;
            cardsByName[language].TryGetValue(name, out cardInfo);
            return cardInfo;
        }

        public void Load()
        {
            Cards.Clear();
            CardsByIndex.Clear();
            cardsByName.Clear();
            Tags.Clear();

            LotdArchive archive = Manager.Archive;

            Dictionary<Language, byte[]> indx = archive.LoadLocalizedBuffer("CARD_Indx_", true);
            Dictionary<Language, byte[]> names = archive.LoadLocalizedBuffer("CARD_Name_", true);
            Dictionary<Language, byte[]> descriptions = archive.LoadLocalizedBuffer("CARD_Desc_", true);
            Dictionary<Language, byte[]> taginfos = archive.LoadLocalizedBuffer("taginfo_", true);

            List<ZibFile> allCardImages = new List<ZibFile>();
            List<string> imageFiles = new List<string>();
            switch (Manager.Version)
            {
                case GameVersion.Lotd:
                    imageFiles.Add("cardcropHD400.jpg.zib");
                    imageFiles.Add("cardcropHD401.jpg.zib");
                    break;
                case GameVersion.LinkEvolution2:
                    imageFiles.Add("2020.full.illust_a.jpg.zib");
                    imageFiles.Add("2020.full.illust_j.jpg.zib");
                    break;
            }
            foreach (string imageFile in imageFiles)
            {
                allCardImages.AddRange(archive.Root.FindFile(imageFile).LoadData<ZibData>().Files.Values);
            }

            Dictionary<short, ZibFile> cardImagesById = new Dictionary<short, ZibFile>();
            foreach (ZibFile file in allCardImages)
            {
                short cardId = short.Parse(file.FileName.Substring(0, file.FileName.IndexOf('.')));
                cardImagesById[cardId] = file;
            }

            List<CardInfo> cards = new List<CardInfo>();
            foreach (Language language in Enum.GetValues(typeof(Language)))
            {                
                if (language != Language.Unknown)
                {
                    LoadCardNamesAndDescriptions(language, cards, indx, names, descriptions);

                    Dictionary<string, CardInfo> languageCardsByName = new Dictionary<string, CardInfo>();
                    cardsByName.Add(language, languageCardsByName);
                    foreach (CardInfo card in cards)
                    {
                        // This will wipe over cards with conflicting names
                        languageCardsByName[card.Name.GetText(language)] = card;
                    }
                }
            }
            CardsByIndex.AddRange(cards);

            // Load card props (card id, atk, def, level, attribute, etc)
            LoadCardProps(cards, Cards, cardImagesById);

            ProcessLimitedCardList(Cards);
            LoadCardGenre(cards);
            LoadRelatedCards(cards, Cards, Tags, taginfos);
            LoadCardNameTypes(Cards, CardNameTypes);

            //PrintLimitedCardList();
        }

        /// <summary>
        /// Loads the card name types / archetypes (e.g. "Harpie")
        /// </summary>
        private void LoadCardNameTypes(Dictionary<short, CardInfo> cards, Dictionary<CardNameType, HashSet<short>> cardNameTypes)
        {
            using (BinaryReader reader = new BinaryReader(new MemoryStream(Manager.Archive.Root.FindFile("bin/CARD_Named.bin").LoadBuffer())))
            {
                ushort numArchetypes = reader.ReadUInt16();
                ushort numCards = reader.ReadUInt16();

                long cardsStartOffset = 4 + (numArchetypes * 4);
                long cardsEndOffset = cardsStartOffset + (numCards * 2);
                Debug.Assert(reader.BaseStream.Length == cardsEndOffset);
                
                for (int i = 0; i < numArchetypes; i++)
                {
                    int offset = reader.ReadInt16();// The offset of the cards for this named group (starts at 0)
                    int count = reader.ReadInt16();// The number of cards for this named group
                    HashSet<short> cardIds = new HashSet<short>();
                    cardNameTypes.Add((CardNameType)i, cardIds);

                    long tempOffset = reader.BaseStream.Position;
                    reader.BaseStream.Position = cardsStartOffset + (offset * 2);
                    for (int j = 0; j < count; j++)
                    {
                        short cardId = reader.ReadInt16();
                        Cards[cardId].NameTypes.Add((CardNameType)i);
                        cardIds.Add(cardId);
                    }

                    reader.BaseStream.Position = tempOffset;
                }
            }
        }

        private void LoadCardGenre(List<CardInfo> cards)
        {
            using (BinaryReader reader = new BinaryReader(new MemoryStream(Manager.Archive.Root.FindFile("bin/CARD_Genre.bin").LoadBuffer())))
            {
                for (int i = 0; i < cards.Count; i++)
                {
                    CardInfo card = cards[i];
                    card.Genre = (CardGenre)reader.ReadUInt64();
                }
            }
        }

        private void LoadCardProps(List<CardInfo> cards, Dictionary<short, CardInfo> cardsById, Dictionary<short, ZibFile> cardImagesById)
        {
            using (BinaryReader reader = new BinaryReader(new MemoryStream(Manager.Archive.Root.FindFile("bin/CARD_Prop.bin").LoadBuffer())))
            {
                for (int i = 0; i < cards.Count; i++)
                {
                    CardInfo card = cards[i];
                    LoadCardProp(card, cardsById, reader.ReadUInt32(), reader.ReadUInt32());
                    if (card.CardId > 0)
                    {
                        card.ImageFile = cardImagesById[card.CardId];
                    }
                }
            }
        }

        private void LoadCardProp(CardInfo card, Dictionary<short, CardInfo> cardsById, uint a1, uint a2)
        {
            uint first = (a1 << 18) | ((a1 & 0x7FC000 | a1 >> 18) >> 5);

            uint second = (((a2 & 1u) | (a2 << 21)) & 0x80000001 | ((a2 & 0x7800) | ((a2 & 0x780 | ((a2 & 0x7E) << 10)) << 8)) << 6 |
                ((a2 & 0x38000 | ((a2 & 0x7C0000 | ((a2 & 0x7800000 | (a2 >> 8) & 0x780000) >> 9)) >> 8)) >> 1));

            short cardId = (short)((first >> 18) & 0x3FFF);
            uint atk = ((first >> 9) & 0x1FF);
            uint def = (first & 0x1FF);
            CardType cardType = (CardType)((second >> 25) & 0x3F);
            CardAttribute attribute = (CardAttribute)((second >> 21) & 0xF);
            uint level = (second >> 17) & 0xF;
            SpellType spellType = (SpellType)((second >> 14) & 7);
            MonsterType monsterType = (MonsterType)((second >> 9) & 0x1F);
            uint pendulumScale1 = (second >> 1) & 0xF;
            uint pendulumScale2 = (second >> 5) & 0xF;

            card.CardId = cardId;
            card.Atk = (int)(atk * 10);
            card.Def = (int)(def * 10);
            card.Level = (byte)level;
            card.Attribute = attribute;
            card.CardType = cardType;
            card.SpellType = spellType;
            card.MonsterType = monsterType;
            card.PendulumScale1 = (byte)pendulumScale1;
            card.PendulumScale2 = (byte)pendulumScale2;

            cardsById.Add(cardId, card);

            // This is a hard coded value in native code. Might as well do the same check here.
            Debug.Assert(cardId < Constants.MaxCardId + 1);

            if (!Enum.IsDefined(typeof(MonsterType), monsterType) ||
                !Enum.IsDefined(typeof(SpellType), spellType) ||
                !Enum.IsDefined(typeof(CardType), cardType) ||
                !Enum.IsDefined(typeof(CardAttribute), attribute))
            {
                //Debug.Assert(false);// TODO: Update for LE
            }
        }

        private void LoadCardNamesAndDescriptions(Language language, List<CardInfo> cards,
            Dictionary<Language, byte[]> indxByLanguage,
            Dictionary<Language, byte[]> namesByLanguage,
            Dictionary<Language, byte[]> descriptionsByLanguage)
        {
            if (language == Language.Unknown)
            {
                return;
            }

            byte[] indx = indxByLanguage[language];
            byte[] names = namesByLanguage[language];
            byte[] descriptions = descriptionsByLanguage[language];

            using (BinaryReader indxReader = new BinaryReader(new MemoryStream(indx)))
            using (BinaryReader namesReader = new BinaryReader(new MemoryStream(names)))
            using (BinaryReader descriptionsReader = new BinaryReader(new MemoryStream(descriptions)))
            {
                Dictionary<uint, string> namesByOffset = ReadStrings(namesReader);
                Dictionary<uint, string> descriptionsByOffset = ReadStrings(descriptionsReader);
                
                int index = 0;
                while (true)
                {
                    uint nameOffset = indxReader.ReadUInt32();
                    uint descriptionOffset = indxReader.ReadUInt32();

                    if (indxReader.BaseStream.Position >= indxReader.BaseStream.Length)
                    {
                        // The last index points to an invalid offset
                        break;
                    }

                    CardInfo card = null;
                    if (cards.Count > index)
                    {
                        card = cards[index];
                    }
                    else
                    {
                        cards.Add(card = new CardInfo(index));
                    }
                    
                    card.Name.SetText(language, namesByOffset[nameOffset]);
                    card.Description.SetText(language, descriptionsByOffset[descriptionOffset]);

                    index++;
                }
            }
        }

        private Dictionary<uint, string> ReadStrings(BinaryReader reader)
        {
            Dictionary<uint, string> result = new Dictionary<uint, string>();
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                uint offset = (uint)reader.BaseStream.Position;
                string name = reader.ReadNullTerminatedString(Encoding.Unicode);
                result.Add(offset, name);
            }
            return result;
        }

        private void LoadRelatedCards(List<CardInfo> cards, Dictionary<short, CardInfo> cardsByCardId, List<CardTagInfo> tags,
            Dictionary<Language, byte[]> taginfos)
        {
            foreach (Language language in Enum.GetValues(typeof(Language)))
            {
                if (language == Language.Unknown)
                {
                    continue;
                }

                using (BinaryReader reader = new BinaryReader(new MemoryStream(taginfos[language])))
                {
                    int count = reader.ReadInt32();
                    for (int i = 0; i < count; i++)
                    {
                        CardTagInfo tagInfo = null;
                        if (i >= tags.Count)
                        {
                            tagInfo = new CardTagInfo();
                            tags.Add(tagInfo);
                        }
                        else
                        {
                            tagInfo = tags[i];
                        }
                        
                        tagInfo.Index = i;
                        tagInfo.MainType = (CardTagInfo.Type)reader.ReadInt16();
                        tagInfo.MainValue = reader.ReadInt16();
                        for (int j = 0; j < tagInfo.Elements.Length; j++)
                        {
                            tagInfo.Elements[j].Type = (CardTagInfo.ElementType)reader.ReadInt16();
                            tagInfo.Elements[j].Value = reader.ReadInt16();
                        }
                        long stringOffset1 = reader.ReadInt64();
                        long stringOffset2 = reader.ReadInt64();

                        long tempOffset = reader.BaseStream.Position;

                        reader.BaseStream.Position = stringOffset1;
                        tagInfo.Text.SetText(language, reader.ReadNullTerminatedString(Encoding.Unicode));

                        reader.BaseStream.Position = stringOffset2;
                        tagInfo.DisplayText.SetText(language, reader.ReadNullTerminatedString(Encoding.Unicode));

                        reader.BaseStream.Position = tempOffset;
                    }
                }
            }

            using (BinaryReader reader = new BinaryReader(new MemoryStream(Manager.Archive.Root.FindFile("bin/tagdata.bin").LoadBuffer())))
            {
                long dataStart = reader.BaseStream.Position + (cards.Count * 8);
                
                for (int i = 0; i < cards.Count; i++)
                {
                    uint shortoffset = reader.ReadUInt32();
                    uint tagCount = reader.ReadUInt32();

                    long tempOffset = reader.BaseStream.Position;

                    long start = dataStart + (shortoffset * 4);
                    reader.BaseStream.Position = start;
                    if (tagCount > 0 && i >= 0)
                    {
                        CardInfo card = cards[i];
                        card.RelatedCards.Clear();
                        for (int j = 0; j < tagCount; j++)
                        {
                            card.RelatedCards.Add(new RelatedCardInfo(cardsByCardId[reader.ReadInt16()], Tags[reader.ReadInt16()]));
                        }
                    }

                    reader.BaseStream.Position = tempOffset;
                }
            }

            CardTagInfo.Type[] knownMainTagTypes = (CardTagInfo.Type[])Enum.GetValues(typeof(CardTagInfo.Type));
            CardTagInfo.ElementType[] knownElementTagTypes = (CardTagInfo.ElementType[])Enum.GetValues(typeof(CardTagInfo.ElementType));
            foreach (CardTagInfo tag in tags)
            {
                Debug.Assert(knownMainTagTypes.Contains(tag.MainType));
                Debug.Assert(tag.MainValue <= 1);

                foreach (CardTagInfo.Element element in tag.Elements)
                {
                    if (element.Type == CardTagInfo.ElementType.None)
                    {
                        continue;
                    }

                    //Debug.Assert(knownElementTagTypes.Contains(element.Type));// TODO: Update for LE
                }

                if (tag.MainType == CardTagInfo.Type.Exact)
                {
                    // Need english here
                    Language language = Language.English;
                    string displayText = tag.DisplayText.GetText(language);
                    string text = tag.Text.GetText(Language.English);
                    int splitIndex = displayText == null ? -1 : displayText.IndexOf(':');
                    if (splitIndex >= 0)
                    {
                        string typeStr = displayText.Substring(0, splitIndex).Trim();
                        string value = displayText.Substring(splitIndex + 1).Trim();
                        switch (typeStr.ToLower())
                        {
                            case "related to":
                                tag.Exact = CardTagInfo.ExactType.RelatedTo;
                                tag.ExactCard = FindCardByName(language, value);
                                break;
                            case "card effect":
                                tag.Exact = CardTagInfo.ExactType.CardEffect;
                                break;
                            case "ritual monster":
                                tag.Exact = CardTagInfo.ExactType.RitualMonster;
                                tag.ExactCard = FindCardByName(language, value);
                                break;
                            case "fusion monster":
                                tag.Exact = CardTagInfo.ExactType.FusionMonster;
                                tag.ExactCard = FindCardByName(language, value);
                                break;
                            case "spell and trap":
                                tag.Exact = CardTagInfo.ExactType.SpellTrap;
                                break;
                            case "works well with":
                                tag.Exact = CardTagInfo.ExactType.WorksWellWith;
                                tag.ExactCard = FindCardByName(language, value);
                                break;
                            default:
                                Debug.Assert(false);
                                break;
                        }
                    }
                    else
                    {                        
                        switch (text.ToLower())
                        {
                            case "banishbeast":
                                tag.Exact = CardTagInfo.ExactType.BanishBeast;
                                break;
                            case "banishdark":
                                tag.Exact = CardTagInfo.ExactType.BanishDark;
                                break;
                            case "banishfish":
                                tag.Exact = CardTagInfo.ExactType.BanishFish;
                                break;
                            case "banishrock":
                                tag.Exact = CardTagInfo.ExactType.BanishRock;
                                break;
                            case "countertrapfairy":
                                tag.Exact = CardTagInfo.ExactType.CounterTrapFairy;
                                break;
                            case "spellcounter":
                                tag.Exact = CardTagInfo.ExactType.SpellCounter;
                                break;
                            default:
                                Debug.Assert(false);
                                break;
                        }
                    }

                    switch (tag.Exact)
                    {
                        case CardTagInfo.ExactType.CardEffect:
                            CardTagInfo.CardEffectType cardEffect;
                            if (!Enum.TryParse(text, true, out cardEffect))
                            {
                                //Debug.Assert(false);// TODO: Update for LE
                            }
                            tag.CardEffect = cardEffect;
                            break;

                        case CardTagInfo.ExactType.SpellTrap:
                            CardTagInfo.CardEffectType spellEffect;
                            if (!Enum.TryParse("Spell_" + text, true, out spellEffect))
                            {
                                //Debug.Assert(false);// TODO: Update for LE
                            }
                            tag.CardEffect = spellEffect;
                            break;
                    }
                }
            }

            foreach (CardInfo card in cards)
            {
                foreach (RelatedCardInfo relatedCardInfo in card.RelatedCards)
                {
                    if (relatedCardInfo.TagInfo.CardEffect != CardTagInfo.CardEffectType.None)
                    {
                        card.CardEffectTags.Add(relatedCardInfo.TagInfo.CardEffect);
                    }
                }
            }
        }

        private void ProcessLimitedCardList(Dictionary<short, CardInfo> cardsById)
        {
            foreach (CardInfo card in cardsById.Values)
            {
                card.Limit = CardLimitation.NotLimited;
            }

            foreach (short cardId in Manager.CardLimits.Forbidden)
            {
                cardsById[cardId].Limit = CardLimitation.Forbidden;
            }

            foreach (short cardId in Manager.CardLimits.Limited)
            {
                cardsById[cardId].Limit = CardLimitation.Limited;
            }

            foreach (short cardId in Manager.CardLimits.SemiLimited)
            {
                cardsById[cardId].Limit = CardLimitation.SemiLimited;
            }
        }

        private void PrintLimitedCardList()
        {
            Debug.WriteLine("========================== Forbidden ==========================");
            foreach (short cardId in Manager.CardLimits.Forbidden)
            {
                Debug.WriteLine(Cards[cardId].Name);
            }

            Debug.WriteLine("========================== Limited ==========================");
            foreach (short cardId in Manager.CardLimits.Limited)
            {
                Debug.WriteLine(Cards[cardId].Name);
            }

            Debug.WriteLine("========================== Semi-limited ==========================");
            foreach (short cardId in Manager.CardLimits.SemiLimited)
            {
                Debug.WriteLine(Cards[cardId].Name);
            }
        }
    }

    public class CardInfo
    {
        public int CardIndex { get; set; }
        public short CardId { get; set; }
        public ZibFile ImageFile { get; set; }
        public LocalizedText Name { get; set; }
        public LocalizedText Description { get; set; }        
        public List<RelatedCardInfo> RelatedCards { get; private set; }
        public HashSet<CardTagInfo.CardEffectType> CardEffectTags { get; private set; }
        
        /// <summary>
        /// Name type / archetype e.g. "Harpie"
        /// </summary>
        public HashSet<CardNameType> NameTypes { get; set; }

        /// <summary>
        /// The set ids this card belongs to (this is loaded from external sources)
        /// A set is a pack, deck or other form of card collection in the official game
        /// </summary>
        public List<int> SetIds { get; private set; }

        public int Atk { get; set; }
        public int Def { get; set; }
        public byte Level { get; set; }

        public bool IsUnknownAtk
        {
            get { return Atk == 5110; }
        }

        public bool IsUnknownDef
        {
            get { return Def == 5110; }
        }

        /// <summary>
        /// Light, Dark, Water, Fire (also Spell / Trap)
        /// </summary>
        public CardAttribute Attribute { get; set; }

        /// <summary>
        /// Fusion, Effect, Tuner, Flip, Ritual (also Spell / Trap)
        /// </summary>
        public CardType CardType { get; set; }

        /// <summary>
        /// Field, Equip, QuickPlay, Continuous
        /// </summary>
        public SpellType SpellType { get; set; }

        /// <summary>
        /// Insect, Fiend, Beast, Aqua, Plant (also Spell / Trap)
        /// </summary>
        public MonsterType MonsterType { get; set; }

        public byte PendulumScale1 { get; set; }
        public byte PendulumScale2 { get; set; }

        public byte PendulumScale
        {
            get { return Math.Max(PendulumScale1, PendulumScale2); }
        }

        /// <summary>
        /// Card limitation (NotLimited, Forbidden, Limited, SemiLimited)
        /// </summary>
        public CardLimitation Limit { get; set; }

        /// <summary>
        /// Card genre (negate effect, direct attack, cannot be destroyed, etc)
        /// </summary>
        public CardGenre Genre { get; set; }

        public CardTypeFlags CardTypeFlags
        {
            get { return GetCardTypeFlags(CardType); }
        }

        public bool IsMonsterToken
        {
            get { return IsMonster && CardTypeFlags.HasFlag(CardTypeFlags.Token); }
        }

        public bool IsEffect
        {
            get { return CardTypeFlags.HasFlag(CardTypeFlags.Effect); }
        }

        public bool IsMonster
        {
            get { return Attribute != CardAttribute.Spell && Attribute != CardAttribute.Trap; }
        }

        public bool IsNormalMonster
        {
            get { return FrameType == CardFrameType.Normal || FrameType == CardFrameType.PendulumNormal; }
        }

        public bool IsPendulum
        {
            get { return CardTypeFlags.HasFlag(CardTypeFlags.Pendulum); }
        }

        public bool IsXyz
        {
            get { return CardTypeFlags.HasFlag(CardTypeFlags.Xyz); }
        }

        public bool IsSynchro
        {
            get { return CardTypeFlags.HasFlag(CardTypeFlags.Synchro); }
        }

        public bool IsFusion
        {
            get { return CardTypeFlags.HasFlag(CardTypeFlags.Fusion); }
        }

        public bool IsMainDeckCard
        {
            get { return !IsExtraDeckCard; }
        }

        public bool IsExtraDeckCard
        {
            get
            {
                return CardTypeFlags.HasFlag(CardTypeFlags.Xyz) || CardTypeFlags.HasFlag(CardTypeFlags.Fusion) ||
                    CardTypeFlags.HasFlag(CardTypeFlags.Synchro);
            }
        }

        public bool IsSpell
        {
            get { return Attribute == CardAttribute.Spell; }
        }

        public bool IsTrap
        {
            get { return Attribute == CardAttribute.Trap; }
        }

        public string FrameName
        {
            get { return GetFrameName(FrameType); }
        }

        public CardFrameType FrameType
        {
            get
            {
                if (IsSpell)
                {
                    return CardFrameType.Spell;
                }

                if (IsTrap)
                {
                    return CardFrameType.Trap;
                }

                CardTypeFlags cardFlags = CardTypeFlags;

                if (cardFlags.HasFlag(CardTypeFlags.Synchro))
                {
                    if (cardFlags.HasFlag(CardTypeFlags.Pendulum))
                    {
                        return CardFrameType.PendulumSynchro;
                    }
                    return CardFrameType.Synchro;
                }                

                if (cardFlags.HasFlag(CardTypeFlags.Xyz))
                {
                    if (cardFlags.HasFlag(CardTypeFlags.Pendulum))
                    {
                        return CardFrameType.PendulumXyz;
                    }
                    return CardFrameType.Xyz;
                }

                if (cardFlags.HasFlag(CardTypeFlags.Pendulum))
                {
                    if (cardFlags.HasFlag(CardTypeFlags.Effect))
                    {
                        return CardFrameType.PendulumEffect;
                    }
                    return CardFrameType.PendulumNormal;
                }

                if (cardFlags.HasFlag(CardTypeFlags.Token))
                {
                    return CardFrameType.Token;
                }

                if (cardFlags.HasFlag(CardTypeFlags.Fusion))
                {
                    return CardFrameType.Fusion;
                }

                if (cardFlags.HasFlag(CardTypeFlags.Ritual))
                {
                    return CardFrameType.Ritual;
                }

                if (cardFlags.HasFlag(CardTypeFlags.Effect) ||
                    cardFlags.HasFlag(CardTypeFlags.SpecialSummon) ||
                    cardFlags.HasFlag(CardTypeFlags.Union) ||
                    cardFlags.HasFlag(CardTypeFlags.Toon) ||
                    cardFlags.HasFlag(CardTypeFlags.Gemini))
                {
                    return CardFrameType.Effect;
                }

                return CardFrameType.Normal;
            }
        }

        public CardInfo(int index)
        {
            CardIndex = index;
            Name = new LocalizedText();
            Description = new LocalizedText();
            RelatedCards = new List<RelatedCardInfo>();
            CardEffectTags = new HashSet<CardTagInfo.CardEffectType>();
            NameTypes = new HashSet<CardNameType>();
            SetIds = new List<int>();
        }

        public string GetDescription(Language language, bool pendulumDescription)
        {
            if (pendulumDescription && !IsPendulum)
            {
                return string.Empty;
            }

            string text = Description.GetText(language);
            if (IsPendulum)
            {
                string pendulumHeader = "[Pendulum Effect]";
                int index = text.IndexOf(pendulumHeader);
                if (pendulumDescription)
                {
                    return index == -1 ? string.Empty : text.Substring(index + pendulumHeader.Length);
                }
                else
                {
                    return index == -1 ? text : text.Substring(0, index);
                }
            }
            return text;
        }

        public static CardTypeFlags GetCardTypeFlags(CardType cardType)
        {
            switch (cardType)
            {
                case CardType.Default: return CardTypeFlags.Default;
                case CardType.Effect: return CardTypeFlags.Effect;
                case CardType.Fusion: return CardTypeFlags.Fusion;
                case CardType.FusionEffect: return CardTypeFlags.Fusion | CardTypeFlags.Effect;
                case CardType.Ritual: return CardTypeFlags.Ritual;
                case CardType.RitualEffect: return CardTypeFlags.Ritual | CardTypeFlags.Effect;
                case CardType.ToonEffect: return CardTypeFlags.Toon | CardTypeFlags.Effect;
                case CardType.SpiritEffect: return CardTypeFlags.Spirit | CardTypeFlags.Effect;
                case CardType.UnionEffect: return CardTypeFlags.Union | CardTypeFlags.Effect;
                case CardType.GeminiEffect: return CardTypeFlags.Gemini | CardTypeFlags.Effect;
                case CardType.Token: return CardTypeFlags.Token;
                case CardType.Spell: return CardTypeFlags.Spell;
                case CardType.Trap: return CardTypeFlags.Trap;
                case CardType.Tuner: return CardTypeFlags.Tuner;
                case CardType.TunerEffect: return CardTypeFlags.Tuner | CardTypeFlags.Effect;
                case CardType.Synchro: return CardTypeFlags.Synchro;
                case CardType.SynchroEffect: return CardTypeFlags.Synchro | CardTypeFlags.Effect;
                case CardType.SynchroTunerEffect: return CardTypeFlags.Synchro | CardTypeFlags.Tuner | CardTypeFlags.Effect;
                case CardType.DarkTunerEffect: return CardTypeFlags.DarkTuner | CardTypeFlags.Effect;
                case CardType.DarkSynchroEffect: return CardTypeFlags.DarkSynchro | CardTypeFlags.Effect;
                case CardType.Xyz: return CardTypeFlags.Xyz;
                case CardType.XyzEffect: return CardTypeFlags.Xyz | CardTypeFlags.Effect;
                case CardType.FlipEffect: return CardTypeFlags.Flip | CardTypeFlags.Effect;
                case CardType.Pendulum: return CardTypeFlags.Pendulum;
                case CardType.PendulumEffect: return CardTypeFlags.Pendulum | CardTypeFlags.Effect;
                case CardType.EffectSp: return CardTypeFlags.Effect | CardTypeFlags.SpecialSummon;
                case CardType.ToonEffectSp: return CardTypeFlags.Toon | CardTypeFlags.Effect | CardTypeFlags.SpecialSummon;
                case CardType.SpiritEffectSp: return CardTypeFlags.Spirit | CardTypeFlags.Effect | CardTypeFlags.SpecialSummon;
                case CardType.TunerEffectSp: return CardTypeFlags.Tuner | CardTypeFlags.Effect | CardTypeFlags.SpecialSummon;
                case CardType.DarkTunerEffectSp: return CardTypeFlags.DarkTuner | CardTypeFlags.Effect | CardTypeFlags.SpecialSummon;
                case CardType.FlipTunerEffect: return CardTypeFlags.Flip | CardTypeFlags.Tuner | CardTypeFlags.Effect;
                case CardType.PendulumTunerEffect: return CardTypeFlags.Pendulum | CardTypeFlags.Tuner | CardTypeFlags.Effect;
                case CardType.XyzPendulumEffect: return CardTypeFlags.Xyz | CardTypeFlags.Pendulum | CardTypeFlags.Effect;
                case CardType.PendulumFlipEffect: return CardTypeFlags.Pendulum | CardTypeFlags.Flip | CardTypeFlags.Effect;
                //case CardType.SynchoPendulumEffect: return CardTypeFlags.Synchro | CardTypeFlags.Pendulum | CardTypeFlags.Effect;
                //case CardType.UnionTunerEffect: return CardTypeFlags.Union | CardTypeFlags.Tuner | CardTypeFlags.Effect;
                //case CardType.RitualSpiritEffect: return CardTypeFlags.Ritual | CardTypeFlags.Spirit | CardTypeFlags.Effect;
                case CardType.AnyNormal: return CardTypeFlags.Any | CardTypeFlags.Normal;
                case CardType.AnyFusion: return CardTypeFlags.Any | CardTypeFlags.Fusion;
                case CardType.AnyFlip: return CardTypeFlags.Any | CardTypeFlags.Flip;
                case CardType.AnyPendulum: return CardTypeFlags.Any | CardTypeFlags.Pendulum;
                case CardType.AnyRitual: return CardTypeFlags.Any | CardTypeFlags.Ritual;
                case CardType.AnySynchro: return CardTypeFlags.Any | CardTypeFlags.Synchro;
                case CardType.AnyTuner: return CardTypeFlags.Any | CardTypeFlags.Tuner;
                case CardType.AnyXyz: return CardTypeFlags.Any | CardTypeFlags.Xyz;
                default:
                    return 0;// TODO: Update for LE //throw new NotImplementedException("Unhandled CardType->CardTypeFlags conversion " + cardType);
            }
        }

        public static string GetFrameName(CardFrameType frameType)
        {
            switch (frameType)
            {
                default:
                case CardFrameType.Normal: return "card_nomal";
                case CardFrameType.Effect: return "card_kouka";
                case CardFrameType.Token: return "card_token";
                case CardFrameType.Ritual: return "card_gisiki";
                case CardFrameType.Fusion: return "card_yugo";
                case CardFrameType.PendulumEffect: return "card_pendulum";
                case CardFrameType.PendulumNormal: return "card_pendulum_n";
                case CardFrameType.PendulumSynchro: return "card_sync_pendulum";
                case CardFrameType.PendulumXyz: return "card_xyz_pendulum";
                case CardFrameType.Synchro: return "card_sync";
                case CardFrameType.Xyz: return "card_xyz";
                case CardFrameType.Spell: return "card_mahou";
                case CardFrameType.Trap: return "card_wana";
            }
        }

        public static string GetFullMonsterTypeName(MonsterType monsterType, CardTypeFlags cardType)
        {
            string result = null;
            foreach (CardTypeFlags flag in Enum.GetValues(typeof(CardTypeFlags)))
            {
                if (cardType.HasFlag(flag))
                {
                    string flagName = GetCardTypeFlagName(flag);
                    if (!string.IsNullOrEmpty(flagName))
                    {
                        // Reverse order
                        result = result == null ? flagName : flagName + "/" + result;
                    }
                }
            }

            return "[" + GetMonsterTypeName(monsterType) + (result == null ? string.Empty : "/" + result) + "]";
        }

        public static string GetMonsterTypeName(MonsterType monsterType)
        {
            switch (monsterType)
            {
                case MonsterType.Dragon: return "Dragon";
                case MonsterType.Zombie: return "Zombie";
                case MonsterType.Fiend: return "Fiend";
                case MonsterType.Pyro: return "Pyro";
                case MonsterType.SeaSerpent: return "Sea Serpent";
                case MonsterType.Rock: return "Rock";
                case MonsterType.Machine: return "Machine";
                case MonsterType.Fish: return "Fish";
                case MonsterType.Dinosaur: return "Dinosaur";
                case MonsterType.Insect: return "Insect";
                case MonsterType.Beast: return "Beast";
                case MonsterType.BeastWarrior: return "Beast-Warrior";
                case MonsterType.Plant: return "Plant";
                case MonsterType.Aqua: return "Aqua";
                case MonsterType.Warrior: return "Warrior";
                case MonsterType.WingedBeast: return "Winged Beast";
                case MonsterType.Fairy: return "Fairy";
                case MonsterType.Spellcaster: return "Spellcaster";
                case MonsterType.Thunder: return "Thunder";
                case MonsterType.Reptile: return "Reptile";
                case MonsterType.Psychic: return "Psychic";
                case MonsterType.Wyrm: return "Wyrm";
                case MonsterType.DivineBeast: return "Divine-Beast";
                case MonsterType.CreatorGod: return "Creator";
                case MonsterType.Spell: return "Spell";
                case MonsterType.Trap: return "Trap";
                case MonsterType.Unknown:
                default:
                    return "?";
            }
        }

        public static string GetCardTypeFlagName(CardTypeFlags flag)
        {
            switch (flag)
            {
                default:
                case CardTypeFlags.Default: return null;
                case CardTypeFlags.Effect: return "Effect";
                case CardTypeFlags.Fusion: return "Fusion";
                case CardTypeFlags.Ritual: return "Ritual";
                case CardTypeFlags.Toon: return "Toon";
                case CardTypeFlags.Spirit: return "Spirit";
                case CardTypeFlags.Union: return "Union";
                case CardTypeFlags.Gemini: return "Gemini";
                case CardTypeFlags.Token: return "Token";
                case CardTypeFlags.Spell: return "Spell";
                case CardTypeFlags.Trap: return "Trap";
                //case CardTypeFlags.Common: return "";
                case CardTypeFlags.Tuner: return "Tuner";
                case CardTypeFlags.DarkTuner: return "Dark Tuner";
                case CardTypeFlags.DarkSynchro: return "Dark Synchro";
                case CardTypeFlags.Synchro: return "Synchro";
                case CardTypeFlags.Xyz: return "Xyz";
                case CardTypeFlags.Flip: return "Flip";
                case CardTypeFlags.Pendulum: return "Pendulum";
                //case CardTypeFlags.SpecialSummon: return "";
            }
        }
    }

    public enum CardAttribute
    {
        Unknown = 0,
        LightMonster = 1,
        DarkMonster = 2,
        WaterMonster = 3,
        FireMonster = 4,
        EarthMonster = 5,
        WindMonster = 6,
        DivineMonster = 7,
        Spell = 8,
        Trap = 9
    }

    public enum CardType
    {
        Default = 0,
        Effect = 1,
        Fusion = 2,
        FusionEffect = 3,// Thousand-Eyes Restrict
        Ritual = 4,
        RitualEffect = 5,// Relinquished
        ToonEffect = 6,// Toon Masked Scorcerer
        SpiritEffect = 7,// Maharaghi
        UnionEffect = 8,// Y-Dragon Head
        GeminiEffect = 9,// Elemental HERO Neos Alius
        Token = 10,
        //11 = Effect? - duel links states this is "God"
        //12 = Effect? - duel links states this is "Dummy"
        Spell = 13,
        Trap = 14,
        Tuner = 15,// Flamvell Guard
        TunerEffect = 16,// Cryomancer of the Ice Barrier
        Synchro = 17, // Gaia Knight, the Force of Earth
        SynchroEffect = 18,// Dark End Dragon
        SynchroTunerEffect = 19,// Formula Synchron
        DarkTunerEffect = 20,// unused
        DarkSynchroEffect = 21,// unused
        Xyz = 22,// Gem-Knight Pearl
        XyzEffect = 23,// Number 39: Utopia
        FlipEffect = 24,
        Pendulum = 25,// Flash Knight
        PendulumEffect = 26,// Stargazer Magician
        EffectSp = 27,// Larvae Moth
        ToonEffectSp = 28,// Manga Ryu-Ran (Sp = special summon "This monster cannot be Normal Summoned or Set")
        SpiritEffectSp = 29,// Yamato-no-Kami
        TunerEffectSp = 30,// Trap Eater
        DarkTunerEffectSp = 31,// unused
        FlipTunerEffect = 32,// Shaddoll Falco
        PendulumTunerEffect = 33,// "Luster Pendulum, the Dracoslayer"
        XyzPendulumEffect = 34,// Odd-Eyes Rebellion Dragon
        PendulumFlipEffect = 35,// Performapal Momoncarpet
        //SynchoPendulumEffect = 36,// unused
        //UnionTunerEffect = 37,// unused
        //RitualSpiritEffect = 38,// unused
        //_______ = 39,// unused - underscores??

        // These values are used for tagdata/taginfo
        AnyNormal = 37,// NORMAL*
        AnySynchro = 38,// SYNC*
        AnyXyz = 39,// XYZ*
        AnyTuner = 40,// TUNER*
        AnyFusion = 41,// FUSION*
        AnyRitual = 42,// RITUAL*
        AnyPendulum = 43,// PEND*
        AnyFlip = 44,// FLIP*
    }

    /// <summary>
    /// Flags version of CardType for easier checking of individual types
    /// </summary>
    [Flags]
    public enum CardTypeFlags : uint
    {
        Default = 0,
        Effect = 1 << 0,
        Fusion = 1 << 1,
        Ritual = 1 << 2,
        Toon = 1 << 3,
        Spirit = 1 << 4,
        Union = 1 << 5,
        Gemini = 1 << 6,
        Token = 1 << 7,
        Spell = 1 << 8,
        Trap = 1 << 9,
        Tuner = 1 << 10,
        DarkTuner = 1 << 11,
        DarkSynchro = 1 << 12,
        Synchro = 1 << 13,
        Xyz = 1 << 14,
        Flip = 1 << 15,
        Pendulum = 1 << 16,
        SpecialSummon = 1 << 17,// "This monster cannot be Normal Summoned or Set"

        Link = 1 << 18,// Not in LOTD

        Normal = 1 << 19,// Special flag used for finding related cards
        Any = 1 << 20,// Special flag used for finding related cards
    }

    public enum MonsterType
    {
        Unknown = 0,
        Dragon = 1,
        Zombie = 2,
        Fiend = 3,
        Pyro = 4,
        SeaSerpent = 5,
        Rock = 6,
        Machine = 7,
        Fish = 8,
        Dinosaur = 9,
        Insect = 10,
        Beast = 11,
        BeastWarrior = 12,
        Plant = 13,
        Aqua = 14,
        Warrior = 15,
        WingedBeast = 16,
        Fairy = 17,
        Spellcaster = 18,
        Thunder = 19,
        Reptile = 20,
        Psychic = 21,
        Wyrm = 22,
        DivineBeast = 23,
        CreatorGod = 24,// This does't appear on any card in the game - its meant for "Holacite the Creator of Light"
        Spell = 25,
        Trap = 26
    }

    /// <summary>
    /// Also known as "Property" - the type of spell / trap card
    /// </summary>
    public enum SpellType
    {
        Normal = 0,

        /// <summary>
        /// Counter trap cards are a unique trap card type that are of spell speed 3.
        /// </summary>
        Counter = 1,

        Field = 2,
        Equip = 3,
        Continuous = 4,
        QuickPlay = 5,
        Ritual = 6
    }

    public enum CardFrameType
    {
        Normal,        
        Effect,
        Token,
        Ritual,
        Fusion,
        PendulumEffect,
        PendulumNormal,
        PendulumSynchro,
        PendulumXyz,
        Synchro,
        Xyz,
        Spell,
        Trap,
    }

    public enum CardLimitation
    {
        NotLimited,
        Forbidden,
        Limited,
        SemiLimited
    }

    [Flags]
    public enum CardGenre : ulong
    {
        None = 0,
        RecoverLP = 1UL << 0,//0x0000000000000001 ICON_ID_GENRE_LPUP
        DamageLP = 1UL << 1,//0x0000000000000002 ICON_ID_GENRE_LPDOWN
        HelpDraw = 1UL << 2,//0x0000000000000004 ICON_ID_GENRE_DRAW
        SpecialSummon = 1UL << 3,//0x0000000000000008 ICON_ID_GENRE_SPSUMMON
        NegateEffect = 1UL << 4,//0x0000000000000010 ICON_ID_GENRE_DISABLE
        SearchDeck = 1UL << 5,//0x0000000000000020 ICON_ID_GENRE_DECKSEARCH
        RecoverFromGraveyard = 1UL << 6,//0x0000000000000040 ICON_ID_GENRE_USEGRAVE
        IncreaseDecreaseAtkDef = 1UL << 7,//0x0000000000000080 ICON_ID_GENRE_POWER
        ChangeBattlePosition = 1UL << 8,//0x0000000000000100 ICON_ID_GENRE_POSITION
        SetControls = 1UL << 9,//0x0000000000000200 ICON_ID_GENRE_CONTROL
        DestroyMonster = 1UL << 10,//0x0000000000000400 ICON_ID_GENRE_BREAKMONST
        DestroySpellCard = 1UL << 11,//0x0000000000000800 ICON_ID_GENRE_BREAKMAGIC
        DestroyHand = 1UL << 12,//0x0000000000001000 ICON_ID_GENRE_HANDDES
        DestroyDeck = 1UL << 13,//0x0000000000002000 ICON_ID_GENRE_DECKDES
        RemoveCard = 1UL << 14,//0x0000000000004000 ICON_ID_GENRE_REMOVECARD
        ReturnCard = 1UL << 15,//0x0000000000008000 ICON_ID_GENRE_CARDBACK
        Piercing = 1UL << 16,//0x0000000000010000 ICON_ID_GENRE_SPEAR
        DirectAttack = 1UL << 17,//0x0000000000020000 ICON_ID_GENRE_DIRECTATK
        AttackMultipleTimes = 1UL << 18,//0x0000000000040000 ICON_ID_GENRE_MANYATK
        CannotBeDestroyed = 1UL << 19,//0x0000000000080000 ICON_ID_GENRE_UNBREAK
        LimitAttack = 1UL << 20,//0x0000000000100000 ICON_ID_GENRE_LIMITATK
        CannotNormalSummon = 1UL << 21,//0x0000000000200000 ICON_ID_GENRE_CANTSUMMON
        FlipEffectMonster = 1UL << 22,//0x0000000000400000 ICON_ID_GENRE_REVERSE
        ToonMonster = 1UL << 23,//0x0000000000800000 ICON_ID_GENRE_TOON
        SpiritMonster = 1UL << 24,//0x0000000001000000 ICON_ID_GENRE_SPIRIT
        UnionMonster = 1UL << 25,//0x0000000002000000 ICON_ID_GENRE_UNION
        GeminiMonster = 1UL << 26,//0x0000000004000000 ICON_ID_GENRE_DUAL
        LvMonster = 1UL << 27,//0x0000000008000000 ICON_ID_GENRE_LEVELUP
        Original = 1UL << 28,//0x0000000010000000 ICON_ID_GENRE_ORIGINAL
        FusionMaterialMonster = 1UL << 29,//0x0000000020000000 ICON_ID_GENRE_FUSION
        Ritual = 1UL << 30,//0x0000000040000000 ICON_ID_GENRE_RITUAL
        Token = 1UL << 31,//0x0000000080000000 ICON_ID_GENRE_TOKEN
        Counter = 1UL << 32,//0x0000000100000000 ICON_ID_GENRE_COUNTER
        Gamble = 1UL << 33,//0x0000000200000000 ICON_ID_GENRE_GAMBLE
        AttributeRelated = 1UL << 34,//0x0000000400000000 ICON_ID_GENRE_ATTR
        TypeRelated = 1UL << 35,//0x0000000800000000 ICON_ID_GENRE_TYPE
        Tuner = 1UL << 36,//0x0000001000000000 ICON_ID_GENRE_TUNER
        SynchroMonster = 1UL << 37,//0x0000002000000000 ICON_ID_GENRE_SYNC
        SendToGraveyard = 1UL << 38,//0x0000004000000000 ICON_ID_GENRE_DROPGRAVE

        // These values don't visibly appear in the game
        NormalMonsterRelated = 1UL << 39,//0x0000008000000000 ICON_ID_GENRE_NORMAL
        LightMonsterRelated = 1UL << 40,//0x0000010000000000 ICON_ID_GENRE_ATTR_LIGHT
        DarkMonsterRelated = 1UL << 41,//0x0000020000000000 ICON_ID_GENRE_ATTR_DARK
        EarthMonsterRelated = 1UL << 42,//0x0000040000000000 ICON_ID_GENRE_ATTR_EARTH
        WaterMonsterRelated = 1UL << 43,//0x0000080000000000 ICON_ID_GENRE_ATTR_WATER
        FireMonsterRelated = 1UL << 44,//0x0000100000000000 ICON_ID_GENRE_ATTR_FIRE
        WindMonsterRelated = 1UL << 45,//0x0000200000000000 ICON_ID_GENRE_ATTR_WIND

        XyzMonster = 1UL << 46,//0x0000400000000000 ICON_ID_GENRE_XYZ
        LevelModifier = 1UL << 47,//0x0000800000000000 ICON_ID_GENRE_LVUPDOWN
        Pendulum = 1UL << 48,//0x0001000000000000 ICON_ID_GENRE_PENDULUM

        // These values aren't on any cards (but appear in game if you force them on a card)
        DivineAttribute = 1UL << 49,//0x0002000000000000 ICON_ID_GENRE_ATTR_GOD
        NewCard = 1UL << 50,//0x0004000000000000 ICON_ID_GENRE_NEW
        GameOriginal = 1UL << 51,//0x0008000000000000 ICON_ID_GENRE_GAME_ORIGINAL
        CardVaritation = 1UL << 52,//0x0010000000000000 ICON_ID_GENRE_PICTURE (assumed)
        //
        // The game uses broken icons for those values:
        //DivineAttribute = ICON_ID_ORDER_ASCENDING
        //NewCard = ICON_ID_ORDER_DESCENDING
        //GameOriginal = ICON_ID_SEARCH
        //CardVaritation = ICON_ID_DUEL_MENU_PHASE

        //Unused8 = 1UL << 53,//0x0020000000000000
        //Unused9 = 1UL << 54,//0x0040000000000000
        //Unused10 = 1UL << 55,//0x0080000000000000
        //Unused11 = 1UL << 56,//0x0100000000000000
        //Unused12 = 1UL << 57,//0x0200000000000000
        //Unused12 = 1UL << 58,//0x0400000000000000
        //Unused13 = 1UL << 59,//0x0800000000000000
        //Unused14 = 1UL << 60,//0x1000000000000000
        //Unused15 = 1UL << 61,//0x2000000000000000
        //Unused16 = 1UL << 62,//0x4000000000000000
        //Unused17 = 1UL << 63,//0x8000000000000000
    }

    public enum CardNameType
    {
        Null,
        Toon,
        Demon,
        Keeper,
        Guardian,
        Scorpion,
        Amazoness,
        Ninja,
        Level,
        EHERO,
        DHERO,
        NeosMaterial,
        NeosFusion,
        Neos,
        Ojama,
        Battery,
        DarkWorld,
        BES,
        Antique,
        Sphinx,
        Machiners,
        Harpie,
        Roid,
        Vehicloid,
        Neospacian,
        Cocoon,
        Alien,
        Mythical,
        HERO,
        Allure,
        Gadget,
        Six,
        Jewel,
        Volcanic,
        BlazeCanon,
        Venom,
        Cloudian,
        Gladial,
        Weapon,
        Takemitsu,
        EvHERO,
        Drunk,
        Arcana,
        Fossil,
        Gunner,
        Forbidden,
        Rainbow,
        CyberFusion,
        Icebarrier,
        AOJ,
        Saber,
        Worm,
        LightLord,
        Frog,
        Nitro,
        Genex,
        MistValley,
        Flamebell,
        NeosNHERO,
        Deformer,
        Chain,
        Natul,
        Clear,
        RedEyes,
        BlackFeather,
        SlashBuster,
        Roaring,
        Jurac,
        RealGenex,
        EarthbindGod,
        Koakimail,
        Infernity,
        X_Saber,
        FortuneLady,
        Dragnity,
        FortuneWitch,
        Synchron,
        Saviour,
        Reptiles,
        Shien,
        Junk,
        Tomabo,
        Sin,
        Gem,
        GemKnight,
        Laval,
        Vailon,
        Scrap,
        Eleki,
        Fusion,
        Infinity,
        Wisel,
        TG,
        Karakuri,
        Ritua,
        Gusta,
        Invelds,
        Reactor,
        Agent,
        Polestar,
        PolestarBeast,
        PolestarGhost,
        PolestarAngel,
        PolestarItem,
        PoleGod,
        SoundWarrior,
        Resonator,
        MHERO,
        VHERO,
        Meklord_Emp,
        Meklord_Sld,
        Meklord,
        Zenmai,
        Penguin,
        Evold,
        Evolder,
        TrapHole,
        TimeGod,
        Sacred,
        Velds,
        Numbers,
        Gagaga,
        Gogogo,
        Photon,
        Ninjutsu,
        Inzector,
        Invasion,
        Bouncer,
        Butterfly,
        HolySeal,
        Majin,
        Heroic,
        Ooparts,
        Spellbook,
        Madolce,
        Geargear,
        Xyz,
        Poseidon,
        Mermail,
        Abyss,
        Magical,
        Nimble,
        Duston,
        Medallion,
        NobleKnight,
        FireKing,
        Galaxy,
        HolySword,
        FireStar,
        FireDance,
        HazeBeast,
        Haze,
        ZexalWeapon,
        Hope,
        GimmickPuppet,
        Dododo,
        BK,
        PhantomMek,
        FireKingBeast,
        ChaosNumbers,
        ChaosXyz,
        Geargearno,
        SDRobo,
        SDRobo2,
        Umbral,
        HolyLightning,
        Bujin,
        Kowakuma,
        Hole,
        CNo39,
        H_Challenger,
        Malicebolus,
        Ghostrick,
        Vampire,
        Cat,
        CyberDragon,
        Cybernetic,
        Shinra,
        Necrovalley,
        Zubaba,
        Fishborg,
        RUM,
        Medallion2,
        Artifact,
        Evolkaiser,
        GalaxyEyes,
        Tachyon,
        Over100,
        Wizard,
        OddEyes,
        LegendDragon,
        LegendKnight,
        WingedKuriboh,
        Stardust,
        Sprout,
        Artorius,
        Lancelot,
        Superheavy,
        Genso,
        Tellarknight,
        Shadoll,
        DragonStar,
        EM,
        Change,
        Higan,
        UA,
        DD,
        DDD,
        Furnimal,
        Deathtoy,
        Qliphot,
        Bunborg,
        Goblin,
        Cthulhu,
        Contract,
        Gottoms,
        Yosen,
        Necroth,
        Spirit_All,
        Spirit_Tamer,
        Spirit_Beast,
        RR,
        Infernoid,
        Jinzo,
        Gaia,
        Monarch,
        Charmer,
        Possessed,
        Crystal,
        Warrior,
        PowerTool,
        BMG,
        EdgeImp,
        Sephira,
        GensoPrincess,
        Spirit_Rider,
        Stellarknight,
        Void,
        Em,
        Dragonsword,
        Igknight,
        Aroma,
        Empowered,
        AetherWeapon,
        FortunePrince,
        Aquaactress,
        Aquarium,
        ChaosSoldier,
        Majespecter,
        Gradle,
        SOz,
        Kaiju,
        SR,
        PSYFrame,
        RedDemon,
        Burgestoma,
        Dante,
        BusterBlader,
        BusterSword,
        Dynamist,
        Shiranui,
        Dragondevil,
        Exodia,
        PhantomKnight,
        Phantom,
        Super,
        Super_Quantum,
        Super_Machine,
        BlueEyes,
        HopeX,
        Moonlight,
        Amorphage,
        ElfSwordsman,
        MagicianGirl,
        BlackMagic,
        Metalphose,
        Tramid,
        ABF,
        Houkai,
        Chaos,
        CyberAngel,
        Cypher,
        Cardian,
        SilentSword,
        SilentMagic,
        MagnetWarrior,
        BlackMagic2,
        Kuriboh,
        Crystron,
        Kagoju,
        ApoQliphot,
        Chichukai,
        ChichukaiRyu,
        Spyral,
        SpyralGear,
        MakaiGekidan,
        MakaiDaihon,
        FallenAngel,
        WW,
        Beast12,
        PendDragon
    }

    public class RelatedCardInfo
    {
        /// <summary>
        /// The related card
        /// </summary>
        public CardInfo Card { get; set; }

        /// <summary>
        /// The relationship tag info
        /// </summary>
        public CardTagInfo TagInfo { get; set; }

        public RelatedCardInfo(CardInfo card, CardTagInfo tagInfo)
        {
            Card = card;
            TagInfo = tagInfo;
        }
    }

    /// <summary>
    /// Describes how cards can be tagged / related to one another
    /// </summary>
    public class CardTagInfo
    {
        /// <summary>
        /// The index of this tag info in the tags collection
        /// </summary>
        public int Index { get; set; }

        public ExactType Exact { get; set; }
        public CardEffectType CardEffect { get; set; }
        public CardInfo ExactCard { get; set; }

        /// <summary>
        /// The main type of this tag - priority seems to be 0,1,2
        /// </summary>
        public Type MainType { get; set; }

        /// <summary>
        /// Unknown - some kind of sub priority?
        /// </summary>
        public short MainValue { get; set; }

        /// <summary>
        /// The elements which make up this tag info
        /// </summary>
        public Element[] Elements { get; set; }

        /// <summary>
        /// The string representation
        /// </summary>
        public LocalizedText Text { get; private set; }

        /// <summary>
        /// The string respresentation which is displayed at the bottom of the relationship window
        /// </summary>
        public LocalizedText DisplayText { get; private set; }

        public CardTagInfo()
        {
            Elements = new Element[8];
            Text = new LocalizedText();
            DisplayText = new LocalizedText();
        }

        /// <summary>
        /// The main type of the tag info
        /// </summary>
        public enum Type
        {
            /// <summary>
            /// An exact relationship of some kind e.g. "Card effect: Negate Attack", "Related to: Exodia the Forbidden One"
            /// </summary>
            Exact = 0,

            /// <summary>
            /// Some kind of impact on the card
            /// </summary>
            Ad = 1,

            /// <summary>
            /// Finding other cards e.g. "Summon 1 Level 4 or lower Gemini monster from your hand." - "{FIND}KIND:DUAL LEVEL:<=4"
            /// </summary>
            Find = 2,

            /// <summary>
            /// Same as Ad but for only XYZ
            /// </summary>
            AdXyz = 257,

            /// <summary>
            /// Same as Find but only for XYZ
            /// </summary>
            FindXyz = 258
        }

        public enum ElementType
        {
            None = -1,
            AtkLessThanOrEquals = 0,
            DefLessThanOrEquals = 2,
            LevelLessThanOrEquals = 4,
            RankLessThanOrEquals = 8,
            AtkLessThan = 256,
            Attribute = 513,// This should map into the enum CardAttribute
            AtkEquals = 512,
            DefEquals = 514,
            CardType = 515,// XYZ/Monster/Effect/etc - should map into CardType
            LevelEquals = 516,
            SpellType = 517,// This should map into enum SpellType
            MonsterType = 518,// This should map into enum MonsterType
            DeckType = 519,
            RankEquals = 520,// For XYZ monsters
            SpecialSummon = 521,
            Tribute = 522,
            Tribute2 = 523,// This monster counts as 2 tribute summons
            AtkGreaterThanOrEquals = 768,
            LevelGreaterThanOrEquals = 772,
            RankGreaterThanOrEquals = 776,
        }

        public struct Element
        {
            public ElementType Type;
            public short Value;
        }

        /// <summary>
        /// Defines what an "Exact" tag info type does
        /// </summary>
        public enum ExactType
        {
            None,

            /// <summary>
            /// Related to an archetype / exact card
            /// </summary>
            RelatedTo,

            /// <summary>
            /// Related to a fusion monster
            /// </summary>
            FusionMonster,

            /// <summary>
            /// Related to a ritual monster
            /// </summary>
            RitualMonster,

            /// <summary>
            /// A spell / trap card effect
            /// </summary>
            SpellTrap,

            /// <summary>
            /// Monster card effect
            /// </summary>
            CardEffect,

            WorksWellWith,

            /// <summary>
            /// Monsters that use Spell Counters
            /// </summary>
            SpellCounter,


            /// <summary>
            /// Fairy-Type effects that trigger with Counter Traps
            /// </summary>
            CounterTrapFairy,

            /// <summary>
            /// Banished Beast and Winged-Beast Type monsters
            /// </summary>
            BanishBeast,
            /// <summary>
            /// Banished Dark monsters
            /// </summary>
            BanishDark,
            /// <summary>
            /// Banished Fish, Sea Serpent, and Aqua-Type monsters
            /// </summary>
            BanishFish,
            /// <summary>
            /// Banished Rock-Type monsters
            /// </summary>
            BanishRock
        }

        public enum CardEffectType
        {
            None,
            AntiAttack,
            AntiDefense,
            AntiDiscard,
            AntiDraw,
            AntiEffectDamage,
            AntiFaceDown,
            AntiMonsterEffect,
            AntiPendulum,
            AntiSpell,
            AntiTrap,
            ATKReduction,
            AttackDirectly,
            AttributeDestruction,
            AttributeEquipBoost,
            BanishOpp,
            BanishPlayer,
            BoostNormal,
            BurnDamageAtk,
            BurnDamageCont,
            BurnDamageDirect,
            BurnDamageMons,
            BurnDamageTrib,
            CannotAttack,
            CannotChangePosition,
            CardDiscard,
            CardDraw,
            ChangeLevel,
            ChooseAttackTarget,
            CoinToss,
            Combo,
            ContinuousSpellTrib,
            DarkCardDraw,
            DEFGain,
            DEFReduction,
            DestroyType,
            Dice,
            DragonBoost,
            EquipDragon,
            EquipFairy,
            EquipMachine,
            EquipSpellcaster,
            EquipWarrior,
            FaceUp,
            FieldPowerAttr,
            FieldPowerType,
            FlipFaceDown,
            Fusion,
            GiveControl,
            GraveToHandMonster,
            GraveToHandSpellTrap,
            IceCount,
            LargeATKGainEquip,
            LifeGain,
            LookAtDeck,
            LookAtHand,
            Mill,
            NegateAttack,
            RecycleToDeck,
            RestrictMonster,
            Ritual,
            SameAttributeBoost,
            Simochi,
            SpellTrapProtect,
            SSGraveyard,
            SSZombie,
            StopFlipNormalSummon,
            StopSpecialSummon,
            SwitchATKDEF,
            SynchroFusion,
            SynchroMaterial,
            TakeControl,
            ToGraveyard,
            Token,
            TokenOpponent,
            TrapMonster,
            Ultimaya,
            ZoneDeny,

            // Spell/trap values
            Spell_DoubleSummon,
            Spell_MonsterDestruction,
            Spell_MonsterProtect,
            Spell_Piercing,
            Spell_PreventBattleDamage,
            Spell_QuickATKboost,
            Spell_SSHandDeck,
            Spell_StopFusion,
            Spell_StopRitual,
            Spell_StopSynchro,
            Spell_StopTribute,
            Spell_StopXyz
        }
    }

    public enum DeckType
    {
        Main = 0,
        Extra = 1,
        Side = 2
    }
}
