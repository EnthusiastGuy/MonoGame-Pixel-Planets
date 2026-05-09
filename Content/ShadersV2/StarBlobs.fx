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

static float3 blob_color = float3(1.0, 1.0, 0.894118);

float time_speed = 0.05;
float rotation = 0.0;

float seed = 3.078;
float circle_amount = 2.0;
float circle_size = 1.0;

float size = 4.93;
int OCTAVES = 4;

struct VertexShaderInput
{
	float4 Position: SV_POSITION;
	float4 Color: COLOR0;
	float2 TextureCoordinates: TEXCOORD0;
};

float star_rand(float2 co, float sz, float sd) {
	co = glslmod(co, float2(1.0, 1.0) * round(sz));
	return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 15.5453 * sd);
}

float2 hash2_star(float2 p) {
	float r = 523.0 * sin(dot(p, float2(53.3158, 43.6143)));
	return float2(frac(15.32354 * r), frac(17.25865 * r));
}

float CellsStar(float2 p, float numCells, float tilesVal, float sz, float sd, float tm) {
	p *= numCells;
	float d = 1.0e10;
	for (int xo = -1; xo <= 1; xo++)
	{
		for (int yo = -1; yo <= 1; yo++)
		{
			float2 tp = floor(p) + float2((float)xo, (float)yo);
			tp = p - tp - hash2_star(glslmod(tp, numCells / tilesVal));
			d = min(d, dot(tp, tp));
		}
	}
	return sqrt(d);
}

float circle_star(float2 uv, float camt, float csize, float sz, float sd) {
	float invert = 1.0 / camt;

	if (glslmod(uv.y, invert * 2.0) < invert) {
		uv.x += invert * 0.5;
	}
	float2 rand_co = floor(uv * camt) / camt;
	uv = glslmod(uv, invert) * camt;

	float r = star_rand(rand_co, sz, sd);
	r = clamp(r, invert, 1.0 - invert);
	float circle = distance(uv, float2(r, r));
	return smoothstep(circle, circle + 0.5, invert * csize * star_rand(rand_co * 1.5, sz, sd));
}

float star_noise(float2 coord, float sz, float sd) {
	float2 i = floor(coord);
	float2 f = frac(coord);

	float a = star_rand(i, sz, sd);
	float b = star_rand(i + float2(1.0, 0.0), sz, sd);
	float c = star_rand(i + float2(0.0, 1.0), sz, sd);
	float d = star_rand(i + float2(1.0, 1.0), sz, sd);

	float2 cubic = f * f * (3.0 - 2.0 * f);

	return lerp(a, b, cubic.x) + (c - a) * cubic.y * (1.0 - cubic.x) + (d - b) * cubic.x * cubic.y;
}

float star_fbm(float2 coord, float sz, float sd, int octaves) {
	float value = 0.0;
	float scl = 0.5;
	for (int i = 0; i < 20; i++) {
		if (i >= octaves)
			break;
		value += star_noise(coord, sz, sd) * scl;
		coord *= 2.0;
		scl *= 0.5;
	}
	return value;
}

float4 computeStarBlobs(float2 inputUV) {
	float2 pixelized = floor(inputUV * pixels) / pixels;

	float2 uv = rotate(pixelized, rotation);

	float angle = atan2(uv.x - 0.5, uv.y - 0.5);
	float d = distance(pixelized, float2(0.5, 0.5));

	// Match Godot's relative_scale = 2.0 (blobs drawn on 2x canvas)
	d *= 0.5;

	float c = 0.0;
	for (int i = 0; i < 15; i++) {
		float r = star_rand(float2((float)i, (float)i), size, seed);
		float2 circleUV = float2(d, angle);
		c += circle_star(circleUV * size - time * time_speed - (1.0 / max(d, 0.001)) * 0.1 + r, circle_amount, circle_size, size, seed);
	}

	c *= 0.37 - d;
	c = step(0.07, c - d);

	return float4(blob_color.rgb, c * 1.0);
}

float4 MainPS(VertexShaderInput input) : COLOR
{
	return computeStarBlobs(input.TextureCoordinates);
}

technique SpriteDrawing {
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
}
