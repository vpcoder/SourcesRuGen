using System;
using System.Collections.Generic;
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
                    botClient.SendTextMessageAsync(_chatId, "Останавливаюсь...", _tittheme).Wait();
                    Environment.Exit(0);
                }
            }
        }

        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }

        
        public void StartBot(string botId, long chatId, int threadId)
        {
            _chatId   = chatId;
            _tittheme = threadId;
            
            var bot = new TelegramBotClient(botId);
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
        
        private static long _chatId;
        private static int _tittheme;

        public void Send(ICollection<string> files, string message)
        {
            if(lastBotLink == null)
                return;
            
            List<FileStream>       streamData = new List<FileStream>();
            List<IAlbumInputMedia> album      = new List<IAlbumInputMedia>(files.Count);
            int                    i          = 0;
            foreach (var file in files)
            {
                var stream = new FileStream(file, FileMode.Open);
                streamData.Add(stream);
                InputMediaPhoto photoElement = new InputMediaPhoto(InputFile.FromStream(stream, Path.GetFileName(file)));
                photoElement.HasSpoiler = true;
                photoElement.Caption    = Path.GetFileName(file);
                album.Add(photoElement);
                i++;
            }
            lastBotLink.SendMediaGroupAsync(_chatId, album, _tittheme).Wait();
            Thread.Sleep(1500);
            lastBotLink.SendTextMessageAsync(_chatId, message, _tittheme).Wait();

            foreach (var stream in streamData)
                stream.Close();
            
            streamData.Clear();
            album.Clear();
        }
    }
}