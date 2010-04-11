// Shader Variable
float Saturation;
float4 Base = float4(0.299f, 0.587f, 0.114f, 0.0f);

// Texture Sampler
sampler inputSampler : register(s0);

// Pixel Shader
float4 SaturateShader(float2 texCoord : TEXCOORD0) : COLOR
{
	// Get the pixel's color
	float4 color = tex2D(inputSampler, texCoord);
	
	// Return the saturated color
	//float4 cTempColor = dot(color, float4(0.3,0.59,0.11,0.0));
	//float doa = dot(cTempColor, Base);
	//float3 final = float3(doa, doa, doa);
	
	//return dot(cTempColor,Base);
	//return float4(final, 1.0f);
	
	
	return float4(lerp(dot(color.xyz, float3(0.299f, 0.587f, 0.114f)), color.xyz, Saturation),
		color.a);
}

// Technique
technique SetSaturation
{
	pass Pass0
	{
		pixelShader = compile ps_1_1 SaturateShader();
	}
}