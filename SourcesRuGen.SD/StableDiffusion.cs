using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SourcesRuGen.Prompts;

namespace SourcesRuGen.SD
{

    public class StableDiffusion
    {

        private string url = "http://127.0.0.1:7860/sdapi/v1/";
        private string path = @"D:\ai\stable-diffusion-webui\outputs\txt2img-images\";

        public string GetFirstMeta(List<string> files)
        {
            if (files.Count == 0)
                return null;
            var first   = files.FirstOrDefault();
            var genData = first.Substring(0, first.Length - 4) + "_gen.txt";
            return File.ReadAllLines(genData)[2];
        }
        
        public List<string> GetFiles(int count)
        {
            var dir = new List<string>(Directory.GetDirectories(path));
            dir.Sort();

            var last = dir[dir.Count - 1];

            var files = new List<string>(Directory.GetFiles(last, "*.png"));
            files.Sort();

            var result = new List<string>(count);
            for (int i = files.Count - 1; i >= 0 && count > 0; i--, count--)
            {
                result.Add(files[i]);
            }
            return result;
        }
        
        public void Call(PromptModel promptModel)
        {
            ChangeCheckPoint(promptModel);

            var json = @"{
  ""prompt"": """ + promptModel.Positive + @""",
  ""negative_prompt"": """ + promptModel.Negative + @""",
  ""batch_size"": " + promptModel.Meta.BatchCount + @",
  ""steps"": " + promptModel.Meta.Sampling + @",
  ""cfg_scale"": " + GetStr(promptModel.Meta.CfgScale) + @",
  ""width"": " + promptModel.Meta.Width + @",
  ""height"": " + promptModel.Meta.Height + @",
  ""restore_faces"": true,
  ""enable_hr"":  true,
  ""denoising_strength"": " + GetStr(promptModel.Meta.DenoisingStrength) + @",
  ""hr_second_pass_steps"": """ + promptModel.Meta.HiresSteps + @""",
  ""hr_upscaler"": """ + promptModel.Meta.Upscaler + @""",
  ""hr_scale"": " + GetStr(promptModel.Meta.UpscaleBy) + @",
  ""send_images"": false,
  ""save_images"": true
}";
            
            Post("txt2img", json);
        }

        private string GetStr(float value)
        {
            return value.ToString("0.000").Replace(",", ".");
        }

        private void ChangeCheckPoint(PromptModel promptModel)
        {
            var json = "{\"sd_model_checkpoint\": \"" + promptModel.Meta.CheckPoint + "\", \"CLIP_stop_at_last_layers\": 2 }";
            Post("options", json);
        }

        private void Post(string command, string json)
        {
            Console.WriteLine("post: " + url+command + "\r\nbody: \r\n" + json);
            var client        = new HttpClient();
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
            var response      = client.PostAsync(url + command, stringContent);

            int iterationWait = 0;
            for (;;)
            {
                try
                {
                    response.Wait();
                    break;
                }
                catch (Exception ex)
                {
                    iterationWait++;
                    Console.WriteLine("wait iteration " + iterationWait);
                    Thread.Sleep(60000);
                }
                if(iterationWait >= 17)
                    break; // Зациклились, слишком долго ждём уже...
            }

            try
            {
                response.Result.Content.ReadAsStreamAsync().Wait();
            }
            catch (Exception ex)
            { }

            Console.WriteLine("\r\n\r\ncomplete gen");
        }
        
    }

}