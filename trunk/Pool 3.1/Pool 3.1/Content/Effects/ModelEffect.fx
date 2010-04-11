float4x4 World;
float4x4 ViewProj;
float4x4 LightViewProj;
float4 LightPosition;

float4 CameraPosition;

float Shineness = 96.0f;
float4 vSpecularColor = {1.0f, 1.0f, 1.0f, 1.0f};
float4 vAmbient = {0.1f, 0.1f, 0.1f, 1.0f};
float4 vDiffuseColor = {1.0f, 1.0f, 1.0f, 1.0f};

Texture TexColor;
sampler ColorSampler = sampler_state
{
    Texture = <TexColor>;

    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
};

struct VertexShaderInput
{
    float4 Position : POSITION;
    float3 Normal	: NORMAL;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position	: POSITION;
    float2 TexCoord : TEXCOORD0;
    float3 Normal	: TEXCOORD1;
    float4 WorldPosition : TEXCOORD2;
    
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

	//
    output.WorldPosition = mul(input.Position, World);
    output.Position = mul(output.WorldPosition, ViewProj);
    
    //
	output.TexCoord = input.TexCoord;
    
    //
	output.Normal = normalize(mul(input.Normal, (float3x3)World));

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float4 Color = tex2D(ColorSampler, input.TexCoord);
	
    float3 LightDir = normalize(LightPosition - input.WorldPosition);
    float3 ViewDir = normalize(CameraPosition - input.WorldPosition);    
    
	// Calculate normal diffuse light.
    float DiffuseLightingFactor = dot(LightDir, input.Normal);
    
    // R = 2 * (N.L) * N – L
    float3 Reflect = normalize(2 * DiffuseLightingFactor * input.Normal - LightDir);  
    float Specular = pow(saturate(dot(Reflect, ViewDir)), Shineness); // R.V^n

    // I = A + Dcolor * Dintensity * N.L + Scolor * Sintensity * (R.V)n
    
	return (Color * (vAmbient + vDiffuseColor * DiffuseLightingFactor) + vSpecularColor * Specular);
}

technique ModelTechnique
{
    pass Pass1
    {

        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
