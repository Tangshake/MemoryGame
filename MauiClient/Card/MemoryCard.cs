using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryGame.Card
{
    public class MemoryCard
    {
        public int SlotNumber { get; set; }

        public int Number { get; set; }

        public bool Enabled { get; set; } = true;

        public bool Reversed { get; set; } = false;

        public string ReverseImage { get; set; }
        public string ObverseImage { get; set; }

        public bool FilpTheCard()
        {
            // If card is already paired it should be disabled
            if (!Enabled) return false;

            // If card is already reversed do nothing
            if (Reversed)
                return false;

            Reversed = true;

            return true;
        }

        public string Show
        {
            get
            {
                return Reversed ? Number.ToString() : "X";
            }
        }
    }
}
