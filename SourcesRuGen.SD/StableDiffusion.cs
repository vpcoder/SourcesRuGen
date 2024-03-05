using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using SourcesRuGen.Prompts;

namespace SourcesRuGen.SD
{

    public class StableDiffusion
    {

        private string url = "http://127.0.0.1:7860/sdapi/v1/";
        private string path = @"D:\ai\stable-diffusion-webui\outputs\txt2img-images\";

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
                    response.Wait(12000000, CancellationToken.None); // 20 минут
                    break;
                }
                catch (Exception ex)
                {
                    iterationWait++;
                    Console.WriteLine("wait iteration " + iterationWait);
                    Thread.Sleep(60000);
                }
                if(iterationWait > 1000)
                    Environment.Exit(-1); // Зациклились, слишком долго ждём уже...
            }

            var text = response.Result.Content.ReadAsStringAsync();
            text.Wait();
            Console.WriteLine("\r\n\r\n"+ text.Result+ "\r\n-----------------------------------------------\r\n\r\n");
        }
        
    }

}