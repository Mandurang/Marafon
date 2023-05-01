using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Models
{
    public class User : BaseEntity
    {
        public UserType UserType { get; set; }
        public long IdTelegram { get; set; }
        public string Name { get; set; }
        public string TelegramUserName { get; set; }
        public string Email { get; set; }
        public decimal Deposit { get; set; }
        public List<Signal> Signals { get; set; }


        /// <summary>
        /// User
        /// </summary>
        public User(UserType userType, long idTelegram, string name, string telegramUserName, string email, decimal deposit, List<Signal> signals)
        {
            UserType = userType;
            IdTelegram = idTelegram;
            Name = name;
            TelegramUserName = telegramUserName;
            Email = email;
            Deposit = deposit;
            Signals = signals;
        }

        public User()
        {

        }
    }

    public enum UserType
    {
        Admin = 1,
        User = 2
    }
}
