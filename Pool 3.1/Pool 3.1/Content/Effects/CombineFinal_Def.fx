/////////////////////////////////////////
//
/////////////////////////////////////////

float4 AmbientColor;

texture colorMap;
texture lightMap;
texture shadowOcclusion;

sampler colorSampler = sampler_state
{
    Texture = (colorMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = ANISOTROPIC;
    MinFilter = ANISOTROPIC;
    Mipfilter = ANISOTROPIC;
};
sampler lightSampler = sampler_state
{
    Texture = (lightMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = ANISOTROPIC;
    MinFilter = ANISOTROPIC;
    Mipfilter = ANISOTROPIC;
};

sampler shadowSampler = sampler_state
{
	Texture = (shadowOcclusion);
    MinFilter = POINT; 
    MagFilter = POINT; 
    MipFilter = POINT;
};

struct VertexShaderInput
{
    float3 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

float2 halfPixel;
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = float4(input.Position,1);
    output.TexCoord = input.TexCoord - halfPixel;
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float shadowTerm = tex2D(shadowSampler,input.TexCoord).r;
    float3 diffuseColor = tex2D(colorSampler,input.TexCoord).rgb;
    float4 light = tex2D(lightSampler,input.TexCoord);
    float3 diffuseLight = light.rgb;
    float specularLight = light.a;
    
    //return float4(shadowTerm * (diffuseColor * (diffuseLight + AmbientColor.xyz) + specularLight), 1);
    return float4(shadowTerm * (diffuseColor * (diffuseLight) + specularLight) + diffuseColor * AmbientColor.xyz, 1);
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
