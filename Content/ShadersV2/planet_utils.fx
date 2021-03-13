#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0
	#define PS_SHADERMODEL ps_4_0
#endif

// Because the fmod available in HLSL is actually
// x - y * trunc(x / y), below is the mod implementation
// from GLSL.
float glslmod(float x, float y) {
	return x - y * floor(x / y);
}

// Same as above, for float2
float2 glslmod(float2 x, float2 y) {
	return x - y * floor(x / y);
}

float rand(float size, float2 sizeModifier, float seed, float2 coord) {
	// land has to be tiled
	// tiling only works for integer values, thus the rounding
	// it would probably be better to only allow integer sizes
	// multiply by vec2(2,1) to simulate planet having another side
	coord = glslmod(coord, sizeModifier * round(size));

	// keep the number below small to avoid weird capping on frac (original: 43758.5453)
	return frac(sin(dot(coord.xy, float2(12.9898, 78.233))) * .5453 * seed);
}

// Used for asteroids
//float simple_rand(float seed, float2 coord) {
//	return frac(sin(dot(coord.xy, float2(12.9898, 78.233))) * .5453 * seed);
//}

float noise(float size, float2 sizeModifier, float seed, float2 coord) {
	float2 i = floor(coord);
	float2 f = frac(coord);

	float a = rand(size, sizeModifier, seed, i);
	float b = rand(size, sizeModifier, seed, i + float2(1.0, 0.0));
	float c = rand(size, sizeModifier, seed, i + float2(0.0, 1.0));
	float d = rand(size, sizeModifier, seed, i + float2(1.0, 1.0));

	float2 cubic = f * f * (3.0 - 2.0 * f);

	return lerp(a, b, cubic.x) + (c - a) * cubic.y * (1.0 - cubic.x) + (d - b) * cubic.x * cubic.y;
}

// Converted fbm function from GLSL
float fbm(float size, float2 sizeModifier, float seed, int octaves, float2 coord) {
	float value = 0.0;
	float scale = 0.5;

	//[unroll(50)]
	for (int i = 0; i < octaves; i++) {
		value = value + noise(size, sizeModifier, seed, coord) * scale;
		coord = coord * 2.0;
		scale = scale * 0.5;
	}
	return value;
}

// by Leukbaars from https://www.shadertoy.com/view/4tK3zR
float circleNoise(float size, float2 sizeModifier, float seed, float2 uv) {
	float uv_y = floor(uv.y);
	uv.x += uv_y * .31;
	float2 f = frac(uv);
	float h = rand(size, sizeModifier, seed, float2(floor(uv.x), floor(uv_y)));
	float m = (length(f - 0.25 - (h * 0.5)));
	float r = h * 0.25;
	return smoothstep(0.0, r, m * 0.75);
}

float circleNoiseCrater(float size, float2 sizeModifier, float seed, float2 uv) {
	float uv_y = floor(uv.y);
	uv.x += uv_y * .31;
	float2 f = frac(uv);
	float h = rand(size, sizeModifier, seed, float2(floor(uv.x), floor(uv_y)));
	float m = (length(f - 0.25 - (h * 0.5)));
	float r = h * 0.25;
	return smoothstep(r - 0.10 * r, r, m);
}

float crater(float size, float2 sizeModifier, float seed, float time, float time_speed, float2 uv) {
	float c = 1.0;
	for (int i = 0; i < 2; i++) {
		c *= circleNoiseCrater(size, sizeModifier, seed, (uv * size) + (float(i + 1) + 10.0) + float2(time * time_speed, 0.0));
	}
	return 1.0 - c;
}

float cloud_alpha(float size, float2 sizeModifier, float seed, float time, float time_speed, int octaves, float2 uv) {
	float c_noise = 0.0;

	// more iterations for more turbulence
	for (int i = 0; i < 9; i++) {
		c_noise += circleNoise(size, sizeModifier, seed, (uv * size * 0.3) + (float(i + 1) + 10.0) + (float2(time * time_speed, 0.0)));
	}
	float fbmVal = fbm(size, sizeModifier, seed, octaves, uv * size + c_noise + float2(time * time_speed, 0.0));

	return fbmVal;//step(a_cutoff, fbm);
}

bool dither(float pixels, float dither_size, float2 uv_pixel, float2 uv_real) {
	return glslmod(uv_pixel.x + uv_real.y, 2.0 / pixels) * dither_size <= 1.0 / pixels;
}

float turbulence(float size, float2 sizeModifier, float seed, float time, float time_speed, float2 uv) {
	float c_noise = 0.0;

	// more iterations for more turbulence
	for (int i = 0; i < 10; i++) {
		c_noise += circleNoise(size, sizeModifier, seed, (uv * size * 0.3) + (float(i + 1) + 10.0) + (float2(time * time_speed, 0.0)));
	}

	return c_noise;
}

// Converted spherify function from GLSL
float2 spherify(float2 uv) {
	float2 centered = uv * 2.0 - 1.0;
	//float z = sqrt(1.0 - dot(centered.xy, centered.xy));
	float z = pow(1.0 - dot(centered.xy, centered.xy), 0.5);
	float2 sphere = centered / (z + 1.0);
	return sphere * 0.5 + 0.5;
};

// Converted
float2 rotate(float2 coord, float angle) {
	coord = coord - 0.5;
	float2x2 modifier = float2x2(float2(cos(angle), -sin(angle)), float2(sin(angle), cos(angle)));
	coord = mul(coord, modifier);
	return coord + 0.5;
}

// Star utils
float2 hash2(float size, float2 sizeModifier, float seed, float time, float2 p) {
	float t = (time + 10.0) * .3;
	//p = glslmod(p, float2(1.0,1.0)*round(size));
	return float2(noise(size, sizeModifier, seed, p), noise(size, sizeModifier, seed, p * float2(.3135 + sin(t), .5813 - cos(t))));
}

// Tileable cell noise by Dave_Hoskins from shadertoy: https://www.shadertoy.com/view/4djGRh
float cells(float size, float2 sizeModifier, float seed, float tiles, float time, float2 p, float numCells) {
	p *= numCells;
	float d = 1.0e10;
	for (int xo = -1; xo <= 1; xo++)
	{
		for (int yo = -1; yo <= 1; yo++)
		{
			float2 tp = floor(p) + float2(float(xo), float(yo));
			tp = p - tp - hash2(size, sizeModifier, seed, time, glslmod(tp, numCells / tiles));
			d = min(d, dot(tp, tp));
		}
	}
	return sqrt(d);
}

float2 scale(float2 coord, float scaleValue) {
	return coord * scaleValue - (1.0 * (scaleValue - 1)) / 2.0;
}

float4 MainPS() : COLOR
{
	return float4(0.0, 0.0, 0.0, 0.0);
}

technique BasicColorDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};