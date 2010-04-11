
// Texture Sampler
sampler inputSampler : register(s0);

Texture ShadowPCM;
sampler PCMSampler = sampler_state
{
    Texture = <ShadowPCM>;

    MinFilter = ANISOTROPIC;
    MagFilter = ANISOTROPIC;
    MipFilter = ANISOTROPIC;
};

// Pixel Shader
float4 ShadowFinalShader(float2 texCoord : TEXCOORD0) : COLOR
{
	float4 t = tex2D(inputSampler, texCoord);
	t *= tex2D(PCMSampler, texCoord).r;
	return t;
}

// Technique
technique SetSaturation
{
	pass Pass0
	{
		pixelShader = compile ps_2_0 ShadowFinalShader();
	}
}