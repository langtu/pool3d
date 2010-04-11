float4x4 World;
float4x4 View;
float4x4 ViewProj;
float4x4 LightViewProj;
float4x4 PrevWorldViewProj;
float4 LightPosition;

float4 CameraPosition;

float Shineness = 96.0f;
float MaxDepth;
float4 vSpecularColor = {1.0f, 1.0f, 1.0f, 1.0f};
float4 vAmbient = {0.1f, 0.1f, 0.1f, 1.0f};
float4 vDiffuseColor = {1.0f, 1.0f, 1.0f, 1.0f};


sampler ColorSampler : register(s0);


/*Texture TexColor;
sampler ColorSampler = sampler_state
{
    Texture = <TexColor>;

    MinFilter = ANISOTROPIC;
    MagFilter = ANISOTROPIC;
    MipFilter = ANISOTROPIC;
};*/

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

sampler normalSampler : register(s3);
//texture NormalMap;
/*sampler2D normalSampler = sampler_state
{
	Texture = <NormalMap>;
	//AddressU = WRAP;
	//AddressV = WRAP;
	MAGFILTER = Linear;
	MINFILTER = Linear;
	MIPFILTER = Linear;
};*/

struct VS_ScreenSpaceShadow_Input
{
    float4 Position : POSITION;
    float3 Normal	: NORMAL;
    float2 TexCoord : TEXCOORD0;
    float3 Binormal : BINORMAL0;
    float3 Tangent  : TANGENT0;
};

struct VS_ScreenSpaceShadow_Output
{
    float4 Position			: POSITION;
    float2 TexCoord			: TEXCOORD0;
    float2 ProjCoord		: TEXCOORD1;
    float4 ScreenCoord		: TEXCOORD2;
    float4 WorldPosition	: TEXCOORD3;
    
    float4 PrevPositionCS	: TEXCOORD4;
    float4 CurrPositionCS	: TEXCOORD5;
    float4 PositionViewS	: TEXCOORD6;
    float3x3 TBN				: TEXCOORD7;
    
    
};

struct PS_ScreenSpaceShadow_Output
{
	float4 Color : COLOR0;
	float4 DepthColor : COLOR1;
	float4 Velocity : COLOR2;
};

VS_ScreenSpaceShadow_Output VS_ScreenSpaceShadow(VS_ScreenSpaceShadow_Input input)
{
    VS_ScreenSpaceShadow_Output output = (VS_ScreenSpaceShadow_Output)0;
	
	//
    output.WorldPosition = mul(input.Position, World);
    output.Position = mul(output.WorldPosition, ViewProj);

	//
	output.PositionViewS = mul(mul(input.Position, World), View);
	output.CurrPositionCS = output.Position;
	output.PrevPositionCS = mul(input.Position, PrevWorldViewProj);
	
	//
	output.TexCoord = input.TexCoord;
	
    float4 ShadowMapPos = mul(output.WorldPosition, LightViewProj);
    output.ProjCoord[0] = ShadowMapPos.x / ShadowMapPos.w / 2.0f + 0.5f;
    output.ProjCoord[1] = -ShadowMapPos.y / ShadowMapPos.w / 2.0f + 0.5f;
    
    /////output.ScreenCoord.x = (output.Position.x * 0.5f + output.Position.w * 0.5f);
    /////output.ScreenCoord.y = (output.Position.w * 0.5f - output.Position.y * 0.5f);
    
    //
    output.ScreenCoord.x = 0.5f * (output.Position.x + output.Position.w + 0.25f);
    output.ScreenCoord.y = 0.5f * (output.Position.w - output.Position.y - 0.25f);
    output.ScreenCoord.z = output.Position.w;
    output.ScreenCoord.w = output.Position.w;
	
	// Normal
	//output.N = normalize(mul(input.Normal, (float3x3)World));
	
	// TBN SPACE
	output.TBN[0] = mul(input.Tangent, World);
    output.TBN[1] = mul(input.Binormal, World);
    output.TBN[2] = mul(input.Normal, World);
    
    return output;
}

PS_ScreenSpaceShadow_Output PS_ScreenSpaceShadow(VS_ScreenSpaceShadow_Output input)
{
	PS_ScreenSpaceShadow_Output output = (PS_ScreenSpaceShadow_Output)0;
	///////////////////////////////////////////////////////////////////////
	
	float4 Color = tex2D(ColorSampler, input.TexCoord);
	
	float fShadowTerm = tex2Dproj(BlurVSampler, input.ScreenCoord).x;
	
	//
    float3 LightDir = normalize(LightPosition - input.WorldPosition);
    float3 ViewDir = normalize(CameraPosition - input.WorldPosition);    
    
	// Calculate normal diffuse light.
    float DiffuseLightingFactor = dot(LightDir, input.TBN[2]);
    
    // R = 2 * (N.L) * N – L
    float3 Reflect = normalize(2 * DiffuseLightingFactor * input.TBN[2] - LightDir);  
    float Specular = pow(saturate(dot(Reflect, ViewDir)), Shineness); // R.V^n
    
    // I = A + Dcolor * Dintensity * N.L + Scolor * Sintensity * (R.V)n
    
    //
    output.Color  = (Color * (vAmbient + vDiffuseColor * DiffuseLightingFactor) + vSpecularColor * Specular) * fShadowTerm;
    
    //
    output.DepthColor = float4(-input.PositionViewS.z / MaxDepth, 1.0f, 1.0f, 1.0f);
    
    // Calculate the instantaneous pixel velocity. Since clip-space coordinates are of the range [-1, 1] 
	// with Y increasing from the bottom to the top of screen, we'll rescale x and y and flip y so that
	// the velocity corresponds to texture coordinates (which are of the range [0,1], and y increases from top to bottom)
	float2 vVelocity = (input.CurrPositionCS.xy / input.CurrPositionCS.w) - (input.PrevPositionCS.xy / input.PrevPositionCS.w);
	vVelocity *= 0.5f;
	vVelocity.y *= -1;
	output.Velocity = float4(vVelocity, 1.0f, 1.0f);
	
    /////////////////////////////////////////////////////////////////////////
    //output.Color = float4(0.0, 0.0, 0.0, 1.0);
    //output.DepthColor = float4(input.Depth, input.Depth, input.Depth, 1.0f);
    //output.Color = output.DepthColor ;
	return output;
	
}

PS_ScreenSpaceShadow_Output PS_ScreenSpaceShadowWithNormalMapping(VS_ScreenSpaceShadow_Output input)
{
	PS_ScreenSpaceShadow_Output output = (PS_ScreenSpaceShadow_Output)0;
	///////////////////////////////////////////////////////////////////////
	
	float4 Color = tex2D(ColorSampler, input.TexCoord);
	
	float fShadowTerm = tex2Dproj(BlurVSampler, input.ScreenCoord).x;
	
	
	//
	float3 Normal = tex2D(normalSampler, input.TexCoord);
	
    Normal = 2.0f * Normal - 1.0f;
    Normal = normalize(mul(Normal, input.TBN));
	
	//
    float3 LightDir = normalize(LightPosition - input.WorldPosition);
    float3 ViewDir = normalize(CameraPosition - input.WorldPosition);    
    
	// Calculate normal diffuse light.
    float DiffuseLightingFactor = saturate(dot(LightDir, Normal));
    
    // R = 2 * (N.L) * N – L
    float3 Reflect = normalize(2 * DiffuseLightingFactor * Normal - LightDir);  
    float Specular = pow(saturate(dot(Reflect, ViewDir)), Shineness); // R.V^n
    
    // I = A + Dcolor * Dintensity * N.L + Scolor * Sintensity * (R.V)n
    
    //
    output.Color  = (Color * (vAmbient + vDiffuseColor * DiffuseLightingFactor) + vSpecularColor * Specular) * fShadowTerm;
    
    //
    output.DepthColor = float4(-input.PositionViewS.z / MaxDepth, 1.0f, 1.0f, 1.0f);
    
    // Calculate the instantaneous pixel velocity. Since clip-space coordinates are of the range [-1, 1] 
	// with Y increasing from the bottom to the top of screen, we'll rescale x and y and flip y so that
	// the velocity corresponds to texture coordinates (which are of the range [0,1], and y increases from top to bottom)
	float2 vVelocity = (input.CurrPositionCS.xy / input.CurrPositionCS.w) - (input.PrevPositionCS.xy / input.PrevPositionCS.w);
	vVelocity *= 0.5f;
	vVelocity.y *= -1;
	output.Velocity = float4(vVelocity, 1.0f, 1.0f);
	
    /////////////////////////////////////////////////////////////////////////
    //output.Color = float4(0.0, 0.0, 0.0, 1.0);
    //output.DepthColor = float4(input.Depth, input.Depth, input.Depth, 1.0f);
    //output.Color = output.DepthColor ;
	return output;
	
}

technique SSSTechnique
{
    pass Pass1
    {
		ZEnable = true;
		ZWriteEnable = true;
		AlphaBlendEnable = false;
		CullMode = CCW;
		AlphaTestEnable = false;
		StencilEnable = true;
		StencilFunc = ALWAYS;
		StencilRef = 1;
		StencilPass = REPLACE;
		StencilZFail = KEEP;
        VertexShader = compile vs_3_0 VS_ScreenSpaceShadow();
        PixelShader = compile ps_3_0 PS_ScreenSpaceShadow();
        
        StencilEnable = false;
    }
}

technique NormalMappingSSSTechnique
{
    pass Pass1
    {
		//SamplerStates[3] = NormalMap;
		sampler[3] = <normalSampler>;

		ZEnable = true;
		ZWriteEnable = true;
		AlphaBlendEnable = false;
		CullMode = CCW;
		AlphaTestEnable = false;
		StencilEnable = true;
		StencilFunc = ALWAYS;
		StencilRef = 1;
		StencilPass = REPLACE;
		StencilZFail = KEEP;
        VertexShader = compile vs_3_0 VS_ScreenSpaceShadow();
        PixelShader = compile ps_3_0 PS_ScreenSpaceShadowWithNormalMapping();
        
        StencilEnable = false;
    }
}