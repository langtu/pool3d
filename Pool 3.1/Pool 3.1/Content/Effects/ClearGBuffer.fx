
// Texture Sampler
sampler inputSampler : register(s0);

struct ColorDepthVelocity_PS_Output
{
    float4 Color : COLOR0;
    float4 Depth : COLOR1;
    float4 Velocity : COLOR2;

};


ColorDepthVelocity_PS_Output ColorDepthVelocity_PS(float2 texCoord : TEXCOORD0)
{
    ColorDepthVelocity_PS_Output output;
    
	// 100 149 237
	output.Color = float4(100.0f / 255.0f, 149.0f / 255.0f, 237.0f / 255.0f, 1.0f);
	
	output.Depth = float4(1.0f, 1.0f, 1.0f, 1.0f);
	output.Velocity = float4(0.0f, 0.0f, 0.0f, 1.0f);
	
    return output;
}

struct SSA_PS_Output
{
    half4 Normal : COLOR0;
	half4 ViewS : COLOR1;

};

SSA_PS_Output SSAO_PS(float2 texCoord : TEXCOORD0)
{
    SSA_PS_Output output;
    
	output.Normal = float4(0.5f, 0.5f, 0.5f, 1.0f);
	output.ViewS = 0.0f;
	
    return output;
}


technique ClearGBufferTechnnique
{
    pass Pass1
    {
		PixelShader = compile ps_2_0 ColorDepthVelocity_PS();
    }
}

technique SSAOClearGBufferTechnnique
{
    pass Pass1
    {
		PixelShader = compile ps_2_0 SSAO_PS();
    }
}
