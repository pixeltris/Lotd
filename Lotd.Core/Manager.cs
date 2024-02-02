using Lotd.FileFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Lotd
{
    public class Manager
    {
        public LotdArchive Archive { get; private set; }
        public DeckData DeckData { get; set; }
        public CharData CharData { get; set; }
        public SkuData SkuData { get; set; }
        public ArenaData ArenaData { get; set; }
        public DuelData DuelData { get; set; }
        public PackDefData PackDefData { get; set; }
        public List<BattlePackData> BattlePackData { get; private set; }
        public List<ShopPackData> ShopPackData { get; set; }

        public CardLimits CardLimits { get; set; }
        public CardManager CardManager { get; set; }

        public Language CurrentLanguage { get; set; }
        public GameVersion Version { get; private set; }

        public Manager(GameVersion version)
        {
            Version = version;
            Archive = new LotdArchive(version);
            CurrentLanguage = Language.English;

            BattlePackData = new List<BattlePackData>();
            ShopPackData = new List<ShopPackData>();
        }

        public void Load()
        {
            Archive.Load();

            BattlePackData.Clear();
            ShopPackData.Clear();

            DeckData = Archive.LoadLocalizedFile<DeckData>();
            CharData = Archive.LoadLocalizedFile<CharData>();
            SkuData = Archive.LoadLocalizedFile<SkuData>();
            ArenaData = Archive.LoadLocalizedFile<ArenaData>();
            DuelData = Archive.LoadLocalizedFile<DuelData>();
            PackDefData = Archive.LoadLocalizedFile<PackDefData>();
            CardLimits = Archive.LoadFiles<CardLimits>()[0];
            BattlePackData.AddRange(Archive.LoadFiles<BattlePackData>().ToList());
            ShopPackData.AddRange(Archive.LoadFiles<ShopPackData>().ToList());

            CardManager = new CardManager(this);
            CardManager.Load();
        }
    }
}
