//////////////////////////////////////////////
#define MAX_SHADER_MATRICES 60


// Array of instance transforms used by the VFetch and ShaderInstancing techniques.
float4x4 InstanceTransforms[MAX_SHADER_MATRICES];

float4x4 World;
float4x4 View;
float4x4 ViewProj;
//float4x4 LightViewProj;
float4x4 PrevWorldViewProj;
float4 LightPosition[2];

float4 CameraPosition;

float Shineness = 96.0f;
float MaxDepth;
float4 vSpecularColor[2];// = {1.0f, 1.0f, 1.0f, 1.0f};
float4 vAmbient; //= {0.1f, 0.1f, 0.1f, 1.0f};
float4 vDiffuseColor[2];// = {1.0f, 1.0f, 1.0f, 1.0f};
float4 materialDiffuseColor = {1.0f, 1.0f, 1.0f, 1.0f};
int totalLights;

float4 vaditionalLightColor[2];
float4 vaditionalLightPositions[2];
float vaditionalLightRadius[2];
int vaditionalLightType[2];
int aditionalLights = 0;
    
// PARALLAX
float2 parallaxscaleBias;

// PARALLAX OCLUSSION

//float    g_fHeightMapScale; 
//bool     g_bDisplayShadows = true;        
//float    g_fShadowSoftening = 0.5f;       

//int      g_nMinSamples;            
//int      g_nMaxSamples;

texture TexColor;
sampler ColorSampler = sampler_state
{
    Texture = <TexColor>;

    MinFilter = ANISOTROPIC;
    MagFilter = ANISOTROPIC;
    MipFilter = ANISOTROPIC;
};

texture TexBlurV;
sampler BlurVSampler = sampler_state
{
    Texture = <TexBlurV>;

    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

//sampler normalSampler : register(s2);
//sampler heightSampler : register(s3);
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

texture SSAOMap;
sampler2D ssaoSampler = sampler_state
{
	Texture = <SSAOMap>;
	
	MAGFILTER = Linear;
	MINFILTER = Linear;
	MIPFILTER = Linear;
};

texture SpecularMap;
sampler2D specularSampler = sampler_state
{
	Texture = <SpecularMap>;
	
	MAGFILTER = Linear;
	MINFILTER = Linear;
	MIPFILTER = Linear;
};

texture EnvironmentMap;
samplerCUBE EnvironmentMapSampler = sampler_state 
{ 
    texture = <EnvironmentMap>;     
};

struct VS_ScreenSpaceShadow_Input
{
    float4 Position : POSITION0;
    float3 Normal	: NORMAL0;
    float2 TexCoord : TEXCOORD0;
    float3 Binormal : BINORMAL0;
    float3 Tangent  : TANGENT0;
};

struct VS_ScreenSpaceShadow_Output
{
    float4 Position			: POSITION;
    float2 TexCoord			: TEXCOORD0;
    float4 ScreenCoord		: TEXCOORD1;
    float4 WorldPosition	: TEXCOORD2;
    float4 PrevPositionCS	: TEXCOORD3;
    float4 CurrPositionCS	: TEXCOORD4;
    float4 PositionViewS	: TEXCOORD5;
    float3x3 TBN				: TEXCOORD6;
    
};

struct PS_ScreenSpaceShadow_Output
{
	float4 Color : COLOR0;
	float4 DepthColor : COLOR1;
	float4 Velocity : COLOR2;
};

struct PS_ScreenSpaceShadowNoMRTSSAO_Output
{
	float4 Color : COLOR0;
	half4 Normal : COLOR1;
	half4 ViewS : COLOR2;
};

VS_ScreenSpaceShadow_Output VS_ScreenSpaceShadow(VS_ScreenSpaceShadow_Input input)
{
    VS_ScreenSpaceShadow_Output output;
	
	//
    output.WorldPosition = mul(input.Position, World);
    output.Position = mul(output.WorldPosition, ViewProj);

	//
	output.PositionViewS = mul(mul(input.Position, World), View);
	output.CurrPositionCS = output.Position;
	output.PrevPositionCS = mul(input.Position, PrevWorldViewProj);
	
	//
	output.TexCoord = input.TexCoord;
	    
    /////output.ScreenCoord.x = (output.Position.x * 0.5f + output.Position.w * 0.5f);
    /////output.ScreenCoord.y = (output.Position.w * 0.5f - output.Position.y * 0.5f);
    
    //
    output.ScreenCoord.x = 0.5f * (output.Position.x + output.Position.w + 0.25f);
    output.ScreenCoord.y = 0.5f * (output.Position.w - output.Position.y - 0.25f);
    output.ScreenCoord.z = output.Position.w;
    output.ScreenCoord.w = output.Position.w;
		
	// TBN SPACE
	output.TBN[0] = mul(input.Tangent, World);
    output.TBN[1] = mul(input.Binormal, World);
    output.TBN[2] = mul(input.Normal, World);
    return output;
}
PS_ScreenSpaceShadowNoMRTSSAO_Output PS_ScreenSpaceShadowNoMRTSSAO(VS_ScreenSpaceShadow_Output input, uniform bool bnormalmapping,
	uniform bool bparallax)
{
	PS_ScreenSpaceShadowNoMRTSSAO_Output output;
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
    
	float4 Color = tex2D(ColorSampler, texCoord);
	
	float fShadowTerm = tex2Dproj(BlurVSampler, input.ScreenCoord).x;
	float4 totalDiffuse = float4(0,0,0,0);
	float4 totalSpecular = float4(0,0,0,0);
	float3 Normal;
	
	if (bnormalmapping)
	{
		Normal = 2.0f * tex2D(normalSampler, texCoord) - 1.0f;
		Normal = normalize(mul(Normal, input.TBN));
	} else
		Normal = input.TBN[2];
		
	
	for (int k = 0; k < totalLights; k++)
	{
		//
		float3 LightDir = normalize(LightPosition[k] - input.WorldPosition);
		
		// self shadowing
	    //float selfshadow = saturate(4.0 * LightDir.z);
	    //totalselfshadowing *= selfshadow;
	    
	    
		// Calculate normal diffuse light.
		float DiffuseLightingFactor = dot(LightDir, Normal);
	    
		// R = 2 * (N.L) * N – L
		float3 Reflect = normalize(2 * DiffuseLightingFactor * Normal - LightDir);  
		float Specular = pow(saturate(dot(Reflect, ViewDir)), Shineness); // R.V^n
	    
		// I = A + Dcolor * Dintensity * N.L + Scolor * Sintensity * (R.V)n
	    totalDiffuse += (vDiffuseColor[k] * DiffuseLightingFactor);
	    totalSpecular += vSpecularColor[k] * Specular;
	    
    }
    float4 totalDiffuse2 = float4(0,0,0,0);
    for (int k = 0; k < aditionalLights; ++k)
    {
		//
		float3 LightDir = (vaditionalLightPositions[k] - input.WorldPosition);
		
		if (vaditionalLightType[k] == 0) // Point Light
		{
			float attenuation = saturate(1.0f - dot(LightDir / vaditionalLightRadius[k], LightDir / vaditionalLightRadius[k]));
			
			LightDir = normalize(LightDir);
			float DiffuseLightingFactor = dot(LightDir, Normal);
			totalDiffuse2 += (vaditionalLightColor[k] * DiffuseLightingFactor) * attenuation;
			
		} else if (vaditionalLightType[k] == 1)
		{
			LightDir = normalize(LightDir);
			//float3 ViewDir = normalize(CameraPosition - input.WorldPosition);
			
			// Calculate normal diffuse light.
			float DiffuseLightingFactor = dot(LightDir, Normal);
			totalDiffuse2 += (vaditionalLightColor[k] * DiffuseLightingFactor);
		}
    }
    //totalDiffuse /= totalLights;
    //totalAmbient /= totalLights;
    
    totalDiffuse = saturate(totalDiffuse);
    //
    //output.Color  = (Color * (vAmbient + vDiffuseColor * DiffuseLightingFactor * materialDiffuseColor) + vSpecularColor * Specular) * fShadowTerm;
    /////output.Color = saturate((Color * (vAmbient + totalDiffuse * materialDiffuseColor) + totalSpecular)) * fShadowTerm;
    output.Color = saturate((Color * (vAmbient + totalDiffuse * materialDiffuseColor) + totalSpecular) * fShadowTerm + totalDiffuse2 * totalDiffuse2.a);
    
    output.Normal = float4((Normal + 1.0f) * 0.5f , 1.0f);
    output.ViewS = input.PositionViewS;
	
	return output;
	
}

PS_ScreenSpaceShadow_Output PS_ScreenSpaceShadow(VS_ScreenSpaceShadow_Output input, uniform bool bnormalmapping,
	uniform bool bparallax)
{
	PS_ScreenSpaceShadow_Output output;
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
    
	float4 Color = tex2D(ColorSampler, texCoord);
	
	float fShadowTerm = tex2Dproj(BlurVSampler, input.ScreenCoord).x;
	float4 totalDiffuse = float4(0,0,0,0);
	float4 totalSpecular = float4(0,0,0,0);
	float3 Normal;
	
	if (bnormalmapping)
	{
		Normal = 2.0f * tex2D(normalSampler, texCoord) - 1.0f;
		Normal = normalize(mul(Normal, input.TBN));
	} else
		Normal = input.TBN[2];
		
	
	for (int k = 0; k < totalLights; k++)
	{
		//
		float3 LightDir = normalize(LightPosition[k] - input.WorldPosition);
		
		// self shadowing
	    //float selfshadow = saturate(4.0 * LightDir.z);
	    //totalselfshadowing *= selfshadow;
	    
	    
		// Calculate normal diffuse light.
		float DiffuseLightingFactor = dot(LightDir, Normal);
	    
		// R = 2 * (N.L) * N – L
		float3 Reflect = normalize(2 * DiffuseLightingFactor * Normal - LightDir);  
		float Specular = pow(saturate(dot(Reflect, ViewDir)), Shineness); // R.V^n
	    
		// I = A + Dcolor * Dintensity * N.L + Scolor * Sintensity * (R.V)n
	    totalDiffuse += (vDiffuseColor[k] * DiffuseLightingFactor);
	    totalSpecular += vSpecularColor[k] * Specular;
	    
    }
    float4 totalDiffuse2 = float4(0,0,0,0);
    for (int k = 0; k < aditionalLights; ++k)
    {
		//
		float3 LightDir = (vaditionalLightPositions[k] - input.WorldPosition);
		
		if (vaditionalLightType[k] == 0) // Point Light
		{
			float attenuation = saturate(1.0f - dot(LightDir / vaditionalLightRadius[k], LightDir / vaditionalLightRadius[k]));
			
			LightDir = normalize(LightDir);
			float DiffuseLightingFactor = dot(LightDir, Normal);
			totalDiffuse2 += (vaditionalLightColor[k] * DiffuseLightingFactor) * attenuation;
			
		} else if (vaditionalLightType[k] == 1)
		{
			LightDir = normalize(LightDir);
			//float3 ViewDir = normalize(CameraPosition - input.WorldPosition);
			
			// Calculate normal diffuse light.
			float DiffuseLightingFactor = dot(LightDir, Normal);
			totalDiffuse2 += (vaditionalLightColor[k] * DiffuseLightingFactor);
		}
    }
    //totalDiffuse /= totalLights;
    //totalAmbient /= totalLights;
    
    totalDiffuse = saturate(totalDiffuse);
    //
    //output.Color  = (Color * (vAmbient + vDiffuseColor * DiffuseLightingFactor * materialDiffuseColor) + vSpecularColor * Specular) * fShadowTerm;
    /////output.Color = saturate((Color * (vAmbient + totalDiffuse * materialDiffuseColor) + totalSpecular)) * fShadowTerm;
    output.Color = saturate((Color * (vAmbient + totalDiffuse * materialDiffuseColor) + totalSpecular) * fShadowTerm + totalDiffuse2 * totalDiffuse2.a);
    
    //
    output.DepthColor = float4(-input.PositionViewS.z / MaxDepth, 1.0f, 1.0f, 1.0f);
    
    // Calculate the instantaneous pixel velocity. Since clip-space coordinates are of the range [-1, 1] 
	// with Y increasing from the bottom to the top of screen, we'll rescale x and y and flip y so that
	// the velocity corresponds to texture coordinates (which are of the range [0,1], and y increases from top to bottom)
	float2 vVelocity = (input.CurrPositionCS.xy / input.CurrPositionCS.w) - (input.PrevPositionCS.xy / input.PrevPositionCS.w);
	vVelocity *= 0.5f;
	vVelocity.y *= -1;
	output.Velocity = float4(vVelocity, 1.0f, 1.0f);
	
	return output;
	
}

float4 PS_ScreenSpaceShadowNoMRT(VS_ScreenSpaceShadow_Output input, uniform bool bnormalmapping,
	uniform bool bparallax, uniform bool bDEM) : COLOR
{

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
        
	float4 Color = tex2D(ColorSampler, texCoord);
	
	float fShadowTerm = tex2Dproj(BlurVSampler, input.ScreenCoord).x;
	float4 totalDiffuse = float4(0,0,0,0);
	float4 totalSpecular = float4(0,0,0,0);
	float3 Normal;
	float ssaoterm = tex2D(ssaoSampler, texCoord).r;
	
	if (bnormalmapping)
	{
		Normal = 2.0f * tex2D(normalSampler, texCoord) - 1.0f;
		Normal = normalize(mul(Normal, input.TBN));
		
	} else
		Normal = input.TBN[2];
	
	if (bDEM)
	{
		float3 Reflection = reflect(input.WorldPosition.xyz - CameraPosition.xyz, Normal);
		// Approximate a Fresnel coefficient for the environment map.
		// This makes the surface less reflective when you are looking
		// straight at it, and more reflective when it is viewed edge-on.
		float3 Fresnel = saturate(1.0f + dot(-ViewDir, Normal));
	    
		float3 envmap = texCUBE(EnvironmentMapSampler, Reflection);
		float3 ccolor = tex2D(ColorSampler, texCoord);
		Color = float4(lerp(ccolor, envmap, Fresnel), 1.0f);
		//Color = float4(envmap, 1.0f);
    }
    
	for (int k = 0; k < totalLights; k++)
	{
		//
		float3 LightDir = normalize(LightPosition[k] - input.WorldPosition);
	    
		// Calculate normal diffuse light.
		float DiffuseLightingFactor = dot(LightDir, Normal);
	    
		// R = 2 * (N.L) * N – L
		float3 Reflect = normalize(2 * DiffuseLightingFactor * Normal - LightDir);  
		float Specular = pow(saturate(dot(Reflect, ViewDir)), Shineness); // R.V^n
	    
		// I = A + Dcolor * Dintensity * N.L + Scolor * Sintensity * (R.V)n
	    totalDiffuse += vDiffuseColor[k] * DiffuseLightingFactor;
	    totalSpecular += vSpecularColor[k] * Specular;
    }
    
    float4 totalDiffuse2 = float4(0,0,0,0);
    for (int k = 0; k < aditionalLights; ++k)
    {
		//
		float3 LightDir = (vaditionalLightPositions[k] - input.WorldPosition);
		
		if (vaditionalLightType[k] == 0) // Point Light
		{
			float attenuation = saturate(1.0f - dot(LightDir / vaditionalLightRadius[k], LightDir / vaditionalLightRadius[k]));
			
			LightDir = normalize(LightDir);
			float DiffuseLightingFactor = dot(LightDir, Normal);
			totalDiffuse2 += (vaditionalLightColor[k] * DiffuseLightingFactor) * attenuation;
			
		} else if (vaditionalLightType[k] == 1)
		{
			LightDir = normalize(LightDir);
			//float3 ViewDir = normalize(CameraPosition - input.WorldPosition);
			
			// Calculate normal diffuse light.
			float DiffuseLightingFactor = dot(LightDir, Normal);
			totalDiffuse2 += (vaditionalLightColor[k] * DiffuseLightingFactor);
		}
    }
    //totalDiffuse /= totalLights;    
    totalDiffuse = saturate(totalDiffuse);
    //
    
    totalSpecular *= tex2D(specularSampler, texCoord);
    return (saturate((Color * (vAmbient + totalDiffuse * materialDiffuseColor) + totalSpecular) * fShadowTerm + totalDiffuse2 * totalDiffuse2.a)) * ssaoterm;
    //return saturate(Color);
    
}


////////////////////////////////////////////////////////////

// Vertex shader helper function shared between the different instancing techniques.
VS_ScreenSpaceShadow_Output VertexShaderCommon(VS_ScreenSpaceShadow_Input input,
                                      float4x4 instanceTransform)
{
	VS_ScreenSpaceShadow_Output output;
	
	//
    output.WorldPosition = mul(input.Position, instanceTransform);
    output.Position = mul(output.WorldPosition, ViewProj);

	//
	output.PositionViewS = mul(mul(input.Position, instanceTransform), View);
	output.CurrPositionCS = output.Position;
	output.PrevPositionCS = mul(input.Position, PrevWorldViewProj);
	
	//
	output.TexCoord = input.TexCoord;
	
    
    /////output.ScreenCoord.x = (output.Position.x * 0.5f + output.Position.w * 0.5f);
    /////output.ScreenCoord.y = (output.Position.w * 0.5f - output.Position.y * 0.5f);
    
    //
    output.ScreenCoord.x = 0.5f * (output.Position.x + output.Position.w + 0.25f);
    output.ScreenCoord.y = 0.5f * (output.Position.w - output.Position.y - 0.25f);
    output.ScreenCoord.z = output.Position.w;
    output.ScreenCoord.w = output.Position.w;
		
	// TBN SPACE
	output.TBN[0] = mul(input.Tangent, instanceTransform);
    output.TBN[1] = mul(input.Binormal, instanceTransform);
    output.TBN[2] = mul(input.Normal, instanceTransform);
    
	return output;
}

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/*struct VS_OUTPUT_POM
{
    float4 position          : POSITION0;
    float2 Texcoord          : TEXCOORD0;
    float3 vLightTS          : TEXCOORD1;   
    float3 vViewTS           : TEXCOORD2;  
    float2 vParallaxOffsetTS : TEXCOORD3;   
    float3 vNormalWS         : TEXCOORD4;   
    float3 vViewWS           : TEXCOORD5; 
    float4 dist				 : TEXCOORD6;
}; */
/*
struct VS_ScreenSpaceShadowPOM_Output
{
    float4 Position			: POSITION;
    float2 TexCoord			: TEXCOORD0_centroid;
    float2 ProjCoord		: TEXCOORD1_centroid;
    float4 ScreenCoord		: TEXCOORD2_centroid;
    float4 WorldPosition	: TEXCOORD3_centroid;
    
    float4 PrevPositionCS	: TEXCOORD4_centroid;
    float4 CurrPositionCS	: TEXCOORD5_centroid;
    float4 PositionViewS	: TEXCOORD6_centroid;
    float2 vParallaxOffsetTS				: TEXCOORD7_centroid;
    //float3x3 TBN				: TEXCOORD7;
    
    
};

struct VS_ScreenSpaceShadow_Input
{
    float4 Position : POSITION;
    float3 Normal	: NORMAL;
    float2 TexCoord : TEXCOORD0;
    float3 Binormal : BINORMAL0;
    float3 Tangent  : TANGENT0;
};


*/
// On Windows shader 3.0 cards, we can use hardware instancing, reading
// the per-instance world transform directly from a secondary vertex stream.
VS_ScreenSpaceShadow_Output HardwareInstancingVertexShader(VS_ScreenSpaceShadow_Input input,
                                                float4x4 instanceTransform : TEXCOORD1)
{
    return VertexShaderCommon(input, transpose(instanceTransform));
}

//#endif
technique NoMRTSSSTechniqueSSAO
{
    pass P0
    {
		sampler[0] = <ColorSampler>;
        VertexShader = compile vs_3_0 VS_ScreenSpaceShadow();
        PixelShader = compile ps_3_0 PS_ScreenSpaceShadowNoMRTSSAO(false, false);
    }
}

technique SSSTechnique
{
    pass P0
    {
		sampler[0] = <ColorSampler>;
        VertexShader = compile vs_3_0 VS_ScreenSpaceShadow();
        PixelShader = compile ps_3_0 PS_ScreenSpaceShadow(false, false);
    }
}
technique NoMRTSSSTechnique
{
    pass P0
    {
		sampler[0] = <ColorSampler>;
		sampler[4] = <ssaoSampler>;
        VertexShader = compile vs_3_0 VS_ScreenSpaceShadow();
        PixelShader = compile ps_3_0 PS_ScreenSpaceShadowNoMRT(false, false, false);
    }
}

technique NoMRTEMSSSTechnique
{
    pass P0
    {
		sampler[0] = <ColorSampler>;
		sampler[4] = <ssaoSampler>;
        VertexShader = compile vs_3_0 VS_ScreenSpaceShadow();
        PixelShader = compile ps_3_0 PS_ScreenSpaceShadowNoMRT(false, false, true);
    }
}
technique NormalMappingSSSTechnique
{
    pass P0
    {
		sampler[0] = <ColorSampler>;
		sampler[2] = <normalSampler>;

        VertexShader = compile vs_3_0 VS_ScreenSpaceShadow();
        PixelShader = compile ps_3_0 PS_ScreenSpaceShadow(true, false);
    }
}

technique NoMRTNormalMappingSSSTechnique
{
    pass P0
    {
		sampler[0] = <ColorSampler>;
		sampler[2] = <normalSampler>;

        VertexShader = compile vs_3_0 VS_ScreenSpaceShadow();
        PixelShader = compile ps_3_0 PS_ScreenSpaceShadowNoMRT(true, false, false);
    }
}

technique NoMRTParallaxMappingSSSTechnique
{
    pass P0
    {
		sampler[0] = <ColorSampler>;
		sampler[2] = <normalSampler>;
		sampler[3] = <heightSampler>;
		sampler[4] = <ssaoSampler>;
        VertexShader = compile vs_3_0 VS_ScreenSpaceShadow();
        PixelShader = compile ps_3_0 PS_ScreenSpaceShadowNoMRT(true, true, false);
    }
}

technique ParallaxMappingSSSTechnique
{
    pass P0
    {
		sampler[0] = <ColorSampler>;
		sampler[2] = <normalSampler>;
		sampler[3] = <heightSampler>;
        VertexShader = compile vs_3_0 VS_ScreenSpaceShadow();
        PixelShader = compile ps_3_0 PS_ScreenSpaceShadow(true, true);
    }
}

// Windows instancing technique for shader 3.0 cards.
technique HardwareInstancingNoMRTSSSTechnique
{
    pass P0
    {
		sampler[0] = <ColorSampler>;
        VertexShader = compile vs_3_0 HardwareInstancingVertexShader();
        PixelShader = compile ps_3_0 PS_ScreenSpaceShadowNoMRT(false, false, false);
    }
}

// Windows instancing technique for shader 3.0 cards.
technique HardwareInstancingSSSTechnique
{
    pass P0
    {
		sampler[0] = <ColorSampler>;
        VertexShader = compile vs_3_0 HardwareInstancingVertexShader();
        PixelShader = compile ps_3_0 PS_ScreenSpaceShadow(false, false);
    }
}