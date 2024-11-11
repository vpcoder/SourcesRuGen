using System;
using System.Collections.Generic;

namespace SourcesRuGen.Prompts
{

    /// <summary>
    ///     Выполянет генерацию цепочки тегов
    /// </summary>
    public class TagChainGenerator
    {

        private Random rnd = new Random();

        public List<TagChunk> Generate(IDictionary<int, List<TagChunk>> dictionary)
        {
            var list = new List<TagChunk>();
            foreach (var variants in dictionary.Values)
                list.Add(variants[rnd.Next(0, variants.Count)]);
            return list;
        }

    }

}