using System;
using System.Collections.Generic;

namespace SourcesRuGen.Prompts
{

    [Serializable]
    public class TagTree
    {
        public Meta           Meta { get; set; }
        public List<TagChunk> Data { get; set; }
    }

}