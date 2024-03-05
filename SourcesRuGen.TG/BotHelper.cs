using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Polling;

namespace SourcesRuGen.TG
{

    public class BotHelper
    {
        
        private static ITelegramBotClient lastBotLink;
        
        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
            if(update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                var message = update.Message;
                if (message.Text.ToLower() == "/start_tits")
                {
                    lastBotLink = botClient;
                    //botClient.SendTextMessageAsync(chatId, "Я исправился, теперь прячу фотки под спойлеры...", tittheme);
                }
                if (message.Text.ToLower() == "/stop_tits")
                {
                    botClient.SendTextMessageAsync(chatId, "Останавливаюсь...", tittheme).Wait();
                    Environment.Exit(0);
                }
            }
        }

        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }

        
        public void StartBot()
        {
            chatId = long.Parse(ConfigurationManager.AppSettings["ChatID"]);
            tittheme = int.Parse(ConfigurationManager.AppSettings["ThreadID"]);
            
            var bot = new TelegramBotClient(ConfigurationManager.AppSettings["BotID"]);
            Console.WriteLine("bot started " + bot.GetMeAsync().Result.FirstName);

            var cts               = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { },
            };
            bot.StartReceiving(
                               HandleUpdateAsync,
                               HandleErrorAsync,
                               receiverOptions,
                               cancellationToken
                              );
        }
        
        private static long chatId;
        private static int tittheme;

        public void Send(ICollection<string> files, string message)
        {
            if(lastBotLink == null)
                return;
            
            lastBotLink.SendTextMessageAsync(chatId, message, tittheme);

            foreach (var file in files)
            {
                using (var stream = new FileStream(file, FileMode.Open))
                {
                    lastBotLink.SendPhotoAsync(chatId, InputFile.FromStream(stream, Path.GetFileName(file)), tittheme, hasSpoiler: true);
                }
            }
        }
    }
}