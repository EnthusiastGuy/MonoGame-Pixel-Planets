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
float pixels = 200.0;

float3 flare_col0 = float3(0.466667, 0.839216, 0.756863);
float3 flare_col1 = float3(1.0, 1.0, 0.894118);

float time_speed = 0.05;
float rotation = 0.0;
float should_dither = 1.0;

float storm_width = 0.3;
float storm_dither_width = 0.0;

float flare_scale = 1.0;
float seed = 3.078;
float circle_amount = 2.0;
float circle_scale = 1.0;

float size = 1.6;
int OCTAVES = 4;

struct VertexShaderInput
{
	float4 Position: SV_POSITION;
	float4 Color: COLOR0;
	float2 TextureCoordinates: TEXCOORD0;
};

float flare_rand(float2 co, float sz, float sd) {
	co = glslmod(co, float2(1.0, 1.0) * round(sz));
	return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 15.5453 * sd);
}

float2 rotate_uv(float2 vec, float angle) {
	vec -= float2(0.5, 0.5);
	float2x2 modifier = float2x2(float2(cos(angle), -sin(angle)), float2(sin(angle), cos(angle)));
	vec = mul(vec, modifier);
	return vec + float2(0.5, 0.5);
}

float circle_flare(float2 uv, float camt, float cscale, float sz, float sd) {
	float invert = 1.0 / camt;

	if (glslmod(uv.y, invert * 2.0) < invert) {
		uv.x += invert * 0.5;
	}
	float2 rand_co = floor(uv * camt) / camt;
	uv = glslmod(uv, invert) * camt;

	float r = flare_rand(rand_co, sz, sd);
	r = clamp(r, invert, 1.0 - invert);
	float circle = distance(uv, float2(r, r));
	return smoothstep(circle, circle + 0.5, invert * cscale * flare_rand(rand_co * 1.5, sz, sd));
}

float flare_noise(float2 coord, float sz, float sd) {
	float2 i = floor(coord);
	float2 f = frac(coord);

	float a = flare_rand(i, sz, sd);
	float b = flare_rand(i + float2(1.0, 0.0), sz, sd);
	float c = flare_rand(i + float2(0.0, 1.0), sz, sd);
	float d = flare_rand(i + float2(1.0, 1.0), sz, sd);

	float2 cubic = f * f * (3.0 - 2.0 * f);

	return lerp(a, b, cubic.x) + (c - a) * cubic.y * (1.0 - cubic.x) + (d - b) * cubic.x * cubic.y;
}

float flare_fbm(float2 coord, float sz, float sd, int octaves) {
	float value = 0.0;
	float scl = 0.5;
	for (int i = 0; i < 20; i++) {
		if (i >= octaves)
			break;
		value += flare_noise(coord, sz, sd) * scl;
		coord *= 2.0;
		scl *= 0.5;
	}
	return value;
}

bool flare_dither(float2 uv1, float2 uv2, float px) {
	return glslmod(uv1.x + uv2.y, 2.0 / px) <= 1.0 / px;
}

float4 computeStarFlares(float2 inputUV) {
	float2 pixelized = floor(inputUV * pixels) / pixels;
	bool dith = flare_dither(inputUV, pixelized, pixels);

	float2 uv = rotate_uv(pixelized, rotation);

	float angle = atan2(uv.x - 0.5, uv.y - 0.5) * 0.4;
	float d = distance(pixelized, float2(0.5, 0.5));

	// Match Godot's relative_scale = 2.0 (flares drawn on 2x canvas)
	d *= 0.5;

	float2 circleUV = float2(d, angle);

	float n = flare_fbm(circleUV * size - time * time_speed, size, seed, OCTAVES);
	float nc = circle_flare(circleUV * flare_scale - time * time_speed + n, circle_amount, circle_scale, size, seed);

	nc *= 1.5;
	float n2 = flare_fbm(circleUV * size - time + float2(100, 100), size, seed, OCTAVES);
	nc -= n2 * 0.1;

	float a = 0.0;
	if (1.0 - d > nc) {
		if (nc > storm_width - storm_dither_width + d && (dith || should_dither < 0.5)) {
			a = 1.0;
		}
		else if (nc > storm_width + d) {
			a = 1.0;
		}
	}

	float interpolate = floor(n2 + nc);
	int idx = (int)interpolate;
	idx = clamp(idx, 0, 1);
	float3 col = idx == 0 ? flare_col0 : flare_col1;

	a *= step(n2 * 0.25, d);

	return float4(col.rgb, a * 1.0);
}

float4 MainPS(VertexShaderInput input) : COLOR
{
	return computeStarFlares(input.TextureCoordinates);
}

technique SpriteDrawing {
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
}
