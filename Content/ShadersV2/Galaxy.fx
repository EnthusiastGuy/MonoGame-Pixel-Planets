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
float rotation = 0.0;
float time_speed = 0.2;
float dither_size = 2.0;
float should_dither = 1.0;

static float3 colors0 = float3(1.0, 1.0, 0.921569);
static float3 colors1 = float3(1.0, 0.913725, 0.552941);
static float3 colors2 = float3(0.709804, 0.878431, 0.4);
static float3 colors3 = float3(0.396078, 0.647059, 0.4);
static float3 colors4 = float3(0.223529, 0.364706, 0.392157);
static float3 colors5 = float3(0.196078, 0.223529, 0.301961);
static float3 colors6 = float3(0.196078, 0.160784, 0.278431);

float size = 50.0;
int OCTAVES = 6;
float seed = 5.881;

float tilt = 4.0;
float n_layers = 4.0;
float layer_height = 0.4;
float zoom = 2.0;
float swirl = -9.0;

int n_colors = 6;

struct VertexShaderInput
{
	float4 Position: SV_POSITION;
	float4 Color: COLOR0;
	float2 TextureCoordinates: TEXCOORD0;
};

float galaxy_rand(float2 coord, float seedVal) {
	return frac(sin(dot(coord.xy, float2(12.9898, 78.233))) * 15.5453 * seedVal);
}

float galaxy_noise(float2 coord, float seedVal) {
	float2 i = floor(coord);
	float2 f = frac(coord);

	float a = galaxy_rand(i, seedVal);
	float b = galaxy_rand(i + float2(1.0, 0.0), seedVal);
	float c = galaxy_rand(i + float2(0.0, 1.0), seedVal);
	float d = galaxy_rand(i + float2(1.0, 1.0), seedVal);

	float2 cubic = f * f * (3.0 - 2.0 * f);

	return lerp(a, b, cubic.x) + (c - a) * cubic.y * (1.0 - cubic.x) + (d - b) * cubic.x * cubic.y;
}

float galaxy_fbm(float2 coord, float seedVal, int octaves) {
	float value = 0.0;
	float scale = 0.5;
	for (int i = 0; i < 20; i++) {
		if (i >= octaves)
			break;
		value += galaxy_noise(coord, seedVal) * scale;
		coord *= 2.0;
		scale *= 0.5;
	}
	return value;
}

bool galaxy_dither(float2 uv1, float2 uv2, float px) {
	return glslmod(uv1.x + uv2.y, 2.0 / px) <= 1.0 / px;
}

float3 pick_color(int idx) {
	if (idx == 0) return colors0;
	if (idx == 1) return colors1;
	if (idx == 2) return colors2;
	if (idx == 3) return colors3;
	if (idx == 4) return colors4;
	if (idx == 5) return colors5;
	return colors6;
}

float4 computeGalaxy(float2 inputUV) {
	float2 uv = floor(inputUV * pixels) / pixels;
	bool dith = galaxy_dither(uv, inputUV, pixels);

	uv *= zoom;
	uv -= (zoom - 1.0) / 2.0;

	uv = rotate(uv, rotation);
	float2 uv2 = uv;

	float2 uvTilt = uv;
	uvTilt.y *= tilt;
	uvTilt.y -= (tilt - 1.0) / 2.0;

	float d_to_center = distance(uvTilt, float2(0.5, 0.5));
	float rotAmt = swirl * pow(d_to_center, 0.4);
	float2 rotated_uv = rotate(uvTilt, rotAmt + time * time_speed);

	float f1 = galaxy_fbm(rotated_uv * size, seed, OCTAVES);
	f1 = floor(f1 * n_layers) / n_layers;

	uv2.y *= tilt;
	uv2.y -= (tilt - 1.0) / 2.0 + f1 * layer_height;

	float d_to_center2 = distance(uv2, float2(0.5, 0.5));
	float rot2 = swirl * pow(d_to_center2, 0.4);
	float2 rotated_uv2 = rotate(uv2, rot2 + time * time_speed);
	float f2 = galaxy_fbm(rotated_uv2 * size + float2(f1, f1) * 10.0, seed, OCTAVES);

	float a = step(f2 + d_to_center2, 0.7);

	f2 *= 2.3;
	if (should_dither > 0.5 && dith) {
		f2 *= 0.94;
	}

	float fi = floor(f2 * (float)n_colors);
	fi = min(fi, (float)n_colors);
	int idx = (int)fi;
	idx = clamp(idx, 0, n_colors);

	float3 col = pick_color(idx);
	return float4(col.rgb, a * 1.0);
}

float4 MainPS(VertexShaderInput input) : COLOR
{
	return computeGalaxy(input.TextureCoordinates);
}

technique SpriteDrawing {
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
}
