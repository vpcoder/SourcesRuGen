using System;
using System.Configuration;

namespace SourcesRuGen.Config
{
    
    public class Configuration : IConfiguration
    {

        private static readonly Lazy<Configuration> instance = new Lazy<Configuration>(() => new Configuration());
        public static           IConfiguration      Instance => instance.Value;
        
        public string BotID         { get; }
        public long   ChatID        { get; }
        public int    ThreadID      { get; }
        public string SDHost        { get; }
        public string SHOutput      { get; }
        public bool   SDRandomModel { get; }
        public string TmpPath       { get; }
        public long   MaxWait       { get; }
        public long   Interval      { get; }
        public bool   SendToTG      { get; }
        public bool   Generation    { get; }

        private Configuration()
        {
            BotID         = ConfigurationManager.AppSettings["BotID"];
            ChatID        = long.Parse(ConfigurationManager.AppSettings["ChatID"]);
            ThreadID      = int.Parse(ConfigurationManager.AppSettings["ThreadID"]);
            SDHost        = ConfigurationManager.AppSettings["SDHost"];
            SHOutput      = ConfigurationManager.AppSettings["SDOutput"];
            SDRandomModel = bool.Parse(ConfigurationManager.AppSettings["SDRandomModel"]);
            TmpPath       = ConfigurationManager.AppSettings["TmpPath"];
            MaxWait       = long.Parse(ConfigurationManager.AppSettings["MaxWait"]);
            Interval      = long.Parse(ConfigurationManager.AppSettings["Interval"]);
            SendToTG      = bool.Parse(ConfigurationManager.AppSettings["SendToTG"]);
            Generation    = bool.Parse(ConfigurationManager.AppSettings["Generation"]);
        }
        
    }

}