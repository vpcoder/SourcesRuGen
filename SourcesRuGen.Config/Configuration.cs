using System;
using System.Configuration;

namespace SourcesRuGen.Config
{
    
    public class Configuration : IConfiguration
    {

        private static readonly Lazy<Configuration> instance = new Lazy<Configuration>(() => new Configuration());
        public static           IConfiguration      Instance => instance.Value;
        
        public string BotID         { get; private set; }
        public long   ChatID        { get; private set; }
        public int    ThreadID      { get; private set; }
        public string SDHost        { get; private set; }
        public string SHOutput      { get; private set; }
        public bool   SDRandomModel { get; private set; }
        public string TmpPath       { get; private set; }
        public long   MaxWait       { get; private set; }
        public long   Interval      { get; private set; }
        public bool   SendToTG      { get; private set; }
        public bool   Generation    { get; private set; }
        public bool   TaskRunFirst  { get; private set; }

        private Configuration()
        {
            Reload();
        }

        public void Reload()
        {
            ConfigurationManager.RefreshSection("appSettings");
            
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
            TaskRunFirst  = bool.Parse(ConfigurationManager.AppSettings["TaskRunFirst"]);
        }
        
    }

}