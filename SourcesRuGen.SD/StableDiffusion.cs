using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using SourcesRuGen.Prompts;

namespace SourcesRuGen.SD
{

    public class StableDiffusion
    {

        private string url;
        private string path;
        private string pathTmp;
        private long   maxWait;

        public StableDiffusion(string url, string path, string pathTmp, long maxWait)
        {
            this.url     = url;
            this.path    = path;
            this.pathTmp = pathTmp;
            this.maxWait = maxWait;
        }
        
        public string GetFirstMeta(List<string> files)
        {
            if (files.Count == 0)
                return null;
            var genData = GetGenTextName(files.FirstOrDefault());
            return File.ReadAllLines(genData)[14] + "\r\nprompt: " + File.ReadAllLines(genData)[2];
        }

        public void MoveToTmp(List<string> files)
        {
            foreach (var file in files)
            {
                File.Move(file, pathTmp + Path.GetFileName(file));
                var genName = GetGenTextName(file);
                File.Move(genName, pathTmp + Path.GetFileName(genName));
            }
        }

        private string GetGenTextName(string file)
        {
            return file.Substring(0, file.Length - 4) + "_gen.txt";
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
                
                if(GetFiles(1).Count != 0 || iterationWait >= maxWait)
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