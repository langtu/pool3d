struct VertexShaderInput
{
    float3 Position : POSITION0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = float4(input.Position,1);
    return output;
}
struct PS_Output
{
    half4 Color : COLOR0;
    half4 Normal : COLOR1;
    half4 Depth : COLOR2;
};

struct PSLS_Output
{
    half4 Color : COLOR0;
    half4 Normal : COLOR1;
    half4 Depth : COLOR2;
    half4 Scatter : COLOR3;
};

PS_Output PS(VertexShaderOutput input)
{
    PS_Output output;
    //black color
    output.Color = 0.0f;
    output.Color.a = 0.0f;
    //when transforming 0.5f into [-1,1], we will get 0.0f
    output.Normal.rgb = 0.5f;
    //no specular power
    output.Normal.a = 0.0f;
    //max depth
    output.Depth = 1.0f;
    
    return output;
}

PSLS_Output PS_LS(VertexShaderOutput input)
{
    PSLS_Output output;
    //black color
    output.Color = 0.0f;
    output.Color.a = 0.0f;
    //when transforming 0.5f into [-1,1], we will get 0.0f
    output.Normal.rgb = 0.5f;
    //no specular power
    output.Normal.a = 0.0f;
    //max depth
    output.Depth = 1.0f;
    
    output.Scatter = 1;
    return output;
}

technique ClearGBuffer
{
    pass P0
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PS();
    }
}


technique ClearGBufferLightShafts
{
    pass P0
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PS_LS();
    }
}