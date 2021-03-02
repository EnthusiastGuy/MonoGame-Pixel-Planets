using Newtonsoft.Json;
using System.Collections.Generic;

namespace ShadersTest
{
    public class Celestial
    {
        public string Name { get; set; }
        public string Info { get; set; }
        public string ShaderID { get; set; }
        [JsonProperty(PropertyName = "Params")]
        public List<Parameter> Parameters { get; set; }
    }
}
