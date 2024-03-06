using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using SourcesRuGen.Config;
using SourcesRuGen.Prompts;

namespace SourcesRuGen.SD
{

    public class StableDiffusion
    {

        private string url;
        private string path;
        private string pathTmp;
        private long   maxWait;

        public StableDiffusion()
        {
            var config = Configuration.Instance;
            this.url     = config.SDHost;
            this.path    = config.SHOutput;
            this.pathTmp = config.TmpPath;
            this.maxWait = config.MaxWait;
        }
        
        public string GetMetaMessage()
        {
            return File.ReadAllText(GetMetaPath());
        }

        public void MoveToTmp(List<string> files)
        {
            foreach (var file in files)
            {
                File.Move(file, pathTmp + Path.GetFileName(file));
                var genName = GetGenTextName(file);
                File.Move(genName, pathTmp + Path.GetFileName(genName));
            }

            try
            {
                File.Delete(GetMetaPath());
            } catch (Exception ignore) { }
        }

        private string GetGenTextName(string file)
        {
            return file.Substring(0, file.Length - 4) + "_gen.txt";
        }
        
        public List<string> GetFiles(int count)
        {
            var last = GetLastDir();
            var files = new List<string>(Directory.GetFiles(last, "*.png"));
            files.Sort();

            var result = new List<string>(count);
            for (int i = files.Count - 1; i >= 0 && count > 0; i--, count--)
            {
                result.Add(files[i]);
            }
            return result;
        }

        private string GetLastDir()
        {
            var dir = new List<string>(Directory.GetDirectories(path));
            dir.Sort();
            return dir[dir.Count - 1];
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
            
            File.WriteAllText(GetMetaPath(), "json: " + promptModel.Meta.Name + "\r\nsd model: " + promptModel.Meta.CheckPoint + "\r\nprompt: " + promptModel.Positive);
            Post("txt2img", json);

            Console.WriteLine("\r\n\r\ncomplete gen, waiting write to disk...");
            Thread.Sleep(20000);
        }
        
        private string GetMetaPath()
        {
            var lastPath = GetLastDir();
            return lastPath + "\\meta.txt";
        }

        private string GetStr(float value)
        {
            return value.ToString("0.000").Replace(",", ".");
        }

        private void ChangeCheckPoint(PromptModel promptModel)
        {
            var json = "{\"sd_model_checkpoint\": \"" + promptModel.Meta.CheckPoint + "\", \"CLIP_stop_at_last_layers\": 2 }";
            Post("options", json);
            
            Console.WriteLine("\r\n\r\nswitch sd checkpoint delay...");
            Thread.Sleep(20000);
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
        }
        
    }

}