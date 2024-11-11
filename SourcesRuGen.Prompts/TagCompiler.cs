using System;
using System.Collections.Generic;

namespace SourcesRuGen.Prompts
{

    /// <summary>
    ///     Выполняет компиляцию тега
    /// </summary>
    public class TagCompiler
    {
        private readonly string START_QUOTE = "[";
        private readonly string END_QUOTE   = "]";
        
        private readonly string START_QQUOTE = "{";
        private readonly string END_QQUOTE   = "}?";

        private Random rnd = new Random();
        

        public string Compile(TagChunk tag)
        {
            if (tag == null || string.IsNullOrWhiteSpace(tag.Tag))
                return null;

            var text = tag.Tag;
            var variations = GetIncludesInQuotes(text, START_QQUOTE, END_QQUOTE);
            if (variations != null && variations.Count > 0)
                foreach (var variation in variations)
                    text = text.Replace(START_QQUOTE + variation + END_QQUOTE, GetVariation(variation));
            
            var variables = GetIncludesInQuotes(text, START_QUOTE, END_QUOTE);
            if (variables != null && variables.Count > 0)
                foreach (var variable in variables)
                    text = text.Replace(START_QUOTE + variable + END_QUOTE, GetVariableValue(variable));
            
            var items = text.Split('|');
            if(items.Length > 1)
                text = items[rnd.Next(0, items.Length)];

            return text == "null" ? "" : Format(text);
        }

        private string GetVariation(string variation)
        {
            var isDisabled = rnd.NextDouble() >= 0.5d;
            return isDisabled ? "" : variation;
        }

        private string Format(string value)
        {
            return value.Replace("--", "-");
        }
        
        public string GetVariableValue(string variable)
        {
            if (string.IsNullOrWhiteSpace(variable))
                return "";

            var rangeTokens = variable.IndexOf("->", StringComparison.Ordinal);
            if (rangeTokens > 0) // Диапазон, например: [0->1]
            {
                var v1     = variable.Substring(0,               rangeTokens).Replace(".", ",");
                var v2     = variable.Substring(rangeTokens + 2, variable.Length - rangeTokens - 2).Replace(".", ",");
                try
                {
                    var value1 = double.Parse(v1);
                    var value2 = double.Parse(v2);
                    return GetRandomValue(value1, value2);
                }
                catch (Exception)
                {
                    Console.WriteLine($"exception! v1: {v1}, v2: {v2}");
                    throw;
                }
            }

            // Набор значений на выбор, например: [0|1|2|null]
            var items = variable.Split('|');
            var value = items[rnd.Next(0, items.Length)];
            return value == "null" ? "" : value;
        }
        
        private string GetRandomValue(double from, double to)
        {
            var value = from + rnd.NextDouble() * (to - from); 
            return string.Format("{0:F3}", value).Replace(",", ".");
        }
        
        private ICollection<string> GetIncludesInQuotes(string text, string firstQuote, string secondQuote)
        {
            var pos  = 0;
            var list = new List<string>();
            for (;;)
            {
                var firstIndex = text.IndexOf(firstQuote, pos, StringComparison.InvariantCulture);

                if (firstIndex < 0)
                    break;
                var endIndex = text.IndexOf(secondQuote, firstIndex + firstQuote.Length, StringComparison.InvariantCulture);

                if (endIndex < 0)
                    throw new ArgumentException("second token '" + secondQuote + "' parse exception");
                list.Add(text.Substring(firstIndex + firstQuote.Length, endIndex - firstIndex - firstQuote.Length));
                pos = endIndex + 1;
            }

            return list;
        }
        
    }

}