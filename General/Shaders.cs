using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace ShadersTest
{
    public static class Shaders
    {
        public static List<Effect> CelestialEffects { get; } = new List<Effect>();

        public static Effect CelestialEffect =>
            CelestialEffects.Count > 0 ? CelestialEffects[0] : null;

        public static void SetEffect()
        {
        }

        public static void Update()
        {
            ApplySharedUniformsEveryFrame();

            List<string> passIds = State.GetPassShaderIds();

            List<Parameter> changedSnapshot = State.GetChangedParams();
            foreach (Parameter param in changedSnapshot)
                param.ValueChanged = false;

            bool gifGodot =
                State.ExportAnimationActive && State.ExportGifGodotTimeMapping;

            float baseTime = State.ExportAnimationActive && !gifGodot
                ? State.ExportAnimationPhase01 * State.ExportAnimationTimeScale
                : (State.Time + State.CustomTime) * State.TimeModifier;

            for (int i = 0; i < CelestialEffects.Count; i++)
            {
                Effect effect = CelestialEffects[i];
                string passId = i < passIds.Count ? passIds[i] : passIds[passIds.Count - 1];

                ApplyUniformSnapshot(effect, passId, changedSnapshot);

                if (gifGodot)
                    ApplyGodotGifTime(passId, effect);
                else if (effect.Parameters["time"] != null)
                {
                    float t = ComputeShaderTime(passId, baseTime, effect);
                    effect.Parameters["time"].SetValue(t);
                    SetSecondaryTimes(effect, t);
                }
            }
        }

        /// <summary>
        /// Godot GIF export uses per-planet <c>set_custom_time(t)</c> with <c>t = i/float(frames)</c>, not <c>update_time</c>.
        /// Matches GUI export paths (e.g. Rivers.gd, Galaxy.gd, Star.gd, Asteroid.gd).
        /// </summary>
        private static void ApplyGodotGifTime(string passShaderId, Effect effect)
        {
            float t = State.ExportAnimationPhase01;

            if (State.GetCurrentCelestialShaderID() == "Asteroid")
            {
                if (effect.Parameters["rotation"] != null)
                    effect.Parameters["rotation"].SetValue(t * MathF.PI * 2f);
                if (effect.Parameters["time"] != null)
                    effect.Parameters["time"].SetValue(0f);
                return;
            }

            if (effect.Parameters["time"] != null)
                effect.Parameters["time"].SetValue(ComputeGodotGifShaderTime(passShaderId, t, effect));

            SetGodotGifLayerTimes(passShaderId, t, effect);
        }

        /// <summary>All secondary per-layer time uniform names used across shaders.</summary>
        private static readonly string[] SecondaryTimeUniforms =
        {
            "time_clouds", "time_land", "time_craters", "time_lava", "time_outer", "time_ring"
        };

        /// <summary>During live play, set every secondary time uniform to the same value as the primary <c>time</c>.</summary>
        private static void SetSecondaryTimes(Effect effect, float value)
        {
            foreach (string name in SecondaryTimeUniforms)
            {
                var p = effect.Parameters[name];
                if (p != null) p.SetValue(value);
            }
        }

        private static void SetIfPresent(Effect fx, string name, float value)
        {
            var p = fx.Parameters[name];
            if (p != null) p.SetValue(value);
        }

        /// <summary>
        /// During Godot-style export, set each secondary layer's time uniform to its own
        /// <c>t * get_multiplier(layer_material)</c> so every layer loops independently at <c>t = 1</c>.
        /// Matches each planet's <c>set_custom_time(t)</c> in Godot.
        /// </summary>
        private static void SetGodotGifLayerTimes(string passShaderId, float t, Effect fx)
        {
            switch (passShaderId)
            {
                case "LandRivers":
                    // Rivers.gd: Cloud uses get_multiplier * 0.5
                    SetIfPresent(fx, "time_clouds",
                        t * GodotStyleMultiplier(
                            GetEffectSingle(fx, "size_clouds", 7.315f),
                            GetEffectSingle(fx, "time_speed_clouds", 0.1f)) * 0.5f);
                    break;

                case "LandMasses":
                    // LandMasses.gd: all layers use full get_multiplier
                    SetIfPresent(fx, "time_land",
                        t * GodotStyleMultiplier(
                            GetEffectSingle(fx, "sizeLakes", 4.292f),
                            GetEffectSingle(fx, "land_time_speed", 0.2f)));
                    SetIfPresent(fx, "time_clouds",
                        t * GodotStyleMultiplier(
                            GetEffectSingle(fx, "sizeClouds", 7.745f),
                            GetEffectSingle(fx, "time_speed_clouds", 0.2f)));
                    break;

                case "IceWorld":
                    // IceWorld.gd: all layers use full get_multiplier
                    SetIfPresent(fx, "time_clouds",
                        t * GodotStyleMultiplier(
                            GetEffectSingle(fx, "sizeClouds", 4f),
                            GetEffectSingle(fx, "time_speed_clouds", 0.1f)));
                    break;

                case "LavaWorld":
                    // LavaWorld.gd: all layers use full get_multiplier
                    SetIfPresent(fx, "time_craters",
                        t * GodotStyleMultiplier(
                            GetEffectSingle(fx, "craters_size", 3.5f),
                            GetEffectSingle(fx, "craters_time_speed", 0.09f)));
                    SetIfPresent(fx, "time_lava",
                        t * GodotStyleMultiplier(
                            GetEffectSingle(fx, "lava_size", 10f),
                            GetEffectSingle(fx, "lava_time_speed", 0.2f)));
                    break;

                case "NoAtmosphere":
                    // NoAtmosphere.gd: both layers use full get_multiplier
                    SetIfPresent(fx, "time_craters",
                        t * GodotStyleMultiplier(
                            GetEffectSingle(fx, "sizeCraters", 5f),
                            GetEffectSingle(fx, "time_speed", 0.4f)));
                    break;

                case "GasPlanet":
                    // GasPlanet.gd: both layers use full get_multiplier
                    SetIfPresent(fx, "time_outer",
                        t * GodotStyleMultiplier(
                            GetEffectSingle(fx, "outer_size", 9f),
                            GetEffectSingle(fx, "outer_cloud_time_speed", 0.7f)));
                    break;

                case "GasPlanetLayers":
                    // GasPlanetLayers.gd: Ring.time = t * 314.15 * time_speed * 0.5
                    SetIfPresent(fx, "time_ring",
                        t * 314.15f * GetEffectSingle(fx, "ring_time_speed", 0.2f) * 0.5f);
                    break;
            }
        }

        /// <summary>Godot <c>get_multiplier</c>: <c>(round(size)*2)/time_speed</c>.</summary>
        private static float GodotStyleMultiplier(float roundedSize, float timeSpeed)
        {
            return (MathF.Round(roundedSize) * 2f) / Math.Max(timeSpeed, 1e-5f);
        }

        private static float ComputeGodotGifShaderTime(string passShaderId, float t, Effect fx)
        {
            switch (passShaderId)
            {
                case "StarBlobs":
                case "StarFlares":
                    return t * GodotStyleMultiplier(GetEffectSingle(fx, "size", 5f), GetEffectSingle(fx, "time_speed", 0.05f));
                case "Star":
                    return t * (1f / Math.Max(GetEffectSingle(fx, "time_speed", 0.05f), 1e-5f));
                case "Galaxy":
                    return t * MathF.PI * 2f * GetEffectSingle(fx, "time_speed", 1f);
                case "BlackHole":
                    return t * 314.15f * GetEffectSingle(fx, "time_speed", 0.2f) * 0.5f;
                case "LandRivers":
                    return t * GodotStyleMultiplier(
                        GetEffectSingle(fx, "planet_size", 4.6f),
                        GetEffectSingle(fx, "planet_time_speed", 0.2f));
                case "LandMasses":
                    // Primary layer is Water, which uses size + time_speed
                    return t * GodotStyleMultiplier(
                        GetEffectSingle(fx, "size", 5.228f),
                        GetEffectSingle(fx, "time_speed", 0.1f));
                case "GasPlanetLayers":
                    return t * GodotStyleMultiplier(
                        GetEffectSingle(fx, "gas_size", 8f),
                        GetEffectSingle(fx, "gas_time_speed", 0.05f));
                case "GasPlanet":
                    return t * GodotStyleMultiplier(
                        GetEffectSingle(fx, "inner_size", 9f),
                        GetEffectSingle(fx, "inner_cloud_time_speed", 0.7f));
                case "TerranDry":
                case "NoAtmosphere":
                case "IceWorld":
                case "LavaWorld":
                    return t * GodotStyleMultiplier(
                        GetEffectSingle(fx, "size", 10f),
                        GetEffectSingle(fx, "time_speed", 0.2f));
                default:
                    return t * GodotStyleMultiplier(
                        GetEffectSingle(fx, "size", 5f),
                        GetEffectSingle(fx, "time_speed", 0.2f));
            }
        }

        private static void ApplyUniformSnapshot(Effect effect, string passShaderId, List<Parameter> changedSnapshot)
        {
            foreach (Parameter param in changedSnapshot)
            {
                string uniformName = MapParameterToUniform(passShaderId, param.Name);
                if (uniformName == null)
                    continue;

                var ep = effect.Parameters[uniformName];
                if (ep == null)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"Shader uniform '{uniformName}' missing on pass '{passShaderId}'.");
                    continue;
                }

                if (param.ValueFloat != null)
                {
                    float v = param.ValueFloat.GetValueOrDefault(0);
                    if (passShaderId == "BlackHole" && uniformName == "rotation")
                        v += 0.7f;
                    ep.SetValue(v);
                }
                else if (param.ValueInt != null)
                {
                    ep.SetValue(param.ValueInt.GetValueOrDefault(0));
                }
            }
        }

        private static string MapParameterToUniform(string passShaderId, string paramName)
        {
            if (State.GetCurrentCelestialShaderID() == "Star")
            {
                if (paramName == "should_dither")
                {
                    if (passShaderId == "StarBlobs")
                        return null;
                    return "should_dither";
                }

                if (paramName == "seed" || paramName == "rotation")
                    return paramName;
            }

            return paramName;
        }

        private static float ComputeShaderTime(string passShaderId, float baseTime, Effect fx)
        {
            switch (passShaderId)
            {
                case "StarBlobs":
                    return StarLayerTime(baseTime, fx, 0.01f);
                case "Star":
                    return StarLayerTime(baseTime, fx, 0.005f);
                case "StarFlares":
                    return StarLayerTime(baseTime, fx, 0.015f);
                case "Galaxy":
                    {
                        float sz = GetEffectSingle(fx, "size", 7f);
                        float ts = GetEffectSingle(fx, "time_speed", 1f);
                        float mult = (MathF.Round(sz) * 2f) / Math.Max(ts, 1e-5f);
                        return baseTime * mult * 0.04f;
                    }
                case "BlackHole":
                    return baseTime * (314.15f * 0.004f);
                default:
                    return baseTime;
            }
        }

        private static float StarLayerTime(float baseTime, Effect fx, float factor)
        {
            float sz = GetEffectSingle(fx, "size", 5f);
            float ts = GetEffectSingle(fx, "time_speed", 0.05f);
            float mult = (MathF.Round(sz) * 2f) / Math.Max(ts, 1e-5f);
            return baseTime * mult * factor;
        }

        private static float GetEffectSingle(Effect fx, string name, float fallback)
        {
            var p = fx.Parameters[name];
            if (p == null)
                return fallback;
            try
            {
                return p.GetValueSingle();
            }
            catch
            {
                return fallback;
            }
        }

        public static void UpdateShader()
        {
            CelestialEffects.Clear();
            foreach (string id in State.GetPassShaderIds())
            {
                CelestialEffects.Add(GameContent.LoadEffect(id));
            }

            State.MarkAllParametersChanged();

            ApplySharedUniformsEveryFrame();
        }

        /// <summary>
        /// Per-frame values that are not necessarily tied to scrollable parameters (resolution, light).
        /// </summary>
        private static void ApplySharedUniformsEveryFrame()
        {
            List<string> passIds = State.GetPassShaderIds();

            for (int i = 0; i < CelestialEffects.Count; i++)
            {
                Effect effect = CelestialEffects[i];
                string passId = i < passIds.Count ? passIds[i] : passIds[passIds.Count - 1];

                if (effect.Parameters["pixels"] != null)
                {
                    float px = State.Pixels;
                    if (passId == "StarBlobs" || passId == "StarFlares")
                        px = State.Pixels * 2f;
                    effect.Parameters["pixels"].SetValue(px);
                }

                if (effect.Parameters["pixels_ring"] != null && passId == "BlackHole")
                    effect.Parameters["pixels_ring"].SetValue(State.Pixels * 3f);

                if (effect.Parameters["light_origin"] != null)
                    effect.Parameters["light_origin"].SetValue(State.LightOrigin);
            }
        }

    }
}
