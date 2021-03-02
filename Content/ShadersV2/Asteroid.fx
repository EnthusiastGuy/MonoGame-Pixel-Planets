#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

#include "planet_utils.fx"

float time = 0.0;
float pixels = 200;
float2 light_origin = float2(0.3, 0.3);

// Parameters
// Asteroid
float rotation = 0.0;

static float3 color1 = float3(0.639, 0.654, 0.760);
static float3 color2 = float3(0.298, 0.407, 0.521);
static float3 color3 = float3(0.227, 0.247, 0.368);

float size = 5.294;
int octaves = 2;
float seed = 1.567; // expected 0 - 10


struct VertexShaderInput
{
	float4 Position: SV_POSITION;
	float4 Color: COLOR0;
	float2 TextureCoordinates: TEXCOORD0;
};

// Layers
float4 computeAsteroid(float2 inputUV) {
	// pixelize uv
	float2 uv = floor(inputUV * pixels) / pixels;

	// we use this value later to dither between colors
	bool dith = dither(pixels, 1.0, uv, inputUV);

	// distance from center
	float d = distance(uv, float2(0.5, 0.5));

	// FIX
	float2 r1 = (rotate(light_origin, rotation) - float2(0.5, 0.5)) * 0.5;

	// two noise values with one slightly offset according to light source, to create shadows later	
	float n1 = fbm(size, float2(2.0, 1.0), seed, octaves, uv * size);
	float n2 = fbm(size, float2(2.0, 1.0), seed, octaves, uv * size + r1);

	// step noise values to determine where the edge of the asteroid is
	// step cutoff value depends on distance from center
	float n1_step = step(0.2, n1 - d);
	float n2_step = step(0.2, n2 - d);

	// with this val we can determine where the shadows should be
	float noise_rel = (n2_step + n2) - (n1_step + n1);

	// two crater values, again one extra for the shadows
	float c1 = crater(size, float2(1.0, 1.0), seed, 0, 0, uv);
	float c2 = crater(size, float2(1.0, 1.0), seed, 0, 0, uv + (light_origin - 0.5) * 0.03);

	// finally add colors
	float3 col = color2;

	// noise
	if (noise_rel < -0.06 || (noise_rel < -0.04 && dith)) {
		col = color1;
	}
	if (noise_rel > 0.05 || (noise_rel > 0.03 && dith)) {
		col = color3;
	}

	// crater
	if (c1 > 0.4) {
		col = color2;
	}
	if (c2 < c1) {
		col = color3;
	}

	return float4(col, n1_step);
}

// Fragment composition here
float4 MainPS(VertexShaderInput input) : COLOR
{
	return computeAsteroid(input.TextureCoordinates);
};

technique SpriteDrawing {
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};