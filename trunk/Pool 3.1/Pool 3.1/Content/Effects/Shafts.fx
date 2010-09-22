float4x4 World;
float4x4 ViewProj;
float4x4 View;

float3 LightColor;
float3 CameraPosition;
texture TexColor;

sampler ModelTextureSampler = sampler_state
{
    Texture = <TexColor>;

    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Mirror;
    AddressV = Mirror;
};

texture AlphaMap;

sampler AlphaSampler = sampler_state
{
    Texture = <AlphaMap>;

    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Mirror;
    AddressV = Mirror;
};
struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float3 Normal : NORMAL0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float3 Normal : TEXCOORD1;
    float4 NormalVS : TEXCOORD2;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    output.Position = mul(worldPosition, ViewProj);
    output.TexCoord = input.TexCoord;
    output.Normal = mul(input.Normal, World);
	
	output.NormalVS = mul(output.Normal, View);
    
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    //float alpha = tex2D(AlphaSampler, input.TexCoord).r;
	float4 output;
	float3 viewNormal = normalize(input.NormalVS);
	float factor = dot(viewNormal, float3(0, 0, -1));
	
	output.rgba = tex2D(ModelTextureSampler, input.TexCoord);
	output *= factor;
    return output;
}

technique RenderHalf4
{
    pass Pass1
    {
        sampler[0] = <ModelTextureSampler>;

        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}

technique ModelTechnique
{
    pass Pass1
    {
        sampler[0] = <ModelTextureSampler>;

        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}