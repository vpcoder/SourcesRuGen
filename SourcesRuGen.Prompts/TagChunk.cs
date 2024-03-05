using System;

namespace SourcesRuGen.Prompts
{

    /// <summary>
    ///     Направление генерации
    /// </summary>
    public enum DirectionType
    {
        Positive,
        Negative,
    }

    /// <summary>
    ///     Звено (тэг) которое будет учавствовать в компиляции
    /// </summary>
    [Serializable]
    public class TagChunk
    {
        public int           Level     { get; set; }
        public string        Tag       { get; set; }
        public int           Chance    { get; set; }
        public DirectionType Direction { get; set; }
    }

}