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

float3 RGB;
float Alpha;

// Define the input for the vertex shader
struct VertexInput
{
    float4 Position : POSITION0;
};

// Define the output for the vertex shader - this will be passed 
// to the pixel shader as input parameter
struct VertexOutput
{
    float4 Position : SV_POSITION;
    float2 WorldPos : TEXCOORD0;
};

// This is our vertex shader
VertexOutput MainVS(VertexInput input)
{
    VertexOutput output;
    
    output.Position = mul(input.Position, ViewProjection);
    output.WorldPos = input.Position.xy;

    return output;
}

// Now we define our pixel shader
float4 MainPS(VertexOutput input) : SV_TARGET
{    
    return float4(RGB, Alpha);
}

technique DefaultTechnique
{
    pass
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
