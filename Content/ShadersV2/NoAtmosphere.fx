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
float rotation = 0.0;

// Parameters
// Planet under
float time_speed = 0.4;
float dither_size = 2.0;	// 1 is neutral
float light_border_1 = 0.615;
float light_border_2 = 0.729;

static float3 color1 = float3(0.639, 0.654, 0.760);
static float3 color2 = float3(0.298, 0.407, 0.521);
static float3 color3 = float3(0.227, 0.247, 0.368);

float size = 8.0;
int OCTAVES = 4;
float seed = 1.012; // expected 0 - 10

// Craters

float light_border_crater = 0.465;

static float3 craterColor1 = float3(0.298, 0.407, 0.521);
static float3 craterColor2 = float3(0.227, 0.247, 0.368);
float sizeCraters = 5.0;
float seedCraters = 4.517; // expected 0 - 10

struct VertexShaderInput
{
	float4 Position: SV_POSITION;
	float4 Color: COLOR0;
	float2 TextureCoordinates: TEXCOORD0;
};

// Layers
float4 computePlanetUnder(float2 inputUV) {
	// pixelize uv
	float2 uv = floor(inputUV * pixels) / pixels;
	// we use this value later to dither between colors
	bool dith = dither(pixels, dither_size, uv, inputUV);

	uv = rotate(uv, rotation);
	
	// check distance from center & distance to light
	float d_circle = distance(uv, float2(0.5, 0.5));
	float d_light = distance(uv, float2(light_origin));

	// cut out a circle
	float a = step(d_circle, 0.5);

	// get a noise value with light distance added
	// this creates a moving dynamic shape
	d_light += fbm(size, float2(1.0, 1.0), seed, OCTAVES, uv * size + float2(time * time_speed, 0.0)) * 0.3; // change the magic 0.3 here for different light strengths

	// size of edge in which colors should be dithered
	float dither_border = (1.0 / pixels) * dither_size;

	// now we can assign colors based on distance to light origin
	float3 col = color1;

	if (d_light > light_border_1) {
		col = color2;
		if (d_light < light_border_1 + dither_border && dith) {
			col = color1;
		}
	}
	if (d_light > light_border_2) {
		col = color3;
		if (d_light < light_border_2 + dither_border && dith) {
			col = color2;
		}
	}

	return float4(col, a);
}

float4 computeCraters(float2 inputUV) {
	// pixelize uv
	float2 uv = floor(inputUV * pixels) / pixels;

	// check distance from center & distance to light
	float d_circle = distance(uv, float2(0.5, 0.5));
	float d_light = distance(uv, light_origin);

	uv = rotate(uv, rotation);
	// map to sphere
	uv = spherify(uv);

	float c1 = crater(sizeCraters, float2(1.0, 1.0), seedCraters, time, time_speed, uv);
	float c2 = crater(sizeCraters, float2(1.0, 1.0), seedCraters, time, time_speed, uv + (light_origin - 0.5) * 0.03);

	float3 col = craterColor1;
	float a = step(0.5, c1);

	if (c2 < c1 - (0.5 - d_light) * 2.0) {
		col = craterColor2;
	}
	if (d_light > light_border_crater) {
		col = craterColor2;
	}

	a *= step(d_circle, 0.5);
	
	return float4(col, a);
}

// Fragment composition here
float4 MainPS(VertexShaderInput input) : COLOR
{
	float2 inputUV = input.TextureCoordinates;

	float4 result;

	float4 planetCraters = computeCraters(inputUV);
	if (planetCraters.a != 0.0) {
		result = planetCraters;
	}
	else {
		result = computePlanetUnder(inputUV);
	}

	return result;
};

technique SpriteDrawing {
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};