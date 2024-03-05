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
            var botHelper = new BotHelper();
            botHelper.StartBot(ConfigurationManager.AppSettings["BotID"], long.Parse(ConfigurationManager.AppSettings["ChatID"]), int.Parse(ConfigurationManager.AppSettings["ThreadID"]));
            var stableDiffusion = new StableDiffusion(ConfigurationManager.AppSettings["SDHost"], ConfigurationManager.AppSettings["SDOutput"], ConfigurationManager.AppSettings["TmpPath"], long.Parse(ConfigurationManager.AppSettings["MaxWait"]));
            
            Console.ReadLine();

            DoGenIteration(stableDiffusion, botHelper);
            PeriodicTask.Run(() =>
            {
                DoGenIteration(stableDiffusion, botHelper);
            }, TimeSpan.FromMinutes(40));
            
            
            Console.ReadLine();
        }

        private static void DoGenIteration(StableDiffusion sd, BotHelper botHelper, bool needGen = true)
        {
            var reader = new TagReader();
            var data   = reader.Read(AppDomain.CurrentDomain.BaseDirectory + "Data\\");
            var model  = new PromptGenerator().Generate(data);

            
            if (needGen)
                sd.Call(model);
            
            var files = sd.GetFiles(model.Meta.BatchCount);
            if(files.Count == 0)
                return;
            botHelper.Send(files,  "Смотри что я щас нарисовал!\r\n" + sd.GetFirstMeta(files));
            sd.MoveToTmp(files);
        }
        
    }

}