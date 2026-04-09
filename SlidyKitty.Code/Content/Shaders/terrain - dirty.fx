#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

// Input parameters
matrix ViewProjection;

float TileWidth;
float Frequency;
float Amplitude;
int   Octaves;
float Lacunarity;
float Gain;

// Hash on a circle — inherently tileable
float Hash(float2 p)
{
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
}

// Tileable value noise
float TileableNoise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);

    float a = Hash(i);
    float b = Hash(i + float2(1, 0));
    float c = Hash(i + float2(0, 1));
    float d = Hash(i + float2(1, 1));

    float2 u = f * f * (3.0 - 2.0 * f);

    return lerp(lerp(a, b, u.x),
                lerp(c, d, u.x),
                u.y);
}

float Terrain(float2 worldPos)
{
    // Convert to 0–1 tile space
    float2 uv = worldPos / TileWidth;

    // Wrap explicitly
    uv = frac(uv);

    // Map to circle to force periodicity
    float2 p = float2(
        cos(uv.x * 6.2831853),
        sin(uv.x * 6.2831853)
    ) + float2(
        cos(uv.y * 6.2831853),
        sin(uv.y * 6.2831853)
    );

    float value = 0.0;
    float freq = Frequency;
    float amp  = 1.0;

    for (int i = 0; i < Octaves; i++)
    {
        value += TileableNoise(p * freq) * amp;
        freq *= Lacunarity;
        amp  *= Gain;
    }

    return value * Amplitude;
}

struct VertexInput
{
    float4 Position : POSITION0;
};

struct VertexOutput
{
    float4 Position : SV_POSITION;
    float2 WorldPos : TEXCOORD0;
};

VertexOutput MainVS(VertexInput input)
{
    VertexOutput output;
    
    output.Position = mul(input.Position, ViewProjection);
    output.WorldPos = input.Position.xy;

    return output;
}

float4 MainPS(VertexOutput input) : SV_TARGET
{
    float h = Terrain(input.WorldPos);

    // Terrain palette (simple example)
    float3 color =
        (h < 0.4) ? float3(0.1, 0.2, 0.6) : // water
        (h < 0.5) ? float3(0.2, 0.5, 0.2) : // grass
        (h < 0.7) ? float3(0.4, 0.35, 0.2) : // dirt
                   float3(0.8, 0.8, 0.8);   // snow

    return float4(color, 1.0);
}

technique DefaultTechnique
{
    pass
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
