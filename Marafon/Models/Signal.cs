using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Models
{
    public class Signal : BaseEntity
    {
        public int UserId { get; set; }
        public User User { get; set; }
        public string BirgeName { get; set; }
        public int CreditLeveraging { get; set; }
        public string CoinName { get; set; }
        public decimal Profit { get; set; }
        public decimal EntryPrice { get; set; }
        public decimal MarkPrice { get; set; } // - выходная соимость(когда вышел)
        public Currency? Currency { get; set; }
        public PositionType PositionType { get; set; }
        public DateTime Created { get; set; }
        public string Description { get; set; }


        /// <summary>
        /// Admin Signal
        /// </summary>
        public Signal(int userId, int creditLeveraging, string coinName, decimal entryPrice, Currency? currency, PositionType positionType, DateTime created)
        {
            UserId = userId;
            CreditLeveraging = creditLeveraging;
            CoinName = coinName;
            EntryPrice = entryPrice;
            Currency = currency;
            PositionType = positionType;
            Created = created;
        }
    }

    public enum Currency
    {
        USDT = 1,
    };

    public enum PositionType
    {
        PositionShort = 1,
        PositionLong = 2
    };
}


///// <summary>
///// User Signal
///// </summary>
//public Signal(int userId, string birgeName, int creditLeveraging, string coinName, decimal profit,
//              decimal entryPrice, decimal markPrice, Currency currency, PositionType positionType, DateTime created, string description)
//{
//    UserId = userId;
//    BirgeName = birgeName;
//    CreditLeveraging = creditLeveraging;
//    CoinName = coinName;
//    Profit = profit;
//    EntryPrice = entryPrice;
//    MarkPrice = markPrice;
//    Currency = currency;
//    PositionType = positionType;
//    Created = created;
//    Description = description;
//}