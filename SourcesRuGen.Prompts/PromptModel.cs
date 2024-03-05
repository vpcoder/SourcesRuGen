using System;

namespace SourcesRuGen.Prompts
{

    /// <summary>
    ///     Скомпилированная модель запроса на генерацию
    /// </summary>
    [Serializable]
    public class PromptModel
    {
        public string Positive { get; set; }
        public string Negative { get; set; }
        public Meta   Meta     { get; set; }

    }

}