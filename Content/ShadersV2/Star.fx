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
float pixels = 100.0;

float time_speed = 0.05;
float rotation = 0.0;
float should_dither = 1.0;

float seed = 4.837;
float size = 4.463;
int OCTAVES = 4;
float TILES = 1.0;

static float3 star_colors0 = float3(0.960784, 1.0, 0.909804);
static float3 star_colors1 = float3(0.466667, 0.839216, 0.756863);
static float3 star_colors2 = float3(0.109804, 0.572549, 0.654902);
static float3 star_colors3 = float3(0.0117647, 0.243137, 0.368627);

int n_colors = 4;

struct VertexShaderInput
{
	float4 Position: SV_POSITION;
	float4 Color: COLOR0;
	float2 TextureCoordinates: TEXCOORD0;
};

// Original GLSL Hash2 — simple sin-based, time-independent
float2 Hash2Star(float2 p) {
	float r = 523.0 * sin(dot(p, float2(53.3158, 43.6143)));
	return float2(frac(15.32354 * r), frac(17.25865 * r));
}

// Tileable cell noise matching Godot's Star.gdshader (Dave_Hoskins)
float CellsStar(float2 p, float numCells) {
	p *= numCells;
	float d = 1.0e10;
	for (int xo = -1; xo <= 1; xo++)
	{
		for (int yo = -1; yo <= 1; yo++)
		{
			float2 tp = floor(p) + float2((float)xo, (float)yo);
			tp = p - tp - Hash2Star(glslmod(tp, numCells / TILES));
			d = min(d, dot(tp, tp));
		}
	}
	return sqrt(d);
}

float3 pick_star_body_color(int idx) {
	if (idx == 0) return star_colors0;
	if (idx == 1) return star_colors1;
	if (idx == 2) return star_colors2;
	return star_colors3;
}

float4 computeStarBody(float2 inputUV) {
	float2 pixelized = floor(inputUV * pixels) / pixels;

	float a = step(distance(pixelized, float2(0.5, 0.5)), 0.49999);

	bool dith = dither(pixels, 1.0, pixelized, inputUV);

	float2 uv = rotate(pixelized, rotation);
	uv = spherify(uv);

	float n = CellsStar(uv - float2(time * time_speed * 2.0, 0.0), 10.0);
	n *= CellsStar(uv - float2(time * time_speed * 1.0, 0.0), 20.0);

	n *= 2.0;
	n = clamp(n, 0.0, 1.0);
	if (dith || should_dither < 0.5) {
		n *= 1.3;
	}

	float fi = floor(n * (float)(n_colors - 1));
	int idx = (int)fi;
	idx = clamp(idx, 0, n_colors - 1);

	float3 col = pick_star_body_color(idx);

	return float4(col.rgb, a * 1.0);
}

float4 MainPS(VertexShaderInput input) : COLOR
{
	return computeStarBody(input.TextureCoordinates);
}

technique SpriteDrawing {
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
}
