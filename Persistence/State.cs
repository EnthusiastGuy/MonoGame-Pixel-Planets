using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using ShadersTest.UI;

namespace ShadersTest
{
    // The state to be loaded from json. Easy to mess around with this way
    class PersistentData
    {
        public List<Celestial> celestials;
        [JsonIgnore]
        public Celestial activeCelestial;

        public void Initialize()
        {
            celestials = Persistence.Predefined.GetCelestials();
            SetActiveCelestial();
        }

        public void SetActiveCelestial()
        {
            activeCelestial = celestials[0];
        }
    }
    public static class State
    {
        public static readonly string STATE_FILE = "state.json";

        public static Vector2 LightOrigin = new Vector2(0.39f, 0.39f);
        public static float Time;
        public static bool TimeStoppedByMouse = false;
        public static float CustomTime;
        public static bool RotationPaused;
        public static float TimeModifier = 1.0f;

        public static float Pixels = 200;

        public static int MouseX = 0;
        public static int MouseY = 0;
        public static bool MouseL = false;
        public static int ClickedMouseX = 0;
        public static int ClickedMouseY = 0;
        public static bool ExitRequested = false;

        public static string HelperLine1 = "Arrow LEFT/RIGHT to see other planets/celestial objects. TAB to experimentally randomize, Tilda (above tab) to revert to defaults.";
        public static string HelperLine2 = "[LightHandlingMessage]";

        private static PersistentData data = LoadOrInitialize();

        public static void Update()
        {
            

            if (!TimeStoppedByMouse && !RotationPaused)
            {
                Time += .04f;
            }

            /*
            int mouseDelta = MouseStates.GetMouseWheelDelta();
            if (mouseDelta != 0)
            {
                TimeModifier += (float)mouseDelta / 1000;

                if (TimeModifier < 0)
                    TimeModifier = 0;

                if (TimeModifier > 3)
                    TimeModifier = 3;
            }*/
        }

        public static void Save()
        {
            string stateData = JsonConvert.SerializeObject(data, Formatting.None, Config.JsonSettings);
            File.WriteAllText(STATE_FILE, stateData);
        }

        public static string GetCurrentCelestialName()
        {
            return data.activeCelestial.Name;
        }

        public static List<DisplayableProperty> GetUIParamsList()
        {
            List<DisplayableProperty> response = new List<DisplayableProperty>();

            int position = 0;
            for (int i = 0; i < data.activeCelestial.Parameters.Count; i++)
            {
                // For now, we're hiding params we don't handle
                if (!data.activeCelestial.Parameters[i].Visible)
                    continue;

                response.Add(new DisplayableProperty() {
                    DisplayName = data.activeCelestial.Parameters[i].DisplayName,
                    Key = data.activeCelestial.Parameters[i].Name,
                    Value = data.activeCelestial.Parameters[i].GetDisplayableValue(),
                    IsDefault = data.activeCelestial.Parameters[i].IsValueDefault(),
                    Position = position++,
                });
            }

            return response;
        }

        public static string GetCurrentCelestialInfo()
        {
            return data.activeCelestial.Info;
        }

        public static string GetCurrentCelestialShaderID()
        {
            return data.activeCelestial.ShaderID;
        }

        public static void NextCelestial()
        {
            for (int i = 0; i < data.celestials.Count; i++)
            {
                if (data.celestials[i] == data.activeCelestial && i < data.celestials.Count - 1)
                {
                    data.activeCelestial = data.celestials[i + 1];
                    return;
                }
                else if (data.celestials[i] == data.activeCelestial)
                {
                    data.activeCelestial = data.celestials[0];
                    return;
                }
            }
        }

        public static void PreviousCelestial()
        {
            for (int i = 0; i < data.celestials.Count; i++)
            {
                if (data.celestials[i] == data.activeCelestial && i >= 1)
                {
                    data.activeCelestial = data.celestials[i - 1];
                    return;
                }
                else if (data.celestials[i] == data.activeCelestial)
                {
                    data.activeCelestial = data.celestials[data.celestials.Count - 1];
                    return;
                }
            }
        }

        public static void RandomizeParams()
        {
            foreach (Parameter param in data.activeCelestial.Parameters)
            {
                param.ValueChanged = true;
                if (param.ValueInt != null)
                {
                    param.ValueInt = General.Procedural.GetIntBetween(param.ValueIntMin, param.ValueIntMax);

                }
                else if (param.ValueFloat != null)
                {
                    param.ValueFloat = General.Procedural.GetFloatBetween(param.ValueFloatMin, param.ValueFloatMax);
                }
            }
        }

        public static void RestoreDefaults()
        {
            foreach (Parameter param in data.activeCelestial.Parameters)
            {
                param.ValueChanged = true;
                if (param.ValueInt != null)
                {
                    param.ValueInt = param.ValueIntDefault;

                }
                else if (param.ValueFloat != null)
                {
                    param.ValueFloat = param.ValueFloatDefault;
                }
            }
        }

        public static void ModifyValueFor(string propKey, int delta)
        {
            int modifier = delta > 0 ? 1 : -1;
            foreach (Parameter param in data.activeCelestial.Parameters)
            {
                if (param.Name.Equals(propKey))
                {
                    param.ValueChanged = true;
                    if (param.ValueInt != null)
                    {
                        param.ValueInt += modifier;
                        if (param.ValueInt > param.ValueIntMax)
                            param.ValueInt = param.ValueIntMax;

                        if (param.ValueInt < param.ValueIntMin)
                            param.ValueInt = param.ValueIntMin;

                    } else if (param.ValueFloat != null)
                    {
                        param.ValueFloat += (float)modifier / 100;
                        if (param.ValueFloat > param.ValueFloatMax)
                            param.ValueFloat = param.ValueFloatMax;

                        if (param.ValueFloat < param.ValueFloatMin)
                            param.ValueFloat = param.ValueFloatMin;
                    }
                }
            }
        }

        public static void SetDefaultValueFor(string propKey)
        {
            foreach (Parameter param in data.activeCelestial.Parameters)
            {
                if (param.Name.Equals(propKey))
                {
                    param.ValueChanged = true;
                    if (param.ValueInt != null)
                    {
                        param.ValueInt = param.ValueIntDefault;
                    }
                    else if (param.ValueFloat != null)
                    {
                        param.ValueFloat = param.ValueFloatDefault;
                    }
                }
            }
        }

        public static List<Parameter> GetChangedParams()
        {
            List<Parameter> changedParams = new List<Parameter>();

            foreach(Parameter param in data.activeCelestial.Parameters)
            {
                if (param.ValueChanged)
                {
                    changedParams.Add(param);
                }

            }

            return changedParams;
        }

        private static PersistentData LoadOrInitialize()
        {
            PersistentData data = new PersistentData();

            if (File.Exists(STATE_FILE))
            {
                data = JsonConvert.DeserializeObject<PersistentData>(File.ReadAllText(STATE_FILE));
                data.SetActiveCelestial();
                
            }
            else
            {
                data.Initialize();
            }

            // Force this to true so it gets sent to the shader the first time this is ran
            // even without being explicitly modified by the user with the UI
            foreach (Celestial celestial in data.celestials)
                foreach (Parameter parameter in celestial.Parameters)
                    parameter.ValueChanged = true;

            return data;
        }
    }
}
