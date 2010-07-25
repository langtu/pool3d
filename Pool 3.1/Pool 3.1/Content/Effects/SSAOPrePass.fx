float4x4 World;
float4x4 View;
float4x4 ViewProj;
float4 CameraPosition;

float2 parallaxscaleBias;

texture NormalMap;
sampler2D normalSampler = sampler_state
{
	Texture = <NormalMap>;
	
	MAGFILTER = Linear;
	MINFILTER = Linear;
	MIPFILTER = Linear;
};

texture HeightMap;
sampler2D heightSampler = sampler_state
{
	Texture = <HeightMap>;
	
	MAGFILTER = Linear;
	MINFILTER = Linear;
	MIPFILTER = Linear;
};

struct VS_Input
{
    float4 Position : POSITION0;
    float3 Normal	: NORMAL0;
    float2 TexCoord : TEXCOORD0;
    float3 Binormal : BINORMAL0;
    float3 Tangent  : TANGENT0;
};

struct VS_SSAO_Output
{
    float4 Position			: POSITION;
    float2 TexCoord			: TEXCOORD0;
    float4 WorldPosition	: TEXCOORD1;
    float4 PositionViewS	: TEXCOORD2;
    float3x3 TBN				: TEXCOORD3;
    
};

struct PS_SSAO_Output
{
	half4 Normal : COLOR0;
	half4 ViewS : COLOR1;
};

VS_SSAO_Output VS_SSAOPrePass(VS_Input input)
{
    VS_SSAO_Output output;
	
	output.WorldPosition = mul(input.Position, World);
    output.Position = mul(output.WorldPosition, ViewProj);
	//
	output.PositionViewS = mul(output.WorldPosition, View);
	
	//
	output.TexCoord = input.TexCoord;
		
	// TBN SPACE
	output.TBN[0] = mul(input.Tangent, World);
    output.TBN[1] = mul(input.Binormal, World);
    output.TBN[2] = mul(input.Normal, World);
    return output;
}


PS_SSAO_Output PS_SSAOPrePass(VS_SSAO_Output input, uniform bool bnormalmapping,
	uniform bool bparallax)
{
	PS_SSAO_Output output;
	///////////////////////////////////////////////////////////////////////
	float2 texCoord;
	float3 ViewDir = normalize(CameraPosition.xyz - input.WorldPosition.xyz);
	if (bparallax == true)
    {
		float3 ViewDirTBN = mul(ViewDir, input.TBN);
		
        float height = tex2D(heightSampler, input.TexCoord).r;
        
        height = height * parallaxscaleBias.x + parallaxscaleBias.y;
        texCoord = input.TexCoord + (height * ViewDirTBN.xy);
    }
    else
        texCoord = input.TexCoord;
	
	float3 Normal;
	
	if (bnormalmapping)
	{
		Normal = 2.0f * tex2D(normalSampler, texCoord) - 1.0f;
		Normal = normalize(mul(Normal, input.TBN));
	} else
		Normal = input.TBN[2];
		
	
    output.Normal = float4((Normal + 1.0f) * 0.5f , 1.0f);
    output.ViewS = input.PositionViewS;
	
	return output;
}

technique SSAO
{
    pass P0
    {
		
        VertexShader = compile vs_3_0 VS_SSAOPrePass();
        PixelShader = compile ps_3_0 PS_SSAOPrePass(false, false);
    }
}
