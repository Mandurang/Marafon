using ConsoleApp1.Models;
using Marafon.DbContexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using User = ConsoleApp1.Models.User;

namespace Marafon
{
    internal class DbMethods
    {
        private readonly ApplicationDbContext _context;

        public DbMethods(ApplicationDbContext context)
        {
            _context = context;
        }

        // метод для создания сообщения Aдмином
        public async Task<long> MsgCreateByAdminToUser(int userId, int creditLeveraging, string coinName, decimal entryPrice, Currency currency, PositionType positionType, DateTime created)
        {
            Signal newAdminSignal = new Signal(userId, creditLeveraging, coinName, entryPrice, currency, positionType, created);
            await _context.Signals.AddAsync(newAdminSignal);
            await _context.SaveChangesAsync();

            return newAdminSignal.Id;    
        }

        public async Task<long[]> MsgCreateByAdminToAllUsers(int creditLeveraging, string coinName, decimal entryPrice, Currency currency, PositionType positionType, DateTime created)
        {
            var users = await _context.Users.ToListAsync();
            var signalIds = new List<long>();

            foreach (var user in users)
            {
                var signalId = await MsgCreateByAdminToUser((int)user.Id, creditLeveraging, coinName, entryPrice, currency, positionType, created);
                signalIds.Add(signalId);
            }

            return signalIds.ToArray();
        }

        // метод для получения сообщений User 
        //public async Task<long> MsgRecievierUser(long idTelegram, int IdThread, long IdTc)
        //{
        //    User newUser = new User(userType, idTelegram, telegramUserName);
        //    var getMSG = await _context.Users.FirstOrDefaultAsync(x => x.idTelegram == idTelegram);
        //    var getId = await _context.DialogMembers.FirstOrDefaultAsync(x => (x.IdThread == IdThread && x.IdTc == IdTc));
        //    if (getMSG.IdThread == getId.IdThread)
        //    {
        //        return getId.IdDriver;
        //    }
        //    else
        //    {
        //        return 404;
        //    }
        //}

        //private Signal ParseMessage(string messageText, int userId)
        //{
        //    // Разбиваем сообщение на части и извлекаем данные
        //    var positionType = messageText == PositionType messageText.Split(':');
        //    var parts = messageText.Split(':');
        //    var coinName = parts[0].Split(':')[1].Trim();
        //    var creditLeveraging = int.Parse(parts[1].Split(':')[1].Trim());
        //    var entryPrice = decimal.Parse(parts[2].Split(':')[1].Trim(), CultureInfo.InvariantCulture);
        //    var dateTime = DateTime.Parse(parts[3].Split(':')[1].Trim(), CultureInfo.InvariantCulture);
        //    var currency = messageText.Split(':');

        //public static List<long> GetAllDriversId(string role)
        //{
        //    //Dictionary<long, string> driversId_Name = new Dictionary<long, string>();
        //    using (ApplicationContext db = new ApplicationContext())
        //    {
        //        var AllDrivers = db.UserRoles.Where(x => x.Role == role).ToList();
        //        List<long> driversIDs = new List<long>();
        //        foreach (var u in AllDrivers)
        //            driversIDs.Add(u.TgId);
        //        return driversIDs;
        //    }

        //}
    }
}
