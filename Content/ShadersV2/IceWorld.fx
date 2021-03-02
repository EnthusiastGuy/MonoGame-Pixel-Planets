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

// Planet
float rotation = 0.0;
float time_speed = 0.25;
float dither_size = 2.0;	// 1 is neutral
float light_border_1 = 0.48;
float light_border_2 = 0.632;

static float3 color1 = float3(0.98, 1.00, 1.00);
static float3 color2 = float3(0.78, 0.83, 0.88);
static float3 color3 = float3(0.57, 0.56, 0.72);

float size = 8.0;
int OCTAVES = 2;
float seed = 3.036; // expected 0 - 10

// Lakes
float lake_cutoff = 0.55;

float light_border_lake_1 = 0.024;
float light_border_lake_2 = 0.047;

static float3 lakeColor1 = float3(0.309, 0.643, 0.721);
static float3 lakeColor2 = float3(0.298, 0.407, 0.521);
static float3 lakeColor3 = float3(0.227, 0.247, 0.368);

float seedLakes = 4.14; // expected 0 - 10
float sizeLakes = 10.0;

// Clouds
float cloud_cover = 0.546;
float stretch = 2.5;
float cloud_curve = 1.3;
float light_border_clouds_1 = 0.566;
float light_border_clouds_2 = 0.781;

int OCTAVES_CLOUDS = 4;
float seedClouds = 1.14; // expected 0 - 10
float sizeClouds = 4.0;
float time_speed_clouds = 0.1;

static float3 base_color = float3(0.882, 0.949, 1.0);
static float3 outline_color = float3(0.752, 0.890, 1.0);
static float3 shadow_base_color = float3(0.368, 0.439, 0.647);
static float3 shadow_outline_color = float3(0.250, 0.286, 0.450);


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
	// map to sphere
	uv = spherify(uv);


	// check distance from center & distance to light
	float d_circle = distance(uv, float2(0.5, 0.5));
	float d_light = distance(uv, float2(light_origin));

	// cut out a circle
	float a = step(d_circle, 0.5);

	// get a noise value with light distance added
	d_light += fbm(size, float2(2.0, 1.0), seed, OCTAVES, uv * size + float2(time * time_speed, 0.0)) * 0.3; // change the magic 0.3 here for different light strengths

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

float4 computeLakes(float2 inputUV) {
	// pixelize uv
	float2 uv = floor(inputUV * pixels) / pixels;

	float d_light = distance(uv, light_origin);

	uv = rotate(uv, rotation);
	// map to sphere
	uv = spherify(uv);

	// some scrolling noise for landmasses
	float fbm1 = fbm(sizeLakes, float2(2.0, 1.0), seedLakes, OCTAVES, uv * size + float2(time * time_speed, 0.0));
	float lake = fbm(sizeLakes, float2(2.0, 1.0), seedLakes, OCTAVES, uv * size + float2(time * time_speed, 0.0));

	// increase contrast on d_light
	d_light = pow(d_light, 2.0) * 0.4;
	d_light -= d_light * lake;

	float3 col = lakeColor1;
	if (d_light > light_border_lake_1) {
		col = lakeColor2;
	}
	if (d_light > light_border_lake_2) {
		col = lakeColor3;
	}

	float a = step(lake_cutoff, lake);
	a *= step(distance(float2(0.5, 0.5), uv), 0.5);

	return float4(col, a);
}

float4 computeClouds(float2 inputUV) {
	// pixelize uv
	float2 uv = floor(inputUV * pixels) / pixels;
	float d_light = distance(uv, light_origin);

	float d_to_center = distance(uv, float2(0.5, 0.5));

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
	if (d_light + c * 0.2 > light_border_2) {
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
		float4 planetLakes = computeLakes(inputUV);
		if (planetLakes.a != 0.0) {
			result = planetLakes;
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