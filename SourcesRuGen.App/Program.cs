using System;
using System.Collections.Generic;
using SourcesRuGen.Config;
using SourcesRuGen.Prompts;
using SourcesRuGen.SD;
using SourcesRuGen.TG;

namespace SourcesRuGenApp
{

    internal class Program
    {
        
        public static void Main(string[] args)
        {
            var config = Configuration.Instance;
            var botHelper       = new BotHelper();
            var stableDiffusion = new StableDiffusion();
            var data  = new TagReader().Read(AppDomain.CurrentDomain.BaseDirectory + "Data\\");

            if (config.SendToTG)
            {
                botHelper.StartBot();
                Console.WriteLine("send /start_tits to me in telegram");
                Console.ReadLine();
            }

            PeriodicTask.Run(() =>
            {
                DoGenIteration(stableDiffusion, botHelper, data, config);
            }, TimeSpan.FromMinutes(config.Interval));

            for (;;)
            {
                var cmd = Console.ReadLine();
                if (cmd == "exit")
                    break;
            }
        }

        private static void DoGenIteration(StableDiffusion sd, BotHelper botHelper, IDictionary<Meta, IDictionary<int, List<TagChunk>>> data, IConfiguration config)
        {
            try
            {
                var generator = new PromptGenerator();
                var tree  = generator.GenetareChain(data);
                var model = generator.Generate(tree);
                
                var files = sd.GetFiles(model.Meta.BatchCount);
                if (files.Count == 0 || !config.SendToTG)
                {

                    if (config.SDRandomModel)
                    {
                        sd.ChangeCheckPoint(model);
                    }

                    for (;;)
                    {
                        Console.WriteLine($"json: {model.Meta.Name}\r\nmodel: {model.Meta.CheckPoint}\r\npositive:\r\n{model.Positive}\r\n\r\nnegative:\r\n{model.Negative}");
                        if (config.Generation)
                        {
                            sd.Call(model);
                        }

                        model = generator.Generate(tree);
                        
                        files = sd.GetFiles(model.Meta.BatchCount);
                        if (files.Count >= model.Meta.BatchCount)
                        {
                            break;
                        }
                    }
                }

                if (config.SendToTG)
                {
                    botHelper.Send(files, "Смотри, это я нарисовал!\r\n" + sd.GetMetaMessage());
                    sd.MoveToTmp(files);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
            }
        }
        
    }

}