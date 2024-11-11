namespace SourcesRuGen.Config
{

    public interface IConfiguration
    {
        string BotID         { get; }
        long   ChatID        { get; }
        int    ThreadID      { get; }
        string SDHost        { get; }
        string SHOutput      { get; }
        bool   SDRandomModel { get; }
        string TmpPath       { get; }
        long   MaxWait       { get; }
        long   Interval      { get; }
        bool   SendToTG      { get; }
        bool   Generation    { get; }
    }

}