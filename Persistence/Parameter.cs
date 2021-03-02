using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace ShadersTest
{
    public class Parameter
    {
        public static Parameter New(string displayName, string name, float valueFloat, float valueFloatDefault, float valueFloatMin, float valueFloatMax)
        {
            return new Parameter()
            {
                Name = name,
                DisplayName = displayName,
                Visible = true,
                ValueFloat = valueFloat,
                ValueFloatDefault = valueFloatDefault,
                ValueFloatMin = valueFloatMin,
                ValueFloatMax = valueFloatMax,
                ValueInt = null,
                ValueCol = null,
            };
        }

        public static Parameter New(string displayName, string name, int valueInt, int valueIntDefault, int valueIntMin, int valueIntMax)
        {
            return new Parameter()
            {
                Name = name,
                DisplayName = displayName,
                Visible = true,
                ValueInt = valueInt,
                ValueIntDefault = valueIntDefault,
                ValueIntMin = valueIntMin,
                ValueIntMax = valueIntMax,
                ValueFloat = null,
                ValueCol = null,
            };
        }

        public static Parameter New(string displayName, string name, Color valueCol, Color valueColDefault)
        {
            return new Parameter()
            {
                Name = name,
                DisplayName = displayName,
                Visible = false,
                ValueCol = valueCol,
                ValueColDefault = valueColDefault,
                ValueFloat = null,
                ValueInt = null,
            };
        }

        public string Name { get; set; }
        public string DisplayName { get; set; }
        public bool Visible { get; set; }
        [JsonProperty(PropertyName = "VFMin")]
        public float ValueFloatMin { get; set; }
        [JsonProperty(PropertyName = "VFMax")]
        public float ValueFloatMax { get; set; }
        [JsonProperty(PropertyName = "VFDef")]
        public float ValueFloatDefault { get; set; }
        [JsonProperty(PropertyName = "VF")]
        public float? ValueFloat { get; set; }
        [JsonProperty(PropertyName = "VIMin")]
        public int ValueIntMin { get; set; }
        [JsonProperty(PropertyName = "VIMax")]
        public int ValueIntMax { get; set; }
        [JsonProperty(PropertyName = "VIDef")]
        public int ValueIntDefault { get; set; }
        [JsonProperty(PropertyName = "VI")]
        public int? ValueInt { get; set; }
        [JsonProperty(PropertyName = "VCDef")]
        public Color ValueColDefault { get; set; }
        [JsonProperty(PropertyName = "VC")]
        public Color? ValueCol { get; set; }

        [JsonIgnore]
        public bool ValueChanged = false;

        public string GetDisplayableValue()
        {
            if (ValueInt != null)
                return ValueInt.ToString();

            if (ValueFloat != null)
                return ValueFloat.ToString();

            if (ValueCol != null)
                return ValueCol.ToString();

            return "#unimplemented value";
        }

        public bool IsValueDefault()
        {
            if (ValueInt != null)
                return ValueInt.Equals(ValueIntDefault);

            if (ValueFloat != null)
                return ValueFloat.Equals(ValueFloatDefault);

            if (ValueCol != null)
                return ValueCol.Equals(ValueColDefault);

            return false;
        }


    }
}
