using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace SourcesRuGen.Prompts
{

    public class TagReader
    {
        
        public IDictionary<Meta, IDictionary<int, List<TagChunk>>> Read(string path)
        {
            IDictionary<Meta, IDictionary<int, List<TagChunk>>> result = new Dictionary<Meta, IDictionary<int, List<TagChunk>>>();
            foreach (var tree in ReadTrees(path))
                result.Add(tree.Meta, tree.Data.GroupBy(o => o.Level).ToDictionary(o => o.Key, o => o.ToList()));
            return result;
        }

        private IEnumerable<TagTree> ReadTrees(string path)
        {
            foreach (var file in Directory.GetFiles(path))
            {
                if(file.EndsWith(".json"))
                    yield return ReadTree(file);
            }
        }
        
        private TagTree ReadTree(string fileName)
        {
            var json = File.ReadAllText(fileName);
            return JsonConvert.DeserializeObject<TagTree>(json);
        }
        
    }

}