
// Texture Sampler
sampler inputSampler : register(s0);

struct PixelShaderOutput
{
    float4 Color : COLOR0;
    float4 Depth : COLOR1;
    float4 Velocity : COLOR2;

};


PixelShaderOutput PixelShaderFunction(float2 texCoord : TEXCOORD0)
{
    PixelShaderOutput output;
    
	// 100 149 237
	output.Color = float4(100.0f / 255.0f, 149.0f / 255.0f, 237.0f / 255.0f, 1.0f);
	
	output.Depth = float4(1.0f, 1.0f, 1.0f, 1.0f);
	output.Velocity = float4(0.0f, 0.0f, 0.0f, 1.0f);
	
    return output;
}

struct SSAOPixelShaderOutput
{
    float4 Color : COLOR0;
    half4 Normal : COLOR1;
	half4 ViewS : COLOR2;

};

SSAOPixelShaderOutput SSAOPixelShaderFunction(float2 texCoord : TEXCOORD0)
{
    SSAOPixelShaderOutput output;
    
	// 100 149 237
	output.Color = float4(100.0f / 255.0f, 149.0f / 255.0f, 237.0f / 255.0f, 1.0f);
	
	output.Normal = float4(0.5f, 0.5f, 0.5f, 1.0f);
	output.ViewS = 0.0f;
	
    return output;
}


technique ClearGBufferTechnnique
{
    pass Pass1
    {
		PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}

technique SSAOClearGBufferTechnnique
{
    pass Pass1
    {
		PixelShader = compile ps_2_0 SSAOPixelShaderFunction();
    }
}
