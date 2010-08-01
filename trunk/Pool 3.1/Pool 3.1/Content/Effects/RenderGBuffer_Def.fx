float4x4 World;
float4x4 ViewProj;
float specularIntensity = 0.8f;
float specularPower = 0.5f;
texture TexColor;

// PARALLAX
float2 parallaxscaleBias;
float3 CameraPosition;

// SCATTERING
bool isScatterObject;

sampler diffuseSampler = sampler_state
{
    Texture = (TexColor);
    MAGFILTER = ANISOTROPIC;
    MINFILTER = ANISOTROPIC;
    MIPFILTER = ANISOTROPIC;
    //AddressU = Wrap;
    //AddressV = Wrap;
};

texture SpecularMap;
sampler specularSampler = sampler_state
{
    Texture = (SpecularMap);
    MagFilter = ANISOTROPIC;
    MinFilter = ANISOTROPIC;
    Mipfilter = ANISOTROPIC;
    //AddressU = Wrap;
    //AddressV = Wrap;
};

texture NormalMap;
sampler normalSampler = sampler_state
{
    Texture = (NormalMap);
    MagFilter = ANISOTROPIC;
    MinFilter = ANISOTROPIC;
    Mipfilter = ANISOTROPIC;
    //AddressU = Wrap;
    //AddressV = Wrap;
};

texture HeightMap;
sampler2D heightSampler = sampler_state
{
	Texture = <HeightMap>;
	
	MAGFILTER = ANISOTROPIC;
	MINFILTER = ANISOTROPIC;
	MIPFILTER = ANISOTROPIC;
};
////////////////////////////////////////

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TexCoord : TEXCOORD0;
    float3 Binormal : BINORMAL0;
    float3 Tangent : TANGENT0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float2 Depth : TEXCOORD1;
    float3 PositionWS : TEXCOORD2;
    float3x3 TBN : TEXCOORD3;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(float4(input.Position.xyz, 1.0f), World);
    output.Position = mul(worldPosition, ViewProj);
    output.PositionWS = worldPosition.xyz;
    
    output.TexCoord = input.TexCoord;
    output.Depth.x = output.Position.z;
    output.Depth.y = output.Position.w;

    // calculate tangent space to world space matrix using the world space tangent,
    // binormal, and normal as basis vectors
    output.TBN[0] = mul(input.Tangent, World);
    output.TBN[1] = mul(input.Binormal, World);
    output.TBN[2] = mul(input.Normal, World);

    return output;
}

struct HalfPixelShaderOutput
{
    half4 Color : COLOR0;
    half4 Normal : COLOR1;
    half4 Depth : COLOR2;
    half4 Scatter : COLOR3;
};

HalfPixelShaderOutput HalfPixelShaderFunction(VertexShaderOutput input, uniform bool bparallax)
{
    HalfPixelShaderOutput output;
    
    float2 texCoord;
	float3 ViewDir = normalize(CameraPosition - input.PositionWS);
	if (bparallax == true)
    {
		float3 ViewDirTBN = mul(ViewDir, input.TBN);
		
        float height = tex2D(heightSampler, input.TexCoord).r;
        
        height = height * parallaxscaleBias.x + parallaxscaleBias.y;
        texCoord = input.TexCoord + (height * ViewDirTBN.xy);
    }
    else
        texCoord = input.TexCoord;
        
    output.Color = tex2D(diffuseSampler, texCoord);
    
    float4 specularAttributes = tex2D(specularSampler, texCoord);
    //specular Intensity
    output.Color.a = specularAttributes.r;
    
    // read the normal from the normal map
    float3 normalFromMap = tex2D(normalSampler, texCoord);
    //tranform to [-1,1]
    normalFromMap = 2.0f * normalFromMap - 1.0f;
    //transform into world space
    normalFromMap = mul(normalFromMap, input.TBN);
    //normalize the result
    normalFromMap = normalize(normalFromMap);
    //output the normal, in [0,1] space
    output.Normal.rgb = 0.5f * (normalFromMap + 1.0f);

    //specular Power
    output.Normal.a = specularAttributes.a;

    output.Depth = input.Depth.x / input.Depth.y;
    
    if (isScatterObject) output.Scatter = 0;
    else output.Scatter = 1;
    return output;
}

struct ColorPixelShaderOutput
{
    float4 Color : COLOR0;
    float4 Normal : COLOR1;
    float4 Depth : COLOR2;
};

ColorPixelShaderOutput ColorPixelShaderFunction(VertexShaderOutput input)
{
    ColorPixelShaderOutput output;
    output.Color = tex2D(diffuseSampler, input.TexCoord);
    
    float4 specularAttributes = tex2D(specularSampler, input.TexCoord);
    //specular Intensity
    output.Color.a = specularAttributes.r;
    
    // read the normal from the normal map
    float3 normalFromMap = tex2D(normalSampler, input.TexCoord);
    //tranform to [-1,1]
    normalFromMap = 2.0f * normalFromMap - 1.0f;
    //transform into world space
    normalFromMap = mul(normalFromMap, input.TBN);
    //normalize the result
    normalFromMap = normalize(normalFromMap);
    //output the normal, in [0,1] space
    output.Normal.rgb = 0.5f * (normalFromMap + 1.0f);

    //specular Power
    output.Normal.a = specularAttributes.a;

    output.Depth = input.Depth.x / input.Depth.y;
    return output;
}

technique RenderHalf4
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 HalfPixelShaderFunction(false);
    }
}

technique ParallaxMappingRenderHalf4
{
    pass Pass1
    {
    
		sampler[0] = <diffuseSampler>;
		sampler[1] = <specularSampler>;
		sampler[2] = <normalSampler>;
		sampler[3] = <heightSampler>;
		
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 HalfPixelShaderFunction(true);
    }
}

technique RenderColor
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 ColorPixelShaderFunction();
    }
}
