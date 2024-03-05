using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SourcesRuGen.Prompts
{

    public class PromptGenerator
    {

        private readonly TagCompiler compiler = new TagCompiler();
        private readonly TagChainGenerator tagChainGenerator = new TagChainGenerator();

        private readonly Random rnd = new Random();
        
        public PromptModel Generate(IDictionary<Meta, IDictionary<int, List<TagChunk>>> data)
        {
            var first = data.Keys.ToList()[rnd.Next(0, data.Keys.Count)];
            var chain  = tagChainGenerator.Generate(data[first]);
            return Generate(chain, first);
        }

        public PromptModel Generate(IList<TagChunk> chain, Meta meta)
        {
            var positive = new List<TagChunk>();
            var negative = new List<TagChunk>();
            foreach (var chunk in chain)
            {
                if(chunk.Direction == DirectionType.Positive)
                    positive.Add(chunk);
                else
                    negative.Add(chunk);
            }
            
            return new PromptModel
            {
                Positive = CompilePrompt(positive),
                Negative = CompilePrompt(negative),
                Meta = new Meta()
                {
                    Width = meta.Width,
                    Height = meta.Height,
                    Sampling = meta.Sampling,
                    Upscaler = meta.Upscaler,
                    UpscaleBy = meta.UpscaleBy,
                    HiresSteps = meta.HiresSteps,
                    DenoisingStrength = meta.DenoisingStrength,
                    BatchCount = meta.BatchCount,
                    CfgScale = meta.CfgScale,
                    UseADetailer = meta.UseADetailer,
                    CheckPoint = compiler.GetVariableValue(meta.CheckPoint),
                },
            };
        }
        
        private string CompilePrompt(IList<TagChunk> chain)
        {
            var list = new List<string>();
            foreach (var chunk in chain)
            {
                var text = compiler.Compile(chunk);
                if(string.IsNullOrWhiteSpace(text))
                    continue;
                list.Add(text);
            }
            return string.Join(", ", list);
        }

    }

}