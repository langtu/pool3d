float4x4 World;
float4x4 ViewProj;
float4x4 LightViewProj;
float4 LightPosition;

float4 CameraPosition;

float Shineness = 96.0f;
float4 vSpecularColor = {1.0f, 1.0f, 1.0f, 1.0f};
float4 vAmbient = {0.1f, 0.1f, 0.1f, 1.0f};
float4 vDiffuseColor = {1.0f, 1.0f, 1.0f, 1.0f};

//sampler ColorSampler : register(s0);


Texture TexColor;
sampler ColorSampler = sampler_state
{
    Texture = <TexColor>;

    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
};

Texture TexBlurV;
sampler BlurVSampler = sampler_state
{
    Texture = <TexBlurV>;

    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

texture SceneTexture;
sampler2D sceneTex = sampler_state
{
	Texture = <SceneTexture>;
    AddressU = Clamp;
	AddressV = Clamp;
	MagFilter = Linear;
	MinFilter = Linear;
};

struct VS_SSSShadow_Input
{
    float4 Position : POSITION;
    float3 Normal	: NORMAL;
    float2 TexCoord : TEXCOORD0;
};

struct VS_SSSShadow_Output
{
    float4 Position			: POSITION;
    float2 TexCoord			: TEXCOORD0;
    float2 ProjCoord		: TEXCOORD1;
    float4 ScreenCoord		: TEXCOORD2;
    float4 WorldPosition	: TEXCOORD3;
    float3 Normal			: TEXCOORD4;
};

VS_SSSShadow_Output VS_ScreenSpaceShadow(VS_SSSShadow_Input input)
{
    VS_SSSShadow_Output output = (VS_SSSShadow_Output)0;
	
    output.WorldPosition = mul(input.Position, World);
    output.Position = mul(output.WorldPosition, ViewProj);

	output.TexCoord = input.TexCoord;
	
    float4 ShadowMapPos = mul(output.WorldPosition, LightViewProj);
    output.ProjCoord[0] = ShadowMapPos.x / ShadowMapPos.w / 2.0f + 0.5f;
    output.ProjCoord[1] = -ShadowMapPos.y / ShadowMapPos.w / 2.0f + 0.5f;
    
    /////output.ScreenCoord.x = (output.Position.x * 0.5f + output.Position.w * 0.5f);
    /////output.ScreenCoord.y = (output.Position.w * 0.5f - output.Position.y * 0.5f);
    
    output.ScreenCoord.x = 0.5f * (output.Position.x + output.Position.w + 0.25f);
    output.ScreenCoord.y = 0.5f * (output.Position.w - output.Position.y - 0.25f);
    output.ScreenCoord.z = output.Position.w;
    output.ScreenCoord.w = output.Position.w;
	
	output.Normal = normalize(mul(input.Normal, (float3x3)World));
	
    return output;
}

float4 PS_ScreenSpaceShadow(VS_SSSShadow_Output input) : COLOR0
{
	float4 Color = tex2D(ColorSampler, input.TexCoord);
	//Color.xyz = (Color.x + Color.y + Color.z) /3.0f;
	float fShadowTerm = tex2Dproj(BlurVSampler, input.ScreenCoord).x;
	
	float3 Normal = input.Normal;
    float3 LightDir = normalize(LightPosition - input.WorldPosition);
    float3 ViewDir = normalize(CameraPosition - input.WorldPosition);    
    
	// Calculate normal diffuse light.
    float DiffuseLightingFactor = dot(LightDir, Normal);
    
    // R = 2 * (N.L) * N – L
    float3 Reflect = normalize(2 * DiffuseLightingFactor * Normal - LightDir);  
    float Specular = pow(saturate(dot(Reflect, ViewDir)), Shineness); // R.V^n
    
    // I = A + Dcolor * Dintensity * N.L + Scolor * Sintensity * (R.V)n
    
	return (Color * (vAmbient + vDiffuseColor * DiffuseLightingFactor) + vSpecularColor * Specular) * fShadowTerm;
	
}

technique SSSTechnique
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VS_ScreenSpaceShadow();
        PixelShader = compile ps_3_0 PS_ScreenSpaceShadow();
    }
}
