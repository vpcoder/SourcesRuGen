using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Polling;
using SourcesRuGen.Config;
using Telegram.Bot.Types.Enums;

namespace SourcesRuGen.TG
{

    public class BotHelper
    {

        private ITelegramBotClient lastBotLink;
        
        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if(update.Type == UpdateType.Message)
            {
                var message = update.Message;
                if (message.Text?.ToLower() == "/start_tits")
                {
                    lastBotLink = botClient;
                    Console.WriteLine("client setup complete");
                }
                if (message.Text?.ToLower() == "/exit")
                {
                    Environment.Exit(0);
                }
            }
        }

        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }

        
        public void StartBot()
        {
            var config = Configuration.Instance;

            var bot = new TelegramBotClient(config.BotID);
            Console.WriteLine("bot started " + bot.GetMeAsync().Result.FirstName);

            var cts               = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new[] { UpdateType.Message },
            };
            bot.StartReceiving(
                               HandleUpdateAsync,
                               HandleErrorAsync,
                               receiverOptions,
                               cancellationToken
                              );
        }

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
            
            var config = Configuration.Instance;
            lastBotLink.SendMediaGroupAsync(config.ChatID, album, config.ThreadID).Wait();
            Thread.Sleep(500);
            lastBotLink.SendTextMessageAsync(config.ChatID, message, config.ThreadID).Wait();

            foreach (var stream in streamData)
                stream.Close();
            
            streamData.Clear();
            album.Clear();
        }
    }
}