// Default effect
#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

#include "planet_utils.fx"

// PARAMETERS
float time = 0.0;
float pixels = 200;
float2 light_origin = float2(0.0, 0.0);

float rotation = 0.0;

float light_distance1 = 0.362;
float light_distance2 = 0.525;
float time_speed = 0.1;
float dither_size = 2.0;
float size = 8.0;
int OCTAVES = 3;
float seed = 1.175; // expected 0 - 10

static float3 color1 = float3(1.000, 0.537, 0.200);
static float3 color2 = float3(0.898, 0.266, 0.219);
static float3 color3 = float3(0.674, 0.184, 0.266);
static float3 color4 = float3(0.317, 0.196, 0.243);
static float3 color5 = float3(0.239, 0.156, 0.211);

struct VertexShaderInput
{
	float4 Position: SV_POSITION;
	float4 Color: COLOR0;
	float2 TextureCoordinates: TEXCOORD0;
};

// Layers
// Land
float4 computeLand(float2 inputUV) {
	float2 uv = floor(inputUV * pixels) / pixels;
	bool dith = dither(pixels, dither_size, uv, inputUV);

	uv = rotate(uv, rotation);

	uv = spherify(uv);

	float d_circle = distance(uv, float2(0.5, 0.5));
	float d_light = distance(uv, float2(light_origin));

	// cut out a circle
	float a = step(d_circle, 0.5);

	// noise
	float f = fbm(size, float2(2.0, 1.0), seed, OCTAVES, uv * size + float2(time * time_speed, 0.0));

	// remap light
	d_light = smoothstep(-0.3, 1.2, d_light);

	if (d_light < light_distance1) {
		d_light *= 0.9;
	}
	if (d_light < light_distance2) {
		d_light *= 0.9;
	}

	float c = d_light * pow(abs(f), 0.8) * 3.5; // change the magic nums here for different light strengths

	// apply dithering
	if (dith) {
		c += 0.02;
		c *= 1.05;
	}

	float posterize = floor(c * 4.0) / 4.0;

	float3 col;

	if (posterize < 0.25)
	{
		col = color1;
	}
	else if (posterize < 0.40)
	{
		col = color2;
	}
	else if (posterize < 0.65)
	{
		col = color3;
	}
	else if (posterize < 0.80)
	{
		col = color4;
	}
	else {
		col = color5;
	}

	return float4(col, a);
}


// Fragment composition here
float4 MainPS(VertexShaderInput input) : COLOR
{
	float2 inputUV = input.TextureCoordinates;

	return computeLand(inputUV);
};

technique SpriteDrawing {
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};