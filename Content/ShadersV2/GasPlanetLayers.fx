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
float2 light_origin = float2(-0.1, 0.3);
float fullScale = 1.0;

// Parameters
// Gas layers
float gas_rotation = 0.0;
float gas_dither_size = 1.0;
float gas_time_speed = 0.05;
float gas_bands = 1.0;

float gas_size = 8.0;
int gas_octaves = 2;
float gas_seed = 3.036; // expected 0 - 10

// color scheme (light -> dark -> darker)
// #eec39a -> #d9a066 -> #8f563b
static float3 lightColors[15] = {
	float3(0.933, 0.764, 0.603),		// #eec39a
	float3(0.913, 0.729, 0.552),
	float3(0.894, 0.698, 0.501),
	float3(0.870, 0.662, 0.450),
	float3(0.850, 0.627, 0.4),			// #d9a066
	float3(0.792, 0.568, 0.364),
	float3(0.733, 0.509, 0.333),
	float3(0.678, 0.454, 0.298),
	float3(0.619, 0.396, 0.266),
	float3(0.560, 0.337, 0.231),		// #8f563b
	float3(0.0, 1.0, 1.0),				// control colors from here on. To watch for overflows
	float3(0.0, 0.8, 0.8),
	float3(0.0, 0.6, 0.6),
	float3(0.0, 0.4, 0.4),
	float3(0.0, 0.2, 0.2)
};

// dark color scheme (light -> dark -> darker)
// #663931 -> #45283c -> #222034
static float3 darkColors[15] = {
	float3(0.4, 0.223, 0.192),			// #663931
	float3(0.368, 0.207, 0.203),
	float3(0.337, 0.192, 0.215),
	float3(0.301, 0.172, 0.223),
	float3(0.270, 0.156, 0.235),		// #45283c
	float3(0.243, 0.149, 0.227),
	float3(0.215, 0.145, 0.223),
	float3(0.188, 0.137, 0.215),
	float3(0.160, 0.133, 0.211),
	float3(0.133, 0.125, 0.203),		// #222034
	float3(1.0, 1.0, 1.0),				// control colors from here on. To watch for overflows
	float3(1.0, 0.8, 0.8),
	float3(1.0, 0.6, 0.6),
	float3(1.0, 0.4, 0.4),
	float3(1.0, 0.2, 0.2)
};

// Ring
float ring_rotation = -.5;
float ring_time_speed = 0.2;
float ring_light_border_1 = 0.52;
float ring_light_border_2 = 0.62;
float ring_width = 0.127;
float ring_perspective = 6.0;
float ring_scale_rel_to_planet = 6.0;
float ring_dither_size = 1.0;

float ring_size = 5.0;
int ring_octaves = 4.0;
float ring_seed = 8.461;

struct VertexShaderInput
{
	float4 Position: SV_POSITION;
	float4 Color: COLOR0;
	float2 TextureCoordinates: TEXCOORD0;
};

float3 colorSelection(float3 colors[15], float posterized) {
	int pos = floor(posterized * 9.5);
	return colors[pos];
};

// Layers
float4 computePlanetUnder(float2 inputUV) {
	// make the planet smaller by ring scale, to fit the ring
	float tScale = ring_scale_rel_to_planet / 2;
	//float2 uv = inputUV * tScale - (1.0 * (tScale - 1)) / 2.0;
	float2 uv = scale(inputUV, tScale);
	uv = scale(uv, 1.0 / fullScale);
	// pixelize uv
	uv = floor(uv * pixels) / pixels;
	float light_d = distance(uv, light_origin);

	// we use this value later to dither between colors
	bool dith = dither(pixels, gas_dither_size, uv, inputUV);

	uv = rotate(uv, gas_rotation);

	// map to sphere
	uv = spherify(uv);

	// a band is just one dimensional noise
	float band = fbm(gas_size, float2(2.0, 1.0), gas_seed, gas_octaves, float2(0.0, uv.y * gas_size * gas_bands));

	// turbulence value is circles on top of each other
	float turb = turbulence(gas_size, float2(2.0, 1.0), gas_seed, time, gas_time_speed, uv);

	// by layering multiple noise values & combining with turbulence and bands
	// we get some dynamic looking shape	
	float fbm1 = fbm(gas_size, float2(2.0, 1.0), gas_seed, gas_octaves, uv * gas_size);
	float fbm2 = fbm(gas_size, float2(2.0, 1.0), gas_seed, gas_octaves, uv * float2(1.0, 2.0) * gas_size + fbm1 + float2(-time * gas_time_speed, 0.0) + turb);

	// all of this is just increasing some contrast & applying light
	fbm2 = fbm2 * pow(band, 2.0) * 7.0;
	float light = fbm2 + light_d * 1.8;
	fbm2 = fbm2 + pow(light_d, 1.0) - 0.3;
	fbm2 = smoothstep(-0.2, 4.0 - fbm2, light);

	// here apply the dither value
	if (dith) {
		fbm2 *= 1.1;
	}

	// finally add colors
	float posterized = floor(fbm2 * 4.0) / 2.0;
	float3 col;

	if (fbm2 < 0.625) {
		col = colorSelection(lightColors, posterized);	// color scheme
	}
	else {
		col = colorSelection(darkColors, posterized - 1.0);	// dark color scheme
	}

	float a = step(length(uv - float2(0.5, 0.5)), 0.5);

	return float4(col, a);
}

float4 computeRing(float2 inputUV) {
	// pixelize uv
	float2 scaledUV = scale(inputUV, 1.0 / fullScale);
	float2 uv = floor(scaledUV * pixels) / pixels;
	float light_d = distance(uv, light_origin);

	uv = rotate(uv, gas_rotation + ring_rotation);

	// center is used to determine ring position
	float2 uv_center = uv - float2(0.0, 0.5);

	// tilt ring
	uv_center *= float2(1.0, ring_perspective);
	float center_d = distance(uv_center, float2(0.5, 0.0));

	// cut out 2 circles of different sizes and only intersection of the 2.
	float ring = smoothstep(0.5 - ring_width * 2.0, 0.5 - ring_width, center_d);
	ring *= smoothstep(center_d - ring_width, center_d, 0.4);

	// pretend like the ring goes behind the planet by removing it if it's in the upper half.
	if (uv.y < 0.5) {
		ring *= step(1.0 / ring_scale_rel_to_planet, distance(uv, float2(0.5, 0.5)));
	}

	// rotate material in the ring
	uv_center = rotate(uv_center + float2(0, 0.5), time * ring_time_speed);
	// some noise
	ring *= fbm(ring_size, float2(2.0, 1.0), ring_seed, ring_octaves, uv_center * ring_size);

	// apply some colors based on final value
	float posterized = floor((ring + pow(light_d, 2.0) * 2.0) * 4.0) / 4.0;
	float3 col;

	if (posterized < 1.0) {
		col = colorSelection(lightColors, posterized);	// color scheme
	}
	else {
		col = colorSelection(darkColors, posterized - 1.0);	// dark color scheme
	}

	float a = step(0.28, ring);

	return float4(col, a);
}

// Fragment composition here
float4 MainPS(VertexShaderInput input) : COLOR
{
	float2 inputUV = input.TextureCoordinates;

	float4 planetRing = computeRing(inputUV);
	float4 result;

	// Optimized rendering. Don't calculate what you don't need to.
	if (planetRing.a != 0.0) {
		result = planetRing;
	}
	else
	{
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