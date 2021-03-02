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

float time_speed = 0.2;
float dither_size = 2.0;	// 1 is neutral
float light_border_1 = 0.4;
float light_border_2 = 0.6;

static float3 color1 = float3(0.560, 0.301, 0.341);
static float3 color2 = float3(0.321, 0.200, 0.247);
static float3 color3 = float3(0.239, 0.160, 0.211);

float size = 10.0;
int octaves = 3;
float seed = 1.551; // expected 0 - 10

// Craters
// TODO check why this speed moves craters faster than other time speeds
// when the same value is applied to all time speeds
float craters_time_speed = 0.09;
float craters_light_border = 0.4;

static float3 cratersColor1 = float3(0.321, 0.200, 0.247);
static float3 cratersColor2 = float3(0.239, 0.160, 0.211);

float craters_size = 3.5;
float craters_seed = 1.561; // expected 0 - 10

// Lava rivers
float lava_time_speed = 0.2;
float lava_light_border_1 = 0.019;
float lava_light_border_2 = 0.036;

float lava_river_cutoff = 0.579;

static float3 lava_color1 = float3(1.000, 0.537, 0.200);
static float3 lava_color2 = float3(0.901, 0.270, 0.223);
static float3 lava_color3 = float3(0.678, 0.184, 0.270);

float lava_size = 10.0;
int lava_octaves = 4;
float lava_seed = 2.627;


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

	// check distance from center & distance to light
	float d_circle = distance(uv, float2(0.5, 0.5));
	float d_light = distance(uv, float2(light_origin));

	// we use this value later to dither between colors
	bool dith = dither(pixels, dither_size, uv, inputUV);

	uv = rotate(uv, rotation);

	// cut out a circle
	float a = step(d_circle, 0.5);
	
	// get a noise value with light distance added
	// this creates a moving dynamic shape
	float fbm1 = fbm(size, float2(1.0, 1.0), seed, octaves, uv);
	d_light += fbm(size, float2(1.0, 1.0), seed, octaves,  uv * size + fbm1 + float2(time * time_speed, 0.0)) * 0.3; // change the magic 0.3 here for different light strengths
	
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

	float c1 = crater(craters_size, float2(1.0, 1.0), craters_seed, time, craters_time_speed, uv);
	float c2 = crater(craters_size, float2(1.0, 1.0), craters_seed, time, craters_time_speed, uv + (light_origin - 0.5) * 0.03);

	float3 col = cratersColor1;
	float a = step(0.5, c1);

	if (c2 < c1 - (0.5 - d_light) * 2.0) {
		col = cratersColor2;
	}
	if (d_light > craters_light_border) {
		col = cratersColor2;
	}

	// cut out a circle
	a *= step(d_circle, 0.5);

	return float4(col, a);
}

float4 computeLava(float2 inputUV) {
	// pixelize uv
	float2 uv = floor(inputUV * pixels) / pixels;
	float d_light = distance(uv, light_origin);

	// apply a tilt
	uv = rotate(uv, rotation);

	// map to sphere
	uv = spherify(uv);

	// some scrolling noise for landmasses
	float fbm1 = fbm(lava_size, float2(2.0, 1.0), lava_seed, lava_octaves, uv * lava_size + float2(time * lava_time_speed, 0.0));
	float river_fbm = fbm(lava_size, float2(2.0, 1.0), lava_seed, lava_octaves, uv + fbm1 * 2.5);

	// increase contrast on d_light
	d_light = pow(d_light, 2.0) * 0.4;
	d_light -= d_light * river_fbm;

	// apply colors
	float3 col = lava_color1;
	if (d_light > lava_light_border_1) {
		col = lava_color2;
	}
	if (d_light > lava_light_border_2) {
		col = lava_color3;
	}

	float a = step(lava_river_cutoff, river_fbm);
	a *= step(distance(float2(0.5, 0.5), uv), 0.5);

	return float4(col, a);
}

// Fragment composition here
float4 MainPS(VertexShaderInput input) : COLOR
{
	float2 inputUV = input.TextureCoordinates;

	float4 lavaRivers = computeLava(inputUV);
	float4 result;

	// Optimized rendering. Don't calculate what you don't need to.
	if (lavaRivers.a != 0.0) {
		result = lavaRivers;
	}
	else
	{
		float4 planetCraters = computeCraters(inputUV);
		if (planetCraters.a != 0.0) {
			result = planetCraters;
		}
		else {
			result = computePlanetUnder(inputUV);
		}
	}

	return result;
};

technique SpriteDrawing {
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};