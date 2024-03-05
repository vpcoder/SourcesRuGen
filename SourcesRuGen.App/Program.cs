using System;
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
            BotHelper botHelper = new BotHelper();
            botHelper.StartBot(ConfigurationManager.AppSettings["BotID"], long.Parse(ConfigurationManager.AppSettings["ChatID"]), int.Parse(ConfigurationManager.AppSettings["ThreadID"]));
            
            Console.ReadLine();

            DoGenIteration(botHelper);
            PeriodicTask.Run(() =>
            {
                DoGenIteration(botHelper);
            }, TimeSpan.FromMinutes(40));
            
            Console.ReadLine();
        }

        private static void DoGenIteration(BotHelper botHelper, bool needGen = true)
        {
            var reader = new TagReader();
            var data   = reader.Read(AppDomain.CurrentDomain.BaseDirectory + "Data\\");
            var model  = new PromptGenerator().Generate(data);

            var sd = new StableDiffusion();
            
            if (needGen)
                sd.Call(model);
            
            var files = sd.GetFiles(model.Meta.BatchCount);
            if(files.Count == 0)
                return;
            botHelper.Send(files,  "Смотри что я щас нарисовал!\r\n" + (needGen? model.Positive : sd.GetFirstMeta(files)));
        }
        
    }

}