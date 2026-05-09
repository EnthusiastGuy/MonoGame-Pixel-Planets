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
float pixels_ring = 300.0;

float2 light_origin = float2(0.607, 0.444);

float rotation = 0.766;
float time_speed = 0.2;
float disk_width = 0.065;
float ring_perspective = 14.0;
float should_dither = 1.0;

float3 hole_c0 = float3(0.152941, 0.152941, 0.211765);
float3 hole_c1 = float3(1.0, 1.0, 0.921569);
float3 hole_c2 = float3(0.929412, 0.482353, 0.223529);

float hole_radius = 0.247;
float hole_light_width = 0.028;

float3 ring_c0 = float3(1.0, 1.0, 0.921569);
float3 ring_c1 = float3(1.0, 0.960784, 0.25098);
float3 ring_c2 = float3(1.0, 0.721569, 0.290196);
float3 ring_c3 = float3(0.929412, 0.482353, 0.223529);
float3 ring_c4 = float3(0.741176, 0.25098, 0.207843);

float disk_size = 6.598;
int OCTAVES = 3;
float seed = 8.175;
int n_colors = 5;

struct VertexShaderInput
{
	float4 Position: SV_POSITION;
	float4 Color: COLOR0;
	float2 TextureCoordinates: TEXCOORD0;
};

float4 black_hole_core(float2 uv, float px) {
	float2 puv = floor(uv * px) / px;
	float d_to_center = distance(puv, float2(0.5, 0.5));

	float3 col = hole_c0;
	if (d_to_center > hole_radius - hole_light_width)
		col = hole_c1;
	if (d_to_center > hole_radius - hole_light_width * 0.5)
		col = hole_c2;

	float a = step(d_to_center, hole_radius);
	return float4(col, a);
}

float ring_rand(float2 coord, float sz, float sd) {
	coord = glslmod(coord, float2(2.0, 1.0) * round(sz));
	return frac(sin(dot(coord.xy, float2(12.9898, 78.233))) * 15.5453 * sd);
}

float ring_noise(float2 coord, float sz, float sd) {
	float2 i = floor(coord);
	float2 f = frac(coord);

	float a = ring_rand(i, sz, sd);
	float b = ring_rand(i + float2(1.0, 0.0), sz, sd);
	float c = ring_rand(i + float2(0.0, 1.0), sz, sd);
	float d = ring_rand(i + float2(1.0, 1.0), sz, sd);

	float2 cubic = f * f * (3.0 - 2.0 * f);

	return lerp(a, b, cubic.x) + (c - a) * cubic.y * (1.0 - cubic.x) + (d - b) * cubic.x * cubic.y;
}

float ring_fbm(float2 coord, float sz, float sd, int octaves) {
	float value = 0.0;
	float scale = 0.5;
	for (int i = 0; i < 20; i++) {
		if (i >= octaves)
			break;
		value += ring_noise(coord, sz, sd) * scale;
		coord *= 2.0;
		scale *= 0.5;
	}
	return value;
}

float ring_circle_noise(float2 uv, float sz, float sd) {
	float uv_y = floor(uv.y);
	uv.x += uv_y * 0.31;
	float2 f = frac(uv);
	float h = ring_rand(float2(floor(uv.x), uv_y), sz, sd);
	float m = (length(f - 0.25 - (h * 0.5)));
	float r = h * 0.25;
	return smoothstep(0.0, r, m * 0.75);
}

bool ring_dither(float2 uv_pixel, float2 uv_real, float px) {
	return glslmod(uv_pixel.x + uv_real.y, 2.0 / px) <= 1.0 / px;
}

float3 ring_pick_color(int idx) {
	if (idx == 0) return ring_c0;
	if (idx == 1) return ring_c1;
	if (idx == 2) return ring_c2;
	if (idx == 3) return ring_c3;
	return ring_c4;
}

float4 accretion_disk(float2 UV) {
	float2 uv = floor(UV * pixels_ring) / pixels_ring;
	bool dith = ring_dither(UV, uv, pixels_ring);

	uv = rotate(uv, rotation);

	float2 uv2 = uv;

	uv.x -= 0.5;
	uv.x *= 1.3;
	uv.x += 0.5;

	uv = rotate(uv, sin(time * time_speed * 2.0) * 0.01);

	float2 l_origin = float2(0.5, 0.5);
	float d_width = disk_width;

	if (uv.y < 0.5) {
		uv.y += smoothstep(distance(float2(0.5, 0.5), uv), 0.5, 0.2);
		d_width += smoothstep(distance(float2(0.5, 0.5), uv), 0.5, 0.3);
		l_origin.y -= smoothstep(distance(float2(0.5, 0.5), uv), 0.5, 0.2);
	}
	else if (uv.y > 0.53) {
		uv.y -= smoothstep(distance(float2(0.5, 0.5), uv), 0.4, 0.17);
		d_width += smoothstep(distance(float2(0.5, 0.5), uv), 0.5, 0.2);
		l_origin.y += smoothstep(distance(float2(0.5, 0.5), uv), 0.5, 0.2);
	}

	float light_d = distance(uv2 * float2(1.0, ring_perspective), l_origin * float2(1.0, ring_perspective)) * 0.3;

	float2 uv_center = uv - float2(0.0, 0.5);
	uv_center *= float2(1.0, ring_perspective);
	float center_d = distance(uv_center, float2(0.5, 0.0));

	float disk = smoothstep(0.1 - d_width * 2.0, 0.5 - d_width, center_d);
	disk *= smoothstep(center_d - d_width, center_d, 0.4);

	uv_center = rotate(uv_center + float2(0, 0.5), time * time_speed * 3.0);

	disk *= pow(ring_fbm(uv_center * disk_size, disk_size, seed, OCTAVES), 0.5);

	if (dith || should_dither < 0.5) {
		disk *= 1.2;
	}

	float n_posterized = (float)(n_colors - 1);
	float posterized = floor((disk + light_d) * n_posterized);
	posterized = min(posterized, n_posterized);
	int pi = (int)posterized;
	pi = clamp(pi, 0, n_colors - 1);

	float3 col = ring_pick_color(pi);

	float disk_a = step(0.15, disk);
	return float4(col, disk_a);
}

float4 MainPS(VertexShaderInput input) : COLOR
{
	float2 uv = input.TextureCoordinates;

	float4 hole = float4(0.0, 0.0, 0.0, 0.0);
	if (uv.x > (1.0 / 3.0) && uv.x < (2.0 / 3.0) && uv.y >(1.0 / 3.0) && uv.y < (2.0 / 3.0)) {
		float2 luv = (uv - (1.0 / 3.0)) * 3.0;
		hole = black_hole_core(luv, pixels);
	}

	float4 ring = accretion_disk(uv);

	float outA = ring.a + hole.a * (1.0 - ring.a);
	float3 outRgb = ring.rgb * ring.a + hole.rgb * hole.a * (1.0 - ring.a);
	return float4(outRgb, outA);
}

technique SpriteDrawing {
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
}
