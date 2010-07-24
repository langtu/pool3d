// Shader Variable
float2 halfPixel = 0;
// Texture Sampler
texture sceneMap;
sampler inputSampler = sampler_state
{
    Texture = <sceneMap>;

    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

texture SSAOMap;
sampler SSAOSampler = sampler_state
{
    Texture = <SSAOMap>;

    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
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

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = float4(input.Position,1);
    //align texture coordinates
    output.TexCoord = input.TexCoord - halfPixel;
    return output;
}

// Pixel Shader
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	// Get the pixel's color
	float4 color = tex2D(inputSampler, input.TexCoord) * tex2D(SSAOSampler, input.TexCoord);
	
	return color;
}

// Technique
technique Multiply
{
	pass Pass0
	{
		VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}