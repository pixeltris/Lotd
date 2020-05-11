using Lotd.FileFormats;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

#pragma warning disable 649

namespace Lotd
{
    /// <summary>
    /// Helper class for loading YDK files (YGOPro)
    /// </summary>
    static class YdkHelper
    {
        static Dictionary<long, long> ydkIdToOfficialId = new Dictionary<long, long>();
        static Dictionary<long, long> officialIdToYdkId = new Dictionary<long, long>();
        const string idMapFile = "YdkIds.txt";

        /// <summary>
        /// File extension used by YGOPRO
        /// </summary>
        public const string FileExtension = ".ydk";

        public static MemTools.YdcDeck LoadDeck(string filePath)
        {
            // Loader is based on https://github.com/Fluorohydride/ygopro/blob/master/gframe/deck_manager.cpp

            MemTools.YdcDeck result = new MemTools.YdcDeck();
            result.DeckName = Path.GetFileNameWithoutExtension(filePath);
            result.MainDeck = new short[Constants.NumMainDeckCards];
            result.ExtraDeck = new short[Constants.NumMainDeckCards];
            result.SideDeck = new short[Constants.NumMainDeckCards];
            result.Unk1 = new byte[12];
            result.Unk2 = new byte[12];
            result.IsDeckComplete = true;
            result.DeckAvatarId = 5;// Joey! TODO: Allow this to be configured...
            if (File.Exists(filePath))
            {
                bool isSide = false;

                string[] lines = File.ReadAllLines(filePath);
                foreach (string line in lines)
                {
                    if (line.StartsWith("!"))
                    {
                        isSide = true;
                    }
                    else if (!line.StartsWith("#"))
                    {
                        long ydkCardId, officialCardId;
                        CardInfo card;
                        if (long.TryParse(line.Trim(), out ydkCardId) &&
                            ydkIdToOfficialId.TryGetValue(ydkCardId, out officialCardId) &&
                            Program.Manager.CardManager.Cards.TryGetValue((short)officialCardId, out card))
                        {
                            if (isSide)
                            {
                                if (result.NumSideDeckCards < result.SideDeck.Length)
                                {
                                    result.SideDeck[result.NumSideDeckCards] = card.CardId;
                                    result.NumSideDeckCards++;
                                }
                            }
                            else if (card.CardTypeFlags.HasFlag(CardTypeFlags.Fusion) ||
                                card.CardTypeFlags.HasFlag(CardTypeFlags.Synchro) ||
                                card.CardTypeFlags.HasFlag(CardTypeFlags.DarkSynchro) ||
                                card.CardTypeFlags.HasFlag(CardTypeFlags.Xyz) ||
                                card.CardTypeFlags.HasFlag(CardTypeFlags.Link))
                            {
                                if (result.NumExtraDeckCards < result.ExtraDeck.Length)
                                {
                                    result.ExtraDeck[result.NumExtraDeckCards] = card.CardId;
                                    result.NumExtraDeckCards++;
                                }
                            }
                            else
                            {
                                if (result.NumMainDeckCards < result.MainDeck.Length)
                                {
                                    result.MainDeck[result.NumMainDeckCards] = card.CardId;
                                    result.NumMainDeckCards++;
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }

        public static void SaveDeck(MemTools.YdcDeck deck, string path)
        {
            try
            {
                using (TextWriter writer = File.CreateText(path))
                {
                    writer.WriteLine("#main");
                    for (int i = 0; i < deck.NumMainDeckCards; i++)
                    {
                        long ydkCardId;
                        if (officialIdToYdkId.TryGetValue(deck.MainDeck[i], out ydkCardId))
                        {
                            writer.WriteLine(ydkCardId);
                        }
                    }
                    writer.WriteLine();

                    writer.WriteLine("#extra");
                    for (int i = 0; i < deck.NumExtraDeckCards; i++)
                    {
                        long ydkCardId;
                        if (officialIdToYdkId.TryGetValue(deck.ExtraDeck[i], out ydkCardId))
                        {
                            writer.WriteLine(ydkCardId);
                        }
                    }
                    writer.WriteLine();

                    writer.WriteLine("!side");
                    for (int i = 0; i < deck.NumSideDeckCards; i++)
                    {
                        long ydkCardId;
                        if (officialIdToYdkId.TryGetValue(deck.SideDeck[i], out ydkCardId))
                        {
                            writer.WriteLine(ydkCardId);
                        }
                    }
                    writer.WriteLine();
                }
            }
            catch
            {
            }
        }

        public static void LoadIdMap()
        {
            ydkIdToOfficialId.Clear();
            officialIdToYdkId.Clear();

            if (File.Exists(idMapFile))
            {
                string[] lines = File.ReadAllLines(idMapFile);
                foreach (string line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        string[] splitted = line.Split();
                        if (splitted.Length >= 2)
                        {
                            long ydkId, officialId;
                            if (long.TryParse(splitted[0], out ydkId) &&
                                long.TryParse(splitted[1], out officialId))
                            {
                                ydkIdToOfficialId[ydkId] = officialId;
                                officialIdToYdkId[officialId] = ydkId;
                            }
                        }
                    }
                }
            }

            if (ydkIdToOfficialId.Count > 0)
            {
                ValidateCardIds();
            }
        }

        public static void GenerateIdMap()
        {
            ydkIdToOfficialId.Clear();
            officialIdToYdkId.Clear();

            Dictionary<string, CardInfo> alternativeCardNames = new Dictionary<string, CardInfo>();
            foreach (CardInfo card in Program.Manager.CardManager.Cards.Values)
            {
                string name = card.Name.English;
                bool alternativeName = false;
                if (name.Contains("#"))
                {
                    name = name.Replace("#", string.Empty);
                    alternativeName = true;
                }
                if (name.Contains("・"))
                {
                    name = name.Replace("・", string.Empty);
                    alternativeName = true;
                }
                if (name.Contains("β"))
                {
                    name = name.Replace("β", "B");
                    alternativeName = true;
                }
                if (name.Contains("α"))
                {
                    name = name.Replace("α", "Alpha");
                    alternativeName = true;
                }
                if (name.Contains("The"))
                {
                    name = name.Replace("The", "the");
                    alternativeName = true;
                }
                if (alternativeName)
                {
                    alternativeCardNames[name] = card;
                }
            }
            // Manually fix a few others
            AddAlternativeCard(alternativeCardNames, 4149, "Necrolancer the Time-lord");//Necrolancer the Timelord
            AddAlternativeCard(alternativeCardNames, 4197, "LaLa Li-Oon");//LaLa Li-oon
            AddAlternativeCard(alternativeCardNames, 4254, "Master  Expert");//Master & Expert
            AddAlternativeCard(alternativeCardNames, 4571, "Man-Eating Black Shark");//Man-eating Black Shark
            AddAlternativeCard(alternativeCardNames, 5362, "Muko");//Null and Void
            AddAlternativeCard(alternativeCardNames, 5394, "After The Struggle");//After the Struggle
            AddAlternativeCard(alternativeCardNames, 5588, "Vampiric Orchis");//Vampire Orchis
            AddAlternativeCard(alternativeCardNames, 6199, "B.E.S. Big Core");//Big Core
            AddAlternativeCard(alternativeCardNames, 8072, "Supernatural Regeneration");//Metaphysical Regeneration
            AddAlternativeCard(alternativeCardNames, 8835, "Silent Graveyard");//Forbidden Graveyard
            AddAlternativeCard(alternativeCardNames, 8858, "Vampiric Koala");//Vampire Koala
            // Link Evolution (2020)
            AddAlternativeCard(alternativeCardNames, 6992, "Cu Chulainn the Awakened");//Cú Chulainn the Awakened
            AddAlternativeCard(alternativeCardNames, 7331, "Fiendish Engine Omega");//Fiendish Engine Ω
            AddAlternativeCard(alternativeCardNames, 7760, "Machine Lord Ur");//Machine Lord Ür
            AddAlternativeCard(alternativeCardNames, 7790, "Beast Machine King Barbaros Ur");//Beast Machine King Barbaros Ür
            AddAlternativeCard(alternativeCardNames, 8330, "Falchion Beta");//Falchionβ
            AddAlternativeCard(alternativeCardNames, 9683, "Damage Vaccine Omega MAX");//Damage Vaccine Ω MAX
            AddAlternativeCard(alternativeCardNames, 10771, "Marina, Princess of Sunflowers");//Mariña, Princess of Sunflowers
            AddAlternativeCard(alternativeCardNames, 10925, "Chirubime, Princess of Autumn Leaves");//Chirubimé, Princess of Autumn Leaves
            AddAlternativeCard(alternativeCardNames, 12865, "Abyss Actor - Twinkle Littlestar");//Abyss Actor - Twinkle Little Star
            AddAlternativeCard(alternativeCardNames, 12996, "Gandora Giga Rays the Dragon of Destruction");//Gigarays Gandora the Dragon of Destruction
            AddAlternativeCard(alternativeCardNames, 13017, "Number F0: Utopic Future - Future Slash");//Number F0: Utopic Future Slash
            AddAlternativeCard(alternativeCardNames, 13018, "Raidraptor - Revolution Falcon - Airraid");//Raidraptor - Revolution Falcon
            AddAlternativeCard(alternativeCardNames, 13203, "Tri-gate Wizard");//Tri-Gate Wizard
            AddAlternativeCard(alternativeCardNames, 13453, "Fire Opalhead");//Fire Opal Head
            AddAlternativeCard(alternativeCardNames, 13457, "Linklebell");//Linkerbell <--- not on ygoprodeck?
            AddAlternativeCard(alternativeCardNames, 13840, "Hope Magician");//Magician of Hope
            AddAlternativeCard(alternativeCardNames, 14221, "Utopic Onomatopeia");//Utopic Onomatopoeia
            AddAlternativeCard(alternativeCardNames, 14341, "Performapal Kuribohrder");//Performapal Kuribohble
            AddAlternativeCard(alternativeCardNames, 14383, "Card of Spirit");//Card of Fate
            AddAlternativeCard(alternativeCardNames, 14677, "Seraphim Papillon");//Seraphim Papillion
            AddAlternativeCard(alternativeCardNames, 14862, "A.I.Love Yousion");//A.I. Love Fusion

            using (WebClient client = new WebClient())
            {
                client.Proxy = null;
                string json = client.DownloadString("https://db.ygoprodeck.com/api/v4/cardinfo.php");
                YgoProCardJson[][] cardsArray = JsonSerializer<YgoProCardJson[][]>.Deserialize(json);
                if (cardsArray.Length > 0 && cardsArray[0].Length > 0)
                {
                    foreach (YgoProCardJson card in cardsArray[0])
                    {
                        if (!ydkIdToOfficialId.ContainsKey(card.id))
                        {
                            CardInfo cardInfo = Program.Manager.CardManager.FindCardByName(Language.English, card.name);
                            if (cardInfo == null)
                            {
                                alternativeCardNames.TryGetValue(card.name, out cardInfo);
                            }
                            if (cardInfo != null)
                            {
                                ydkIdToOfficialId[card.id] = cardInfo.CardId;
                                officialIdToYdkId[cardInfo.CardId] = card.id;
                            }
                        }
                    }
                }
            }

            using (TextWriter writer = File.CreateText(idMapFile))
            {
                foreach (KeyValuePair<long, long> cardId in ydkIdToOfficialId)
                {
                    writer.WriteLine(cardId.Key + " " + cardId.Value);
                }
            }

            if (ydkIdToOfficialId.Count > 0)
            {
                ValidateCardIds();
            }
        }

        private static void AddAlternativeCard(Dictionary<string, CardInfo> alternativeCardNames, short id, string name)
        {
            CardInfo card;
            if (Program.Manager.CardManager.Cards.TryGetValue(id, out card))
            {
                alternativeCardNames[name] = card;
            }
        }

        private static void ValidateCardIds()
        {
            int numMissingCards = 0;
            foreach (CardInfo card in Program.Manager.CardManager.Cards.Values)
            {
                if (!officialIdToYdkId.ContainsKey(card.CardId) && card.CardId != 0)
                {
                    numMissingCards++;
                    Debug.WriteLine("Couldn't find YDK card '" + card.Name.English + "' (" + card.CardId + ")");
                }
            }
            if (numMissingCards > 0)
            {
                Debug.WriteLine("Failed to find " + numMissingCards + " YDK card ids");
            }
        }

        [DataContract]
        class YgoProCardJson
        {
            [DataMember]
            public long id;
            [DataMember]
            public string name;
        }

        static class JsonSerializer<T> where T : class
        {
            public static T Deserialize(string json)
            {
                using (MemoryStream stream = new MemoryStream(Encoding.Unicode.GetBytes(json)))
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                    return serializer.ReadObject(stream) as T;
                }
            }
        }
    }
}
