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
float Scale; // controls terrain frequency

struct VertexInput
{
    float4 Position : POSITION0;   // world position
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

float4 MainPS(VertexOutput input) : COLOR
{
    float2 p = input.WorldPos * Scale;

    // Simple terrain-like pattern
    float height =
        sin(p.x) * 0.5 +
        sin(p.y) * 0.5;

    float3 color = lerp(
        float3(0.15, 0.4, 0.15),  // grass
        float3(0.4, 0.35, 0.2),   // dirt
        saturate(height)
    );

    return float4(color, 1);
}


technique DefaultTechnique
{
    pass
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
