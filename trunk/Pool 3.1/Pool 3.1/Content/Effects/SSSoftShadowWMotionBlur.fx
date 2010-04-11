float4x4 World;
float4x4 ViewProj;
float4x4 LightViewProj;
float4 LightPos;

float4x4 modelView;
float4x4 prevModelView;
float4x4 modelViewProj;
float4x4 prevModelViewProj;
float    blurScale;

float4 vecEye;

float shineness = 96.0f;
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
    ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
	MAGFILTER = LINEAR;
	MINFILTER = LINEAR;
};

struct VS_SSSShadow_Input
{
    float4 Position : POSITION0;
    float3 Normal	: NORMAL;
    float2 TexCoord : TEXCOORD0;
};

struct VS_SSSShadow_Output
{
    float4 Position			: POSITION0;
    float2 TexCoord			: TEXCOORD0;
    float2 ProjCoord		: TEXCOORD1;
    float4 ScreenCoord		: TEXCOORD2;
    float4 WorldPosition	: TEXCOORD3;
    float3 Normal			: TEXCOORD4;
    float3 velocity			: TEXCOORD5;
};

VS_SSSShadow_Output VS_SSSShadow(VS_SSSShadow_Input input)
{
    VS_SSSShadow_Output output;
	
	float4 P = mul(input.Position, modelView);
	float4 Pprev = mul(input.Position, prevModelView);

	//float3 N = mul(input.normal, (float3x3) modelView);

	float3 motionVector = P.xyz - Pprev.xyz;

	P = mul(input.Position, modelViewProj);
	Pprev = mul(input.Position, prevModelViewProj);

	Pprev = lerp(P, Pprev, blurScale);


	//Out.pos = P;
	

	P.xyz = P.xyz / P.w;
	Pprev.xyz = Pprev.xyz / Pprev.w;
	
	output.velocity = (P.xyz - Pprev.xyz);
	//Out.pos3D = mul(In.Position, modelViewProj);
	
	////////////////////////////////////////////////////
    output.WorldPosition = mul(input.Position, World);
    output.Position = mul(output.WorldPosition, ViewProj);

	output.TexCoord = input.TexCoord;
	
    float4 ShadowMapPos = mul(output.WorldPosition, LightViewProj);
    output.ProjCoord[0] = ShadowMapPos.x / ShadowMapPos.w / 2.0f + 0.5f;
    output.ProjCoord[1] = -ShadowMapPos.y / ShadowMapPos.w / 2.0f + 0.5f;
    
    //output.ScreenCoord.x = (output.Position.x * 0.5f + output.Position.w * 0.5f);
    //output.ScreenCoord.y = (output.Position.w * 0.5f - output.Position.y * 0.5f);
    
    output.ScreenCoord.x = 0.5f * (output.Position.x + output.Position.w + 0.25f);
    output.ScreenCoord.y = 0.5f * (output.Position.w - output.Position.y - 0.25f);
    output.ScreenCoord.z = output.Position.w;
    output.ScreenCoord.w = output.Position.w;
	
	output.Normal = normalize(mul(input.Normal, (float3x3)World));
	
    return output;
}

float DotProduct(float4 LightPos, float3 Pos3D, float3 Normal)
{
    float3 LightDir = normalize(LightPos - Pos3D);
    return dot(LightDir, Normal);
}

float4 PS_SSShadow(VS_SSSShadow_Output input) : COLOR0
{
	float4 Color = tex2D(ColorSampler, input.TexCoord);
	
	float fShadowTerm = tex2Dproj(BlurVSampler, input.ScreenCoord);
	
	//float DiffuseLightingFactor = DotProduct(LightPos, input.WorldPosition, input.Normal);
	
	float3 Normal = input.Normal;
    float3 LightDir = normalize(LightPos - input.WorldPosition);
    float3 ViewDir = normalize(vecEye - input.WorldPosition);    
    
	// Calculate normal diffuse light.
    float DiffuseLightingFactor = dot(LightDir, Normal);
    
    // R = 2 * (N.L) * N – L
    float3 Reflect = normalize(2 * DiffuseLightingFactor * Normal - LightDir);  
    float Specular = pow(saturate(dot(Reflect, ViewDir)), shineness); // R.V^n

    // I = A + Dcolor * Dintensity * N.L + Scolor * Sintensity * (R.V)n
    
	return (Color * (vAmbient + vDiffuseColor * DiffuseLightingFactor) + vSpecularColor * Specular) * fShadowTerm;
	
	//return Color * DiffuseLightingFactor * fShadowTerm;
}

technique SSSTechnique
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VS_SSSShadow();
        PixelShader = compile ps_2_0 PS_SSShadow();
    }
}
