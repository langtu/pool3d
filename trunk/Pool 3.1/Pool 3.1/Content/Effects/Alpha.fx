float4x4 World;
//float4x4 View;
float4x4 ViewProj;
//float4x4 PrevWorldViewProj;

//float4 CameraPosition;
float3 LightPosition;


Texture TexColor;
sampler ColorSampler = sampler_state
{
    Texture = <TexColor>;

    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
};

Texture AlphaColor;
sampler AlphaSampler = sampler_state
{
    Texture = <AlphaColor>;

    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
};

struct VS_ModelInput
{
    float4 Position : POSITION;
    //float3 Normal	: NORMAL;
    float2 TexCoord : TEXCOORD0;
};

struct VS_ModelOutput
{
    float4 Position	: POSITION;
    float2 TexCoord : TEXCOORD0;
    float4 WorldPosition : TEXCOORD1;
    
};


VS_ModelOutput VertexShaderFunction(VS_ModelInput input)
{
    VS_ModelOutput output;

    output.WorldPosition = mul(input.Position, World);
    output.Position = mul(output.WorldPosition, ViewProj);
    
    //
	output.TexCoord = input.TexCoord;
    	
    return output;
}

float4 NoMRTPixelShaderFunction(VS_ModelOutput input) : COLOR0
{
    float4 Color = tex2D(ColorSampler, input.TexCoord);
    float alphaColor = tex2D(AlphaSampler, input.TexCoord).r;
    
    //float intensity = (Color.x + Color.y + Color.z) / 3.0f;
    //Color.xyz = intensity;
    
    float3 att = LightPosition - input.WorldPosition.xyz;
    
    
	float alpha = saturate(1.0f - length(att) / 70.0f);
	//if (alpha < 0.0f) alpha = -alpha;
	//if (alpha == 0.0f) alpha = 0.1f;
	
	
	//Color.a *= alpha * (1-alpha)* (1-alpha) * 6.7;
	//Color *= Color.a;
	//Color.a *= alpha;
	//Color.a *= 0.5f;
	//Color.a = 1 - Color.a*0.5f;
	//Color.a = 1-alphaColor;
	Color.a *= 1-input.TexCoord.y;
    return (Color);
}

technique RenderHalf4
{
    pass P0
    {
		sampler[0] = <ColorSampler>;
		
		AlphaBlendEnable = True;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;
		
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 NoMRTPixelShaderFunction();
    }
}
