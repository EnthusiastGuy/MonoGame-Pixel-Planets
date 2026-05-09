using System;
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

        /// <summary>When true, <see cref="Time"/> is not advanced (used during GIF/sheet export).</summary>
        public static bool SuspendTimeForExport;

        /// <summary>When true, shader <c>time</c> is driven by <see cref="ExportAnimationPhase01"/> instead of <see cref="Time"/>.</summary>
        public static bool ExportAnimationActive;

        /// <summary>0..1 animation phase for export; mapped to a fixed time range for seamless loops.</summary>
        public static float ExportAnimationPhase01;

        /// <summary>Scales <see cref="ExportAnimationPhase01"/> into the same space as live <see cref="Time"/> before per-shader multipliers.</summary>
        public static float ExportAnimationTimeScale = 80f;

        /// <summary>When true with <see cref="ExportAnimationActive"/>, shader <c>time</c> follows Godot <c>set_custom_time(t)</c> (normalized <c>t</c>), not live-style <see cref="ComputeShaderTime"/>.</summary>
        public static bool ExportGifGodotTimeMapping;

        /// <summary>Used to match GIF frame stepping to live <see cref="Time"/> (set from <see cref="Game.TargetElapsedTime"/>).</summary>
        public static float ApproxUpdatesPerSecond = 60f;

        public static float Pixels = 200;

        public static int MouseX = 0;
        public static int MouseY = 0;
        public static bool MouseL = false;
        public static int ClickedMouseX = 0;
        public static int ClickedMouseY = 0;
        public static bool ExitRequested = false;

        public static bool ExportViewOpen;

        public static string HelperLine1 = "LEFT/RIGHT: body  |  P: PNG  |  F2: export  |  X: full shot  |  TAB / ~ : rand / defaults";
        public static string HelperLine2 = "[LightHandlingMessage]";

        private static PersistentData data = LoadOrInitialize();

        public static void Update()
        {
            

            if (!SuspendTimeForExport && !TimeStoppedByMouse && !RotationPaused)
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

        public static List<Parameter> GetActiveCelestialParameters()
        {
            return data.activeCelestial.Parameters;
        }

        public static void MarkAllParametersChanged()
        {
            foreach (Parameter p in data.activeCelestial.Parameters)
                p.ValueChanged = true;
        }

        public static string GetCurrentCelestialShaderID()
        {
            return data.activeCelestial.ShaderID;
        }

        /// <summary>Returns effect asset names for the active body (single entry when no multi-pass).</summary>
        public static System.Collections.Generic.List<string> GetPassShaderIds()
        {
            var ids = data.activeCelestial.PassShaderIds;
            if (ids != null && ids.Count > 0)
                return ids;
            return new System.Collections.Generic.List<string> { data.activeCelestial.ShaderID };
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

        /// <summary>
        /// First matching UI time-speed parameter for the active body (names ordered so land/planet beat water/cloud fallbacks).
        /// Shader uniforms still apply this during export; used here only for optional GIF frame boosting when speed &lt; 1.
        /// </summary>
        public static bool TryGetPrimaryAnimationTimeSpeed(out float value)
        {
            string[] keys =
            {
                "planet_time_speed",
                "land_time_speed",
                "gas_time_speed",
                "time_speed",
                "water_time_speed",
                "inner_cloud_time_speed",
                "outer_cloud_time_speed",
                "ring_time_speed",
                "lava_time_speed",
                "craters_time_speed",
                "time_speed_clouds",
            };

            foreach (string key in keys)
            {
                foreach (Parameter p in data.activeCelestial.Parameters)
                {
                    if (p.Name == key && p.ValueFloat.HasValue)
                    {
                        value = p.ValueFloat.Value;
                        return true;
                    }
                }
            }

            value = 1f;
            return false;
        }

        /// <summary>
        /// Scale so each GIF frame advances <c>time</c> like live play for one frame delay.
        /// Phases use <c>i / frameCount</c> (Godot: <c>lerp(0,1,i/frames)</c>), so step in phase is <c>1/frameCount</c>
        /// and the loop wrap matches inter-frame spacing — seam-free sampling on a circle.
        /// </summary>
        public static float ComputeGifExportAnimationTimeScale(int frameCount, int delayHundredths)
        {
            if (frameCount <= 1)
                return ExportAnimationTimeScale;

            float delaySec = Math.Max(delayHundredths, 1) / 100f;
            float stepPerFrame = delaySec * ApproxUpdatesPerSecond * 0.04f * TimeModifier;
            return frameCount * stepPerFrame;
        }

        /// <summary>Sprite sheet: each cell advances one live update worth of <c>time</c> uniform.</summary>
        public static float ComputeSpriteSheetExportAnimationTimeScale(int cellCount)
        {
            if (cellCount <= 1)
                return ExportAnimationTimeScale;

            return (cellCount - 1) * 0.04f * TimeModifier;
        }

        /// <summary>
        /// When planet time speed is below 1 (in absolute value), add frames so the loop stays smooth cover equivalent motion.
        /// </summary>
        public static int AdjustGifFrameCountForPlanetTimeSpeed(int requestedFrames)
        {
            if (requestedFrames <= 1)
                return requestedFrames;

            if (!TryGetPrimaryAnimationTimeSpeed(out float spd))
                return requestedFrames;

            float a = Math.Abs(spd);
            if (a >= 1f)
                return requestedFrames;

            int boosted = (int)Math.Ceiling(requestedFrames / Math.Max(a, 0.05f));
            return Math.Min(Math.Max(boosted, requestedFrames), 240);
        }

        private static PersistentData LoadOrInitialize()
        {
            PersistentData data = new PersistentData();

            if (File.Exists(STATE_FILE))
            {
                data = JsonConvert.DeserializeObject<PersistentData>(File.ReadAllText(STATE_FILE));
                data.SetActiveCelestial();
                MergePassShaderMetadata(data);
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

        /// <summary>
        /// Older state.json files lack PassShaderIds; copy them from the template list so Star stays multi-pass.
        /// </summary>
        private static void MergePassShaderMetadata(PersistentData data)
        {
            if (data.celestials == null)
                return;

            List<Celestial> templates = Persistence.Predefined.GetCelestials();
            foreach (Celestial c in data.celestials)
            {
                foreach (Celestial t in templates)
                {
                    if (t.ShaderID == c.ShaderID && t.Name == c.Name)
                    {
                        if ((c.PassShaderIds == null || c.PassShaderIds.Count == 0) &&
                            t.PassShaderIds != null && t.PassShaderIds.Count > 0)
                        {
                            c.PassShaderIds = new List<string>(t.PassShaderIds);
                        }
                        break;
                    }
                }
            }
        }
    }
}
