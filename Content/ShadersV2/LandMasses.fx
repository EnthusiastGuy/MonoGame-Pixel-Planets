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

// Water
float time_speed = 0.1;
float dither_size = 2.0;	// 1 is neutral
float light_border_1 = 0.4;
float light_border_2 = 0.6;

static float3 color1 = float3(0.572, 0.909, 0.752);
static float3 color2 = float3(0.309, 0.643, 0.721);
static float3 color3 = float3(0.172, 0.207, 0.301);

float size = 5.228;
int OCTAVES = 3;
float seed = 10; // expected 0 - 10

// Land
float land_time_speed = 0.2;
float land_dither_size = 2.0;
float light_border_land_1 = 0.32;
float light_border_land_2 = 0.534;
float land_cutoff = 0.633;

static float3 landColor1 = float3(0.784, 0.831, 0.364);
static float3 landColor2 = float3(0.388, 0.670, 0.247);
static float3 landColor3 = float3(0.184, 0.341, 0.325);
static float3 landColor4 = float3(0.156, 0.207, 0.250);

float land_seed = 7.947; // expected 0 - 10
int land_octaves = 6;

float sizeLakes = 10.0;

// Clouds
float cloud_cover = 0.515;
float time_speed_clouds = 0.2;
float stretch = 2;
float cloud_curve = 1.3;
static float light_border_clouds_1 = 0.52;
static float light_border_clouds_2 = 0.62;

int OCTAVES_CLOUDS = 2;
float seedClouds = 5.39; // expected 0 - 10
float sizeClouds = 7.745;

static float3 base_color = float3(0.874, 0.878, 0.909);
static float3 outline_color = float3(0.639, 0.654, 0.760);
static float3 shadow_base_color = float3(0.407, 0.435, 0.6);
static float3 shadow_outline_color = float3(0.250, 0.286, 0.450);

struct VertexShaderInput
{
	float4 Position: SV_POSITION;
	float4 Color: COLOR0;
	float2 TextureCoordinates: TEXCOORD0;
};

// Layers
float4 computeWater(float2 inputUV) {
	// pixelize uv
	float2 uv = floor(inputUV * pixels) / pixels;

	// we use this value later to dither between colors
	bool dith = dither(pixels, dither_size, uv, inputUV);

	// map to sphere
	uv = spherify(uv);

	// TODO figure out why this rotation call is after spherify in THIS instance only
	uv = rotate(uv, rotation);

	// check distance from center & distance to light
	float d_circle = distance(uv, float2(0.5, 0.5));
	float d_light = distance(uv, float2(light_origin));

	// cut out a circle
	float a = step(d_circle, 0.5);

	// get a noise value with light distance added
	d_light += fbm(size, float2(2.0, 1.0), seed, OCTAVES, uv * size + float2(time * time_speed, 0.0)) * 0.7; // change the magic 0.7 here for different light strengths

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

float4 computeLand(float2 inputUV) {
	// pixelize uv
	float2 uv = floor(inputUV * pixels) / pixels;

	// distance to light source
	float d_light = distance(uv, light_origin);

	// give planet a tilt
	uv = rotate(uv, rotation);

	// map to sphere
	uv = spherify(uv);

	// some scrolling noise for landmasses
	// some scrolling noise for landmasses
	float2 base_fbm_uv = (uv)*size + float2(time * land_time_speed, 0.0);

	float fbm1 = fbm(sizeLakes, float2(2.0, 1.0), land_seed, land_octaves, base_fbm_uv);
	float fbm2 = fbm(sizeLakes, float2(2.0, 1.0), land_seed, land_octaves, base_fbm_uv - light_origin * fbm1);
	float fbm3 = fbm(sizeLakes, float2(2.0, 1.0), land_seed, land_octaves, base_fbm_uv - light_origin * 1.5 * fbm1);
	float fbm4 = fbm(sizeLakes, float2(2.0, 1.0), land_seed, land_octaves, base_fbm_uv - light_origin * 2.0 * fbm1);

	// lots of magic numbers here
	// you can mess with them, it changes the color distribution
	if (d_light < light_border_land_1) {
		fbm4 *= 0.9;
	}
	if (d_light > light_border_land_1) {
		fbm2 *= 1.05;
		fbm3 *= 1.05;
		fbm4 *= 1.05;
	}
	if (d_light > light_border_land_2) {
		fbm2 *= 1.3;
		fbm3 *= 1.4;
		fbm4 *= 1.8;
	}

	// increase contrast on d_light
	d_light = pow(d_light, 2.0) * 0.1;

	float3 col = landColor4;
	if (fbm4 + d_light < fbm1) {
		col = landColor3;
	}
	if (fbm3 + d_light < fbm1) {
		col = landColor2;
	}
	if (fbm2 + d_light < fbm1) {
		col = landColor1;
	}

	float a = step(land_cutoff, fbm1);

	return float4(col, a);
}

float4 computeClouds(float2 inputUV) {
	// pixelize uv
	float2 uv = floor(inputUV * pixels) / pixels;

	// distance to light source
	float d_light = distance(uv, light_origin);

	// distance to center
	float d_to_center = distance(uv, float2(0.5, 0.5));

	// give planet a tilt
	uv = rotate(uv, rotation);

	// map to sphere
	uv = spherify(uv);

	// slightly make uv go down on the right, and up in the left
	uv.y += smoothstep(0.0, cloud_curve, abs(uv.x - 0.4));

	float c = cloud_alpha(sizeClouds, float2(1.0, 1.0), seedClouds, time, time_speed_clouds, OCTAVES_CLOUDS, uv * float2(1.0, stretch));

	// assign some colors based on cloud depth & distance from light
	float3 col = base_color;
	if (c < cloud_cover + 0.03) {
		col = outline_color;
	}

	if (d_light + c * 0.2 > light_border_1) {
		col = shadow_base_color;
	}

	/*if (d_light + c * 0.2 > light_border_2) {
		col = shadow_outline_color;
	}*/

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
		float4 planetLand = computeLand(inputUV);
		if (planetLand.a != 0.0) {
			result = planetLand;
		}
		else {
			result = computeWater(inputUV);
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