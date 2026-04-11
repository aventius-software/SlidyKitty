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
float Lacunarity;
float Gain;

struct VertexInput
{
    float4 Position : POSITION0;
};

struct VertexOutput
{
    float4 Position : SV_POSITION;
    float2 WorldPos : TEXCOORD0;
};

float Hash(float2 p)
{
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
}

float TileableNoise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);

    float a = Hash(i);
    float b = Hash(i + float2(1.0, 0.0));
    float c = Hash(i + float2(0.0, 1.0));
    float d = Hash(i + float2(1.0, 1.0));

    float2 u = f * f * (3.0 - 2.0 * f);

    return lerp(
        lerp(a, b, u.x),
        lerp(c, d, u.x),
        u.y
    );
}

#define OCTAVES 5

float Terrain(float2 worldPos)
{
    // Normalize into tile space
    float2 uv = frac(worldPos / TileWidth);

    // Periodic mapping (forces exact tiling)
    float2 p =
        float2(cos(uv.x * 6.2831853), sin(uv.x * 6.2831853)) +
        float2(cos(uv.y * 6.2831853), sin(uv.y * 6.2831853));

    float value = 0.0;

    float freq = Frequency;
    float amp  = 1.0;

    value += TileableNoise(p * freq) * amp;
    freq *= Lacunarity; amp *= Gain;

    value += TileableNoise(p * freq) * amp;
    freq *= Lacunarity; amp *= Gain;

    value += TileableNoise(p * freq) * amp;
    freq *= Lacunarity; amp *= Gain;

    value += TileableNoise(p * freq) * amp;
    freq *= Lacunarity; amp *= Gain;

    value += TileableNoise(p * freq) * amp;

    return value * Amplitude;
}

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

    float3 color =
        (h < 0.4) ? float3(0.1, 0.2, 0.6) :
        (h < 0.5) ? float3(0.2, 0.5, 0.2) :
        (h < 0.7) ? float3(0.4, 0.35, 0.2) :
                    float3(0.8, 0.8, 0.8);

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
