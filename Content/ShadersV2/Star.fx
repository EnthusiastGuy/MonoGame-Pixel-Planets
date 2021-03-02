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

// Star body
float star_time_speed = 0.05;

float3 star_color = float3(1, 0.545, 0.105);

float star_size = 10.0;
float star_tiles = 1.0;

static float3 starColors[15] = {
	float3(0.847, 0.941, 0.917),
	float3(0.752, 0.862, 0.854),
	float3(0.662, 0.784, 0.796),
	float3(0.568, 0.709, 0.733),
	float3(0.474, 0.631, 0.674),
	float3(0.384, 0.552, 0.611),
	float3(0.290, 0.474, 0.552),
	float3(0.196, 0.4, 0.490),
	float3(0.105, 0.321, 0.431),
	float3(0.011, 0.243, 0.368),
	float3(0.0, 1.0, 1.0),				// control colors from here on. To watch for overflows
	float3(0.0, 0.8, 0.8),
	float3(0.0, 0.6, 0.6),
	float3(0.0, 0.4, 0.4),
	float3(0.0, 0.2, 0.2)
};

struct VertexShaderInput
{
	float4 Position: SV_POSITION;
	float4 Color: COLOR0;
	float2 TextureCoordinates: TEXCOORD0;
};

// TODO, resolve the magic in here
float3 colorSelection(float3 colors[15], float posterized) {
	int pos = floor(posterized * 9.5);
	return colors[pos];
};

// Layers
float4 computeStarBody(float2 inputUV) {
	// pixelize uv
	float2 uv = floor(inputUV * pixels) / pixels;

	// TODO check why UV's places are inverted here as opposed to other shaders
	bool dith = dither(pixels, 1.0, inputUV, uv);

	uv = rotate(uv, rotation);

	// map to sphere
	uv = spherify(uv);

	// use two different sized cells for some variation
	float n = cells(star_size, float2(1.0, 1.0), star_size, star_tiles, time * star_time_speed, uv - float2(time * star_time_speed * 2.0, 0), 10);
	n *= cells(star_size, float2(1.0, 1.0), star_size, star_tiles, time * star_time_speed, uv - float2(time * star_time_speed * 2.0, 0), 20);

	// adjust cell value to get better looking stuff
	n *= 2.;
	n = clamp(n, 0.0, 1.0);
	if (dith) { // here we dither
		n *= 1.3;
	}

	// constrain values 4 possibilities and then choose color based on those
	float interpolate = floor(n * 3.0) / 3.0;
	float3 col = colorSelection(starColors, interpolate);

	// cut out a circle
	float a = step(distance(uv, float2(0.5, 0.5)), 0.5);
	
	return float4(col, a);
}

// Fragment composition here
float4 MainPS(VertexShaderInput input) : COLOR
{
	float2 inputUV = input.TextureCoordinates;
	float4 starBody = computeStarBody(inputUV);
	return starBody;
};

technique SpriteDrawing {
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};