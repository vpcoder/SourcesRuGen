using System;

namespace SourcesRuGen.Prompts
{

    [Serializable]
    public class Meta
    {
        public int      Width             { get; set; }
        public int      Height            { get; set; }
        public int      Sampling          { get; set; }
        public string   Upscaler          { get; set; }
        public float    UpscaleBy         { get; set; }
        public int      HiresSteps        { get; set; }
        public float    DenoisingStrength { get; set; }
        public int      BatchCount        { get; set; }
        public float    CfgScale          { get; set; }
        public bool     UseADetailer      { get; set; }
        public string   CheckPoint        { get; set; }
    }

}