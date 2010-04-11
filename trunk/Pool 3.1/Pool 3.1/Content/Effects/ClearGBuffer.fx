
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
    PixelShaderOutput output = (PixelShaderOutput)0;
    
	// 100 149 237
	output.Color = float4(100.0f / 255.0f, 149.0f / 255.0f, 237.0f / 255.0f, 1.0f);
	
	output.Depth = float4(1.0f, 1.0f, 1.0f, 1.0f);
	output.Velocity = float4(0.0f, 0.0f, 0.0f, 1.0f);
	
    return output;
}

technique Technique1
{
    pass Pass1
    {
		//ZEnable = false;
		//ZWriteEnable = false;
		PixelShader = compile ps_2_0 PixelShaderFunction();
		//ZEnable = true;
		//ZWriteEnable = true;
    }
}
