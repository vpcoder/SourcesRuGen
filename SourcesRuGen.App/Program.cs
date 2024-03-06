using System;
using System.Collections.Generic;
using System.Configuration;
using SourcesRuGen.Prompts;
using SourcesRuGen.SD;
using SourcesRuGen.TG;

namespace SourcesRuGenApp
{

    internal class Program
    {
        
        public static void Main(string[] args)
        {
            var botHelper = new BotHelper(); 
            botHelper.StartBot();
            var stableDiffusion = new StableDiffusion();
            
            Console.WriteLine("send /start_tits to me in telegram");
            Console.ReadLine();

            PeriodicTask.Run(() =>
            {
                DoGenIteration(stableDiffusion, botHelper);
            }, TimeSpan.FromMinutes(40));

            for (;;)
            {
                var cmd = Console.ReadLine();
                if (cmd == "exit")
                    break;
            }
        }

        private static void DoGenIteration(StableDiffusion sd, BotHelper botHelper)
        {
            try
            {
                var reader = new TagReader();
                var data   = reader.Read(AppDomain.CurrentDomain.BaseDirectory + "Data\\");
                var model  = new PromptGenerator().Generate(data);

                var files = sd.GetFiles(model.Meta.BatchCount);
                if (files.Count == 0)
                {
                    sd.Call(model);
                    files = sd.GetFiles(model.Meta.BatchCount);

                    if (files.Count == 0)
                        return;
                }

                botHelper.Send(files, "Смотри, это я нарисовал!\r\n" + sd.GetMetaMessage());
                sd.MoveToTmp(files);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
            }
        }
        
    }

}