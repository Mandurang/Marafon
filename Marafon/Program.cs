using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Polling;
using Microsoft.VisualBasic;
using Microsoft.EntityFrameworkCore;
using Marafon.DbContexts;
using Telegram.Bot.Types.ReplyMarkups;
using Marafon.KeyBoard;
using Marafon;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Requests;
using ConsoleApp1.Models;
using Microsoft.Bot.Schema.Teams;
using System.Globalization;

namespace ConsoleApp1
{
    public class Program
    {
        private static ApplicationDbContext _db;
        public static ITelegramBotClient bot = new TelegramBotClient("6258949261:AAGWtcYwVDlVBIg2V-s10teor_9TOVRtUkc");

        public static void Main()
        {
            string connetionString = "Host=localhost;Database=Marafon;Username=postgres;Password=qwerty!";

            //var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            //optionsBuilder.UseNpgsql(connetionString);

            Console.WriteLine("Запущен бот" + bot.GetMeAsync().Result.FirstName);
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { },// разрешено получать все виды апдейтов
            };
            bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
                );
            Console.WriteLine($"Начал прослушку");
            Console.ReadLine();

            cts.Cancel();
        }

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update?.Message?.Text != null)
            {
                var message = update.Message;

                // Обработка команды /sig
                if (message.Text.ToLower().StartsWith("/sig"))
                {
                    // Разбираем аргументы команды
                    var args = message.Text.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (args.Length < 5)
                    {
                        // Некорректные аргументы команды
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Некорректные аргументы команды /sig.", cancellationToken: cancellationToken);
                        return;
                    }

                    // Извлекаем значения аргументов
                    var positionType = args[0].ToLower() == "long" ? PositionType.PositionLong : PositionType.PositionShort;
                    var coinName = args[1];
                    var creditLeverage = int.Parse(args[2]);
                    var entryPrice = decimal.Parse(args[3], CultureInfo.InvariantCulture);
                    var timeSignal = args[4];
                    

                    // Создаем новый сигнал
                    var signal = new Signal((int)message.From.Id, creditLeverage, coinName, entryPrice, Currency.USDT, positionType, DateTime.UtcNow);

                    // Сохраняем сигнал в базе данных
                    _db.Signals.Add(signal);
                    await _db.SaveChangesAsync();

                    var subscribers = _db.Users.Include(s => s.TelegramUserName).ToList();

                    // Отправляем сообщение с подтверждением
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Новый сигнал успешно создан.", cancellationToken: cancellationToken);

                    

                    foreach (var subscriber in subscribers)
                    {
                        try
                        {
                            // Создаем сообщение с новым сигналом для отправки
                            string sigMessage = $"Новый сигнал: {signal.PositionType} {signal.CoinName} {signal.EntryPrice} {signal.CreditLeveraging}x";
                            var messageToUser = new Message
                            {
                                Text = sigMessage,
                                Chat = new Chat
                                {
                                    Id = subscriber.IdTelegram
                                }
                            };

                            // Отправляем сообщение подписчику
                            await botClient.SendTextMessageAsync(message.Chat.Id, message.Text, cancellationToken: cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            // Обработка ошибок при отправке сообщения
                            Console.WriteLine($"Ошибка при отправке сообщения пользователю {subscriber.Id}: {ex.Message}");
                        }
                    }
                }
            }
        }

        public static Task HandleErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Ошибка телеграм АПИ:\n{apiRequestException.ErrorCode}\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        //public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        //{
        //    if (update.Type == UpdateType.Message && update?.Message?.Text != null)
        //    {
        //        await HandleMessage(botClient, update.Message);
        //        return;
        //    }

        //    if (update.Type == UpdateType.CallbackQuery)
        //    {
        //        await HandleCallbackQuery(botClient, update.CallbackQuery);
        //        return;
        //    }
        //}

        //public static async Task HandleMessage(ITelegramBotClient botClient, Message message)
        //{
        //    if (message.Text.ToLower() == "/start")
        //    {
        //        await botClient.SendTextMessageAsync(message.Chat.Id, "Choose commands: /Statistick | /Signal");
        //    }

        //    if (message.Text.ToLower() == "/signal")
        //    {
        //        InlineKeyboardMarkup StartRegSignal = new(new[]
        //            {
        //            new[]
        //            {
        //                InlineKeyboardButton.WithCallbackData(text: "Позиция", callbackData: "PositionType"),
        //            },
        //            new[]
        //            {
        //                InlineKeyboardButton.WithCallbackData(text: "Имя монеты", callbackData: "СoinName"),
        //            },
        //            new[]
        //            {
        //                InlineKeyboardButton.WithCallbackData(text: "Кредитное Плечо", callbackData: "СreditLeveraging"),
        //            },
        //            new[]
        //            {
        //                InlineKeyboardButton.WithCallbackData(text: "Точка входа", callbackData: "EntryPrice"),
        //            },
        //            new[]
        //            {
        //                InlineKeyboardButton.WithCallbackData(text: "Время отправки сигнала", callbackData: "Created")
        //            },
        //        });


        //        var sendMessageRequest = new SendMessageRequest(message.Chat.Id, "Выберите параметр:");
        //        sendMessageRequest.ReplyMarkup = StartRegSignal;

        //        await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите пункт меню:", replyMarkup: StartRegSignal);
        //    }
        //}


        //public static async Task HandleCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        //{
        //    switch (callbackQuery.Data)
        //    {
        //        case "PositionType":
        //            // сохраняем информацию о выбранном типе позиции в базу данных
        //            var userIdPosition = callbackQuery.From.Id;
        //            var userPosition = await _db.Users.FindAsync(userIdPosition);

        //            // извлекаем PositionType из первого сигнала пользователя
        //            var positionType = userPosition.Signals.FirstOrDefault()?.PositionType;

        //            // отправляем сообщение пользователю с найденным PositionType
        //            await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"PositionType: {positionType}");
        //            await _db.SaveChangesAsync();
        //            break;

        //        case "СoinName":
        //            // код для обработки выбранного параметра
        //            break;
        //        case "СreditLeveraging":
        //            var creditLeveragingStr = callbackQuery.Data;
        //            if (int.TryParse(creditLeveragingStr, out int creditLeveraging))
        //            {
        //                // сохраняем информацию о выбранном кредитном рычаге в базу данных
        //                var userIdCredit = callbackQuery.From.Id;
        //                var userCredit = await _db.Users.FindAsync(userIdCredit);
        //                var signalСredit = userCredit.Signals.LastOrDefault(); // получаем последний добавленный сигнал пользователя
        //                if (signalСredit != null)
        //                {
        //                    signalСredit.CreditLeveraging = creditLeveraging;
        //                    await _db.SaveChangesAsync();
        //                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Кредитный рычаг успешно сохранен: {creditLeveraging}");
        //                }
        //                else
        //                {
        //                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Не удалось сохранить кредитный рычаг");
        //                }
        //            }
        //            else
        //            {
        //                await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Не удалось сохранить кредитный рычаг, выбранный вами вариант не является числом");
        //            }
        //            break;
        //        case "EntryPrice":
        //            var entryPriceStr = callbackQuery.Data;
        //            if (decimal.TryParse(entryPriceStr, out decimal entryPrice))
        //            {
        //                // сохраняем информацию о выбранной цене входа в базу данных
        //                var userIdEntryPrice = callbackQuery.From.Id;
        //                var userEntryPrice = await _db.Users.FindAsync(userIdEntryPrice);
        //                var signalEntry = userEntryPrice.Signals.LastOrDefault(); // получаем последний добавленный сигнал пользователя
        //                if (signalEntry != null)
        //                {
        //                    signalEntry.EntryPrice = entryPrice;
        //                    await _db.SaveChangesAsync();
        //                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Цена входа успешно сохранена: {entryPrice}");
        //                }
        //                else
        //                {
        //                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Не удалось сохранить цену входа");
        //                }
        //            }
        //            else
        //            {
        //                await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Не удалось сохранить цену входа, выбранный вами вариант не является числом");
        //            }
        //            break;
        //        case "Created":
        //            var createdStr = callbackQuery.Data;
        //            if (DateTime.TryParse(createdStr, out DateTime created))
        //            {
        //                // сохраняем информацию о выбранной дате создания сигнала в базу данных
        //                var userIdCreated = callbackQuery.From.Id;
        //                var userCreated = await _db.Users.FindAsync(userIdCreated);
        //                var signalCreated = userCreated.Signals.LastOrDefault(); // получаем последний добавленный сигнал пользователя
        //                if (signalCreated != null)
        //                {
        //                    signalCreated.Created = created;
        //                    await _db.SaveChangesAsync();
        //                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Дата создания успешно сохранена: {created}");
        //                }
        //                else
        //                {
        //                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Не удалось сохранить дату создания");
        //                }
        //            }
        //            else
        //            {
        //                await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Не удалось сохранить дату создания, выбранный вами вариант не является датой");
        //            }
        //            break;
        //        case "SendSignal":
        //            var signalData = callbackQuery.Data;
        //            var signalParts = signalData.Split(';');

        //            // проверяем, что количество параметров в сообщении правильное
        //            if (signalParts.Length != 6)
        //            {
        //                await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Ошибка сохранения сигнала: неверное количество параметров.");
        //                break;
        //            }

        //            // парсим параметры сигнала
        //            var coinName = signalParts[0];
        //            var entryPriceString = signalParts[1];
        //            var markPriceStr = signalParts[2];
        //            var currencyStr = signalParts[3];
        //            var positionTypeStr = signalParts[4];
        //            var description = signalParts[5];

        //            // проверяем корректность введенных данных
        //            if (!decimal.TryParse(entryPriceString, out decimal entryAnyPrice) ||
        //                !decimal.TryParse(markPriceStr, out decimal markPrice) ||
        //                !Enum.TryParse(currencyStr, out Currency currency) ||
        //                !Enum.TryParse(positionTypeStr, out PositionType positionAnyType))
        //            {
        //                await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Ошибка сохранения сигнала: неверный формат параметров.");
        //                break;
        //            }

        //            // получаем пользователя и сохраняем сигнал в базу данных
        //            var userId = callbackQuery.From.Id;
        //            var user = await _db.Users.FindAsync(userId);
        //            if (user == null)
        //            {
        //                await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Ошибка сохранения сигнала: пользователь не найден.");
        //                break;
        //            }
        //            var signal = new Signal((int)userId, 0, coinName, entryAnyPrice, currency, positionAnyType, DateTime.UtcNow);
        //            _db.Signals.Add(signal);
        //            await _db.SaveChangesAsync();

        //            // отправляем сообщение пользователю с подтверждением сохранения сигнала
        //            await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Сигнал сохранен:\n{signal}");
        //            break;
        //    }
        //}


        //private static async Task SendSignalToAllUsers(ITelegramBotClient botClient, string signalText)
        //{
        //    // получаем список всех пользователей из базы данных
        //    var users = await _db.Users.ToListAsync();

        //    // отправляем сигнал каждому пользователю
        //    foreach (var user in users)
        //    {
        //        // проверяем, что это не бот и пользователь имеет роль "User"
        //        if (user.IdTelegram != 0 && user.UserType == UserType.User)
        //        {
        //            await botClient.SendTextMessageAsync(chatId: user.IdTelegram, text: signalText);
        //        }
        //    }
        //}



        //public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        //{
        //    Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));

        //    if(update.Type == UpdateType.Message && update?.Message?.Text != null)
        //    {
        //        string Time = DateTime.Now.ToShortTimeString();
        //        var message = update.Message;

        //        if(message.Text.ToLower() == "/sig")
        //        {
        //            await botClient.SendTextMessageAsync(
        //                message.Chat.Id,
        //                "load...",
        //                replyMarkup: new ReplyKeyboardRemove());
        //        }

        //        var userData = await DbMethods.GetUserRole(message.From.Id);
        //        if (userData != null)
        //        {
        //            await botClient.SendTextMessageAsync(
        //                message.Chat.Id,
        //                text: "Вы уже зарегистрировались",
        //                replyMarkup: KeyBoards..ToMenu);
        //        }
        //    }

        //    if (update.Type == UpdateType.Message /*тут будет проверка на админа или на пользывателя*/)
        //    {
        //        string Time = DateTime.Now.ToShortTimeString();
        //        var message = update.Message;
        //        if (message.Text == "/start")
        //        {
        //            //await DataBaseMethods.ToggleInDialogsStatus(update.Message.Chat.Id, 0);
        //            await botClient.SendTextMessageAsync(
        //                chatId: message.Chat,
        //                text: "Главное меню",
        //                replyMarkup: KeyBoards.Menu);
        //            return;
        //        }


        //    }
        //    if (update.Type == UpdateType.CallbackQuery)
        //    {
        //        // Тут получает нажатия на inline кнопки
        //    }

        //}
        //public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        //{
        //    // Данный Хендлер получает ошибки и выводит их в консоль в виде JSON
        //    Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        //}


        //    if(message.Text == "/Signal")
        //    {
        //        ReplyKeyboardMarkup keyboard = new(new[]
        //        {
        //            new []
        //    {
        //        InlineKeyboardButton.WithCallbackData(text: "Позиция", callbackData: "PositionType"),
        //    },
        //    new []
        //    {
        //        InlineKeyboardButton.WithCallbackData(text: "Имя менты", callbackData: "СoinName"),
        //    },
        //    new []
        //    {
        //        InlineKeyboardButton.WithCallbackData(text: "Кредитное Плечо", callbackData: "СreditLeveraging"),
        //    },
        //    new []
        //    {
        //        InlineKeyboardButton.WithCallbackData(text: "Точка входа", callbackData: "EntryPrice"),
        //    },
        //    new []
        //    {
        //        InlineKeyboardButton.WithCallbackData(text: "Время отправки сигнала", callbackData: "Created"),
        //    },
        //        });
        //    }
        //}
        //public async static void Bot_OnMessage(object sender, MessageEventArgs e)
        //{
        //    if (e.Message.Text.ToLower() == "/signal")
        //    {
        //        if (e.Message.Type == MessageType.Text)
        //        {
        //            await SendTextMessage(e.Message.Text);
        //        }
        //        // Отправляем клавиатуру пользователю для получения значений
        //        var replyKeyboard = new ReplyKeyboardMarkup(new[]
        //        {
        //            new KeyboardButton("CoinName:"),
        //            new KeyboardButton("CreditLeveraging:"),
        //            new KeyboardButton("EntryPrice:"),
        //            new KeyboardButton("DateTime:"),
        //            new KeyboardButton("PositionType:")
        //        });

        //        await botClient.SendTextMessageAsync(chatId: e.Message.Chat, text: "Введите данные для создания нового сигнала:", replyMarkup: replyKeyboard);
        //    }
        //    else
        //    {
        //        //// Парсим сообщение и создаем новый сигнал
        //        //var signal = ParseMessage(e.Message.Text);

        //        //// Сохраняем новый сигнал в базе данных
        //        //await SaveSignal(signal);

        //        // Отправляем сообщение пользователю об успешном сохранении
        //        await botClient.SendTextMessageAsync(
        //            chatId: e.Message.Chat, 
        //            text: "Новый сигнал успешно отправлен");
        //    }
        //}

        //private async Task SaveSignal(Signal signal)
        //{
        //    using (var dbContext = new ApplicationDbContext())
        //    {
        //        dbContext.Signals.Add(signal);
        //        await dbContext.SaveChangesAsync();
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

        //    // Создаем новый сигнал
        //    var signalAdmin = new Signal(userId, creditLeveraging, coinName, entryPrice, currency, positionType, dateTime);

        //    return signalAdmin;
        //}



        //private static async void Bot_OnMessage(object sender, MessageEventArgs e)
        //{
        //    if (IsAdmin(e.Message.From.Id, e.Message.From.Username) && e.Message.Chat.Type == ChatType.Private)
        //    {
        //        if (e.Message.Type == MessageType.Text)
        //        {
        //            await SendTextMessage(e.Message.Text);
        //        }
        //        else if (e.Message.Type == MessageType.Photo)
        //        {
        //            await SendPhoto(e.Message.Photo.LastOrDefault()?.FileId, e.Message.Caption);
        //        }
        //    }
        //}

        //private static Task SendPhoto(object fileId, string caption)
        //{
        //    throw new NotImplementedException();
        //}

        //private static async Task SendTextMessage(string text)
        //{
        //    try
        //    {
        //        //var messageToSignal = await botClient.SendTextMessageAsync(chatIdSignal, text, replyToMessageId: topicIdPotrock);
        //        //Console.WriteLine($"Message sent to topic {topicIdPotrock} with message ID {messageToSignal.MessageId}");

        //        //var messageToRusik = await botClient.SendTextMessageAsync(chatIdSignalRusick, text, replyToMessageId: topicIdRusick);
        //        //Console.WriteLine($"Message sent to topic {topicIdRusick} with message ID {messageToRusik.MessageId}");

        //        //var messageToLern = await botClient.SendTextMessageAsync(chatIdSignalLern, text, replyToMessageId: topicIdLern);
        //        //Console.WriteLine($"Message sent to topic {topicIdLern} with message ID {messageToLern.MessageId}");

        //        //var messageToKarter = await botClient.SendTextMessageAsync(chatIdSignalKarter, text, replyToMessageId: topicIdKarter);
        //        //Console.WriteLine($"Message sent to topic {topicIdKarter} with message ID {messageToLern.MessageId}");

        //        //var messageToSignalCryptoCasta = await botClient.SendTextMessageAsync(chatIdSignalCryptoCasta, text);
        //        //Console.WriteLine($"Message ID {messageToSignalCryptoCasta.MessageId}");
        //        ///Test group
        //        var messageToSignalNachalstvo = await botClient.SendTextMessageAsync(chatIdNachalstvo, text);
        //        Console.WriteLine($"Message ID {messageToSignalNachalstvo.MessageId}");
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error sending message: {ex.Message}");
        //    }
        //}
    }
}
