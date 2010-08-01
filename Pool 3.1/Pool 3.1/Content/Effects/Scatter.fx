Texture blackTex;

sampler2D blackTexture = sampler_state
{
	Texture = <blackTex>;
    ADDRESSU = WRAP;
	ADDRESSV = WRAP;
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	MIPFILTER = LINEAR;
};


Texture frameTex;

sampler2D frameSampler = sampler_state
{
	Texture = <frameTex>;
    ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

float4x4 View;
float4x4 WorldViewProjection;

half Density = 0.8f;
half Weight = 0.9f;
half Decay = 0.5f;
half Exposition = 0.5f;

half3 LightPosition;
half3 LightDirection = {1.0f,1.0f,1.0f};
half3 CameraPos;

int numSamples;

struct VertexShaderInput
{
    float3 Position : POSITION0;
    half2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    half2 TexCoord : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = float4(input.Position,1);
    //align texture coordinates
    output.TexCoord = input.TexCoord;
    return output;
}

half4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    half4 screenPos = mul(LightPosition, WorldViewProjection); 
    half2 ssPos = screenPos.xy / screenPos.w 
			* float2(0.5,-0.5) + 0.5;
			
	half2 oriTexCoord = input.TexCoord;
			
	half2 deltaTexCoord = (input.TexCoord - ssPos);
    
    deltaTexCoord *= 1.0f / numSamples * Density;
    
    half3 color = tex2D(blackTexture, input.TexCoord).r;
    if (color.r != 0.0) color = tex2D(frameSampler, input.TexCoord);
    
    half illuminationDecay = 1.0f;
    
	for (int i = 0; i < numSamples; i++)
	{
		oriTexCoord -= deltaTexCoord;
			
		half3 sample = tex2D(blackTexture, oriTexCoord);
		sample *= illuminationDecay * Weight;
		color += sample;
			
		illuminationDecay *= Decay;
	}
	
	half amount = dot(mul(LightDirection,View), half3(0.0f,0.0f,-1.0f));
	half4 sampleFrame = tex2D(frameSampler, input.TexCoord);
    
    //return half4(1,1, 1, 1);
    return saturate(amount-0.8) * half4(color * Exposition, 1) + sampleFrame;
}


technique Scatter
{
    pass P0
    {
		VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}