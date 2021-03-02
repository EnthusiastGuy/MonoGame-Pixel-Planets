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
// Inner cloud
float inner_cloud_cover = 0.0;
float inner_cloud_time_speed = 0.7;
float inner_stretch = 1.0;
float inner_cloud_curve = 1.3;

float inner_light_border_1 = 0.52;
float inner_light_border_2 = 0.62;

static float3 inner_base_color = float3(0.231, 0.125, 0.152);
static float3 inner_outline_color = float3(0.231, 0.125, 0.152);
static float3 inner_shadow_base = float3(0.129, 0.094, 0.105);
static float3 inner_shadow_outline = float3(0.129, 0.094, 0.105);

float inner_size = 9.0;
int inner_octaves = 5;
float inner_seed = 5.939;	// expected 0 - 10

// Outer clouds
float outer_cloud_cover = 0.538;
float outer_cloud_time_speed = 0.47;
float outer_stretch = 1;
float outer_cloud_curve = 1.3;

float outer_light_border_1 = 0.439;
float outer_light_border_2 = 0.746;

static float3 outer_base_color = float3(0.941, 0.709, 0.254);
static float3 outer_outline_color = float3(0.811, 0.458, 0.168);
static float3 outer_shadow_base = float3(0.670, 0.317, 0.188);
static float3 outer_shadow_outline = float3(0.490, 0.219, 0.2);

float outer_size = 9.0;
int outer_octaves = 5;
float outer_seed = 5.939;	// expected 0 - 10

struct VertexShaderInput
{
	float4 Position: SV_POSITION;
	float4 Color: COLOR0;
	float2 TextureCoordinates: TEXCOORD0;
};

// Layers
float4 computeInnerCloud(float2 inputUV) {
	// pixelize uv
	float2 uv = floor(inputUV * pixels) / pixels;
	// distance to light source
	float d_light = distance(uv, float2(light_origin));

	uv = rotate(uv, rotation);
	// map to sphere
	uv = spherify(uv);

	// slightly make uv go down on the right, and up in the left
	uv.y += smoothstep(0.0, inner_cloud_curve, abs(uv.x - 0.4));

	float c = cloud_alpha(inner_size, float2(1.0, 1.0), inner_seed, time, inner_cloud_time_speed, inner_octaves, uv * float2(1.0, inner_stretch));

	// assign some colors based on cloud depth & distance from light
	float3 col = inner_base_color;

	if (c < inner_cloud_cover + 0.03) {
		col = inner_outline_color.rgb;
	}
	if (d_light + c * 0.2 > inner_light_border_1) {
		col = inner_shadow_base.rgb;

	}
	if (d_light + c * 0.2 > inner_light_border_2) {
		col = inner_shadow_outline.rgb;
	}

	return float4(col, step(inner_cloud_cover, c));
}

float4 computeOuterClouds(float2 inputUV) {
	// pixelize uv
	float2 uv = floor(inputUV * pixels) / pixels;

	// distance to light source
	float d_light = distance(uv, light_origin);

	uv = rotate(uv, rotation);
	// map to sphere
	uv = spherify(uv);

	// slightly make uv go down on the right, and up in the left
	uv.y += smoothstep(0.0, outer_cloud_curve, abs(uv.x - 0.4));

	float c = cloud_alpha(outer_size, float2(1.0, 1.0), outer_seed, time, outer_cloud_time_speed, outer_octaves, uv * float2(1.0, outer_stretch));

	// assign some colors based on cloud depth & distance from light
	float3 col = outer_base_color;

	if (c < outer_cloud_cover + 0.03) {
		col = outer_outline_color.rgb;
	}
	if (d_light + c * 0.2 > outer_light_border_1) {
		col = outer_shadow_base.rgb;

	}
	if (d_light + c * 0.2 > outer_light_border_2) {
		col = outer_shadow_outline.rgb;
	}

	return float4(col, step(outer_cloud_cover, c));
}

// Fragment composition here
float4 MainPS(VertexShaderInput input) : COLOR
{
	float2 inputUV = input.TextureCoordinates;
	float4 result;

	// Optimized rendering. Don't calculate what you don't need to.
	float4 outerClouds = computeOuterClouds(inputUV);
	if (outerClouds.a != 0.0) {
		result = outerClouds;
	}
	else {
		result = computeInnerCloud(inputUV);
	}

	return result;
};

technique SpriteDrawing {
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};