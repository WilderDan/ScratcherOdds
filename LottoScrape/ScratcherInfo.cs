using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LottoScrape
{
    internal class ScratcherInfo
    {
        public string Name { get; set; }
        public int TicketPrice { get; set; }
        public double OneInChanceWin { get; set; }
        public string ImageUrl { get; set; }
        public string StartDate { get; set;}
        public string EndDate { get; set;}
        public string ExpireDate { get; set;}
        public int TopPrize {  get; set; }
        public string PageUrl { get; set; }

        public List<PrizeRow> PrizeTable { get; set; }

        public int TotalPrizeCount { 
            get {
                int total = 0;

                foreach(var row in PrizeTable)
                {
                    total += row.TotalPrizes;
                }
                return total;
            } 
        }

        public int TotalTickets
        {
            get
            {
                return (int) (TotalPrizeCount * OneInChanceWin);
            }
        }

        public int TotalClaimed
        {
            get
            {
                int total = 0;

                foreach( var row in PrizeTable)
                {
                    total += row.TotalPrizes - row.UnclaimedPrizes;
                }

                return total;
            }
        }

        public int TotalTicketsBought
        {
            get
            {
                return (int)(TotalClaimed / (1 / OneInChanceWin));
            }
        }

        public int RemainingTickets
        {
            get
            {
                return TotalTickets - TotalTicketsBought;
            }
        }

        public double ExpectedWinOnLaunch
        {
            get
            {
                double total = 0.0;

                foreach ( var row in PrizeTable)
                {
                    total += (1.0 * row.TotalPrizes / TotalTickets) * row.PrizeLevel;
                }

                return Math.Round(total, 2);
            }
        }

        public double ExpectedWinAdjusted
        {
            get
            {
                double total = 0.0;

                foreach (var row in PrizeTable)
                {
                    total += (1.0 * row.UnclaimedPrizes / RemainingTickets) * row.PrizeLevel;
                }

                return Math.Round(total, 2);
            }
        }

        public double ExpectedValueOnLaunch
        {
            get
            {
                return ExpectedWinOnLaunch - TicketPrice;
            }
        }

        public double ExpectedValueAdjusted
        {
            get
            {
                return ExpectedWinAdjusted - TicketPrice;
            }
        }
    }

    record PrizeRow(int PrizeLevel, int TotalPrizes, int UnclaimedPrizes);
}
