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

// This rotation applies to both land and clouds. It can be split if needed
float rotation = 0.0;

// Land
float planet_time_speed = 0.1;
float planet_seed = 8.98; // expected 0 - 10
float planet_size = 4.6;
float river_cutoff = 0.368;
float dither_size = 3.951;
float light_border_1 = 0.52;
float light_border_2 = 0.62;
int planet_octaves = 6;

static float3 col1 = float3(0.388, 0.670, 0.247);
static float3 col2 = float3(0.231, 0.490, 0.309);
static float3 col3 = float3(0.184, 0.341, 0.325);
static float3 col4 = float3(0.156, 0.207, 0.250);
static float3 river_col = float3(0.309, 0.643, 0.721);
static float3 river_col_dark = float3(0.250, 0.286, 0.450);

// Clouds
float cloud_cover = 0.47;
float time_speed_clouds = 0.1;
float stretch = 2.0;
float cloud_curve = 1.3;
float light_border_clouds_1 = 0.52;
float light_border_clouds_2 = 0.62;

static float3 base_color = float3(0.960, 1.000, 0.909);
static float3 outline_color = float3(0.874, 0.878, 0.909);
static float3 shadow_base_color = float3(0.407, 0.435, 0.6);
static float3 shadow_outline_color = float3(0.250, 0.286, 0.450);

float size_clouds = 7.315;
int cloud_octaves = 2;
float seed_clouds = 5.939; // expected 0 - 10

//
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
	float d_light = distance(uv, light_origin);
	bool dith = dither(pixels, dither_size, uv, inputUV);

	uv = rotate(uv, rotation);
	// map to sphere
	uv = spherify(uv);

	float2 base_fbm_uv = uv * planet_size + float2(time * planet_time_speed, 0.0);

	float fbm1 = fbm(planet_size, float2(2.0, 1.0), planet_seed, planet_octaves, base_fbm_uv);
	float fbm2 = fbm(planet_size, float2(2.0, 1.0), planet_seed, planet_octaves, base_fbm_uv - light_origin * fbm1);
	float fbm3 = fbm(planet_size, float2(2.0, 1.0), planet_seed, planet_octaves, base_fbm_uv - light_origin * 1.5 * fbm1);
	float fbm4 = fbm(planet_size, float2(2.0, 1.0), planet_seed, planet_octaves, base_fbm_uv - light_origin * 2.0 * fbm1);

	float river_fbm = fbm(planet_size, float2(2.0, 1.0), planet_seed, planet_octaves, base_fbm_uv + fbm1 * 6.0);
	river_fbm = step(river_cutoff, river_fbm);

	float dither_border = (1.0 / pixels) * dither_size;

	if (d_light < light_border_1) {
		fbm4 *= 0.9;
	}
	if (d_light > light_border_1) {
		fbm2 *= 1.05;
		fbm3 *= 1.05;
		fbm4 *= 1.05;
	}
	if (d_light > light_border_2) {
		fbm2 *= 1.3;
		fbm3 *= 1.4;
		fbm4 *= 1.8;
		if (d_light < light_border_2 + dither_border && dith) {
			fbm4 *= 0.5;
		}
	}

	d_light = pow(d_light, 2.0) * 0.4;
	float3 col = col4;
	if (fbm4 + d_light < fbm1 * 1.5) {
		col = col3;
	}
	if (fbm3 + d_light < fbm1 * 1.0) {
		col = col2;
	}
	if (fbm2 + d_light < fbm1) {
		col = col1;
	}
	if (river_fbm < fbm1 * 0.5) {
		col = river_col_dark;
		if (fbm4 + d_light < fbm1 * 1.5) {
			col = river_col;
		}
	}

	return float4(col, step(distance(float2(0.5, 0.5), uv), 0.5));
}

// The clouds
float4 computeClouds(float2 inputUV) {
	// pixelize uv
	float2 uv = floor(inputUV * pixels) / pixels;
	// distance to light source
	float d_light = distance(uv, light_origin);

	float d_to_center = distance(uv, float2(0.5, 0.5));

	uv = rotate(uv, rotation);
	// map to sphere
	uv = spherify(uv);

	// slightly make uv go down on the right, and up in the left
	uv.y += smoothstep(0.0, cloud_curve, abs(uv.x - 0.4));

	float c = cloud_alpha(size_clouds, float2(1.0, 1.0), seed_clouds, time, time_speed_clouds, cloud_octaves, uv * float2(1.0, stretch));

	// assign some colors based on cloud depth & distance from light
	float3 col = base_color;
	if (c < cloud_cover + 0.03) {
		col = outline_color;
	}
	if (d_light + c * 0.2 > light_border_clouds_1) {
		col = shadow_base_color;

	}
	if (d_light + c * 0.2 > light_border_clouds_2) {
		col = shadow_outline_color;
	}

	c *= step(d_to_center, 0.5);

	return float4(col, step(cloud_cover, c));
}

// Fragment composition here
float4 MainPS(VertexShaderInput input) : COLOR
{
	float2 inputUV = input.TextureCoordinates;

	float4 planetClouds = computeClouds(inputUV);
	float4 result;

	// Optimized rendering. Don't calculate what you don't need to.
	if (planetClouds.a != 0.0) {
		result = planetClouds;
	}
	else {
		result = computeLand(inputUV);
	}

	return result;
};

technique SpriteDrawing {
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};