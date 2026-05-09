using Newtonsoft.Json;
using System.Collections.Generic;

namespace ShadersTest
{
    public class Celestial
    {
        public string Name { get; set; }
        public string Info { get; set; }
        public string ShaderID { get; set; }

        /// <summary>
        /// When set, each ID is an effect under Content/ShadersV2 drawn back-to-back (e.g. Star layers).
        /// If null or empty, <see cref="ShaderID"/> is used as a single pass.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<string> PassShaderIds { get; set; }

        [JsonProperty(PropertyName = "Params")]
        public List<Parameter> Parameters { get; set; }
    }
}
