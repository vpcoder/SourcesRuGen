namespace SourcesRuGen.Config
{

    public interface IConfiguration
    {
        string BotID    { get; }
        long   ChatID   { get; }
        int    ThreadID { get; }
        string SDHost   { get; }
        string SHOutput { get; }
        string TmpPath  { get; }
        long   MaxWait  { get; }
    }

}