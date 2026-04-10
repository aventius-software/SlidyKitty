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
    //float h = Terrain(input.WorldPos);
    
    return float4(0.8, 0.8, 0.8, 1.0);
}

technique DefaultTechnique
{
    pass
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
