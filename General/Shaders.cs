using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace ShadersTest
{
    public static class Shaders
    {
        public static Effect CelestialEffect;

        public static void SetEffect()
        {
        }

        public static void Update()
        {
            UpdateEffect(CelestialEffect);
        }

        private static void UpdateEffect(Effect effect)
        {
            if (effect.Parameters["time"] != null)
            {
                effect.Parameters["time"].SetValue((State.Time + State.CustomTime) * State.TimeModifier);
            }

            // Check if any parameters are changed and send them to the shader if so
            List<Parameter> changedParams = State.GetChangedParams();

            foreach (Parameter param in changedParams)
            {
                param.ValueChanged = false;

                // If the param is not implemented, skip it
                if (effect.Parameters[param.Name] == null)
                {
                    System.Console.WriteLine(
                        string.Format("Parameter {0} is not exposed in the shader for modification. Maybe you forgot to remove the static from 'static [type] {0} ?'", param.Name)
                    );
                    continue;
                }


                if (param.ValueFloat != null)
                {
                    effect.Parameters[param.Name].SetValue(param.ValueFloat.GetValueOrDefault(0));
                }
                else if (param.ValueInt != null)
                {
                    effect.Parameters[param.Name].SetValue(param.ValueInt.GetValueOrDefault(0));
                }
            }
        }

        [Obsolete("This implementation should be moved so it works from parameters.")]
        public static void UpdateShader()
        {
            string currentShaderId = State.GetCurrentCelestialShaderID();
            CelestialEffect = GameContent.LoadEffect(currentShaderId);
            CelestialEffect.Parameters["pixels"].SetValue(State.Pixels);

            // a star does not need light origin
            if (CelestialEffect.Parameters["light_origin"] != null)
            {
                CelestialEffect.Parameters["light_origin"].SetValue(State.LightOrigin);
            }

        }
    }

    
}
