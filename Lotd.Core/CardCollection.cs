using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lotd
{
    public class CardCollection
    {
        public List<short> CardIds { get; set; }

        public CardCollection()
        {
            CardIds = new List<short>();
        }

        public void Add(short cardId)
        {
            CardIds.Add(cardId);
        }

        public void Remove(short cardId)
        {
            CardIds.Remove(cardId);
        }

        public void RemoveAll(short cardId)
        {
            while (CardIds.Remove(cardId))
            {
            }
        }

        public void Clear()
        {
            CardIds.Clear();
        }

        public void Sort()
        {
            CardIds.Sort();
        }
    }
}
