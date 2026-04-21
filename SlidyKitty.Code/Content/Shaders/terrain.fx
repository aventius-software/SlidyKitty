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
    // Keep existing setup but create a grid coordinate variable for logic
    float2 tileCoord = input.WorldPos / TileWidth;
    
    // Wrap explicitly
    tileCoord = tileCoord * 40;
    
    // Calculate the integer grid index
    float x = floor(tileCoord.x);
    float y = floor(tileCoord.y);
    
    // Checkerboard Logic: If sum is even, one color. Else another.
    // We can use mod or bitwise logic.
    // Note: The original 'uv' wrapper might be intended for texture mapping later, 
    // but for the checkerboard we need the grid index.
    float check = frac(x + y); // 0 or 1 if we treat x+y as int
    
    float3 color;
    
    if (floor(x + y) % 2 == 0)
    {
        float red = (1.0 / 255.0) * 160;
        float green = (1.0 / 255.0) * 90;
        float blue = (1.0 / 255.0) * 80;
        color = float3(red, green, blue);
    }
    else
    {
        float red = (1.0 / 255.0) * 120;
        float green = (1.0 / 255.0) * 70;
        float blue = (1.0 / 255.0) * 70;
        color = float3(red, green, blue);
    }

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
